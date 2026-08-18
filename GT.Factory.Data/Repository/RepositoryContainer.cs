using GT.Factory.Data.Core;
using GT.Factory.Data.Service;

namespace GT.Factory.Data.Repository
{
    /// <summary>
    /// Repository 容器
    /// 
    /// 作用：
    /// 1️⃣ 统一管理所有 Repository 实例
    /// 2️⃣ 避免业务层到处 new Repository
    /// 3️⃣ 保持结构清晰
    /// 
    /// 注意：
    /// 该类不包含业务逻辑
    /// 仅负责组织仓储实例
    /// </summary>
    public class RepositoryContainer
    {

        /// <summary>
        /// 零件绑定仓储
        /// </summary>
        public PartRepository Parts { get; }

        /// <summary>
        /// 生产数据仓储
        /// </summary>
        public ProcessRepository Processes { get; }

        /// <summary>
        /// 本地生产数据仓储
        /// </summary>
        public AccessRepository Access { get; }

        /// <summary>
        /// 报警仓储
        /// </summary>
        public EquipmentStatusRepository Alarms { get; }

        /// <summary>
        /// 异常缓存
        /// </summary>
        public SyncQueueRepository SyncQueue { get; }

        /// <summary>
        /// 构造函数
        /// 统一注入 DatabaseManager
        /// </summary>
        public RepositoryContainer(DatabaseManager manager)
        {
            Parts = new PartRepository(manager);
            Processes = new ProcessRepository(manager);
            Access = new AccessRepository(manager);
            Alarms = new EquipmentStatusRepository(manager);
            SyncQueue = new SyncQueueRepository(manager);
        }
    }
}
