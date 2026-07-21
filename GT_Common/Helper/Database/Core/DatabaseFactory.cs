using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using GT_Common.Helper.Database.Providers;
using GT_Common.Helper.Database.Routing;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database
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
