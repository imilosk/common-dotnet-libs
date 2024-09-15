using System.Data;
using Dapper;

namespace IMilosk.Data.Dapper.Utils.TypeHandlers;

public static class DapperTypeRegistration
{
    public static void TryRegisterGuidTypeHandler(bool replaceExisting = false)
    {
        if (!replaceExisting && SqlMapper.HasTypeHandler(typeof(Guid)))
        {
            return;
        }

        SqlMapper.AddTypeHandler(new SqlGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
    }

    public static void TryRegisterDatetime2TypeHandler()
    {
        SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
        SqlMapper.AddTypeMap(typeof(DateTime?), DbType.DateTime2);
    }

    public static void TryRegisterUriTypeHandler(bool replaceExisting = false)
    {
        if (!replaceExisting && SqlMapper.HasTypeHandler(typeof(Uri)))
        {
            return;
        }

        SqlMapper.AddTypeHandler(new SqlUriTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Uri));
    }
}