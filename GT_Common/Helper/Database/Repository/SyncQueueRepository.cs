using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using GT_Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Repository
{
    /// <summary>
    /// 缓存层
    /// </summary>
    public class SyncQueueRepository
    {
        private readonly IDatabase _cacheDb;

        public SyncQueueRepository(DatabaseManager manager)
        {
            _cacheDb = manager.LocalCache
                ?? throw new Exception("LocalCache 未配置");
        }

        /// <summary>
        /// 插入缓存数据
        /// </summary>
        public async Task InsertAsync(string dataType, object data)
        {
            var json = JsonConvert.SerializeObject(data);

            const string sql = @"
            INSERT INTO SyncQueue
            (DataType, DataJson, CreateTime, RetryCount)
            VALUES
            (@DataType, @DataJson, @CreateTime, 0)";

            await _cacheDb.ExecuteAsync(sql, new
            {
                DataType = dataType,
                DataJson = json,
                CreateTime = DateTime.Now
            });
        }

        /// <summary>
        /// 获取所有待同步数据
        /// </summary>
        public async Task<List<SyncItem>> GetPendingAsync()
        {
            const string sql = @"
            SELECT *
            FROM SyncQueue
            WHERE Status = 0
            ORDER BY Id";

            return await _cacheDb.QueryAsync<SyncItem>(sql);
        }

        /// <summary>
        /// 获取所有同步数据
        /// </summary>
        public async Task<List<SyncItem>> GetAllAsync()
        {
            const string sql = "SELECT * FROM SyncQueue";
            return await _cacheDb.QueryAsync<SyncItem>(sql);
        }

        /// <summary>
        /// 删除已同步数据
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            const string sql = "DELETE FROM SyncQueue WHERE Id=@id";

            await _cacheDb.ExecuteAsync(sql, new { id });
        }

        /// <summary>
        /// 增加重试次数
        /// </summary>
        public async Task IncreaseRetryAsync(int id)
        {
            const string sql = @"
            UPDATE SyncQueue
            SET RetryCount = RetryCount + 1
            WHERE Id=@id";

            await _cacheDb.ExecuteAsync(sql, new { id });
        }

        /// <summary>
        /// 标记永久失败
        /// </summary>
        public async Task MarkAsFailedAsync(int id)
        {
            const string sql = @"
            UPDATE SyncQueue
            SET Status = 2
            WHERE Id = @id";

            await _cacheDb.ExecuteAsync(sql, new { id });
        }

        /// <summary>
        /// 死信查询
        /// </summary>
        public async Task<List<SyncItem>> GetFailedAsync()
        {
            const string sql = @"
            SELECT *
            FROM SyncQueue
            WHERE Status = 2";

            return await _cacheDb.QueryAsync<SyncItem>(sql);
        }


    }
}
