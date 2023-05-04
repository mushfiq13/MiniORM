namespace AdoNet.Persistence.ScriptSignatures;

public interface IQueryScript
{
    (string script, IList<Type> tableCreationOrder) Create<TEntity>(object? id)
        where TEntity : class;
}
