using System.Data;
using IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Implementations;

public class SqlTransactionManager : ISqlTransactionManager
{
    public IDbTransaction? BeginTransaction(IDbConnection connection)
    {
        if (connection.State is ConnectionState.Closed)
        {
            connection.Open();
        }

        if (connection.State is ConnectionState.Open)
        {
            return connection.BeginTransaction();
        }

        return null;
    }
}