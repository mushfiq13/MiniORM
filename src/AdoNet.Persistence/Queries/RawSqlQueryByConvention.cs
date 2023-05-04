namespace AdoNet.Persistence.Queries;

public class RawSqlQueryByConvention : IQueryScript
{
    public readonly static IQueryScript Default = new RawSqlQueryByConvention();
    Dictionary<string, int> _countAddedType = default!;
    List<string> _alias = default!;

    private RawSqlQueryByConvention() { }

    public (string script, IList<Type> tableCreationOrder) Create<TEntity>(object? id)
        where TEntity : class
    {
        _alias = new();
        _countAddedType = new();

        var mainType = typeof(TEntity);
        _countAddedType.TryAdd(mainType.Name, 1);
        _alias.Add(GetCorrelationName(new(mainType.Name)).ToString());

        List<Type> tableCreationOrder = new();
        var joinClause = GenerateJoinClauseAndTables(mainType, ref tableCreationOrder);
        var query = FinalizeScript(new(mainType.Name),
            joinClause,
            id).ToString();

        return (query, tableCreationOrder);
    }

    private StringBuilder GenerateJoinClauseAndTables(
        Type tableType,
        ref List<Type> tableCreationOrder)
    {
        tableCreationOrder.Add(tableType);
        StringBuilder joinClause = new();

        foreach (var property in tableType.GetProperties())
        {
            var propertyType = property.PropertyType;

            if (propertyType == typeof(string))
                continue;

            if (propertyType.IsAssignableTo(typeof(ICollection)))
            {
                foreach (var argumentType in propertyType.GetGenericArguments())
                {
                    joinClause.Append(
                        CreateJoinClause(tableType.Name, argumentType.Name, tableType.Name, string.Empty));
                    joinClause.Append(
                        GenerateJoinClauseAndTables(argumentType, ref tableCreationOrder));
                }
            }
            else if (propertyType.IsClass)
            {
                joinClause.Append(
                    CreateJoinClause(tableType.Name, propertyType.Name, string.Empty, property.Name));
                joinClause.Append(
                    GenerateJoinClauseAndTables(propertyType, ref tableCreationOrder));
            }
        }

        return joinClause;
    }

    private StringBuilder CreateJoinClause(
        string principleTableName,
        string childTableName,
        string _suffixLeft,
        string _suffixRight)
    {
        _countAddedType.TryAdd(childTableName, default!);
        _countAddedType[childTableName] += 1;

        var _correlationNameForPrinciple = GetCorrelationName(new(principleTableName));
        var _correlationNameForChild = GetCorrelationName(new(childTableName));

        _alias.Add(_correlationNameForChild.ToString());

        return new StringBuilder()
            .Append($"LEFT JOIN [{childTableName}] AS [{_correlationNameForChild}]")
            .AppendLine($" ON [{_correlationNameForChild}].[{_suffixLeft}Id]" +
                $" = [{_correlationNameForPrinciple}].[{_suffixRight}Id]");
    }

    private StringBuilder FinalizeScript(
        StringBuilder primaryTable,
        StringBuilder joinClause,
        object queryById = default!)
    {
        var whereClause = new StringBuilder(queryById != null
                ? $"WHERE [{GetCorrelationName(primaryTable)}].[Id] = '{queryById}'"
                : "");

        return new StringBuilder()
            .AppendLine("SELECT")
            .AppendJoin(",\n", _alias.Select(t => $"[{t}].*"))
            .AppendLine()
            .AppendLine($"FROM {primaryTable} AS [{GetCorrelationName(primaryTable)}]")
            .Append(joinClause)
            .Append(whereClause);
    }

    private StringBuilder GetCorrelationName(StringBuilder tableName)
    {
        return new StringBuilder($"{tableName}{_countAddedType[$"{tableName}"]}");
    }
}