using Dapper;
using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// Access 数据库实现
    /// 
    /// 适用场景：
    /// 1. 老厂系统
    /// 2. 单机小系统
    /// 3. 低并发场景
    /// 
    /// 注意：
    /// 1. Access 不支持 @参数名 形式，必须使用 ?
    /// 2. 不支持高并发
    /// 3. 建议仅用于轻量级用途
    /// </summary>
    public class AccessDatabase : IDatabase
    {
        private readonly DatabaseNodeOptions _options;

        public AccessDatabase(DatabaseNodeOptions options)
        {
            _options = options ??
                throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// 创建 Access 连接
        /// </summary>
        private OleDbConnection CreateConnection()
        {
            return new OleDbConnection(_options.RuntimeConnectionString);
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

        #region IDatabase 实现

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();

                return await conn.ExecuteAsync(
                    ConvertSql(sql),
                    param);
            }
        }

        /// <summary>
        /// 查询单条数据
        /// </summary>
        public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();

                return await conn.QuerySingleOrDefaultAsync<T>(
                    ConvertSql(sql),
                    param);
            }
        }

        /// <summary>
        /// 查询多条数据
        /// </summary>
        public async Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                await conn.OpenAsync();

                var result = await conn.QueryAsync<T>(
                    ConvertSql(sql),
                    param);

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

            return new AccessDatabaseTransaction(conn, transaction);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// Access 参数替换
        /// 
        /// Access 不支持 @Name 参数
        /// 只支持 ? 占位符
        /// 
        /// 示例：
        /// SELECT * FROM Users WHERE Id=@Id
        /// 替换为：
        /// SELECT * FROM Users WHERE Id=?
        /// </summary>
        private string ConvertSql(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;

            return System.Text.RegularExpressions.Regex
                .Replace(sql, @"@\w+", "?");
        }

        #endregion
    }
}