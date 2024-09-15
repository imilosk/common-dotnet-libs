using System.Data;
using Dapper;

namespace IMilosk.Data.Dapper.Utils.TypeHandlers;

public class SqlUriTypeHandler : SqlMapper.TypeHandler<Uri>
{
    public override void SetValue(IDbDataParameter parameter, Uri? uri)
    {
        parameter.Value = uri?.ToString();
        parameter.DbType = DbType.String;
    }

    public override Uri Parse(object value)
    {
        return new Uri((string)value);
    }
}