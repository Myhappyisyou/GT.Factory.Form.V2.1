using Dapper;
using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// SQLite 数据库实现
    /// 
    /// 适用场景：
    /// 1. 本地缓存数据库
    /// 2. 断网缓存
    /// 3. 单机设备
    /// 
    /// 优点：
    /// - 无需安装数据库服务
    /// - 部署简单
    /// - 轻量级
    /// </summary>
    public class SqliteDatabase : IDatabase
    {
        private readonly DatabaseNodeOptions _options;

        public SqliteDatabase(DatabaseNodeOptions options)
        {
            _options = options ??
                throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 创建 SQLite 连接
        /// </summary>
        private SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(_options.RuntimeConnectionString);
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PingAsync()
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    await conn.OpenAsync();
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.ExecuteAsync(sql, param);
            }
        }

        /// <summary>
        /// 查询单条记录
        /// </summary>
        public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.QuerySingleOrDefaultAsync<T>(sql, param);
            }
        }

        /// <summary>
        /// 查询多条记录
        /// </summary>
        public async Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                var result = await conn.QueryAsync<T>(sql, param);
                return result.ToList();
            }
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        public async Task<IDatabaseTransaction> BeginTransactionAsync()
        {
            var conn = CreateConnection();
            await conn.OpenAsync();

            var transaction = conn.BeginTransaction();

            return new SqliteTransaction(conn, transaction);
        }
    }
}
