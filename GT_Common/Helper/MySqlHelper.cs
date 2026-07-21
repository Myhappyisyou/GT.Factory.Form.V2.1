using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace GT_Common.Helper
{
    public class MySqlHelper : IDisposable
    {
        private readonly string _connectionString;

        public MySqlHelper(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            _connectionString = connectionString;
        }

        #region 基础方法
        public async Task<int> ExecuteNonQueryAsync(string sql, object parameters = null)
        {
            using (var conn = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                AddParameters(cmd, parameters);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<object> ExecuteScalarAsync(string sql, object parameters = null)
        {
            using (var conn = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                AddParameters(cmd, parameters);
                await conn.OpenAsync();
                return await cmd.ExecuteScalarAsync();
            }
        }
        #endregion

        #region ORM 风格泛型查询
        public async Task<List<T>> QueryAsync<T>(string sql, object parameters = null) where T : new()
        {
            using (var conn = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                AddParameters(cmd, parameters);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var result = new List<T>();
                    var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    while (await reader.ReadAsync())
                    {
                        T obj = new T();
                        foreach (var prop in props)
                        {
                            if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull) continue;
                            var value = reader[prop.Name];
                            prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType), null);
                        }
                        result.Add(obj);
                    }
                    return result;
                }
            }
        }
        #endregion

        #region 事务
        public async Task<bool> ExecuteTransactionAsync(List<(string sql, object parameters)> commands)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var command in commands)
                        {
                            using (var cmd = new MySqlCommand(command.sql, conn, transaction as MySqlTransaction))
                            {
                                AddParameters(cmd, command.parameters);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        await transaction.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
        }
        #endregion

        #region 辅助方法
        private void AddParameters(MySqlCommand cmd, object parameters)
        {
            if (parameters == null) return;

            if (parameters is Dictionary<string, object>)
            {
                var dict = (Dictionary<string, object>)parameters;
                foreach (var kv in dict)
                {
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                }
            }
            else // 匿名对象支持
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters, null) ?? DBNull.Value);
                }
            }
        }

        public void Dispose()
        {
            // 无需显式释放
        }
        #endregion
    }

    public static class DataReaderExtensions
    {
        public static bool HasColumn(this IDataRecord reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
