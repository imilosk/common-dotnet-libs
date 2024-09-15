namespace IMilosk.Data.FluentMigrator.Utils;

public interface IMigrationDatabaseInfo
{
    public string DatabaseName { get; set; }
    public string SchemaName { get; set; }
}