namespace AdoNet.Persistence.ScriptsExecute;

public class ScriptExecuter : IScriptExecuter
{
    private readonly IDbCommand _dbCommand;

    private ScriptExecuter(IDbCommand dbCommand)
        => _dbCommand = dbCommand;

    public void ExecuteNonQuery(string sql)
    {
        if (sql.Trim().Length == 0)
            return;

        _dbCommand.CommandText = sql.ToString();
        _dbCommand.ExecuteNonQuery();
    }

    public IList<List<(string column, object value)>> ExecuteQuery(string query)
    {
        if (query.Trim().Length == 0)
            return default!;

        List<List<(string, object)>> records = new();

        _dbCommand.CommandText = query.ToString();

        using (var reader = _dbCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                List<(string, object)> record = new();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    record.Add((reader.GetName(i),
                        reader.GetValue(i)));
                }

                records.Add(record);
            }
        }

        return records;
    }

    public static IScriptExecuter CreateInstance(IDbCommand command)
        => new ScriptExecuter(command);
}
