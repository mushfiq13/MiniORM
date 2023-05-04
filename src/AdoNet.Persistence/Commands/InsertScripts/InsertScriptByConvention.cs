namespace AdoNet.Persistence.Commands.InsertScripts;

public class InsertScriptByConvention : INonQueryScript
{
    public readonly static INonQueryScript Default = new InsertScriptByConvention();
    Dictionary<string, List<StringBuilder>> _individualTableScript = new();

    private InsertScriptByConvention() { }

    public string Create<TEntity>(TEntity entity) where TEntity : class
    {
        GenerateScript(entity);

        var linearOrder = TopologicalSort<TEntity>.SortInLinearOder();

        return FinalizeScriptByOrder(linearOrder);
    }

    private object GenerateScript(
       object item,
       string foreignKey = null!,
       string foreignValue = null!)
    {
        var type = item.GetType();
        List<string> records = new();
        List<string> columns = new();
        string? curItemId = null;

        foreach (var property in type.GetProperties())
        {
            var propertyType = property.PropertyType;
            var propertyName = propertyType.Name;
            var value = property.GetValue(item, null);

            if (value == null)
                continue;

            if (propertyType == typeof(DateTime))
            {
                DateTime.TryParseExact(value.ToString(),
                    "YYYY-MM-DD HH:MM:SS",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime formattedDate);

                value = formattedDate;
            }

            if (propertyType == typeof(Guid))
            {
                records.Add($"'{value}'");
                columns.Add(property.Name);
                curItemId = value.ToString()!;
            }
            else if (property.Name == "Id")
            {
                continue;
            }
            else if (propertyType.IsValueType || propertyType == typeof(string))
            {
                records.Add($"'{value}'");
                columns.Add(property.Name);
            }
            else if (value is IEnumerable)
            {
                var nestedObj = value as IEnumerable;

                foreach (var obj in nestedObj!)
                {
                    GenerateScript(obj, $"{type.Name}Id", curItemId!);
                }
            }
            else if (propertyType.IsClass)
            {
                var propertyItemId = GenerateScript(value);

                records.Add($"'{propertyItemId}'");
                columns.Add($"{property.Name}Id");
            }
        }

        if (foreignKey != null)
        {
            columns.Add(foreignKey);
            records.Add($"'{foreignValue}'");
        }

        CreateAndSaveScript(type.Name, columns, records);

        return curItemId!;
    }

    private void CreateAndSaveScript(string tableName, List<string> columns, List<string> records)
    {
        var sql = new StringBuilder($"insert into {tableName}")
            .Append(" ( ")
            .AppendJoin(',', columns)
            .Append(" ) VALUES ( ")
            .AppendJoin(',', records)
            .AppendLine(" ); ");

        if (_individualTableScript.ContainsKey(tableName) == false)
        {
            _individualTableScript[tableName] = new();
        }

        _individualTableScript[tableName].Add(sql);
    }

    private string FinalizeScriptByOrder(IEnumerable<string> tableOrder)
    {
        var finalScript = new StringBuilder();

        foreach (var table in tableOrder)
        {
            _individualTableScript[table]?.ForEach(sql => finalScript.Append(sql));
        }

        return finalScript.ToString();
    }
}
