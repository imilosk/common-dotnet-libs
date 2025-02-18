using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Mapping;

public static class SqlDataMapper
{
    public static List<DtoColumnMapping> GetColumnMapping<T>()
    {
        var type = typeof(T);
        var columns = new List<DtoColumnMapping>();
        MapMembers(type.GetFields(), columns);
        MapMembers(type.GetProperties(), columns);

        return columns;
    }

    private static void MapMembers(MemberInfo[] members, List<DtoColumnMapping> columns)
    {
        foreach (var memberInfo in members)
        {
            var memberInfoName = memberInfo.Name;
            var tableColumnName = GetDatabaseFieldName(memberInfo);

            if (ShouldSkip(memberInfo))
            {
                continue;
            }

            var mapping = new DtoColumnMapping
            {
                FieldName = memberInfoName,
                TableColumnName = tableColumnName
            };
            columns.Add(mapping);
        }
    }

    private static bool ShouldSkip(MemberInfo fieldInfo)
    {
        var notMappedAttribute = fieldInfo.GetCustomAttribute(typeof(NotMappedAttribute), false);

        return notMappedAttribute is not null;
    }

    private static string GetDatabaseFieldName(MemberInfo fieldInfo)
    {
        var columnAttribute = (ColumnAttribute?)fieldInfo.GetCustomAttribute(typeof(ColumnAttribute), false);

        return columnAttribute?.Name ?? fieldInfo.Name;
    }
}