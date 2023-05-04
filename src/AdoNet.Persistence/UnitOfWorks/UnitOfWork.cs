using AdoNet.Persistence.Repositories;
using Microsoft.Data.SqlClient;

namespace AdoNet.Persistence.UnitOfWorks;

public abstract class UnitOfWork
{
    private IDbConnection _connection;
    private IDbTransaction _transaction;
    private List<Action> _writeOperationsFunctionRef = new();

    protected UnitOfWork(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public void Save()
    {
        try
        {
            _writeOperationsFunctionRef.ForEach(action => action());
            _transaction.Commit();
        }
        catch
        {
            _transaction.Rollback();
            throw;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _transaction?.Dispose();
    }

    public Repository<TEntity> Set<TEntity>()
        where TEntity : class
    {
        var repo = Repository<TEntity>.CreateInstance(
            _connection,
            _transaction,
            out Action funcRef);
        _writeOperationsFunctionRef.Add(funcRef);

        return repo;
    }
}
