using IMilosk.Data.SqlKata.Utils.Filtering;
using SqlKata;

namespace IMilosk.Data.SqlKata.Utils.Extensions;

public static class QueryExtensions
{
    public static Query ApplyFilter(this Query query, QueryFilter filter)
    {
        return QueryFilterHandler.ApplyFilter(query, filter);
    }

    public static Query ApplyFilters(this Query query, IEnumerable<QueryFilter> filters)
    {
        return QueryFilterHandler.ApplyFilters(query, filters);
    }
}