using GT_Common.Helper.Database.Abstractions;
using MySqlConnector;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Providers
{
    /// <summary>
    /// MySQL 事务封装
    /// </summary>
    public class MySqlDatabaseTransaction : IDatabaseTransaction
    {
        private readonly MySqlConnection _connection;
        private readonly MySqlTransaction _transaction;

        public MySqlDatabaseTransaction(
            MySqlConnection connection,
            MySqlTransaction transaction)
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