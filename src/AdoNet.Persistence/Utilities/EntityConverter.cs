using System.Reflection;

namespace AdoNet.Persistence.Utilities;

public class EntityConverter
{
    private Dictionary<string, bool> _idCreationMap = default!;
    private readonly IDictionary<string, Dictionary<string, object>> _seperatedTables;
    private readonly IDictionary<Type, SortedSet<string>> _tableIdReference;

    public EntityConverter(
        IDictionary<string, Dictionary<string, object>> seperatedTables,
        IDictionary<Type, SortedSet<string>> tableIdReference)
    {
        _seperatedTables = seperatedTables;
        _tableIdReference = tableIdReference;
    }

    public IList<TEntity> Convert<TEntity>() where TEntity : class
    {
        if (_tableIdReference.ContainsKey(typeof(TEntity)) == false)
            return default!;

        List<TEntity> items = new();
        _idCreationMap = new();

        foreach (var id in _tableIdReference[typeof(TEntity)]!)
        {
            items.Add(
                CreateObject(typeof(TEntity), id) as TEntity);
        }

        return items;
    }

    private object CreateObject(Type type, object objId)
    {
        var objIdString = objId.ToString();

        if (_idCreationMap.TryAdd(objIdString!, true) == false)
            return null!;

        var instance = Activator.CreateInstance(type);
        _seperatedTables.TryGetValue(objIdString!, out var table);

        if (table == null)
            return null!;

        foreach (var property in type.GetProperties())
        {
            SetValueToProperty(objId, ref instance!, property, table);
        }

        return instance!;
    }

    private void SetValueToProperty(
       object objId,
       ref object instance,
       PropertyInfo property,
       Dictionary<string, object> table)
    {
        var propertyType = property.PropertyType;

        if (propertyType == typeof(string) || propertyType.IsValueType)
        {
            table.TryGetValue(property.Name, out var value);
            property.SetValue(instance, value == DBNull.Value ? null : value);
        }
        else if (propertyType.IsAssignableTo(typeof(ICollection)))
        {
            var foreignKey = $"{property.DeclaringType!.Name}Id";
            var collectiveInstance = Activator.CreateInstance(propertyType);
            var addMethod = property.PropertyType.GetMethod("Add")!;
            var argumentType = propertyType.GetGenericArguments().FirstOrDefault();
            var idList = GetIdsByForeignKey(objId, argumentType!, foreignKey);

            foreach (var id in idList)
            {
                addMethod.Invoke(collectiveInstance, new object[]
                {
                    CreateObject(argumentType!, id)
                });
            }

            property.SetValue(instance, collectiveInstance);
        }
        else if (propertyType.IsClass)
        {
            var column = $"{property.Name}Id";

            table.TryGetValue(column, out var value);
            property.SetValue(instance, CreateObject(propertyType, value!));
        }
    }

    private IList<string> GetIdsByForeignKey(
        object principleTableId,
        Type tableType,
        string foreignKeyColumn)
    {
        if (_tableIdReference.ContainsKey(tableType) == false)
            return default!;

        List<string> idList = new();

        foreach (var id in _tableIdReference[tableType])
        {
            _seperatedTables[id].TryGetValue(foreignKeyColumn, out var value);

            if (value?.ToString() == principleTableId.ToString())
                idList.Add(id);
        }

        return idList;
    }
}