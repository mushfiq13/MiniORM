namespace AdoNet.Persistence.Utilities;

public class RowSqlDataCompresser
{
    public readonly static RowSqlDataCompresser Default = new RowSqlDataCompresser();

    public (
        IDictionary<string, Dictionary<string, object>> seperatedTables,
        IDictionary<Type, SortedSet<string>> tableIdReference)
            Compress(IList<Type> tableOrder,
                IList<List<(string column, object value)>> records)
    {
        Dictionary<string, Dictionary<string, object>> seperatedTables = new();
        Dictionary<Type, SortedSet<string>> tableIdReference = new();

        foreach (var row in records)
        {
            var curIdxInTableOrder = 0;
            string curTableId = string.Empty;

            foreach (var field in row)
            {
                if (field.column.Equals("Id")) // New Table
                {
                    curTableId = field.value.ToString()!;

                    seperatedTables.TryAdd(curTableId, new());
                    tableIdReference.TryAdd(tableOrder[curIdxInTableOrder], new());
                    tableIdReference[tableOrder[curIdxInTableOrder]].Add(curTableId);

                    ++curIdxInTableOrder;
                }

                seperatedTables[curTableId].TryAdd(field.column, field.value);
            }
        }

        return (seperatedTables, tableIdReference);
    }
}
