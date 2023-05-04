namespace AdoNet.Persistence.ScriptSignatures;

public interface INonQueryScript
{
    string Create<TEntity>(TEntity entity) where TEntity : class;
}
