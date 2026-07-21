using GT_Common.Helper.Database.Core;
using GT_Common.Helper.Database.Repository;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Service
{
    /// <summary>
    /// 负责业务决策（主库失败 → 写缓存）
    /// </summary>
    public class ProcessService
    {
        private readonly ProcessRepository _primaryRepo;
        private readonly SyncQueueRepository _syncRepo;

        public ProcessService(
            DatabaseManager manager,
            SyncQueueRepository syncRepo)
        {
            _primaryRepo = new ProcessRepository(manager);
            _syncRepo = syncRepo;
        }

        public async Task UploadAsync(ProcessPropertyPayload payload)
        {
            try
            {
                await _primaryRepo.UploadBasicDataAsync(payload);
            }
            catch
            {
                await _syncRepo.InsertAsync("GTProcessProperty", payload);
            }
        }
    }
}
