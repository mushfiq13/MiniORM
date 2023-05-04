namespace AdoNet.Persistence.ScriptSignatures;

public interface IScriptExecuter
{
    void ExecuteNonQuery(string sql);
    IList<List<(string column, object value)>> ExecuteQuery(string query);
}
