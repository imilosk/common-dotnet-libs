namespace IMilosk.Data.SqlKata.Utils.Filtering;

public struct QueryFilter
{
    public string? Field;
    public FilterOperator FilterOperator;
    public object? Value;
}