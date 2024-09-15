using FluentMigrator;
using FluentMigrator.Runner.Processors;
using IMilosk.Data.SqlClient.DatabaseConnector.Interfaces;
using IMilosk.Data.SqlClient.DatabaseConnector.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IMilosk.Data.FluentMigrator.Utils;

public abstract class CountryMigrationBase : MigrationBase
{
    private readonly IServiceProvider _serviceProvider;

    protected DatabaseSettings DatabaseSettings = DatabaseSettings.EmptyDatabaseSettings;

    protected CountryMigrationBase(
        IMigrationDatabaseInfo migrationDatabaseInfo,
        IServiceProvider serviceProvider
    ) : base(migrationDatabaseInfo)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Up()
    {
        using var outerScope = _serviceProvider.CreateScope();

        var connectionFactory = outerScope.ServiceProvider.GetRequiredService<IDatabaseConnectionFactory>();
        var multiDatabaseSettings = outerScope.ServiceProvider.GetRequiredService<MultiDatabaseSettings>();

        foreach (var (databaseName, databaseSettings) in multiDatabaseSettings.Databases)
        {
            if (databaseName == MigrationDatabaseInfo.DatabaseName)
            {
                continue;
            }

            using var innerScope = _serviceProvider.CreateScope();

            var processorOptions = innerScope.ServiceProvider.GetRequiredService<IOptionsSnapshot<ProcessorOptions>>();

            using var connection = connectionFactory.GetConnection(databaseName);
            processorOptions.Value.ConnectionString = connection.ConnectionString;

            var migrationProcessor = innerScope.ServiceProvider.GetRequiredService<IMigrationProcessor>();

            DatabaseSettings = databaseSettings;
            Context = CreateMigrationContext(migrationProcessor, innerScope.ServiceProvider, connection);

            CreateSchemaIfNotExist(databaseSettings.Schema);
            PerCountryUpMigration();

            ExecuteExpressions(migrationProcessor);
        }
    }

    protected abstract void PerCountryUpMigration();
}