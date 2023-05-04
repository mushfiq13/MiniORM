namespace AdoNet.Persistence.Utilities;

internal static class TopologicalSort<TEntity>
    where TEntity : class
{
    static Dictionary<string, byte> _map = new();
    static List<string> _linearOrder = new();

    public static IList<string> SortInLinearOder()
    {
        var data = DirectAcyclicGraph<TEntity>.Create();

        foreach (var v in data.vertices)
        {
            if (_map.ContainsKey(v))
                continue;

            DFS(v, data.graph);
        }

        return _linearOrder;
    }

    private static void DFS(string u, IDictionary<string, List<string>> graph)
    {
        if (u == null) return;

        _map.Add(u, 1);

        foreach (var v in graph[u])
        {
            if (_map.ContainsKey(v)) continue;
            if (_map.ContainsKey(v) && _map[v] == 1) return;

            DFS(v, graph);
        }

        _linearOrder.Add(u);
        _map[u] = 2;
    }
}
