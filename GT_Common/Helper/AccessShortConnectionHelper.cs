using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;

namespace GT_Common.Helper
{
    /// <summary>
    /// Access 短连接助手，支持每次操作自动打开/关闭连接
    /// 并可以动态切换数据库路径
    /// </summary>
    public sealed class AccessShortConnectionHelper
    {
        public string _dbPath;
        private readonly object _lockObj = new object();

        private bool _isConnected;
        /// <summary>
        /// 当前数据库连接状态（仅在最近一次操作或测试后更新）
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock (_lockObj)
                {
                    return _isConnected;
                }
            }
            private set
            {
                lock (_lockObj)
                {
                    _isConnected = value;
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        public AccessShortConnectionHelper(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentNullException(nameof(dbPath));

            _dbPath = dbPath;
        }

        /// <summary>
        /// 获取或设置数据库路径
        /// </summary>
        public string DatabasePath
        {
            get => _dbPath;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));

                lock (_lockObj)
                {
                    if (!string.Equals(_dbPath, value, StringComparison.OrdinalIgnoreCase))
                    {
                        string oldPath = _dbPath;
                        _dbPath = value;

                        try
                        {
                            TestConnection(); // 测试新路径是否有效
                        }
                        catch
                        {
                            _dbPath = oldPath; // 恢复旧路径
                            throw;
                        }
                    }
                }

                lock (_lockObj)
                {
                    _dbPath = value;
                    _isConnected = false; // 更换路径后重置状态
                }
            }
        }


        private string ConnectionString => $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={_dbPath};Persist Security Info=False;";

        private void ValidateDatabaseFile()
        {
            if (string.IsNullOrWhiteSpace(_dbPath))
                throw new InvalidOperationException("数据库路径未设置");

            if (!File.Exists(_dbPath))
                throw new FileNotFoundException("数据库文件不存在", _dbPath);
        }

        /// <summary>
        /// 测试数据库是否可连接
        /// </summary>
        public bool TestConnection()
        {
            ValidateDatabaseFile();
            try
            {
                using (var conn = new OleDbConnection(ConnectionString))
                {
                    conn.Open();
                    IsConnected = (conn.State == ConnectionState.Open);
                    conn.Close();
                }
            }
            catch
            {
                IsConnected = false;
            }
            return IsConnected;
        }

        /// <summary>
        /// 执行非查询 SQL
        /// </summary>
        public int ExecuteNonQuery(string sql, params OleDbParameter[] parameters)
        {
            ValidateDatabaseFile();

            lock (_lockObj)
            {
                using (var conn = new OleDbConnection(ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    IsConnected = conn.State == ConnectionState.Open;

                    cmd.CommandText = sql;
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 执行查询 SQL 返回 DataTable
        /// </summary>
        public DataTable ExecuteDataTable(string sql, params OleDbParameter[] parameters)
        {
            ValidateDatabaseFile();

            lock (_lockObj)
            {
                using (var conn = new OleDbConnection(ConnectionString))
                using (var cmd = conn.CreateCommand())
                using (var adapter = new OleDbDataAdapter(cmd))
                {
                    conn.Open();
                    IsConnected = conn.State == ConnectionState.Open;

                    cmd.CommandText = sql;
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public int ExecuteBatchNonQuery(string sql, IEnumerable<OleDbParameter[]> parameterSets)
        {
            ValidateDatabaseFile();
            if (parameterSets == null)
                throw new ArgumentNullException(nameof(parameterSets));

            lock (_lockObj)
            {
                using (var conn = new OleDbConnection(ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    IsConnected = conn.State == ConnectionState.Open;

                    using (var tran = conn.BeginTransaction())
                    {
                        cmd.Transaction = tran;
                        cmd.CommandText = sql;

                        int total = 0;
                        try
                        {
                            foreach (var parameters in parameterSets)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddRange(parameters);
                                total += cmd.ExecuteNonQuery();
                            }

                            tran.Commit();
                            return total;
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 执行查询 SQL 返回第一行第一列
        /// </summary>
        public object ExecuteScalar(string sql, params OleDbParameter[] parameters)
        {
            ValidateDatabaseFile();

            lock (_lockObj)
            {
                using (var conn = new OleDbConnection(ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    IsConnected = conn.State == ConnectionState.Open;

                    cmd.CommandText = sql;
                    if (parameters != null && parameters.Length > 0)
                        cmd.Parameters.AddRange(parameters);

                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}

