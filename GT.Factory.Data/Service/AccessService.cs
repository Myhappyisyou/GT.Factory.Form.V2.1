using GT.Factory.Common.Models;
using GT.Factory.Data.Core;
using GT.Factory.Data.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT.Factory.Data.Service
{
    /// <summary>
    /// 负责业务决策（主库失败 → 写缓存）
    /// </summary>
    public class AccessService
    {
        private readonly AccessRepository _localRepo;
        private readonly SyncQueueRepository _syncRepo;

        public AccessService(
            AccessRepository accessRepository,
            SyncQueueRepository syncRepo)
        {
            _localRepo = accessRepository;
            _syncRepo = syncRepo;
        }

        public async Task<List<AccessTestDataDto>> SearchAccessDataAsync(
            SearchDataPayload payload)
        {
            try
            {
                return await _localRepo.SearchAccessDataAsync(payload);
            }
            catch
            {
                return new List<AccessTestDataDto>();
            }
        }

    }
}
