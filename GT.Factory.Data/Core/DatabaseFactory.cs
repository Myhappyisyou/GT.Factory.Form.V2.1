using GT.Factory.Common.Enums;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using GT.Factory.Data.Providers;
using GT.Factory.Data.Routing;
using System;

namespace GT.Factory.Data
{
    /// <summary>
    /// 数据库工厂
    /// 根据数据库类型创建对应实现
    /// </summary>
    public static class DatabaseFactory
    {
        public static IDatabase Create(DatabaseNodeOptions options)
        {
            IDatabase db;
            switch (options.DbType)
            {
                case DatabaseType.SqlServer:
                    db = new SqlServerDatabase(options);
                    break;
                case DatabaseType.SQLite:
                    db = new SqliteDatabase(options);
                    break;
                case DatabaseType.MySql:
                    db = new MySqlDatabase(options);
                    break;
                case DatabaseType.Access:
                    db = new AccessDatabase(options);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // ✅ 如果启用路由
            if (options.EnableRouting)
            {
                var router = new YearDatabaseRouter("BYD");

                db = new RoutingDatabaseDecorator(db, options, router);
            }

            return db;
        }
    }
}
