using System.Data.Common;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;

public interface IDatabaseConnector
{
    DbConnection GetConnection();
}