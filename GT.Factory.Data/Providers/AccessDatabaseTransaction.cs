using GT.Factory.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Threading.Tasks;

namespace GT.Factory.Data.Providers
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

        public int Execute(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            throw new System.NotImplementedException();
        }

        public T QuerySingle<T>(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }

        public List<T> Query<T>(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> ExecuteAsync(string sql, object param = null, CommandType commandType = CommandType.Text)
        {
            throw new System.NotImplementedException();
        }

        public Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<T>> QueryAsync<T>(string sql, object param = null)
        {
            throw new System.NotImplementedException();
        }
    }
}