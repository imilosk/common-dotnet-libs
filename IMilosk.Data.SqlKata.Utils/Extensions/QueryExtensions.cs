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

    private static class ComponentTypes
    {
        public const string Join = "join";
    }

    public static Query FromNoLock(this Query queryBase, string table)
    {
        return queryBase.FromRaw($"{table} WITH(NOLOCK)");
    }

    public static TQ FromNoLock<TQ>(this TQ queryBase, string table) where TQ : BaseQuery<TQ>
    {
        return queryBase.FromRaw($"{table} WITH(NOLOCK)");
    }

    public static Query JoinNoLock(
        this Query queryBase,
        string table,
        Func<Join, Join> callback,
        string type = SqlJoinTypes.Inner
    )
    {
        return queryBase.AddComponent(ComponentTypes.Join, new BaseJoin
        {
            Join = callback(new Join().AsType(type))
                .FromNoLock(table)
        });
    }

    public static Query JoinNoLock(
        this Query queryBase,
        string table,
        string first,
        string second,
        string op = "=",
        string type = SqlJoinTypes.Inner
    )
    {
        return queryBase.AddComponent(ComponentTypes.Join, new BaseJoin
        {
            Join = new Join()
                .AsType(type)
                .WhereColumns(first, op, second)
                .FromNoLock(table),
        });
    }

    public static Query LeftJoinNoLock(this Query queryBase, string table, Func<Join, Join> callback)
    {
        return JoinNoLock(queryBase, table, callback, SqlJoinTypes.Left);
    }

    public static Query LeftJoinNoLock(this Query queryBase, string table, string first, string second, string op = "=")
    {
        return JoinNoLock(queryBase, table, first, second, op, SqlJoinTypes.Left);
    }

    public static Query RightJoinNoLock(this Query queryBase, string table, Func<Join, Join> callback)
    {
        return JoinNoLock(queryBase, table, callback, SqlJoinTypes.Right);
    }

    public static Query RightJoinNoLock(
        this Query queryBase,
        string table,
        string first,
        string second,
        string op = "="
    )
    {
        return JoinNoLock(queryBase, table, first, second, op, SqlJoinTypes.Right);
    }
}

public static class SqlJoinTypes
{
    public const string Inner = "inner join";
    public const string Left = "left join";
    public const string Right = "right join";
    public const string Outer = "outer join";
    public const string Cross = "cross join";
}