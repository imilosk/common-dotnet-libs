using System.Data;
using Dapper;

namespace IMilosk.Data.Dapper.Utils.TypeHandlers;

public class SqlGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid guid)
    {
        parameter.Value = guid.ToString();
        parameter.DbType = DbType.String;
    }

    public override Guid Parse(object value)
    {
        return new Guid((string)value);
    }
}