using System.Data.Common;
using IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;
using IMilosk.Data.SqlClient.DatabaseConnector.Settings;
using Microsoft.Data.SqlClient;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Implementations;

public class DatabaseConnector : IDatabaseConnector
{
    private readonly string _connectionString;

    public DatabaseConnector(DatabaseSettings settings)
    {
        _connectionString = settings.ConnectionStringTemplate
            .Replace("<SERVER>", settings.Server)
            .Replace("<DATABASE>", settings.Database)
            .Replace("<USER>", settings.User)
            .Replace("<PASSWORD>", settings.Password);
    }

    public DbConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}