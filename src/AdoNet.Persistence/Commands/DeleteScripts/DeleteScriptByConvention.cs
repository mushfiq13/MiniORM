namespace AdoNet.Persistence.Commands.DeleteScripts;

public class DeleteScriptByConvention : INonQueryScript
{
    public readonly static INonQueryScript Default = new DeleteScriptByConvention();
    private readonly Dictionary<string, List<object>> _individualTableScript = new();

    private DeleteScriptByConvention() { }

    public string Create<TEntity>(TEntity entity)
        where TEntity : class
    {
        GenerateScript(entity);

        var linearOrderReversed = TopologicalSort<TEntity>.SortInLinearOder().Reverse();

        return FinalizeScriptByOrder(linearOrderReversed);
    }

    private void GenerateScript(object item)
    {
        if (item == null)
            return;

        var type = item.GetType();

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(item, null);

            if (value == null || property.PropertyType == typeof(string))
                continue;

            if (property.Name.Equals("Id"))
            {
                _individualTableScript.TryAdd(type.Name, new());
                _individualTableScript[type.Name].Add(value);
            }
            else if (value is IEnumerable)
            {
                foreach (var curValue in (value as IEnumerable)!)
                {
                    GenerateScript(curValue);
                }
            }
            else if (property.PropertyType.IsClass)
            {
                GenerateScript(value);
            }
        }
    }

    private string FinalizeScriptByOrder(IEnumerable<string> tableOrder)
    {
        var finalScript = new StringBuilder();

        foreach (var table in tableOrder)
        {
            _individualTableScript.TryGetValue(table, out var scripts);

            scripts?.ForEach(id =>
            {
                finalScript.AppendLine($"DELETE FROM {table} WHERE Id = '{id}'");
            });
        }

        return finalScript.ToString();
    }
}
