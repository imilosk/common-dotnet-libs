using System.Data;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;

public interface ISqlTransactionManager
{
    IDbTransaction? BeginTransaction(IDbConnection connection);
}