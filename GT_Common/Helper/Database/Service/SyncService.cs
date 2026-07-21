using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using GT_Common.Helper.Database.Repository;
using GT_Common.Helper.Logging;
using GT_Common.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Service
{
    /// <summary>
    /// 缓存回传服务（批量事务优化版）
    /// 
    /// 作用：
    /// 1️⃣ 从本地缓存读取待同步数据
    /// 2️⃣ 批量写入主库
    /// 3️⃣ 成功后删除缓存
    /// 4️⃣ 失败自动回滚
    /// 
    /// 特点：
    /// ✅ 支持批量大小控制
    /// ✅ 支持最大重试限制
    /// ✅ 支持事务一致性
    /// ✅ 支持主库健康检测
    /// </summary>
    public class SyncService
    {
        private readonly IDatabase _primary;
        private readonly SyncQueueRepository _syncRepo;
        private readonly ProcessRepository _processRepo;
        private readonly DatabaseHealthMonitor _healthMonitor;

        /// <summary>
        /// 单次批量同步最大条数
        /// 可根据设备性能调整
        /// </summary>
        private const int BatchSize = 100;

        /// <summary>
        /// 最大重试次数
        /// 超过后标记为永久失败
        /// </summary>
        private const int MaxRetryCount = 5;

        public SyncService(
            DatabaseManager manager,
            SyncQueueRepository syncRepo)
        {
            _primary = manager.Primary;
            _syncRepo = syncRepo;
            _processRepo = new ProcessRepository(manager);
            _healthMonitor = new DatabaseHealthMonitor(manager.Primary);
        }

        /// <summary>
        /// 执行批量回传
        /// </summary>
        public async Task SyncAsync()
        {
            // ✅ 1️⃣ 主库健康检测
            if (!await _healthMonitor.IsHealthyAsync())
            {
                DisplayLog.Warn("主库不可用，暂停回传");
                return;
            }

            // ✅ 2️⃣ 获取待同步数据（限制批量大小）
            var pending = (await _syncRepo.GetPendingAsync())
                            .Take(BatchSize)
                            .ToList();

            if (pending.Count == 0)
                return;

            // ✅ 3️⃣ 开启主库事务
            var transaction = await _primary.BeginTransactionAsync();

            try
            {
                foreach (var item in pending)
                {
                    try
                    {
                        switch (item.DataType)
                        {
                            case "GTProcessProperty":

                                var payload =
                                    JsonConvert.DeserializeObject<ProcessPropertyPayload>(item.DataJson);

                                await _processRepo.UploadBasicDataAsync(payload);
                                break;

                                // ✅ 后续可扩展其他类型
                        }

                        // ✅ 写入成功后删除缓存
                        await _syncRepo.DeleteAsync(item.Id);
                    }
                    catch (Exception ex)
                    {
                        DisplayLog.Warn($"单条回传失败 ID={item.Id}");

                        if (item.RetryCount + 1 >= MaxRetryCount)
                        {
                            await _syncRepo.MarkAsFailedAsync(item.Id);
                        }
                        else
                        {
                            await _syncRepo.IncreaseRetryAsync(item.Id);
                        }
                    }
                }

                // ✅ 4️⃣ 提交事务
                await transaction.CommitAsync();

                DisplayLog.Info($"批量回传完成，本次处理 {pending.Count} 条");
            }
            catch (Exception ex)
            {
                // ✅ 5️⃣ 事务失败整体回滚
                await transaction.RollbackAsync();

                DisplayLog.Error("批量回传事务失败，已回滚", ex);
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }
    }
}
