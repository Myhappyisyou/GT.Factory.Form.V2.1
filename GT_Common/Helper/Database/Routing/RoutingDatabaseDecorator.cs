using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Routing
{
    /// <summary>
    /// 数据库路由装饰器
    /// 
    /// 作用：
    /// 在执行SQL前动态替换数据库名称
    /// 不修改真实数据库实现
    /// </summary>
    public class RoutingDatabaseDecorator : IDatabase
    {
        private readonly IDatabase _inner;
        private readonly DatabaseNodeOptions _options;
        private readonly IDatabaseRouter _router;
        private readonly DatabaseProvisionService _provisionService;

        public RoutingDatabaseDecorator(
            IDatabase inner,
            DatabaseNodeOptions options,
            IDatabaseRouter router)
        {
            _inner = inner;
            _options = options;
            _router = router;
            _provisionService = new DatabaseProvisionService(options.ConnectionString);
        }

        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            await ApplyRouting();
            return await _inner.ExecuteAsync(sql, param);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            await ApplyRouting();
            return await _inner.QuerySingleAsync<T>(sql, param);
        }

        public async Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            await ApplyRouting();
            return await _inner.QueryAsync<T>(sql, param);
        }

        public async Task<IDatabaseTransaction> BeginTransactionAsync()
        {
            await ApplyRouting();
            return await _inner.BeginTransactionAsync();
        }

        public async Task<bool> PingAsync()
        {
            await ApplyRouting();
            return await _inner.PingAsync();
        }

        /// <summary>
        /// 替换连接字符串中的数据库名
        /// </summary>
        private async Task ApplyRouting()
        {
            if (_router == null)
                return;

            var newDbName = _router.GetDatabaseName();

            // ✅ 自动创建数据库
            await _provisionService.EnsureDatabaseExistsAsync(newDbName);

            var builder = new SqlConnectionStringBuilder(
                _options.ConnectionString);

            builder.InitialCatalog = newDbName;

            _options.ConnectionString = builder.ConnectionString;
        }
    }
}