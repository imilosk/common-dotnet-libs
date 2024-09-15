using System.ComponentModel.DataAnnotations;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Settings;

public class DatabaseSettings
{
    public static readonly DatabaseSettings EmptyDatabaseSettings = new();

    [Required] public string ConnectionStringTemplate { get; init; } = string.Empty;

    [Required] public string Server { get; init; } = string.Empty;

    [Required] public string Database { get; init; } = string.Empty;

    [Required] public string Schema { get; init; } = string.Empty;

    [Required] public string User { get; init; } = string.Empty;

    [Required] public string Password { get; init; } = string.Empty;
}