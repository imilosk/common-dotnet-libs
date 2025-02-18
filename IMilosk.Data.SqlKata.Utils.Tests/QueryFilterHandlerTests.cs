using IMilosk.Data.SqlKata.Utils.Extensions;
using IMilosk.Data.SqlKata.Utils.Filtering;
using SqlKata;

namespace IMilosk.Data.SqlKata.Utils.Tests;

public class QueryFilterHandlerTests
{
    [Fact]
    public void TestIntegerInFilter_Succeeds()
    {
        var filterSet = SetupTest([10, 20]);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    [Fact]
    public void TestLongInFilter_Succeeds()
    {
        var filterSet = SetupTest([11L, 12L]);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    [Fact]
    public void TestStringInFilter_Succeeds()
    {
        var filterSet = SetupTest(["10", "20"]);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    [Fact]
    public void TestGuidInFilter_Succeeds()
    {
        var filterSet = SetupTest([Guid.NewGuid(), Guid.Empty]);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    [Fact]
    public void TestCharInFilter_Succeeds()
    {
        var filterSet = SetupTest(['a', 'b']);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    [Fact]
    public void TestDatetimeInFilter_Succeeds()
    {
        var filterSet = SetupTest([DateTime.UtcNow, DateTime.MinValue]);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    [Fact]
    public void TestDecimalInFilter_Succeeds()
    {
        var filterSet = SetupTest([11.11M, 12.12M]);

        var query = ExecuteTest(filterSet);
        VerifyResults(query);
    }

    private static T[] SetupTest<T>(T[] values) where T : notnull
    {
        return values;
    }

    private static Query ExecuteTest<T>(T filterSet) where T : notnull
    {
        var filters = new ComparisonFilterBuilder()
            .AddFilter(FilterOperator.In, filterSet)
            .Build();

        return new Query()
            .ApplyFilters(filters);
    }

    private static void VerifyResults(Query query)
    {
        Assert.NotNull(query);
    }
}