using System.Data;
using FluentMigrator;
using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.IfDatabase;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Update;
using FluentMigrator.Infrastructure;

namespace IMilosk.Data.FluentMigrator.Utils;

public abstract class MigrationBase : ForwardOnlyMigration
{
    protected readonly IMigrationDatabaseInfo MigrationDatabaseInfo;

    private MigrationContext _migrationContext = null!;

    protected MigrationContext Context
    {
        get => _migrationContext ?? throw new InvalidOperationException("Context not set");
        set => _migrationContext = value;
    }

    protected new IAlterExpressionRoot Alter => new AlterExpressionRoot(Context);
    protected new ICreateExpressionRoot Create => new CreateExpressionRoot(Context);
    protected new IRenameExpressionRoot Rename => new RenameExpressionRoot(Context);
    protected new IInsertExpressionRoot Insert => new InsertExpressionRoot(Context);
    protected new ISchemaExpressionRoot Schema => new SchemaExpressionRoot(Context);
    protected new IDeleteExpressionRoot Delete => new DeleteExpressionRoot(Context);
    protected new IExecuteExpressionRoot Execute => new ExecuteExpressionRoot(Context);
    protected new IUpdateExpressionRoot Update => new UpdateExpressionRoot(Context);

    protected MigrationBase(IMigrationDatabaseInfo migrationDatabaseInfo)
    {
        MigrationDatabaseInfo = migrationDatabaseInfo;
    }

    public abstract override void Up();

    public new IIfDatabaseExpressionRoot IfDatabase(params string[] databaseType)
    {
        return new IfDatabaseExpressionRoot(Context, databaseType);
    }

    public new IIfDatabaseExpressionRoot IfDatabase(Predicate<string> databaseTypeFunc)
    {
        return new IfDatabaseExpressionRoot(Context, databaseTypeFunc);
    }

    protected void CreateSchemaIfNotExist(string schemaName)
    {
        if (string.IsNullOrEmpty(schemaName))
        {
            throw new InvalidOperationException("Database schema not set!");
        }

        var exists = Schema.Schema(schemaName)
            .Exists();

        if (exists)
        {
            return;
        }

        Create.Schema(schemaName);
    }

    protected MigrationContext CreateMigrationContext(
        IMigrationProcessor migrationProcessor,
        IServiceProvider serviceProvider,
        IDbConnection connection
    )
    {
        return new MigrationContext(
            migrationProcessor,
            serviceProvider,
            ApplicationContext,
            connection.ConnectionString
        );
    }

    protected void ExecuteExpressions(IMigrationProcessor migrationProcessor)
    {
        var expressions = Context.Expressions;
        foreach (var expression in expressions)
        {
            expression.ExecuteWith(migrationProcessor);
        }
    }
}