using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GT_Common.Helper
{
    public sealed class AccessMdbHelper : IDisposable
    {
        #region 字段和属性

        private readonly object _lockObj = new object();
        private OleDbConnection _connection;
        public string _dbPath;
        private bool _disposed = false;

        /// <summary>
        /// 获取或设置当前数据库文件路径
        /// </summary>
        public string DatabasePath
        {
            get => _dbPath;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(value));

                if (!string.Equals(_dbPath, value, StringComparison.OrdinalIgnoreCase))
                {
                    lock (_lockObj)
                    {
                        if (!string.Equals(_dbPath, value, StringComparison.OrdinalIgnoreCase))
                        {
                            string oldPath = _dbPath;
                            _dbPath = value;

                            try
                            {
                                ResetConnection();
                                TestConnection(); // 测试新路径是否有效
                                OnDatabasePathChanged(oldPath, value);
                            }
                            catch
                            {
                                _dbPath = oldPath; // 恢复旧路径
                                throw;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                lock (_lockObj)
                {
                    return _connection?.State ?? ConnectionState.Closed;
                }
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 数据库路径变更事件
        /// </summary>
        public event EventHandler<DatabasePathChangedEventArgs> DatabasePathChanged;

        /// <summary>
        /// 连接状态变更事件
        /// </summary>
        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化 AccessDatabase 实例
        /// </summary>
        /// <param name="dbPath">数据库文件路径</param>
        public AccessMdbHelper(string dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
                throw new ArgumentNullException(nameof(dbPath));

            _dbPath = dbPath;
            Initialize();
        }

        private void Initialize()
        {
            // 注册编码提供程序（.NET Core 需要）
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch { /* 忽略 */ }

            // 验证路径
            ValidateDatabaseFile();
        }

        #endregion

        #region 公共方法



        /// <summary>
        /// 执行查询并返回 DataTable
        /// </summary>
        public DataTable ExecuteDataTable(string sql, params OleDbParameter[] parameters)
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();
            ValidateParameters(sql, parameters); // 添加验证

            lock (_lockObj)
            {
                using (var cmd = CreateCommand(sql, parameters))
                using (var adapter = new OleDbDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);

                    // 确保DataTable有正确的表名
                    if (dt.TableName == null || dt.TableName == "")
                    {
                        // 尝试从SQL中提取表名
                        string tableName = ExtractTableNameFromSql(sql);
                        if (!string.IsNullOrEmpty(tableName))
                        {
                            dt.TableName = tableName;
                        }
                    }

                    return dt;
                }
            }
        }

        // 从SQL语句中提取表名的辅助方法
        private string ExtractTableNameFromSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return null;

            string upperSql = sql.ToUpper().Trim();

            // 简单提取表名逻辑
            if (upperSql.StartsWith("SELECT"))
            {
                int fromIndex = upperSql.IndexOf("FROM");
                if (fromIndex > 0)
                {
                    string afterFrom = sql.Substring(fromIndex + 4).Trim();
                    string[] parts = afterFrom.Split(new[] { ' ', ',', ';', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        // 去除可能的方括号
                        return parts[0].Replace("[", "").Replace("]", "");
                    }
                }
            }
            else if (upperSql.StartsWith("INSERT") || upperSql.StartsWith("UPDATE") || upperSql.StartsWith("DELETE"))
            {
                string[] parts = sql.Split(new[] { ' ', ';', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    // 去除可能的方括号
                    return parts[1].Replace("[", "").Replace("]", "");
                }
            }

            return null;
        }

        /// <summary>
        /// 执行非查询SQL语句
        /// </summary>
        public int ExecuteNonQuery(string sql, params OleDbParameter[] parameters)
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();
            ValidateParameters(sql, parameters); // 添加验证

            lock (_lockObj)
            {
                using (var cmd = CreateCommand(sql, parameters))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }


        public int ExecuteNonQuery(string sql, OleDbParameter[] parameters, OleDbTransaction transaction)
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();
            ValidateParameters(sql, parameters);

            lock (_lockObj)
            {
                var cmd = CreateCommand(sql, parameters);
                cmd.Transaction = transaction; // 💥 设置事务
                return cmd.ExecuteNonQuery();
            }
        }

   
        /// <summary>
        /// 执行查询并返回第一行第一列的值
        /// </summary>
        public object ExecuteScalar(string sql, params OleDbParameter[] parameters)
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();
            ValidateParameters(sql, parameters); // 添加验证

            lock (_lockObj)
            {
                using (var cmd = CreateCommand(sql, parameters))
                {
                    return cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public AccessTransaction BeginTransaction()
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();

            lock (_lockObj)
            {
                var connection = GetConnection();
                return new AccessTransaction(connection.BeginTransaction(), this);
            }
        }

        /// <summary>
        /// 创建表
        /// </summary>
        public void CreateTable(string tableName, ColumnDefinition[] columns, string primaryKey = null)
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));

            if (columns == null || columns.Length == 0)
                throw new ArgumentException("必须至少定义一个列", nameof(columns));

            var sql = new System.Text.StringBuilder();
            sql.Append($"CREATE TABLE [{tableName}] (");

            for (int i = 0; i < columns.Length; i++)
            {
                if (i > 0) sql.Append(", ");

                sql.Append($"[{columns[i].Name}] {columns[i].DataType}");

                if (!string.IsNullOrWhiteSpace(primaryKey) &&
                    columns[i].Name.Equals(primaryKey, StringComparison.OrdinalIgnoreCase))
                {
                    sql.Append(" PRIMARY KEY");
                }
            }

            sql.Append(")");

            ExecuteNonQuery(sql.ToString());
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public bool TableExists(string tableName)
        {

            ValidateNotDisposed();
            ValidateDatabaseFile();

            lock (_lockObj)
            {
                using (var connection = GetConnection())
                {
                    var schema = connection.GetSchema("Tables", new[] { null, null, tableName, "TABLE" });
                    return schema.Rows.Count > 0;
                }
            }
        }

        /// <summary>
        /// 获取数据库中所有表名
        /// </summary>
        public string[] GetTableNames()
        {
            ValidateNotDisposed();
            ValidateDatabaseFile();

            lock (_lockObj)
            {
                using (var connection = GetConnection())
                {
                    var schema = connection.GetSchema("Tables");
                    var tables = new string[schema.Rows.Count];

                    for (int i = 0; i < schema.Rows.Count; i++)
                    {
                        tables[i] = schema.Rows[i]["TABLE_NAME"].ToString();
                    }

                    return tables;
                }
            }
        }

        #endregion

        #region 私有方法

       

        // 添加验证参数数量与占位符匹配的方法
        private void ValidateParameters(string sql, OleDbParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return;

            int paramCount = CountPlaceholders(sql);
            if (paramCount != parameters.Length)
            {
                throw new ArgumentException(
                    $"参数数量不匹配。SQL需要 {paramCount} 个参数，但提供了 {parameters.Length} 个\nSQL: {sql}");
            }
        }

        private int CountPlaceholders(string sql)
        {
            // Access 使用 ? 作为参数占位符
            int count = 0;
            int index = -1;
            while ((index = sql.IndexOf('?', index + 1)) != -1)
            {
                count++;
            }
            return count;
        }


        private OleDbConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new OleDbConnection(ConnectionString);
                _connection.StateChange += OnConnectionStateChange;
                _connection.Open();
            }
            else if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        private OleDbCommand CreateCommand(string sql, OleDbParameter[] parameters)
        {
            var cmd = GetConnection().CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 30;

            if (parameters != null && parameters.Length > 0)
            {
                // 修复：清除现有参数集合
                cmd.Parameters.Clear();

                // 修复：按顺序添加参数（Access 只支持位置参数）
                foreach (var param in parameters)
                {
                    // 创建参数副本（防止重复使用问题）
                    var newParam = (OleDbParameter)((ICloneable)param).Clone();
                    cmd.Parameters.Add(newParam);
                }
            }

            return cmd;
        }

        private void ResetConnection()
        {
            if (_connection != null)
            {
                _connection.StateChange -= OnConnectionStateChange;

                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }

                _connection.Dispose();
                _connection = null;
            }
        }

        private void TestConnection()
        {
            lock (_lockObj)
            {
                using (var testConn = new OleDbConnection(ConnectionString))
                {
                    testConn.Open();
                    using (var cmd = testConn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1";
                        cmd.ExecuteScalar();
                    }
                }
            }
        }

        private void ValidateDatabaseFile()
        {
            if (string.IsNullOrWhiteSpace(_dbPath))
                throw new InvalidOperationException("数据库路径未设置");

            if (!File.Exists(_dbPath))
                throw new FileNotFoundException("数据库文件不存在", _dbPath);
        }

        private void ValidateNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        private string ConnectionString => $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={_dbPath};Persist Security Info=False;";

        #endregion

        #region 事件处理

        private void OnDatabasePathChanged(string oldPath, string newPath)
        {
            DatabasePathChanged?.Invoke(this, new DatabasePathChangedEventArgs(oldPath, newPath));
        }

        private void OnConnectionStateChange(object sender, StateChangeEventArgs e)
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(e.OriginalState, e.CurrentState));
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_lockObj)
                {
                    if (!_disposed)
                    {
                        ResetConnection();
                        _disposed = true;
                    }
                }
            }
        }

        #endregion

        #region 嵌套类型

        public sealed class AccessTransaction : IDisposable
        {
            private readonly OleDbTransaction _transaction;
            private readonly AccessMdbHelper _database;
            private bool _completed = false;
            private bool _disposed = false;

            internal AccessTransaction(OleDbTransaction transaction, AccessMdbHelper database)
            {
                _transaction = transaction;
                _database = database;
            }

            public void Commit()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(AccessTransaction));
                if (_completed) throw new InvalidOperationException("事务已经完成");

                _transaction.Commit();
                _completed = true;
            }

            public void Rollback()
            {
                if (_disposed) throw new ObjectDisposedException(nameof(AccessTransaction));
                if (_completed) throw new InvalidOperationException("事务已经完成");

                _transaction.Rollback();
                _completed = true;
            }

            public int ExecuteNonQuery(string sql, OleDbParameter[] parameters)
            {
                return _database.ExecuteNonQuery(sql, parameters, _transaction);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (!_completed)
                    {
                        try { _transaction.Rollback(); } catch { /* 忽略 */ }
                    }

                    _transaction.Dispose();
                    _disposed = true;
                }
            }
        }

        public class DatabasePathChangedEventArgs : EventArgs
        {
            public string OldPath { get; }
            public string NewPath { get; }

            public DatabasePathChangedEventArgs(string oldPath, string newPath)
            {
                OldPath = oldPath;
                NewPath = newPath;
            }
        }

        public class ConnectionStateChangedEventArgs : EventArgs
        {
            public ConnectionState OriginalState { get; }
            public ConnectionState CurrentState { get; }

            public ConnectionStateChangedEventArgs(ConnectionState originalState, ConnectionState currentState)
            {
                OriginalState = originalState;
                CurrentState = currentState;
            }
        }

        public class ColumnDefinition
        {
            public string Name { get; }
            public string DataType { get; }

            public ColumnDefinition(string name, string dataType)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
            }
        }

        #endregion
    }
}



