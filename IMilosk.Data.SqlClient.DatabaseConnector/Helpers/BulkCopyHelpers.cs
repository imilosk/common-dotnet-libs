using System.Data;
using IMilosk.Data.SqlClient.DatabaseConnector.Mapping;
using Microsoft.Data.SqlClient;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Helpers;

public static class BulkCopyHelpers
{
    public static SqlBulkCopy InitializeBulkCopy(
        SqlConnection connection,
        string destinationTableName,
        int batchSize,
        List<DtoColumnMapping> dtoColumnMappings
    )
    {
        if (connection.State is ConnectionState.Closed)
        {
            connection.Open();
        }

        var bulkCopy = new SqlBulkCopy(connection);

        MapColumns(bulkCopy, dtoColumnMappings);
        bulkCopy.DestinationTableName = destinationTableName;
        bulkCopy.BatchSize = batchSize;

        return bulkCopy;
    }

    private static void MapColumns(SqlBulkCopy bulkCopy, List<DtoColumnMapping> dtoColumnMappings)
    {
        foreach (var mapping in dtoColumnMappings)
        {
            bulkCopy.ColumnMappings.Add(mapping.FieldName, mapping.TableColumnName);
        }
    }
}