using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Routing;
using GT_Common.MyEnum;
using System;

namespace GT_Common.Helper.Database.Core
{
    /// <summary>
    /// 数据库管理器
    /// 
    /// 作用：
    /// 1️⃣ 管理不同职责数据库（主库/缓存/MES/日志）
    /// 2️⃣ 根据配置自动创建数据库实例
    /// 3️⃣ 支持动态路由策略（如按年份分库）
    /// 
    /// 设计原则：
    /// - 不使用 static
    /// - 构造完成后只读
    /// - 线程安全
    /// </summary>
    public class DatabaseManager
    {
        /// <summary>
        /// 主业务数据库
        /// 用于核心业务数据
        /// </summary>
        public IDatabase Primary { get; }

        /// <summary>
        /// 本地缓存数据库
        /// 用于主库失败时的数据补偿
        /// </summary>
        public IDatabase LocalCache { get; }

        /// <summary>
        /// MES 数据库
        /// 用于对接外部 MES
        /// </summary>
        public IDatabase Mes { get; }

        /// <summary>
        /// 日志数据库
        /// 用于系统日志存储
        /// </summary>
        public IDatabase Log { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">多数据库配置</param>
        public DatabaseManager(MultiDatabaseOptions options)
        {
            if (!options.Primary.Enabled)
                throw new Exception("主库必须启用");

            Primary = CreateDatabase(options.Primary);

            if (options.LocalCache?.Enabled == true)
                LocalCache = CreateDatabase(options.LocalCache);

            if (options.Mes?.Enabled == true)
                Mes = CreateDatabase(options.Mes);

            if (options.Log?.Enabled == true)
                Log = CreateDatabase(options.Log);
        }

        #region 私有方法

        /// <summary>
        /// 根据节点配置创建数据库实例
        /// 
        /// 支持：
        /// ✅ 普通数据库
        /// ✅ 动态分库
        /// ✅ 路由装饰器
        /// </summary>
        private IDatabase CreateDatabase(DatabaseNodeOptions node)
        {
            // 1️⃣ 创建基础数据库实现
            IDatabase db = DatabaseFactory.Create(node);

            // 2️⃣ 若启用动态路由
            if (node.EnableRouting)
            {
                // 这里可扩展多个路由策略
                IDatabaseRouter router = CreateRouter(node);

                db = new RoutingDatabaseDecorator(
                    db,
                    node,
                    router);
            }

            return db;
        }

        /// <summary>
        /// 创建路由策略
        /// 
        /// 可扩展：
        /// YearRouting / ShiftRouting 等
        /// </summary>
        private IDatabaseRouter CreateRouter(DatabaseNodeOptions node)
        {
            switch (node.RoutingStrategy)
            {
                case "YearRouting":
                    return new YearDatabaseRouter(GetBaseDatabaseName(node));

                default:
                    throw new NotSupportedException(
                        $"不支持的路由策略: {node.RoutingStrategy}");
            }
        }

        /// <summary>
        /// 从连接字符串中提取基础数据库名
        /// </summary>
        private string GetBaseDatabaseName(DatabaseNodeOptions node)
        {
            if (node.DbType == DatabaseType.SqlServer)
            {
                var builder = new System.Data.SqlClient
                    .SqlConnectionStringBuilder(node.ConnectionString);

                return builder.InitialCatalog;
            }

            throw new NotSupportedException(
                "当前路由仅支持 SQL Server");
        }

        #endregion
    }
}