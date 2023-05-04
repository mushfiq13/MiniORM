namespace AdoNet.Persistence.Repositories;

public class Repository<TEntity>
    where TEntity : class
{
    List<TEntity> _addedEntities = new();
    List<TEntity> _updatedEntities = new();
    List<TEntity> _removedEntities = new();
    IScriptExecuter _scriptExecuter;

    private Repository(in IDbConnection connection,
        in IDbTransaction transaction,
        out Action writeOperationsFunctionRef)
    {
        writeOperationsFunctionRef = ExecuteWriteOperations;

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        _scriptExecuter = ScriptExecuter.CreateInstance(command);
    }

    public void Add(TEntity entity)
    {
        if (_addedEntities.Contains(entity))
        {
            return;
        }

        _addedEntities.Add(entity);
    }

    public void Edit(TEntity entity)
    {
        if (_addedEntities.Contains(entity) == false)
        {
            _updatedEntities.Add(entity);
            return;
        }

        Remove(entity);
    }

    public void Remove(TEntity entity)
    {
        if (_addedEntities.Contains(entity))
        {
            _addedEntities.Remove(entity);
            return;
        }

        _removedEntities.Add(entity);
    }

    public TEntity GetById<TKey>(TKey id)
    {
        (string script, IList<Type> tableCreationOrder) = RawSqlQueryByConvention.Default
            .Create<TEntity>(id);

        return ReadTable(script, tableCreationOrder)?.SingleOrDefault()!;
    }

    public IList<TEntity> GetAll()
    {
        (string script, IList<Type> tableCreationOrder) =
            RawSqlQueryByConvention.Default.Create<TEntity>(null);

        return ReadTable(script, tableCreationOrder);
    }

    public void ExecuteWriteOperations()
    {
        void Execute(List<TEntity> scripts, Func<TEntity, string> func)
        {
            scripts.ForEach(entity =>
            {
                var command = func(entity);
                _scriptExecuter.ExecuteNonQuery(command);
            });
        }

        Execute(_addedEntities, InsertScriptByConvention.Default.Create);
        Execute(_updatedEntities, UpdateScriptByConvention.Default.Create);
        Execute(_removedEntities, DeleteScriptByConvention.Default.Create);

        _addedEntities.Clear();
        _updatedEntities.Clear();
        _removedEntities.Clear();
    }

    private IList<TEntity> ReadTable(string script, IList<Type> tableCreationOrder)
    {
        var records = _scriptExecuter.ExecuteQuery(script);
        var compressedData = RowSqlDataCompresser.Default
            .Compress(tableCreationOrder, records);

        var converter = new EntityConverter(
            compressedData.seperatedTables,
            compressedData.tableIdReference);

        return converter.Convert<TEntity>();
    }

    public static Repository<TEntity> CreateInstance(in IDbConnection connection,
        in IDbTransaction transaction,
        out Action writeOperationsFunctionRef)
    {
        return new Repository<TEntity>(connection, transaction, out writeOperationsFunctionRef);
    }
}
