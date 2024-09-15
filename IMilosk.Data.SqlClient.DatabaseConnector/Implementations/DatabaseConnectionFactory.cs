using System.Data;
using IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;
using IMilosk.Data.SqlClient.DatabaseConnector.Settings;
using Microsoft.Data.SqlClient;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Implementations;

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly MultiDatabaseSettings _settings;

    public DatabaseConnectionFactory(MultiDatabaseSettings settings)
    {
        _settings = settings;
    }

    public IDbConnection GetConnection(string settingKey)
    {
        var databaseSettings = _settings.Databases[settingKey];

        var connectionString = databaseSettings.ConnectionStringTemplate
            .Replace("<SERVER>", databaseSettings.Server)
            .Replace("<DATABASE>", databaseSettings.Database)
            .Replace("<USER>", databaseSettings.User)
            .Replace("<PASSWORD>", databaseSettings.Password);

        return new SqlConnection(connectionString);
    }
}