using Serilog;
using GT.Factory.Server.Core.Alarms;
using GT.Factory.Server.Core.ClientInfor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using TaskContracts.Models;
using Consumables = TaskContracts.Models.Consumables;
using User = TaskContracts.Models.User;
using GT.Factory.Logging.Logging;

namespace GT.Factory.Server.Help
{
    public static class UploadSql
    {
        #region 用户相关

        //  插入员工记录
        public static void InsertOrUpdateUser(AccessMdbHelper db, User user)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
            string updateSql = @"

                                UPDATE [Users]
                                
                                SET
                                
                                [登录次数] = [登录次数] + 1,
                                
                                [最后登录时间] = Format(Now(), ""YYYY-MM-DD HH:MM:SS"")
                                
                                WHERE [ID] = ?";

            var param = new[]
            {
               new OleDbParameter("ID", OleDbType.Integer) { Value = user.ID }
            };

            int rowsAffected = db.ExecuteNonQuery(updateSql, param);

            // 2. 如果未更新到记录（说明ID不存在），则插入新记录
            if (rowsAffected == 0) InsertNewUser(db, user);
        }

        //  插入新员工登录信息
        public static void InsertNewUser(AccessMdbHelper db, User user)
        {
            string insertSql = @"
                               INSERT INTO [Users] (
                                                     [用户名], [用户密码], [用户权限],
                                                     [厂牌UID值], [最后登录时间], [登录次数], [工号], [MES账号], [MES密码]
                                                    ) VALUES (?, ?, ?, ?, Format(Now(), ""YYYY-MM-DD HH:MM:SS""), ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UserName ?? "" },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UserPassword ?? "" },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UserRole ?? "" },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UID ?? "" },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value =  1 },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.JobNub ?? "" },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.MesAccount ?? "" },
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.MesPassword ?? "" }
            };

            db.ExecuteNonQuery(insertSql, parameters);
        }

        // 查询员工信息
        public static User SelectUsersInforByUID(AccessMdbHelper db,string UID)
        {
            string updateSql = @"SELECT * FROM [UsersInfor] WHERE [厂牌UID] = ?";

            var param = new OleDbParameter[] { new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = UID } };
         
            DataTable dt = db.ExecuteDataTable(updateSql,param);

            if (dt.Rows.Count==1)
            {
                DataRow row = dt.Rows[0];
                User user = new User
                {
                    ID = Convert.ToInt32(row["ID"]),
                    UserName = row["用户名"].ToString(),
                    UserPassword = row["用户密码"].ToString(),
                    UserRole = row["用户权限"].ToString(),
                    UID = row["厂牌UID"].ToString(),
                    JobNub = row["工号"].ToString(),
                    MesAccount = row["MES账号"].ToString(),
                    MesPassword = row["MES密码"].ToString()
                };
                return user;
            }
            else
            {
                return null;
            }

        }

        // 查询员工信息
        public static User SelectUsersInforByJobNub(AccessMdbHelper db, string JobNub)
        {
            string updateSql = @"SELECT * FROM [UsersInfor] WHERE [工号] = ?";

            var param = new OleDbParameter[] { new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = JobNub } };

            DataTable dt = db.ExecuteDataTable(updateSql, param);

            if (dt.Rows.Count == 1)
            {
                DataRow row = dt.Rows[0];
                return new User
                {
                    ID = Convert.ToInt32(row["ID"]),
                    UserName = row["用户名"].ToString(),
                    UserPassword = row["用户密码"].ToString(),
                    UserRole = row["用户权限"].ToString(),
                    UID = row["厂牌UID"].ToString(),
                    JobNub = row["工号"].ToString(),
                    MesAccount = row["MES账号"].ToString(),
                    MesPassword = row["MES密码"].ToString()
                };
            }
            else
            {
                return null;
            }

        }

        // 查询员工信息
        public static User SelectUsersInforByJobNub(AccessMdbHelper db, string JobNub, string PassWord)
        {
            string updateSql = @"SELECT * FROM [UsersInfor] WHERE [工号] = ? AND [用户密码] = ?";

            var param = new OleDbParameter[]
            {
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = JobNub } ,
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = PassWord } ,
            };
            DataTable dt;
            try
            {
                dt = db.ExecuteDataTable(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户查询失败", ex);
                return null;
            }

            if (dt.Rows.Count == 1)
            {
                DataRow row = dt.Rows[0];
                return new User
                {
                    ID = Convert.ToInt32(row["ID"]),
                    UserName = row["用户名"].ToString(),
                    UserPassword = row["用户密码"].ToString(),
                    UserRole = row["用户权限"].ToString(),
                    UID = row["厂牌UID"].ToString(),
                    JobNub = row["工号"].ToString(),
                    MesAccount = row["MES账号"].ToString(),
                    MesPassword = row["MES密码"].ToString()
                };
            }
            else
            {
                return null;
            }
        }

        // 查询员工信息
        public static List<User> SelectUsersInfors(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [UsersInfor]";

            DataTable dt = db.ExecuteDataTable(updateSql);

            return MapDataTableToList<User>(dt);
        }

        //  插入新员工信息
        public static void InsertNewUserInfor(AccessMdbHelper db, User user)
        {
            try
            {
                string insertSql = @"
                               INSERT INTO [UsersInfor] (
                                                     [用户名], [用户密码], [用户权限],
                                                     [厂牌UID], [工号], [MES账号], [MES密码]
                                                    ) VALUES (?, ?, ?, ?, ?, ?, ?)";

                var parameters = new[]
                {
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UserName ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UserPassword ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UserRole ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.UID ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.JobNub ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.MesAccount ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = user.MesPassword ?? "" }
                };

                db.ExecuteNonQuery(insertSql, parameters);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                throw;
            }
           
        }

        // 更新员工信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void UpdateUsersInfor(AccessMdbHelper db, User user)
        {
            try
            {
                // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
                string updateSql = @"
                                UPDATE [UsersInfor]
                                SET
                                [用户名] = ?,
                                [用户密码] = ?,
                                [用户权限] = ?,
                                [厂牌UID] = ?,
                                [工号] = ?,
                                [MES账号] = ?,
                                [MES密码] = ?
                                WHERE [ID] = ?";

                var param = new[]
                {
                    new OleDbParameter("用户名", OleDbType.VarChar) { Value = user.UserName },
                    new OleDbParameter("用户密码", OleDbType.VarChar) { Value = user.UserPassword },
                    new OleDbParameter("用户权限", OleDbType.VarChar) { Value = user.UserRole },
                    new OleDbParameter("厂牌UID", OleDbType.VarChar) { Value = user.UID },
                    new OleDbParameter("工号", OleDbType.VarChar) { Value = user.JobNub },
                    new OleDbParameter("MES账号", OleDbType.VarChar) { Value = user.MesAccount },
                    new OleDbParameter("MES密码", OleDbType.VarChar) { Value = user.MesPassword },
                    new OleDbParameter("ID", OleDbType.Integer) { Value = user.ID }
                };

                db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
            }
        }

        // 删除员工信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void DeleteUsersInfor(AccessMdbHelper db, User user)
        {
            try
            {
                // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
                string updateSql = @"
                                DELETE FROM [UsersInfor]
                                WHERE [工号] = ?";

                var param = new[]
                {
                    new OleDbParameter("工号", OleDbType.VarChar) { Value = user.JobNub }
                };

                db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
            }

        }

        #endregion

        #region 插入测试数据

        //  插入测试数据 带返回插入结果
        public static bool InsertProcessPropertyWithResult(
                AccessMdbHelper db,
                ProcessData processData
                )
        {
            int insertedCount = 0;

            if (string.IsNullOrWhiteSpace(processData.DataItem.Sn))
            {
                DisplayLog.Info("条码为空，插入取消",true);
                return false;
            }

            if (processData.DataItem.Data == null || processData.DataItem.Data.Length == 0)
            {
                DisplayLog.Info("测试项为空，插入取消",true);
                return false;
            }

            try
            {

                using (var tx = db.BeginTransaction())
                {
                    for (int i = 0; i < processData.DataItem.Data.Length; i++)
                    {
                        string sql = @"INSERT INTO [生产信息]
                                    ([工位名称],[当前工单号],[产品条码],[操作人员],[测试时间],
                                    [测试结果],[测试节拍],[测试项名称],[测试项上限],
                                    [测试项下限],[测试项实际值],[更新标识])
                                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter("?", processData.Process_no),
                            new OleDbParameter("?", processData.Order_no),
                            new OleDbParameter("?", processData.DataItem.Sn),
                            new OleDbParameter("?", processData.Operator_no),
                            new OleDbParameter("?", processData.Do_time),
                            new OleDbParameter("?", processData.Ok_flag[i]),
                            new OleDbParameter("?", processData.Test_beat),
                            new OleDbParameter("?", processData.DataItem.Data_name[i]),
                            new OleDbParameter("?", processData.DataItem.Data_up[i]),
                            new OleDbParameter("?", processData.DataItem.Data_down[i]),
                            new OleDbParameter("?", processData.DataItem.Data[i]),
                            new OleDbParameter("?", "F"),
                        };

                        int affected = tx.ExecuteNonQuery(sql, parameters);
                        insertedCount += affected;
                    }

                    if (insertedCount == processData.DataItem.Data.Length)
                    {
                        // 提交事务
                        tx.Commit();
                        return true;
                    }
                    else
                    {
                        tx.Rollback();
                        DisplayLog.Info($"{processData.DataItem.Sn}只插入了 {insertedCount} 条，期望 {processData.DataItem.Data.Length} 条，事务已回滚。");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"插入失败", ex, true);
                return false;
            }
        }


        #endregion

        #region 易损件

        //  更新易损件记录

        public static void UpdateConsumables(AccessMdbHelper db, Consumables consumables)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
            string updateSql = @"

                                UPDATE [易损件信息]
                                
                                SET
                                
                                [易损件已使用次数] = [易损件已使用次数] + 1,
                                
                                [易损件剩余使用次数] = [易损件剩余使用次数] - 1
                                
                                WHERE [机台名称] = ? AND [易损件所在位置] = ? AND [易损件名称] = ?";

            var param = new[]
            {
                    new OleDbParameter("机台名称", OleDbType.VarWChar) { Value = consumables.StationName },
                    new OleDbParameter("易损件所在位置", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarWChar) { Value = consumables.Name }
            };

            int rowsAffected = db.ExecuteNonQuery(updateSql, param);

            // 2. 如果未更新到记录（说明ID不存在），则插入新记录
            if (rowsAffected == 0) InsertConsumables(db, consumables);
        }

        //  插入易损件信息
        public static bool InsertConsumables(AccessMdbHelper db, Consumables consumables)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
            try
            {
                string updateSql = @"

                                INSERT INTO [易损件信息]
                                ([易损件所在工位],[机台名称],[易损件所在位置],[易损件名称],[易损件理论使用次数],[易损件已使用次数],[易损件剩余使用次数])
                                
                                VALUES (?, ?, ?, ?, ?, ?, ?)";

                var param = new[]
                {
                    new OleDbParameter("易损件所在工位", OleDbType.VarWChar) { Value = consumables.ProcessName },
                    new OleDbParameter("机台名称", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件所在位置", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarWChar) { Value = consumables.Name },
                    new OleDbParameter("易损件理论使用次数", OleDbType.VarWChar) { Value = consumables.TheoreticalCount },
                    new OleDbParameter("易损件已使用次数", OleDbType.VarWChar) { Value = consumables.UsedCount },
                    new OleDbParameter("易损件剩余使用次数", OleDbType.VarWChar) { Value = consumables.RemainderCount },

            };

                int rowsAffected = db.ExecuteNonQuery(updateSql, param);
                return rowsAffected > 0;

            }
            catch (Exception ex)
            {
                Log.Error("InsertConsumables失败：" + ex);
                return false;
            }

        }

        public static bool InsertOrUpdateConsumablesInfor(AccessMdbHelper db, Consumables consumables)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
            string updateSql = @"
                                    UPDATE [易损件信息]
                                    SET
                                        [易损件所在工位] = ?,
                                        [机台名称] = ?,
                                        [易损件所在位置] = ?,
                                        [易损件名称] = ?,
                                        [易损件理论使用次数] = ?,
                                        [易损件已使用次数] = ?,
                                        [易损件剩余使用次数] = ?
                                    WHERE [ID] = ?";

            var param = new[]
            {
                    new OleDbParameter("易损件所在工位", OleDbType.VarChar) { Value = consumables.ProcessName },
                    new OleDbParameter("机台名称", OleDbType.VarChar) { Value = consumables.StationName },
                    new OleDbParameter("易损件所在位置", OleDbType.VarChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarChar) { Value = consumables.Name },
                    new OleDbParameter("易损件理论使用次数", OleDbType.VarChar) { Value = consumables.TheoreticalCount },
                    new OleDbParameter("易损件已使用次数", OleDbType.VarChar) { Value = consumables.UsedCount },
                    new OleDbParameter("易损件剩余使用次数", OleDbType.VarChar) { Value = consumables.RemainderCount },
                    new OleDbParameter("ID", OleDbType.Integer) { Value = consumables.ID }
            };

            try
            {
                int rowsAffected = db.ExecuteNonQuery(updateSql, param);

                // 2. 如果未更新到记录（说明ID不存在），则插入新记录
                if (rowsAffected == 0) 
                    return InsertConsumables(db, consumables);

                return true;
            }
            catch (Exception exp)
            {
                return false;

                Log.Error(exp.Message);
            }
        }


        // 查询易损件信息
        public static List<Consumables> SelectConsumables(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [易损件信息]";

            DataTable dt = db.ExecuteDataTable(updateSql);

            return MapDataTableToList<Consumables>(dt);
        }
        // 查询易损件信息
        public static List<Consumables> SelectConsumables(AccessMdbHelper db, string StationName)
        {
            string updateSql = @"SELECT * FROM [易损件信息] WHERE [易损件所在工位] = ?";

            var param = new OleDbParameter[]
            {
                new OleDbParameter ("易损件所在工位", OleDbType.VarWChar){  Value = StationName } ,
            };

            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql, param);

                return MapDataTableToList<Consumables>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("查询易损件信息失败", ex);
                return null;
            }
        }


        // 查询员工信息
        public static Consumables SelectConsumablesInfor(AccessMdbHelper db, string ProcessName, string Location, string Name)
        {
            string updateSql = @"SELECT * FROM [易损件信息]  WHERE [机台名称] = ?,[易损件所在位置] = ?,[易损件名称] = ?";

            var param = new OleDbParameter[]
            {
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = ProcessName } ,
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = Location } ,
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = Name } ,

            };

            DataTable dt = db.ExecuteDataTable(updateSql, param);

            if (dt.Rows.Count == 1)
            {
                DataRow row = dt.Rows[0];
                return new Consumables
                {
                    ID = Convert.ToInt32(row["ID"]),
                    ProcessName = row["易损件所在工位"].ToString(),
                    StationName = row["机台名称"].ToString(),
                    Location = row["易损件所在位置"].ToString(),
                    Name = row["易损件名称"].ToString(),
                    TheoreticalCount = Convert.ToInt16(row["易损件理论使用次数"]),
                    UsedCount = Convert.ToInt16(row["易损件已使用次数"]),
                    RemainderCount = Convert.ToInt16(row["易损件剩余使用次数"])
                };
            }
            else
            {
                return null;
            }

        }


        // 删除易损件信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static int DeleteConsumableInfor(AccessMdbHelper db, Consumables consumables)
        {
            try
            {
                // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
                string updateSql = @"
                                DELETE FROM [易损件信息]
                                WHERE [ID] = ?";

                var param = new[]
                {
                    new OleDbParameter("ID", OleDbType.Integer) { Value = consumables.ID }
                };

               return  db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return 0;
            }

        }



        #endregion

        #region 报警表

        public static List<AlaemParse> SelectProcessAlarmParse(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [GTProcessAlarmParse]";

            DataTable dt = db.ExecuteDataTable(updateSql);

            return MapDataTableToList<AlaemParse>(dt);
        }

        //  插入报警信息
        public static void InsertAlarmInfor(AccessMdbHelper db, AlarmInfo alarmInfo)
        {
            try
            {
                string insertSql = @"
                               INSERT INTO [故障信息] (
                                                     [故障发生工位], [机台名称], [故障类型],
                                                     [故障描述], [发生时间], [结束时间],[更新标识]
                                                    ) VALUES (?, ?, ?, ?, ?, ?, ?)";

                var parameters = new[]
                {
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.ProcessName ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.AlarmStation ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.AlarmGrade ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.Description ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.CreateTime ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.CloseTime ?? "" },
                    new OleDbParameter{ OleDbType = OleDbType.VarWChar, Value = "F" }
                };

                db.ExecuteNonQuery(insertSql, parameters);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"插入故障信息失败", ex, true);
            }
        }


        #endregion

        #region 服务器时间

        public static string GetNetworkTime(AccessMdbHelper db)
        {
            // 这个方法实际上获取的是服务器时间
            string updateSql = @"SELECT Format(Date(), ""yyyy年mm月"") AS ServerDate";
            DataTable dt = db.ExecuteDataTable(updateSql);
            return dt.Rows[0]["ServerDate"].ToString();
        }

        #endregion

        #region 客户端IP配置

        public static List<ProcessClient> SelectProcessClients(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [GTClientInfor]";

            DataTable dt = db.ExecuteDataTable(updateSql);

            return MapDataTableToList<ProcessClient>(dt);
        }

        #endregion

        #region 私有方法

        //  反射
        private static List<T> MapDataTableToList<T>(DataTable table) where T : new()
        {
            var list = new List<T>();
            var props = typeof(T).GetProperties();

            foreach (DataRow row in table.Rows)
            {
                T obj = new T();

                foreach (var prop in props)
                {
                    var displayNameAttr = prop.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                                              .FirstOrDefault() as DisplayNameAttribute;

                    string columnName = displayNameAttr != null ? displayNameAttr.DisplayName : prop.Name;

                    if (table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                    {
                        prop.SetValue(obj, Convert.ChangeType(row[columnName], prop.PropertyType));
                    }
                }

                list.Add(obj);
            }

            return list;
        }

        #endregion

        #region SQL SERVER

        //  查询产品型号
        public static List<CodeRule.CodeRule> SelectCodeRule()
        {
            var rules = new List<CodeRule.CodeRule>();

            string sqlStr = $"SELECT * FROM GTCodeRule ORDER BY RuleName";
            DataTable dt = new DataTable();
            try
            {
                dt = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr).Tables[0];
                return MapDataTableToList<CodeRule.CodeRule>(dt);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
                return null;
            }
        }

        //  报警解析
        public static void  UploadAlarmParse(DataGridViewRow row)
        {
            string sql = @"
                        IF NOT EXISTS(SELECT * FROM GTProcessAlarmParse 
                                     WHERE process_no = @process_no 
                                     AND plc_address = @plc_address 
                                     AND station_no = @station_no)
                        BEGIN
                            INSERT INTO GTProcessAlarmParse(process_no, plc_address, alarm_description, alarm_grade，station_no) 
                            VALUES(@process_no,@plc_address, @alarm_description, @alarm_grade, @station_no)
                        END
                        ELSE
                        BEGIN
                            UPDATE GTProcessAlarmParse 
                            SET alarm_description = @alarm_description 
                            WHERE process_no = @process_no 
                            AND plc_address = @plc_address 
                            AND station_no = @station_no
                        END";

            var parameters = new[]
            {
                new SqlParameter("@process_no", row.Cells["process_no"].Value.ToString()),
                new SqlParameter("@plc_address", row.Cells["plc_address"].Value.ToString()),
                new SqlParameter("@alarm_description", row.Cells["alarm_parse"].Value.ToString()),
                new SqlParameter("@alarm_grade", row.Cells["alarm_grade"].Value.ToString()),
                new SqlParameter("@station_no", row.Cells["alarm_station"].Value.ToString())
            };

            try
            {
                MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql, parameters);
            }
            catch (Exception exp)
            {
                Log.Error(exp.Message);
            }
        }


        #endregion

    }
}
