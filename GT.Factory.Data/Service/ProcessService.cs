using GT.Factory.Common.Models;
using GT.Factory.Data.Core;
using GT.Factory.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT.Factory.Data.Service
{
    /// <summary>
    /// 负责业务决策（主库失败 → 写缓存）
    /// </summary>
    public class ProcessService
    {
        private readonly ProcessRepository _primaryRepo;
        private readonly SyncQueueRepository _syncRepo;

        public ProcessService(
            ProcessRepository processRepository,
            SyncQueueRepository syncRepo)
        {
            _primaryRepo = processRepository;
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

        public async Task UploadFourAsync(ProcessPropertyFourPayload payload)
        {
            try
            {
                await _primaryRepo.UploadFourAsync(payload);
            }
            catch
            {
                await _syncRepo.InsertAsync("GTProcessProperty", payload);
            }
        }

        public async Task UploadFileAsync(ProcessFilePayload payload)
        {
            try
            {
                await _primaryRepo.UploadProcessFile(payload);
            }
            catch
            {
                await _syncRepo.InsertAsync("GTProcessProperty", payload);
            }
        }

        /// <summary>
        /// 根据条件查询生产数据
        /// </summary>
        public async Task<List<dynamic>> SearchDataAsync(SearchDataPayload payload)
        {
            return await _primaryRepo.SearchDataAsync(payload);
        }

        /// <summary>
        /// 根据条件查询校准数据
        /// </summary>
        public async Task<List<dynamic>> SelectCalibrationAsync(string process_no, DateTime? start_time, DateTime? end_time)
        {
            return await _primaryRepo.SelectCalibration(process_no, start_time, end_time);
        }
    }
}
