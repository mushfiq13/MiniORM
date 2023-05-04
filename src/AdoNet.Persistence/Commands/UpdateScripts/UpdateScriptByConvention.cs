namespace AdoNet.Persistence.Commands.UpdateScripts;

public class UpdateScriptByConvention : INonQueryScript
{
    public readonly static INonQueryScript Default = new UpdateScriptByConvention();

    private UpdateScriptByConvention() { }

    public string Create<TEntity>(TEntity entity)
        where TEntity : class
    {
        return CreateScript(entity);
    }

    private string CreateScript(object item)
    {
        object? itemId = null!;
        var itemType = item.GetType();
        List<string> updateableColumnWithNewValue = new();

        foreach (var property in itemType.GetProperties())
        {
            var value = property.GetValue(item, null);

            if (property.Name.Equals("Id"))
            {
                itemId = value;
            }
            else if (property.PropertyType == typeof(string)
                || property.PropertyType.IsValueType)
            {
                updateableColumnWithNewValue.Add($"{property.Name}='{value}'");
            }
        }

        return new StringBuilder($"UPDATE {itemType.Name}")
            .AppendLine()
            .Append($"SET ")
            .AppendJoin(", ", updateableColumnWithNewValue)
            .AppendLine()
            .AppendLine($"WHERE Id = '{itemId}'").ToString();
    }
}
