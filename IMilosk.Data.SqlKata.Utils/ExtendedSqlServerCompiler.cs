using SqlKata;
using SqlKata.Compilers;

namespace IMilosk.Data.SqlKata.Utils;

public class ExtendedSqlServerCompiler : SqlServerCompiler
{
    protected override SqlResult CompileDeleteQuery(Query query)
    {
        var ctx = base.CompileDeleteQuery(query);

        if (!query.HasLimit() || ctx.RawSql.StartsWith("DELETE TOP"))
        {
            return ctx;
        }

        var limit = query.GetOneComponent<LimitClause>("limit", EngineCode)
            .Limit;
        ctx.RawSql = ctx.RawSql.Replace("DELETE", $"DELETE TOP({limit})");

        return ctx;
    }
}