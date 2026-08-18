using System;
using GT.Factory.Data.Repository;
using GT.Factory.Data.Service;

namespace GT.Factory.Data.Core
{
    /// <summary>
    /// 数据运行环境
    ///
    /// 负责：
    /// 1. 创建 DatabaseManager
    /// 2. 创建 Repository 容器
    /// 3. 创建数据业务服务
    ///
    /// 不负责：
    /// - 业务逻辑
    /// - SQL实现
    /// - UI交互
    /// </summary>
    public class DataRuntime : IDisposable
    {

        /// <summary>
        /// 数据库管理器
        /// </summary>
        public DatabaseManager Database { get; }


        /// <summary>
        /// 仓储容器
        /// </summary>
        public RepositoryContainer Repositories { get; }


        /// <summary>
        /// 生产数据服务
        /// </summary>
        public ProcessService ProcessService { get; }


        /// <summary>
        /// 本地数据
        /// </summary>
        public AccessService AccessService { get; }


        /// <summary>
        /// 同步服务
        /// </summary>
        public SyncService SyncService { get; }


        /// <summary>
        /// 数据库健康监控
        /// </summary>
        public DatabaseHealthMonitor HealthMonitor { get; }



        public DataRuntime(
            MultiDatabaseOptions options)
        {

            if (options == null)
                throw new ArgumentNullException(nameof(options));


            // 创建数据库核心
            Database =
                new DatabaseManager(options);



            // 创建Repository
            Repositories =
                new RepositoryContainer(Database);



            // 创建业务服务
            ProcessService =
                new ProcessService(
                    Repositories.Processes,
                    Repositories.SyncQueue);



            // 创建业务服务
            AccessService =
                new AccessService(
                    Repositories.Access,
                    Repositories.SyncQueue);



            SyncService =
                new SyncService(
                    Database,
                    Repositories.SyncQueue);



            // 健康检测
            HealthMonitor =
                new DatabaseHealthMonitor(
                    Database.Primary);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }



        //public void Dispose()
        //{
        //    Database?.Dispose();
        //}
    }
}