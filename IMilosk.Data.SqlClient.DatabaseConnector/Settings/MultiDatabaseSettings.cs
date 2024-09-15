using System.ComponentModel.DataAnnotations;

namespace IMilosk.Data.SqlClient.DatabaseConnector.Settings;

public class MultiDatabaseSettings
{
    [Required]
    public Dictionary<string, DatabaseSettings> Databases { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}