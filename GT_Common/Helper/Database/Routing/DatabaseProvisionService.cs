using Dapper;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Routing
{
    /// <summary>
    /// 数据库创建与初始化服务
    /// 
    /// 负责：
    /// 1. 检查数据库是否存在
    /// 2. 若不存在则自动创建
    /// 3. 可扩展执行初始化脚本
    /// </summary>
    public class DatabaseProvisionService
    {
        private readonly string _masterConnectionString;

        /// <summary>
        /// 构造函数
        /// 
        /// 注意：
        /// 连接字符串必须指向 master 数据库
        /// </summary>
        public DatabaseProvisionService(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            _masterConnectionString = builder.ConnectionString;
        }

        /// <summary>
        /// 确保数据库存在
        /// </summary>
        public async Task EnsureDatabaseExistsAsync(string databaseName)
        {
            using (var conn = new SqlConnection(_masterConnectionString))
            {
                await conn.OpenAsync();

                var checkSql = @"
                SELECT COUNT(*) 
                FROM sys.databases 
                WHERE name = @dbName";

                var count = await conn.ExecuteScalarAsync<int>(
                    checkSql,
                    new { dbName = databaseName });

                if (count == 0)
                {
                    var createSql = $"CREATE DATABASE [{databaseName}]";

                    await conn.ExecuteAsync(createSql);

                    // ✅ 可扩展：创建表结构
                    await InitializeTablesAsync(databaseName);
                }
            }
        }

        /// <summary>
        /// 初始化数据库表结构
        /// 可扩展执行 SQL 脚本
        /// </summary>
        private async Task InitializeTablesAsync(string databaseName)
        {
            // 示例：创建一张日志表
            var builder = new SqlConnectionStringBuilder(_masterConnectionString)
            {
                InitialCatalog = databaseName
            };

            using (var conn = new SqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='Logs')
                CREATE TABLE Logs(
                    Id INT IDENTITY PRIMARY KEY,
                    Message NVARCHAR(500),
                    CreateTime DATETIME DEFAULT GETDATE()
                )";

                await conn.ExecuteAsync(sql);
            }
        }
    }
}
