using SqlKata;

namespace IMilosk.Data.SqlKata.Utils.Filtering;

public static class QueryFilterHandler
{
    private static readonly Exception UnsupportedComparisonOperatorException =
        new NotSupportedException("The filter operator is not a valid comparison operator.");

    private static readonly Exception ParameterCannotBeNullException =
        new NotSupportedException("Parameter cannot be null.");

    private static readonly Exception UnsupportedFilterDataTypeException =
        new NotSupportedException("Unsupported filter datatype.");

    internal static Query ApplyFilters(Query query, IEnumerable<QueryFilter> queryFilters)
    {
        foreach (var filter in queryFilters)
        {
            ApplyFilter(query, filter);
        }

        return query;
    }

    internal static Query ApplyFilter(Query query, QueryFilter queryFilter)
    {
        switch (queryFilter.FilterOperator)
        {
            case FilterOperator.Eq
                or FilterOperator.Neq
                or FilterOperator.Gt
                or FilterOperator.Lt
                or FilterOperator.Gte
                or FilterOperator.Lte:
                query.Where(queryFilter.Field,
                    GetComparisonSqlOperator(queryFilter.FilterOperator),
                    queryFilter.Value
                );
                break;
            case FilterOperator.Like:
                query.WhereLike(queryFilter.Field, queryFilter.Value);
                break;
            case FilterOperator.IsNull:
                query.WhereNull(queryFilter.Field);
                break;
            case FilterOperator.IsNotNull:
                query.WhereNotNull(queryFilter.Field);
                break;
            case FilterOperator.Limit:
                query.Limit((int)(queryFilter.Value ?? 0));
                break;
            case FilterOperator.Offset:
                query.Offset((int)(queryFilter.Value ?? 0));
                break;
            case FilterOperator.SortAsc:
                query.OrderBy(queryFilter.Field);
                break;
            case FilterOperator.SortDesc:
                query.OrderByDesc(queryFilter.Field);
                break;
            case FilterOperator.In:
                ApplyInFilter(query, queryFilter);
                break;
            default:
                throw new ArgumentException($"Unsupported filter operator {queryFilter.FilterOperator}");
        }

        return query;
    }

    private static void ApplyInFilter(Query query, QueryFilter queryFilter)
    {
        var filterValue = queryFilter.Value ?? throw ParameterCannotBeNullException;

        switch (filterValue)
        {
            case IEnumerable<int>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<int>
                );
                break;

            case IEnumerable<long>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<long>
                );
                break;

            case IEnumerable<decimal>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<decimal>
                );
                break;

            case IEnumerable<string>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<string>
                );
                break;

            case IEnumerable<char>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<char>
                );
                break;

            case IEnumerable<Guid>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<Guid>
                );
                break;

            case IEnumerable<DateTime>:
                query.WhereIn(
                    queryFilter.Field,
                    queryFilter.Value as IEnumerable<DateTime>
                );
                break;

            default:
                throw UnsupportedFilterDataTypeException;
        }
    }

    private static string GetComparisonSqlOperator(FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Eq => "=",
            FilterOperator.Neq => "<>",
            FilterOperator.Gt => ">",
            FilterOperator.Gte => ">=",
            FilterOperator.Lt => "<",
            FilterOperator.Lte => "<=",
            _ => throw UnsupportedComparisonOperatorException,
        };
    }
}