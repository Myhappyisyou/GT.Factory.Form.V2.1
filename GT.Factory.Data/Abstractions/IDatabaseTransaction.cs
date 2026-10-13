using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Data.Abstractions
{
    /// <summary>
    /// 数据库事务接口
    /// </summary>
    public interface IDatabaseTransaction : IAsyncDisposable
    {

        #region Sync

        /// <summary>
        /// 执行事务内非查询 SQL。
        /// </summary>
        int Execute(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        /// 查询事务内单条记录。
        /// </summary>
        T QuerySingle<T>(
            string sql,
            object param = null);

        /// <summary>
        /// 查询事务内多条记录。
        /// </summary>
        List<T> Query<T>(
            string sql,
            object param = null);

        #endregion

        #region Async

        /// <summary>
        /// 执行事务内非查询 SQL。
        /// </summary>
        Task<int> ExecuteAsync(
            string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        /// 查询事务内单条记录。
        /// </summary>
        Task<T> QuerySingleAsync<T>(
            string sql,
            object param = null);

        /// <summary>
        /// 查询事务内多条记录。
        /// </summary>
        Task<List<T>> QueryAsync<T>(
            string sql,
            object param = null);

        #endregion


        #region Transaction

        /// <summary>
        /// 提交事务。
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// 回滚事务。
        /// </summary>
        Task RollbackAsync();

        #endregion
    }
}
