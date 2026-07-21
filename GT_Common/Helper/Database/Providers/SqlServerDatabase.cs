using Dapper;
using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// SQL Server 数据库实现
    /// 
    /// 说明：
    /// 1. 实现 IDatabase 接口
    /// 2. 使用 Dapper 作为 ORM
    /// 3. 每次操作创建独立连接（推荐做法）
    /// 4. 支持事务
    /// 
    /// 注意：
    /// 本类只负责“执行数据库操作”
    /// 不负责路由策略（由 RoutingDecorator 负责）
    /// </summary>
    public class SqlServerDatabase : IDatabase
    {
        /// <summary>
        /// 原始数据库配置
        /// </summary>
        private readonly DatabaseNodeOptions _options;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">数据库节点配置</param>
        public SqlServerDatabase(DatabaseNodeOptions options)
        {
            _options = options ??
                throw new ArgumentNullException(nameof(options));
        }

        #region 私有方法

        /// <summary>
        /// 创建数据库连接
        /// 
        /// 说明：
        /// 每次操作创建新连接，由连接池管理。
        /// 不建议持有长期连接（避免连接泄漏）。
        /// </summary>
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_options.ConnectionString);
        }

        #endregion

        #region 数据库是否连接

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

                    // ✅ 简单检测即可
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region IDatabase 实现

        /// <summary>
        /// 执行非查询语句
        /// 
        /// 用于：
        /// INSERT / UPDATE / DELETE
        /// </summary>
        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.ExecuteAsync(
                    sql,
                    param,
                    commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// 查询单条记录
        /// 
        /// 若无数据返回 default(T)
        /// </summary>
        public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                return await conn.QuerySingleOrDefaultAsync<T>(
                    sql,
                    param,
                    commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// 查询多条记录
        /// </summary>
        public async Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            using (var conn = CreateConnection())
            {
                var result = await conn.QueryAsync<T>(
                    sql,
                    param,
                    commandType: CommandType.Text);

                return result.ToList();
            }
        }

        /// <summary>
        /// 开启事务
        /// 
        /// 注意：
        /// 调用方必须 using await using 释放事务
        /// </summary>
        public async Task<IDatabaseTransaction> BeginTransactionAsync()
        {
            var conn = CreateConnection();

            await conn.OpenAsync();

            var transaction = conn.BeginTransaction();

            return new SqlServerTransaction(conn, transaction);
        }

        #endregion
    }
}