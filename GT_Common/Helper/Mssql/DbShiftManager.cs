using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace GT_Common.Helper.Mssql
{
    public static class DbShiftManager
    {
        private static readonly object _lock = new object();
        private static string _currentDbChecked = null;

        private static string BakPath = @"E:\GT_System\Server\Config\SqlBackup\BYD_Template.bak";  // 模板备份
        private static string DataPath = @"D:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA";                   // MDF/LDF存放路径

        /// <summary>
        /// 每次数据库操作前调用，保证数据库存在并切换 Conn1
        /// </summary>
        public static void EnsureShiftDb()
        {
            ShiftInfo shift = ShiftService.GetCurrentShift(DateTime.Now);
            string dbName = $"BYD_{shift.StartTime.Year}";

            if (_currentDbChecked == dbName) return;

            lock (_lock)
            {
                if (_currentDbChecked == dbName) return;

                string masterConn = new SqlConnectionStringBuilder(MSSqlHelper.Conn1)
                {
                    InitialCatalog = "master"
                }.ToString();

                // 使用 SQL Server 分布式锁，防止多客户端同时建库
                using (var conn = new SqlConnection(masterConn))
                {
                    conn.Open();

                    using (var cmd = new SqlCommand("sp_getapplock", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Resource", $"CreateDb_{dbName}");
                        cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
                        cmd.Parameters.AddWithValue("@LockTimeout", 10000);
                        cmd.Parameters.AddWithValue("@LockOwner", "Session"); // ✅ 关键

                        int result = Convert.ToInt32(cmd.ExecuteScalar());

                        if (result >= 0)
                        {
                            try
                            {
                                InitDb(dbName, conn);
                            }
                            finally
                            {
                                using (var releaseCmd = new SqlCommand("sp_releaseapplock", conn))
                                {
                                    releaseCmd.CommandType = CommandType.StoredProcedure;
                                    releaseCmd.Parameters.AddWithValue("@Resource", $"CreateDb_{dbName}");
                                    releaseCmd.Parameters.AddWithValue("@LockOwner", "Session"); // ✅ 必须一致
                                    releaseCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                // 切换 Conn1 到当前年班次库
                var builder = new SqlConnectionStringBuilder(MSSqlHelper.Conn1)
                {
                    InitialCatalog = dbName
                };
                MSSqlHelper.Conn1 = builder.ToString();

                _currentDbChecked = dbName;
            }
        }

        /// <summary>
        /// 建库逻辑（如果不存在从模板恢复）
        /// </summary>
        private static void InitDb(string dbName, SqlConnection conn)
        {
            string checkSql = $"SELECT COUNT(1) FROM sys.databases WHERE name = '{dbName}'";
            int exists = Convert.ToInt32(MSSqlHelper.ExecuteScalar(conn, CommandType.Text, checkSql));
            if (exists > 0) return;

            string mdf = $@"{DataPath}\{dbName}.mdf";
            string ldf = $@"{DataPath}\{dbName}_log.ldf";

            string restoreSql = $@"
                                    RESTORE DATABASE [{dbName}]
                                    FROM DISK = '{BakPath}'
                                    WITH 
                                        MOVE 'BYD_Template' TO '{mdf}',
                                        MOVE 'BYD_Template_log' TO '{ldf}',
                                        REPLACE";

            try
            {
                MSSqlHelper.ExecuteNonQuery(conn, CommandType.Text, restoreSql);
            }
            catch (Exception ex)
            {
                // 日志（必须有）
                Log.Error($"建库失败: {dbName}", ex);
                throw;
            }
        }
    }
}
