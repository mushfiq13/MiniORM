namespace AdoNet.Persistence.Utilities;

internal static class DirectAcyclicGraph<TEntity>
    where TEntity : class
{
    private static Dictionary<string, List<string>> _graph = new();
    private static List<string> _vertices = new();

    public static (IList<string> vertices, IDictionary<string, List<string>> graph) Create()
    {
        GenerateDAG(typeof(TEntity));

        return (_vertices, _graph);
    }

    private static void GenerateDAG(Type currentType)
    {
        bool itemHasChildObject = false;

        foreach (var property in currentType.GetProperties())
        {
            if (property.PropertyType == typeof(string))
            {
                continue;
            }

            if (property.PropertyType.IsAssignableTo(typeof(ICollection)))
            {
                foreach (var argument in property.PropertyType.GetGenericArguments())
                {
                    AddToTree(argument.Name, currentType.Name);
                    GenerateDAG(argument);
                }
            }
            else if (property.PropertyType.IsClass)
            {
                itemHasChildObject = true;

                AddToTree(currentType.Name, property.PropertyType.Name);
                GenerateDAG(property.PropertyType);
            }
        }

        if (_vertices.Contains(currentType.Name) == false)
        {
            _vertices.Add(currentType.Name);
        }

        if (itemHasChildObject == false)
        {
            AddToTree(currentType.Name, null!);
        }
    }

    private static void AddToTree(string parentNode, string? childNode)
    {
        _graph.TryAdd(parentNode, new());

        if (childNode != null && _graph[parentNode].Contains(childNode) == false)
            _graph[parentNode]?.Add(childNode);
    }
}
