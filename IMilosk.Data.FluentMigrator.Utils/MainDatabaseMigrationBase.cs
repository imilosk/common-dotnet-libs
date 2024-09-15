using FluentMigrator;
using FluentMigrator.Runner.Processors;
using IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;
using IMilosk.Data.SqlClient.DatabaseConnector.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IMilosk.Data.FluentMigrator.Utils;

public abstract class MainDatabaseMigrationBase : MigrationBase
{
    private readonly IServiceProvider _serviceProvider;

    protected DatabaseSettings DatabaseSettings = DatabaseSettings.EmptyDatabaseSettings;

    protected MainDatabaseMigrationBase(
        IMigrationDatabaseInfo migrationDatabaseInfo,
        IServiceProvider serviceProvider
    ) : base(migrationDatabaseInfo)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Up()
    {
        using var scope = _serviceProvider.CreateScope();

        DatabaseSettings = scope.ServiceProvider.GetRequiredService<DatabaseSettings>();
        var databaseConnector = scope.ServiceProvider.GetRequiredService<IDatabaseConnector>();
        var processorOptions = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>();
        var migrationProcessor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();

        using var connection = databaseConnector.GetConnection();
        processorOptions.Value.ConnectionString = connection.ConnectionString;

        Context = CreateMigrationContext(migrationProcessor, scope.ServiceProvider, connection);

        CreateSchemaIfNotExist(DatabaseSettings.Schema);
        MainDatabaseUpMigration();

        ExecuteExpressions(migrationProcessor);
    }

    protected abstract void MainDatabaseUpMigration();
}