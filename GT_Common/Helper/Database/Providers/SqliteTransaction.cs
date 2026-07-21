using GT_Common.Helper.Database.Abstractions;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// SQLite 事务封装
    /// </summary>
    public class SqliteTransaction : IDatabaseTransaction
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;

        public SqliteTransaction(
            SQLiteConnection connection,
            SQLiteTransaction transaction)
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
