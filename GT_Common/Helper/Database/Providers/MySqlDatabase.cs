using Dapper;
using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// MySQL 数据库实现
    /// 
    /// 适用场景：
    /// 1. MES 数据库
    /// 2. 主业务数据库
    /// 3. 云端数据库
    /// </summary>
    public class MySqlDatabase : IDatabase
    {
        private readonly DatabaseNodeOptions _options;

        public MySqlDatabase(DatabaseNodeOptions options)
        {
            _options = options ??
                throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 创建 MySQL 连接
        /// </summary>
        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_options.ConnectionString);
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

            return new MySqlDatabaseTransaction(conn, transaction);
        }
    }
}
