using GT_Common.Helper.Database.Abstractions;
using System.Data.SqlClient;
using System.Threading.Tasks;

/// <summary>
/// SQL Server 事务实现
/// 兼容无 CommitAsync 的版本
/// </summary>
public class SqlServerTransaction : IDatabaseTransaction
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    public SqlServerTransaction(
        SqlConnection connection,
        SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /// <summary>
    /// 提交事务
    /// 如果底层不支持异步，则包装为 Task
    /// </summary>
    public Task CommitAsync()
    {
        _transaction.Commit();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public Task RollbackAsync()
    {
        _transaction.Rollback();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public ValueTask DisposeAsync()
    {
        _transaction.Dispose();
        _connection.Dispose();
        return default;
    }
}
