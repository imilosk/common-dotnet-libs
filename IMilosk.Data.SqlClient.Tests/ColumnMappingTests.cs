using IMilosk.Data.SqlClient.DatabaseConnector.Mapping;

namespace IMilosk.Data.SqlClient.Tests;

public class ColumnMappingTests
{
    [Fact]
    public void TestColumnMapping()
    {
        var mapping = SqlDataMapper.GetColumnMapping<TestRequest>();

        Assert.Equal(4, mapping.Count);

        Assert.Equal("column0", mapping[0].TableColumnName);
        Assert.Equal("column1", mapping[1].TableColumnName);
        Assert.Equal("column2", mapping[2].TableColumnName);
        Assert.Equal("column3", mapping[3].TableColumnName);
    }
}