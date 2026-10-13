using Dapper;
using GT.Factory.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
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
    private bool _completed;

    public SqlServerTransaction(
        SqlConnection connection,
        SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }


    #region Sync

    /// <summary>
    /// 在当前事务中执行 SQL。
    /// </summary>
    public int Execute(
        string sql,
        object param = null,
        CommandType commandType = CommandType.Text)
    {
        CommandDefinition command =
            new CommandDefinition(
                sql,
                param,
                _transaction,
                null,
                commandType);

        return _connection.Execute(command);
    }

    /// <summary>
    /// 在当前事务中查询单条记录。
    /// </summary>
    public T QuerySingle<T>(
        string sql,
        object param = null)
    {
        CommandDefinition command =
            new CommandDefinition(
                sql,
                param,
                _transaction,
                null,
                CommandType.Text);

        return _connection.QuerySingleOrDefault<T>(command);
    }

    /// <summary>
    /// 在当前事务中查询多条记录。
    /// </summary>
    public List<T> Query<T>(
        string sql,
        object param = null)
    {
        CommandDefinition command =
            new CommandDefinition(
                sql,
                param,
                _transaction,
                null,
                CommandType.Text);

        IEnumerable<T> result =
            _connection.Query<T>(command);

        return new List<T>(result);
    }

    #endregion

    #region Async

    /// <summary>
    /// 在当前事务中异步执行 SQL。
    /// </summary>
    public async Task<int> ExecuteAsync(
        string sql,
        object param = null,
        CommandType commandType = CommandType.Text)
    {
        CommandDefinition command =
            new CommandDefinition(
                sql,
                param,
                _transaction,
                null,
                commandType);

        return await _connection.ExecuteAsync(command);
    }

    /// <summary>
    /// 在当前事务中异步查询单条记录。
    /// </summary>
    public async Task<T> QuerySingleAsync<T>(
        string sql,
        object param = null)
    {
        CommandDefinition command =
            new CommandDefinition(
                sql,
                param,
                _transaction,
                null,
                CommandType.Text);

        return await _connection.QuerySingleOrDefaultAsync<T>(command);
    }

    /// <summary>
    /// 在当前事务中异步查询多条记录。
    /// </summary>
    public async Task<List<T>> QueryAsync<T>(
        string sql,
        object param = null)
    {
        CommandDefinition command =
            new CommandDefinition(
                sql,
                param,
                _transaction,
                null,
                CommandType.Text);

        IEnumerable<T> result =
            await _connection.QueryAsync<T>(command);

        return new List<T>(result);
    }

    #endregion


    #region Transaction Control

    /// <summary>
    /// 提交事务。
    /// </summary>
    public Task CommitAsync()
    {
        if (_completed)
        {
            return Task.CompletedTask;
        }

        _transaction.Commit();

        _completed = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// 回滚事务。
    /// </summary>
    public Task RollbackAsync()
    {
        if (_completed)
        {
            return Task.CompletedTask;
        }

        _transaction.Rollback();

        _completed = true;

        return Task.CompletedTask;
    }

    #endregion

    #region Dispose

    /// <summary>
    /// 释放事务和数据库连接。
    ///
    /// 注意：
    /// Transaction 创建时拥有 Connection，
    /// 因此这里两个对象都需要释放。
    /// </summary>
    public ValueTask DisposeAsync()
    {
        try
        {
            _transaction.Dispose();
        }
        finally
        {
            _connection.Dispose();
        }

        return default(ValueTask);
    }

    #endregion
}


