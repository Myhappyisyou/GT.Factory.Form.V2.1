using GT_Common.Helper.Database.Abstractions;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// Access 事务封装
    /// </summary>
    public class AccessDatabaseTransaction : IDatabaseTransaction
    {
        private readonly OleDbConnection _connection;
        private readonly OleDbTransaction _transaction;

        public AccessDatabaseTransaction(
            OleDbConnection connection,
            OleDbTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public Task CommitAsync()
        {
            _transaction.Commit();
            return Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            _transaction.Rollback();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _transaction.Dispose();
            _connection.Dispose();
            return default;
        }
    }
}