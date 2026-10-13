using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Data.Abstractions
{
    /// <summary>
    /// 统一数据库操作接口
    /// 所有数据库实现必须遵循此接口
    /// 业务层永远依赖接口，不依赖具体数据库类型
    /// </summary>
    public interface IDatabase
    {

        #region Sync

        /// <summary>
        /// 执行非查询SQL（INSERT / UPDATE / DELETE）
        /// </summary>
        int Execute(
        string sql,
            object param = null,
            CommandType commandType = CommandType.Text);

        /// <summary>
        /// 查询单条记录
        /// </summary>
        T QuerySingle<T>(
            string sql,
            object param = null);

        /// <summary>
        /// 查询多条记录
        /// </summary>
        List<T> Query<T>(
            string sql,
            object param = null);

        /// <summary>
        /// 数据库健康检测
        /// 用于检测数据库是否可连接
        /// </summary>
        bool Ping();

        #endregion


        /// <summary>
        /// 执行非查询SQL（INSERT / UPDATE / DELETE）
        /// </summary>
        Task<int> ExecuteAsync(string sql, object param = null);

        /// <summary>
        /// 查询单条记录
        /// </summary>
        Task<T> QuerySingleAsync<T>(string sql, object param = null);

        /// <summary>
        /// 查询多条记录
        /// </summary>
        Task<List<T>> QueryAsync<T>(string sql, object param = null);

        /// <summary>
        /// 开启事务
        /// </summary>
        Task<IDatabaseTransaction> BeginTransactionAsync();


         /// <summary> 
         /// 按指定隔离级别创建数据库事务。
         /// </summary> 
         Task<IDatabaseTransaction> BeginTransactionAsync( IsolationLevel isolationLevel);

        /// <summary>
        /// 数据库健康检测
        /// 用于检测数据库是否可连接
        /// </summary>
        Task<bool> PingAsync();
    }
}
