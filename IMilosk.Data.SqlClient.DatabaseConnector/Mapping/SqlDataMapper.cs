using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Mapping;

public static class SqlDataMapper
{
    public static List<DtoColumnMapping> GetColumnMapping<T>()
    {
        var type = typeof(T);
        var columns = new List<DtoColumnMapping>();

        foreach (var propertyInfo in type.GetProperties())
        {
            if (ShouldSkip(propertyInfo))
            {
                continue;
            }

            columns.Add(new DtoColumnMapping
            {
                FieldName = propertyInfo.Name,
                TableColumnName = GetDatabaseFieldName(propertyInfo),
            });
        }

        return columns;
    }

    public static IList<string> GetColumnNames<T>()
    {
        var columnMapping = GetColumnMapping<T>();

        return columnMapping.Select(mapping => mapping.FieldName).ToList();
    }

    public static Dictionary<string, object?> GetAnonymousObject<T>(T entity)
    {
        var type = typeof(T);
        var dict = new Dictionary<string, object?>();

        foreach (var propertyInfo in type.GetProperties())
        {
            if (ShouldSkip(propertyInfo))
            {
                continue;
            }

            dict.Add(GetDatabaseFieldName(propertyInfo), propertyInfo.GetValue(entity));
        }

        return dict;
    }

    private static bool ShouldSkip(PropertyInfo fieldInfo)
    {
        var notMappedAttribute = fieldInfo.GetCustomAttribute(typeof(NotMappedAttribute), false);

        return notMappedAttribute is not null;
    }

    private static string GetDatabaseFieldName(PropertyInfo fieldInfo)
    {
        var columnAttribute = (ColumnAttribute?)fieldInfo.GetCustomAttribute(typeof(ColumnAttribute), false);

        return columnAttribute?.Name ?? fieldInfo.Name;
    }
}