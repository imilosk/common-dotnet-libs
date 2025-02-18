using System.ComponentModel.DataAnnotations.Schema;

namespace IMilosk.Data.SqlClient.Tests;

public class TestRequest
{
    [Column("column0")] public long Field0;

    [Column("column1")] public string Property1 { get; } = string.Empty;

    [Column("column2")] public string Property2 { get; set; } = string.Empty;

    [NotMapped]
    public string PropertyUnassignable
    {
        set => throw new InvalidDataException();
    }

    [Column("column3")] public string Property3 { get; init; } = string.Empty;
}