using System.Data;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;

public interface IDatabaseConnectionFactory
{
    public IDbConnection GetConnection(string settingKey);
}