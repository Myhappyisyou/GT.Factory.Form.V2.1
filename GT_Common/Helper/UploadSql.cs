using GT_Common.Helper;
using GT_Common.ProcessConfig;
using GT_Common.Helper.Alarms;
using GT_Common.Helper.Mssql;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GT_Common.Helper.Logging;

namespace GT_Common
{
    public static class UploadSql
    {
        #region access

        #region 用户相关操作
        //  插入员工记录
        public static void Ac_InsertOrUpdateUser(AccessMdbHelper db, User user)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
            string updateSql = @"

                                UPDATE [Users]
                                
                                SET
                                
                                [登录次数] = [登录次数] + 1,
                                
                                [最后登录时间] = Format(Now(), ""YYYY-MM-DD HH:MM:SS"")
                                
                                WHERE [厂牌UID值] = ?";

            var param = new[]
            {
               new OleDbParameter("厂牌UID值", OleDbType.VarWChar) { Value = user.UID }
            };
            int rowsAffected = 0;
            try
            {
                rowsAffected = db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户登录信息更新失败", ex);
                return;
            }

            // 2. 如果未更新到记录（说明ID不存在），则插入新记录
            if (rowsAffected == 0) Ac_InsertNewUser(db, user);
        }

        //  插入新员工登录信息
        public static void Ac_InsertNewUser(AccessMdbHelper db, User user)
        {
            string insertSql = @"
                               INSERT INTO [UsersInfor] (
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
            try
            {
                db.ExecuteNonQuery(insertSql, parameters);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户登录信息插入失败", ex);
            }
        }

        // 查询员工信息
        public static User Ac_SelectUsersInforByUID(AccessMdbHelper db, string UID)
        {
            string updateSql = @"SELECT * FROM [UsersInfor] WHERE [厂牌UID] = ?";

            var param = new OleDbParameter[] { new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = UID } };
            DataTable dt;
            try
            {
                dt = db.ExecuteDataTable(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户查询失败", ex, true);
                return null;
            }

            if (dt.Rows.Count == 1)
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
        public static User Ac_SelectUsersInforByJobNub(AccessMdbHelper db, string JobNub,string PassWord)
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
        public static List<User> Ac_SelectUsersInfors(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [UsersInfor]";
            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql);

                return MapDataTableToList<User>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户登录信息插入失败", ex);
                return new List<User>();
            }
          
        }

        //  插入新员工信息
        public static void Ac_InsertNewUserInfor(AccessMdbHelper db, User user)
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
            catch (Exception ex)
            {
                DisplayLog.Error($"插入新员工信息失败", ex);
            }
        }

        // 更新员工信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void Ac_UpdateUsersInfor(AccessMdbHelper db, User user)
        {
            try
            {
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
            catch (Exception ex)
            {
                DisplayLog.Error($"更新员工信息失败", ex);
            }
        }

        // 删除员工信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void Ac_DeleteUsersInfor(AccessMdbHelper db, User user)
        {
            try
            {
                string updateSql = @"
                                DELETE FROM [UsersInfor]
                                WHERE [工号] = ?";

                var param = new[]
                {
                    new OleDbParameter("工号", OleDbType.VarChar) { Value = user.JobNub }
                };

                db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"删除员工信息失败", ex);
            }
        }

        #endregion

        #region 插入测试数据

        //  插入测试数据
        public static void Ac_InsertProcessProperty(
                AccessMdbHelper db,
                string process_no,
                string order_no,
                string bar_no,
                string operator_no,
                string[] ok_flag,
                string test_beat,
                string[] test_item,
                string[] test_item_up,
                string[] test_item_down,
                string[] test_item_value
            )
        {
            if (string.IsNullOrWhiteSpace(bar_no))
            {
                DisplayLog.Info("条码为空，无法保存记录");
                return;
            }

            if (test_item_value == null || test_item_value.Length == 0)
            {
                DisplayLog.Info("测试项为空，插入取消");
                return;
            }

            try
            {
                using (var tran = db.BeginTransaction())
                {
                    for (int i = 0; i < test_item_value.Length; i++)
                    {
                        string sql = @"
                                        INSERT INTO [生产信息] 
                                        ([工位名称], [当前工单号], [产品条码], [操作人员], [测试时间], [测试结果], [测试节拍], [测试项名称], [测试项上限], [测试项下限], [测试项实际值], [更新标识]) 
                                        VALUES (?, ?, ?, ?, Format(Now(), 'yyyy-mm-dd hh:nn:ss'), ?, ?, ?, ?, ?, ?, ?)";

                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter { Value = process_no },
                            new OleDbParameter { Value = order_no },
                            new OleDbParameter { Value = bar_no },
                            new OleDbParameter { Value = operator_no },
                            new OleDbParameter { Value = ok_flag[i] },
                            new OleDbParameter { Value = test_beat },
                            new OleDbParameter { Value = test_item[i] },
                            new OleDbParameter { Value = test_item_up[i] },
                            new OleDbParameter { Value = test_item_down[i] },
                            new OleDbParameter { Value = test_item_value[i] },
                            new OleDbParameter { Value = "F" }
                        };
                        int affected = tran.ExecuteNonQuery(sql, parameters);
                    }

                    tran.Commit(); // 提交事务
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码{bar_no}插入数据失败",ex);
            }
        }

        //  插入测试数据 带返回插入结果
        public static bool Ac_InsertProcessPropertyWithResult(
                AccessMdbHelper db,
                string process_no,
                string order_no,
                string bar_no,
                string operator_no,
                string do_time,
                string[] ok_flag,
                string test_beat,
                string[] test_item,
                string[] test_item_up,
                string[] test_item_down,
                string[] test_item_value
                )
        {
            int insertedCount = 0;

            if (string.IsNullOrWhiteSpace(bar_no))
            {
                DisplayLog.Info("条码为空，插入取消");
                return false;
            }

            if (test_item_value == null || test_item_value.Length == 0)
            {
                DisplayLog.Info("测试项为空，插入取消");
                return false;
            }

            try
            {
               
                using (var tx = DbContext.CurrentDb.BeginTransaction())
                {
                    for (int i = 0; i < test_item_value.Length; i++)
                    {
                        string sql = @"INSERT INTO [生产信息]
                                    ([工位名称],[当前工单号],[产品条码],[操作人员],[测试时间],
                                    [测试结果],[测试节拍],[测试项名称],[测试项上限],
                                    [测试项下限],[测试项实际值],[更新标识])
                                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter("?", process_no),
                            new OleDbParameter("?", order_no),
                            new OleDbParameter("?", bar_no),
                            new OleDbParameter("?", operator_no),
                            new OleDbParameter("?", do_time),
                            new OleDbParameter("?", ok_flag[i]),
                            new OleDbParameter("?", test_beat),
                            new OleDbParameter("?", test_item[i]),
                            new OleDbParameter("?", test_item_up[i]),
                            new OleDbParameter("?", test_item_down[i]),
                            new OleDbParameter("?", test_item_value[i]),
                            new OleDbParameter("?", "F"),
                        };

                        int affected = tx.ExecuteNonQuery(sql, parameters);
                        insertedCount += affected;
                    }

                    if (insertedCount == test_item_value.Length)
                    {
                        // 提交事务
                        tx.Commit();
                        DisplayLog.Info($"{bar_no}数据写入Access成功");

                        return true;
                    }
                    else
                    {
                        tx.Rollback();
                        DisplayLog.Error($"{bar_no}只插入了 {insertedCount} 条，期望 {test_item_value.Length} 条，事务已回滚。",null);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"{bar_no}数据写入失败: {ex.Message}", ex);
                return false;
            }
        }


        #endregion

        #region 易损件

        //  更新易损件记录
        public static void Ac_UpdateConsumables(AccessMdbHelper db, Consumables  consumables)
        {
            string updateSql = @"

                                UPDATE [易损件信息]
                                
                                SET
                                
                                [易损件已使用次数] = [易损件已使用次数] + 1,
                                
                                [易损件剩余使用次数] = [易损件剩余使用次数] - 1
                                
                                WHERE 
                                [易损件所在工位] = ? AND 
                                [机台名称] = ? AND 
                                [易损件所在位置] = ? AND 
                                [易损件名称] = ?";

            var param = new[]
            {
                    new OleDbParameter("易损件所在工位", OleDbType.VarWChar) { Value = consumables.ProcessName },

                    new OleDbParameter("机台名称", OleDbType.VarWChar) { Value = consumables.StationName },
                    new OleDbParameter("易损件所在位置", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarWChar) { Value = consumables.Name }
            };

            try
            {
                int rowsAffected = db.ExecuteNonQuery(updateSql, param);

                // 2. 如果未更新到记录（说明ID不存在），则插入新记录

                if (rowsAffected == 0) Ac_InsertConsumables(db, consumables);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("更新易损件记录失败", ex);
            }
        }


        public static void InsertOrUpdateConsumablesInfor(AccessMdbHelper db, Consumables consumables)
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
                if (rowsAffected == 0) Ac_InsertConsumables(db, consumables);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 数据库操作失败: {ex.Message}");
                Console.WriteLine($"异常类型: {ex.GetType().Name}");
            }
        }


        //  插入易损件信息
        public static void Ac_InsertConsumables(AccessMdbHelper db, Consumables consumables)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
          
            string updateSql = @"

                                INSERT INTO [易损件信息]
                                ([易损件所在工位],[机台名称],[易损件所在位置],[易损件名称],[易损件理论使用次数],[易损件已使用次数],[易损件剩余使用次数])
                                
                                VALUES (?, ?, ?, ?, ?, ?, ?)";

            var param = new[]
            {
                    new OleDbParameter("易损件所在工位", OleDbType.VarWChar) { Value = consumables.ProcessName },
                    new OleDbParameter("机台名称", OleDbType.VarWChar) { Value = consumables.StationName },
                    new OleDbParameter("易损件所在位置", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarWChar) { Value = consumables.Name },
                    new OleDbParameter("易损件理论使用次数", OleDbType.VarWChar) { Value = consumables.TheoreticalCount },
                    new OleDbParameter("易损件已使用次数", OleDbType.VarWChar) { Value = consumables.UsedCount },
                    new OleDbParameter("易损件剩余使用次数", OleDbType.VarWChar) { Value = consumables.RemainderCount },
            };
            try
            {
                int rowsAffected = db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("插入易损件信息失败", ex);
            }
        }

        // 查询易损件信息
        public static List<Consumables> Ac_SelectConsumables(AccessMdbHelper db,string StationName)
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
            catch (Exception ex )
            {
                DisplayLog.Error("查询易损件信息失败", ex);
                return null;
            }
        }

        // 查询易损件信息
        public static Consumables Ac_SelectConsumablesInfor(AccessMdbHelper db, string ProcessName, string Location, string Name)
        {
            string updateSql = @"SELECT * FROM [易损件信息]  WHERE [机台名称] = ?,[易损件所在位置] = ?,[易损件名称] = ?";

            var param = new OleDbParameter[]
            {
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = ProcessName } ,
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = Location } ,
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = Name } ,

            };
            try
            {
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
            catch (Exception ex)
            {
                DisplayLog.Error($"查询{Location}{Name}易损件失败", ex);
                return null;
            }
        }

        // 查询所有易损件信息
        public static List<Consumables> Ac_SelectAllConsumables(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [易损件信息]";

            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql);

                return MapDataTableToList<Consumables>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("查询易损件信息失败", ex);
                return null;
            }
        }

        // 删除易损件信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void Ac_DeleteConsumableInfor(AccessMdbHelper db, Consumables consumables)
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

                db.ExecuteNonQuery(updateSql, param);

                DisplayLog.Info($"删除易损件信息{consumables.ID}成功");

            }
            catch (Exception ex)
            {
                DisplayLog.Error($"删除易损件信息{consumables.ID}失败", ex);
                throw;
            }

        }

        #endregion


        #region 报警表

        //  查询报警解析
        public static List<AlarmParse> Ac_SelectProcessAlarmParse(AccessMdbHelper db,string process_no)
        {
            string updateSql = @"SELECT * FROM [GTProcessAlarmParse] WHERE [工序号] = ?";

            var param = new[]
            {
                 new OleDbParameter("工序号", OleDbType.VarWChar) { Value = process_no },
            };

            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql, param);

                return MapDataTableToList<AlarmParse>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("查询报警解析失败", ex);
                return null;
            }
        }

        //  插入报警信息
        public static void Ac_InsertAlarmInfor(AccessMdbHelper db, AlarmInfo alarmInfo)
        {
            try
            {
                string insertSql = @"
                               INSERT INTO [故障信息] (
                                                     [故障发生工位], [机台名称], [故障类型],
                                                     [故障描述], [发生时间], [结束时间]
                                                    ) VALUES (?, ?, ?, ?, ?, ?)";

                var parameters = new[]
                {
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.AlarmStation ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.ProcessName ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.AlarmGrade ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.Description ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.CreateTime ?? "" },
                    new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = alarmInfo.CloseTime ?? "" },
                };

                db.ExecuteNonQuery(insertSql, parameters);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"插入故障信息失败", ex);
            }
        }

        #endregion


        #region 客户端IP配置

        //  查询客户端IP信息
        public static List<ProcessClient> Ac_SelectProcessClients(AccessMdbHelper db)
        {
            string updateSql = @"SELECT * FROM [GTClientInfor]";

            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql);

                return MapDataTableToList<ProcessClient>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("查询客户端IP信息失败", ex);
                return null;
            }
        }

        #endregion


        #region 服务器时间

        public static string GetNetworkTime(AccessShortConnectionHelper db)
        {
            // 这个方法实际上获取的是服务器时间
            string updateSql = @"SELECT Format(Date(), ""yyyy年mm月"") AS ServerDate";
            DataTable dt = db.ExecuteDataTable(updateSql);
            return dt.Rows[0]["ServerDate"].ToString();
        }

        #endregion

        #region 查询

        /// <summary>
        /// 查询数据库（DataSet）
        /// </summary>
        public static DataSet SearchAccessData(string processNo, DateTime? startTime, DateTime? endTime,
            string okFlag, string filterColumn, string filterValue)
        {
            var db= AccessDbManager.GetCurrentDb();
            //AccessMdbHelper db;

            var value = MonthlyAccessDbManager.GetCurrentMonthDbPath(string.Format("{0:yyyy年MM月}", startTime));
            //db = new AccessMdbHelper(path);
            db.DatabasePath = value.dbPath;
            var sb = new StringBuilder();
            sb.Append($"SELECT * FROM [生产信息]  WHERE [工位名称] = ?");

            var ps = new List<OleDbParameter>
            {
                new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = processNo } ,
            };

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                sb.Append($" AND [{filterColumn}] = ?");
                ps.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = filterValue });
            }

            if (startTime.HasValue && endTime.HasValue)
            {
                sb.Append(" AND [测试时间] >= ? AND [测试时间] <= ?");
                ps.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", startTime) });
                ps.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", endTime) });
            }

            if (!string.IsNullOrEmpty(okFlag) && okFlag != "All")
            {
                sb.Append(" AND [测试结果] = ?");
                ps.Add(new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = okFlag });
            }

            //sb.Append(" ORDER BY [测试时间] DESC");

            sb.Append(" ORDER BY [ID] ");


            try
            {
                DataTable dt = db.ExecuteDataTable(sb.ToString(), ps.ToArray());

                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
                return ds;
            }
            catch (Exception ex)
            {
                DisplayLog.Error("生产数据查询失败",ex);
                return null;
            }
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

        #endregion



        #region access 短链接


        #region 报警表

        //  查询报警解析
        public static List<AlarmParse> Ac_Server_SelectProcessAlarmParse(AccessShortConnectionHelper db, string process_no)
        {
            string updateSql = @"SELECT * FROM [GTProcessAlarmParse] WHERE [工序号] = ?";

            var param = new[]
            {
                 new OleDbParameter("工序号", OleDbType.VarWChar) { Value = process_no },
            };

            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql, param);

                return MapDataTableToList<AlarmParse>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("查询报警解析失败", ex);
                return null;
            }
        }

        //  插入报警信息
        public static void Ac_Server_InsertAlarmInfor(AccessShortConnectionHelper db, AlarmInfo alarmInfo)
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
                DisplayLog.Error($"插入故障信息失败", ex);
            }
        }

        #endregion

        #region 用户相关操作
        //  插入员工记录
        public static void Ac_Server_InsertOrUpdateUser(AccessShortConnectionHelper db, User user)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）
            string updateSql = @"

                                UPDATE [Users]
                                
                                SET
                                
                                [登录次数] = [登录次数] + 1,
                                
                                [最后登录时间] = Format(Now(), ""YYYY-MM-DD HH:MM:SS"")
                                
                                WHERE [厂牌UID值] = ?";

            var param = new[]
            {
               new OleDbParameter("厂牌UID值", OleDbType.VarWChar) { Value = user.UID }
            };
            int rowsAffected = 0;
            try
            {
                rowsAffected = db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户登录信息更新失败", ex);
                return;
            }

            // 2. 如果未更新到记录（说明ID不存在），则插入新记录
            if (rowsAffected == 0) Ac_Server_InsertNewUser(db, user);
        }

        //  插入新员工登录信息
        public static void Ac_Server_InsertNewUser(AccessShortConnectionHelper db, User user)
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
            try
            {
                db.ExecuteNonQuery(insertSql, parameters);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户登录信息插入失败", ex);
            }
        }

        // 查询员工信息
        public static User Ac_Server_SelectUsersInforByUID(AccessShortConnectionHelper db, string UID)
        {
            string updateSql = @"SELECT * FROM [UsersInfor] WHERE [厂牌UID] = ?";

            var param = new OleDbParameter[] { new OleDbParameter { OleDbType = OleDbType.VarWChar, Value = UID } };
            DataTable dt;
            try
            {
                dt = db.ExecuteDataTable(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户查询失败", ex, true);
                return null;
            }

            if (dt.Rows.Count == 1)
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
        public static User Ac_Server_SelectUsersInforByJobNub(AccessShortConnectionHelper db, string JobNub, string PassWord)
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
        public static List<User> Ac_Server_SelectUsersInfors(AccessShortConnectionHelper db)
        {
            string updateSql = @"SELECT * FROM [UsersInfor]";
            try
            {
                DataTable dt = db.ExecuteDataTable(updateSql);

                return MapDataTableToList<User>(dt);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"用户登录信息插入失败", ex);
                return new List<User>();
            }

        }

        //  插入新员工信息
        public static void Ac_Server_InsertNewUserInfor(AccessShortConnectionHelper db, User user)
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
            catch (Exception ex)
            {
                DisplayLog.Error($"插入新员工信息失败", ex);
            }
        }

        // 更新员工信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void Ac_Server_UpdateUsersInfor(AccessShortConnectionHelper db, User user)
        {
            try
            {
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
            catch (Exception ex)
            {
                DisplayLog.Error($"更新员工信息失败", ex);
            }
        }

        // 删除员工信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void Ac_Server_DeleteUsersInfor(AccessShortConnectionHelper db, User user)
        {
            try
            {
                string updateSql = @"
                                DELETE FROM [UsersInfor]
                                WHERE [工号] = ?";

                var param = new[]
                {
                    new OleDbParameter("工号", OleDbType.VarChar) { Value = user.JobNub }
                };

                db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"删除员工信息失败", ex);
            }
        }

        #endregion

        //  易损件

        public static void Ac_Server_UpdateConsumables(AccessShortConnectionHelper db, Consumables consumables)
        {
            string updateSql = @"

                                UPDATE [易损件信息]
                                
                                SET
                                
                                [易损件已使用次数] = [易损件已使用次数] + 1,
                                
                                [易损件剩余使用次数] = [易损件剩余使用次数] - 1
                                
                                WHERE 
                                [易损件所在工位] = ? AND 
                                [机台名称] = ? AND 
                                [易损件所在位置] = ? AND 
                                [易损件名称] = ?";

            var param = new[]
            {
                    new OleDbParameter("易损件所在工位", OleDbType.VarWChar) { Value = consumables.ProcessName },

                    new OleDbParameter("机台名称", OleDbType.VarWChar) { Value = consumables.StationName },
                    new OleDbParameter("易损件所在位置", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarWChar) { Value = consumables.Name }
            };

            try
            {
                int rowsAffected = db.ExecuteNonQuery(updateSql, param);

                // 2. 如果未更新到记录（说明ID不存在），则插入新记录

                if (rowsAffected == 0) Ac_Server_InsertConsumables(db, consumables);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("更新易损件记录失败", ex);
            }
        }


        public static void Ac_Server_InsertOrUpdateConsumablesInfor(AccessShortConnectionHelper db, Consumables consumables)
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
                if (rowsAffected == 0) Ac_Server_InsertConsumables(db, consumables);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 数据库操作失败: {ex.Message}");
                Console.WriteLine($"异常类型: {ex.GetType().Name}");
            }
        }


        //  插入易损件信息
        public static void Ac_Server_InsertConsumables(AccessShortConnectionHelper db, Consumables consumables)
        {
            // 1. 使用正确的 SQL 语法（方括号包裹中文/空格字段名）

            string updateSql = @"

                                INSERT INTO [易损件信息]
                                ([易损件所在工位],[机台名称],[易损件所在位置],[易损件名称],[易损件理论使用次数],[易损件已使用次数],[易损件剩余使用次数])
                                
                                VALUES (?, ?, ?, ?, ?, ?, ?)";

            var param = new[]
            {
                    new OleDbParameter("易损件所在工位", OleDbType.VarWChar) { Value = consumables.ProcessName },
                    new OleDbParameter("机台名称", OleDbType.VarWChar) { Value = consumables.StationName },
                    new OleDbParameter("易损件所在位置", OleDbType.VarWChar) { Value = consumables.Location },
                    new OleDbParameter("易损件名称", OleDbType.VarWChar) { Value = consumables.Name },
                    new OleDbParameter("易损件理论使用次数", OleDbType.VarWChar) { Value = consumables.TheoreticalCount },
                    new OleDbParameter("易损件已使用次数", OleDbType.VarWChar) { Value = consumables.UsedCount },
                    new OleDbParameter("易损件剩余使用次数", OleDbType.VarWChar) { Value = consumables.RemainderCount },
            };
            try
            {
                int rowsAffected = db.ExecuteNonQuery(updateSql, param);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("插入易损件信息失败", ex);
            }
        }

        // 查询易损件信息
        public static List<Consumables> Ac_Server_SelectConsumables(AccessShortConnectionHelper db, string StationName)
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


        //  插入测试数据 带返回插入结果
        public static bool Ac_Server_InsertProcessPropertyWithResult(
                AccessShortConnectionHelper db,
                string process_no,
                string order_no,
                string bar_no,
                string operator_no,
                string do_time,
                string[] ok_flag,
                string test_beat,
                string[] test_item,
                string[] test_item_up,
                string[] test_item_down,
                string[] test_item_value
                )
        {

            if (string.IsNullOrWhiteSpace(bar_no))
            {
                DisplayLog.Info("条码为空，插入取消");
                return false;
            }

            if (test_item_value == null || test_item_value.Length == 0)
            {
                DisplayLog.Info("测试项为空，插入取消");
                return false;
            }

            try
            {
                string sql = @"INSERT INTO [生产信息]
                                    ([工位名称],[当前工单号],[产品条码],[操作人员],[测试时间],
                                    [测试结果],[测试节拍],[测试项名称],[测试项上限],
                                    [测试项下限],[测试项实际值],[更新标识])
                                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                List<OleDbParameter[]> batchParams = new List<OleDbParameter[]>();

                for (int i = 0; i < test_item_value.Length; i++)
                    {
                      
                        var parameters = new OleDbParameter[]
                        {
                            new OleDbParameter("?", process_no),
                            new OleDbParameter("?", order_no),
                            new OleDbParameter("?", bar_no),
                            new OleDbParameter("?", operator_no),
                            new OleDbParameter("?", do_time),
                            new OleDbParameter("?", ok_flag[i]),
                            new OleDbParameter("?", test_beat),
                            new OleDbParameter("?", test_item[i]),
                            new OleDbParameter("?", test_item_up[i]),
                            new OleDbParameter("?", test_item_down[i]),
                            new OleDbParameter("?", test_item_value[i]),
                            new OleDbParameter("?", "F"),
                        };
                    batchParams.Add(parameters);
                }

                // 一次提交所有
                db.ExecuteBatchNonQuery(sql, batchParams);

                return true;

            }
            catch (Exception ex)
            {
                DisplayLog.Info($"插入失败: {ex.Message}");
                return false;
            }
        }


        // 删除易损件信息 DELETE FROM Employees WHERE DepartmentID = 5;
        public static void Ac_Server_DeleteConsumableInfor(AccessShortConnectionHelper db, Consumables consumables)
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

                db.ExecuteNonQuery(updateSql, param);
                DisplayLog.Info($"删除{consumables.ID}易损件信息成功");

            }
            catch (Exception ex)
            {
                DisplayLog.Error($"删除{consumables.ID}易损件信息失败", ex);
                throw;
            }
        }

        #endregion

        ///  Sql Server

        #region 
        /// <summary>
        /// 上传基础数据
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="ok_flag"></param>
        /// <param name="ng_msg"></param>
        /// <param name="bar_no"></param>
        /// <param name="process_data"></param>
        public static Task<bool> UploadBasicData(UpdateSql updateSql,string process_no, string ok_flag, string ng_msg, string bar_no, params string[] process_data)
        {
            return Task.Run(() =>
            {
                string sqlStr1 = $"Insert into GTProcessProperty (bar_no,process_no,do_time,vou_no,ok_flag,ng_msg,user_id,flag,eqpt_loc_id,major_state,second_state,aux_state,{GenerateFieldStr1(process_data)}) " +
                    $"values ('{bar_no}','{process_no}',GETDATE(),'0','{ok_flag}','{ng_msg}','','','','0','0','0',{TransArrayToStr(process_data)})";
               
                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sqlStr1}");
                    DisplayLog.Warn($"Upload skipped: bar_no is empty.");
                    return false;
                }
                try
                {
                    // 生成字段名和值参数（@p0,@p1...）
                    var fieldStr = GenerateFieldStr1(process_data); // 如：data1,data2,...
                    var paramNames = string.Join(",", process_data.Select((_, i) => $"@p{i}")); // @p0,@p1,...
                    var sql = $"INSERT INTO GTProcessProperty (bar_no, process_no, do_time, vou_no, ok_flag, ng_msg, user_id, flag, eqpt_loc_id, major_state, second_state, aux_state, {fieldStr}) " +
                              $"VALUES (@bar_no, @process_no, GETDATE(), '0', @ok_flag, @ng_msg, '', '', '', '0', '0', '0', {paramNames})";

                    // 参数构造
                    var paramList = new List<SqlParameter>
                    {
                        new SqlParameter("@bar_no", bar_no),
                        new SqlParameter("@process_no", process_no),
                        new SqlParameter("@ok_flag", ok_flag),
                        new SqlParameter("@ng_msg", ng_msg)
                    };

                    for (int i = 0; i < process_data.Length; i++)
                    {
                        paramList.Add(new SqlParameter($"@p{i}", process_data[i]));
                    }
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql, paramList.ToArray());

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                    return true; // ✅ 写入成功;

                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败",ex);
                    SqlDataItem item = new SqlDataItem()
                    {
                        Sn = process_no,
                        Process = process_no,
                        SqlData = sqlStr1
                    };
                    updateSql.Update(item);
                    return false; // ❌ 写入失败
                }
            });
        }

        /// <summary>
        /// 上传基础数据
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="ok_flag"></param>
        /// <param name="ng_msg"></param>
        /// <param name="bar_no"></param>
        /// <param name="process_data"></param>
        public static Task<bool> UploadBasicData1(UpdateSql updateSql, string bar_no, string process_no,string vou_no, string ok_flag, string ng_msg, string user_id, params string[] process_data)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    return false;
                }
                try
                {
                    // 生成字段名和值参数（@p0,@p1...）
                    var fieldStr = GenerateFieldStr1(process_data); // 如：data1,data2,...
                    var paramNames = string.Join(",", process_data.Select((_, i) => $"@p{i}")); // @p0,@p1,...
                    var sql = $"INSERT INTO GTProcessProperty (bar_no, process_no, do_time, vou_no, ok_flag, ng_msg, user_id, flag, eqpt_loc_id, major_state, second_state, aux_state, {fieldStr}) " +
                              $"VALUES (@bar_no, @process_no, GETDATE(), @vou_no, @ok_flag, @ng_msg, @user_id, '', '', '0', '0', '0', {paramNames})";

                    // 参数构造
                    var paramList = new List<SqlParameter>
                    {
                        new SqlParameter("@bar_no", bar_no),
                        new SqlParameter("@process_no", process_no),
                        new SqlParameter("@vou_no", vou_no ?? "0"),
                        new SqlParameter("@ok_flag", (object)ok_flag ?? "NG"),
                        new SqlParameter("@ng_msg", (object)ng_msg?? "NG"),
                        new SqlParameter("@user_id", (object)user_id?? "NG"),
                    };

                    for (int i = 0; i < process_data.Length; i++)
                    {
                        paramList.Add(new SqlParameter($"@p{i}", process_data[i]));
                    }

                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql, paramList.ToArray());

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");

                    return true;
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);

                    //SqlDataItem item = new SqlDataItem()
                    //{
                    //    Sn = process_no,
                    //    Process = process_no,
                    //    SqlData = ex.Message
                    //};
                    //updateSql.Update(item);

                    return false;
                }
            });
        }


        #region 特殊版本，上限，下限，测试值，结果。

        public static Task<bool> UploadBasicData1(UpdateSql updateSql,
        string bar_no, string process_no, string vou_no, string ok_flag,
        string ng_msg, string user_id, DataItem dataItem)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(bar_no) || dataItem == null)
                {
                    return false;
                }

                try
                {
                    // 取出四类数组（可能为 null）
                    var data = dataItem.Data ?? Array.Empty<string>();
                    var dataUp = dataItem.Data_up ?? Array.Empty<string>();
                    var dataDown = dataItem.Data_down ?? Array.Empty<string>();
                    var dataResult = dataItem.Data_result ?? Array.Empty<string>();

                    // 按索引顺序合并，单个索引的顺序为：Data_up, Data_down, Data, Data_result
                    int maxLen = Math.Max(Math.Max(data.Length, dataUp.Length), Math.Max(dataDown.Length, dataResult.Length));
                    var allValues = new List<string>(capacity: maxLen * 4);
                    for (int i = 0; i < maxLen; i++)
                    {
                        allValues.Add(GetAtOrEmpty(dataUp, i));
                        allValues.Add(GetAtOrEmpty(dataDown, i));
                        allValues.Add(GetAtOrEmpty(data, i));
                        allValues.Add(GetAtOrEmpty(dataResult, i));
                    }

                    // 生成字段名与参数占位
                    string fieldStr = GenerateFieldStr1(allValues.Count); // data001,data002,...
                    string paramNames = string.Join(",", allValues.Select((_, i) => $"@p{i}"));

                    string sql =
                        $"INSERT INTO GTProcessProperty (" +
                        $"bar_no, process_no, do_time, vou_no, ok_flag, ng_msg, user_id, " +
                        $"flag, eqpt_loc_id, major_state, second_state, aux_state, {fieldStr}) " +
                        $"VALUES (@bar_no, @process_no, GETDATE(), @vou_no, @ok_flag, @ng_msg, @user_id, " +
                        $"'', '', '0', '0', '0', {paramNames})";

                    // 公共参数
                    var paramList = new List<SqlParameter>
                    {
                        new SqlParameter("@bar_no", bar_no),
                        new SqlParameter("@process_no", process_no),
                        new SqlParameter("@vou_no", (object)vou_no ?? "0"),
                        new SqlParameter("@ok_flag", (object)ok_flag ?? "NG"),
                        new SqlParameter("@ng_msg", (object)ng_msg ?? (object)dataItem.NgMsg ?? "NG"),
                        new SqlParameter("@user_id", (object)user_id ?? "Unknown")
                    };

                    // 数据参数（空值用空字符串）
                    for (int i = 0; i < allValues.Count; i++)
                    {
                        paramList.Add(new SqlParameter($"@p{i}", allValues[i] ?? ""));
                    }

                    // 执行上传
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql, paramList.ToArray());
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                    // 可选：补偿写入 updateSql
                    // updateSql.Update(new SqlDataItem { Sn = bar_no, Process = process_no, SqlData = ex.Message });
                    return false;
                }
            });
        }

        // 辅助：安全读取数组索引
        private static string GetAtOrEmpty(string[] arr, int idx)
        {
            if (arr == null || idx < 0 || idx >= arr.Length) return "";
            return arr[idx] ?? "";
        }

        // 生成 data001 ... dataNN 字段名
        private static string GenerateFieldStr1(int count)
        {
            var list = new List<string>(count);
            for (int i = 1; i <= count; i++)
            {
                list.Add($"data{i:D3}");
            }
            return string.Join(",", list);
        }

        #endregion

        /// <summary>
        /// 上传异常再次上传
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="process_no"></param>
        /// <param name="sql"></param>
        public static void InsetSql(UpdateSql updateSql, string sn, string process_no, string sql)
        {
            //异步保存数据
            Task.Factory.StartNew(() =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start(); // 启动计时器

                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql);
                    DisplayLog.Info($"产品码--{sn}--数据上传成功");

                    SqlDataItem item = new SqlDataItem()
                    {
                        Sn = sn,
                        Process = process_no,
                        SqlData = sql
                    };
                    updateSql.Deletedate(item);
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{process_no}--数据上传失败", ex);

                    SqlDataItem item = new SqlDataItem()
                    {
                        Sn = sn,
                        Process = process_no,
                        SqlData = sql
                    };
                    updateSql.Update(item);
                }
                finally
                {
                    stopwatch.Stop(); // 停止计时器
                }
            });
        }
        /// <summary>
        /// 点检数据
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="ok_flag"></param>
        /// <param name="ng_msg"></param>
        /// <param name="bar_no"></param>
        /// <param name="test_item"></param>
        /// <param name="process_data"></param>
        public static void UploadCalibrationTestData(string process_no, string ok_flag, string ng_msg, string bar_no,string test_item, params string[] process_data)
        {
            Task.Factory.StartNew(() =>
            {                
                //Serilog.Log.Information(sqlStr);
                string sqlStr = $"Insert into SHProcessPropertyCalibration (bar_no,process_no,do_time,vou_no,ok_flag,ng_msg,test_item,user_id,flag,eqpt_loc_id,major_state,second_state,aux_state,{GenerateFieldStr1(process_data)}) " +
                    $"values ('{bar_no}','{process_no}',GETDATE(),'0','{ok_flag}','{ng_msg}','{test_item}','','','','0','0','0',{TransArrayToStr(process_data)})";

                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                    DisplayLog.Info($"产品码--{bar_no}--点检项--{test_item}--数据上传成功");

                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--点检项--{test_item}--数据上传失败",ex);
                }
            });
        }

        /// <summary>
        /// 零件码绑定
        /// </summary>
        /// <param name="bar_no"></param>
        /// <param name="process_no"></param>
        /// <param name="part1_name"></param>
        /// <param name="part2_name"></param>
        /// <param name="part1_bar"></param>
        /// <param name="part2_bar"></param>
        public static void UploadPartNumber(string bar_no, string process_no, string part1_name, string part2_name, string part1_bar, string part2_bar)
        {
            Task.Run(() =>
            {
                string sqlStr1 = $"IF NOT EXISTS(SELECT * FROM GTProcessPart WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{ part1_name }')" +
                $" INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) VALUES('{ bar_no}', '{ process_no}', '{ part1_name}', '{ part1_bar}') ELSE " +
                $" UPDATE GTProcessPart SET part_bar = '{ part1_bar}' WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{ part1_name }'";

                string sqlStr2 = $"IF NOT EXISTS(SELECT * FROM GTProcessPart WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{ part2_name }')" +
                $" INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) VALUES('{ bar_no}', '{ process_no}', '{ part2_name}', '{ part2_bar}') ELSE " +
                $" UPDATE GTProcessPart SET part_bar = '{ part2_bar}' WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{ part2_name }'";
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr1);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr2);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }

        /// <summary>
        /// 零件码绑定
        /// </summary>
        /// <param name="bar_no"></param>
        /// <param name="process_no"></param>
        /// <param name="part1_name"></param>
        /// <param name="part2_name"></param>
        /// <param name="part3_name"></param>
        /// <param name="part1_bar"></param>
        /// <param name="part2_bar"></param>
        /// <param name="part3_bar"></param>
        public static void UploadPartNumber(string bar_no, string process_no, string part1_name, string part2_name, string part3_name, string part1_bar, string part2_bar, string part3_bar)
        {
            Task.Factory.StartNew(() =>
            {

                string sqlStr1 = $"IF NOT EXISTS(SELECT * FROM GTProcessPart WHERE main_bar = '{ bar_no }' AND process_no = '{process_no}' AND part_name = '{part1_name}')" +
                $" INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) VALUES('{ bar_no}', '{ process_no}', '{part1_name}', '{ part1_bar}') ELSE " +
                $" UPDATE GTProcessPart SET part_bar = '{ part1_bar}' WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{part1_name }'";

                string sqlStr2 = $"IF NOT EXISTS(SELECT * FROM GTProcessPart WHERE main_bar = '{ bar_no }' AND process_no = '{process_no}' AND part_name = '{part2_name}')" +
                $" INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) VALUES('{ bar_no}', '{process_no}', '{part2_name}', '{ part2_bar}') ELSE " +
                $" UPDATE GTProcessPart SET part_bar = '{ part2_bar}' WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{part2_name}'";

                string sqlStr3 = $"IF NOT EXISTS(SELECT * FROM GTProcessPart WHERE main_bar = '{ bar_no }' AND process_no = '{process_no }' AND part_name = '{part3_name}')" +
                $" INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) VALUES('{ bar_no}', '{ process_no}','{ part3_name}', '{ part3_bar}') ELSE " +
                $" UPDATE GTProcessPart SET part_bar = '{ part3_bar}' WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{part3_name}'";

                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr1);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");

                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr2);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");

                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr3);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");

                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar_no"></param>
        /// <param name="process_no"></param>
        /// <param name="part1_name"></param>
        /// <param name="part1_bar"></param>
        public static void UploadPartNumber(string bar_no, string process_no, string part_name, string part_bar)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"IF NOT EXISTS(SELECT * FROM GTProcessPart WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{ part_name }')" +
                $" INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) VALUES('{ bar_no}', '{ process_no}', '{ part_name}', '{ part_bar}') ELSE " +
                $" UPDATE GTProcessPart SET part_bar = '{ part_bar}' WHERE main_bar = '{ bar_no }' AND process_no = '{ process_no }' AND part_name = '{ part_name }'";

                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }

        public static Task UploadSinglePartAsync(string bar_no, string processNo, PartInfo part)
        {
            if (string.IsNullOrWhiteSpace(part?.Name) || string.IsNullOrWhiteSpace(part?.BarCode))
                throw new ArgumentException($"产品码{bar_no}零件信息不完整");

            string sql = @"
                        IF NOT EXISTS(SELECT id FROM GTProcessPart 
                                     WHERE main_bar = @mainBar 
                                     AND process_no = @processNo 
                                     AND part_name = @partName)
                        BEGIN
                            INSERT INTO GTProcessPart(main_bar, process_no, part_name, part_bar) 
                            VALUES(@mainBar, @processNo, @partName, @partBar)
                        END
                        ELSE
                        BEGIN
                            UPDATE GTProcessPart 
                            SET part_bar = @partBar 
                            WHERE main_bar = @mainBar 
                            AND process_no = @processNo 
                            AND part_name = @partName
                        END";

            var parameters = new[]
            {
                new SqlParameter("@mainBar", bar_no),
                new SqlParameter("@processNo", processNo),
                new SqlParameter("@partName", part.Name),
                new SqlParameter("@partBar", part.BarCode)
            };

            try
            {
                MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sql, parameters);
                DisplayLog.Info($"产品码--{bar_no}--{part.BarCode}--{part.Name} 绑定成功");
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{bar_no}--{part.BarCode}--{part.Name} 绑定失败", ex);
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// 查询绑定
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        /// <summary>
        /// 根据部件码查询最新绑定的主码
        /// </summary>
        public static string QueryMainBarByPartBar(string process_no, string part_bar)
        {
            string sql = @"
                            SELECT TOP 1 main_bar
                            FROM GTProcessPart
                            WHERE process_no = @process_no
                              AND part_bar = @part_bar
                            ORDER BY id DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(MSSqlHelper.Conn1))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@process_no", process_no));
                    cmd.Parameters.Add(new SqlParameter("@part_bar", part_bar));

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    DisplayLog.Info($"部件码--{part_bar}--查询主码{result}成功");

                    if (result == null || result == DBNull.Value)
                        return null;

                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"部件码--{part_bar}--查询主码失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 查询绑定
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        /// <summary>
        /// 根据部件码查询最新绑定的主码
        /// </summary>
        public static string QueryPartBarByMainBar(string process_no, string main_bar)
        {
            string sql = @"
                            SELECT TOP 1 part_bar
                            FROM GTProcessPart
                            WHERE process_no = @process_no
                              AND Main_bar = @main_bar
                            ORDER BY id DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(MSSqlHelper.Conn1))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@process_no", process_no));
                    cmd.Parameters.Add(new SqlParameter("@main_bar", main_bar));

                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    DisplayLog.Info($"主码--{main_bar}--查询主码{result}成功");

                    if (result == null || result == DBNull.Value)
                        return null;

                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"主码--{main_bar}--查询主码失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 上传接口调用
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="part_bar"></param>
        /// <param name="return_result"></param>
        /// <param name="feedback_msg"></param>
        public static void UploadInterfaceCall(string process_no, string part_bar, string return_result, string feedback_msg)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"insert into SHProcessInterfaceCall (process_no,part_bar,do_time,return_result,feedback_msg) values ('{process_no}','{part_bar}'," +
                $"GETDATE(),'{return_result}','{feedback_msg}')";
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                    DisplayLog.Info($"产品码--{part_bar}--数据上传成功");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{part_bar}--数据上传失败", ex);
                }
            });
        }

        /// <summary>
        /// 上传文件路径，包含图片，曲线
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="bar_no"></param>
        /// <param name="fileType"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        public static void UploadProcessFile(string process_no, string bar_no, string fileType, string fileName,string filePath)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"insert into GTProcessFile (bar_no,process_no,file_type,name,do_time,path,flag) values ('{bar_no}','{process_no}','{fileType}','{fileName}',GETDATE(),'{filePath}','0')";
                
                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sqlStr}");
                    return;
                }
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }

        /// <summary>
        /// 上传文件路径，包含图片，曲线
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="bar_no"></param>
        /// <param name="fileType"></param>
        /// <param name="fileName"></param>
        /// <param name="ok_flag"></param>
        /// <param name="ng_msg"></param>
        /// <param name="filePath"></param>
        public static void UpdateSHProcessFile(string process_no, string bar_no, string fileType, string fileName, string ok_flag, string ng_msg, string filePath)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"insert into GTProcessFile (bar_no,process_no,file_type,name,do_time,ok_flag,ng_msg,path,flag) values ('{bar_no}','{process_no}','{fileType}','{fileName}',GETDATE(),'{ok_flag}','{ng_msg}','{filePath}','0')";

                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sqlStr}");

                    return;
                }
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");

                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }


        /// <summary>
        /// 点检项
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="bar_no"></param>
        /// <param name="test_item_type"></param>
        /// <param name="test_item"></param>
        /// <param name="test_item_value"></param>
        /// <param name="do_time"></param>
        public static void UploadCalibration(string process_no, string bar_no, string test_item_type, string test_item, string test_item_value, string do_time)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"insert into GTProcessCalibration (bar_no,process_no,do_time,test_item,test_item_type,test_item_up,test_item_down,test_item_value,flag) " +
                                                          $"values ('{bar_no}','{process_no}','{do_time}','{test_item}','{test_item_type}','','','{test_item_value}','0')";

                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sqlStr}");

                    return;
                }
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }


        /// <summary>
        /// 批量上传三花点检/抽检表数据
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="bar_no"></param>
        /// <param name="process_no"></param>
        /// <param name="do_time"></param>
        /// <param name="test_item"></param>
        /// <param name="test_item_type"></param>
        /// <param name="test_item_up"></param>
        /// <param name="test_item_down"></param>
        /// <param name="test_item_value"></param>
        public static async Task<bool> UpdateSHPeriodOrCalibrationValues(string table_name, string bar_no, string process_no, string do_time,  string[] test_item, string[] test_item_type, string[] test_item_up, string[] test_item_down, string[] test_item_value)
        {
            //异步保存数据
            return await Task.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"insert into {table_name} (bar_no,process_no,do_time,test_item,test_item_type,test_item_up,test_item_down,test_item_value,flag) values ");

                List<string> list = new List<string>();

                for (int i = 0; i < test_item_value.Length; i++)
                {
                    string[] values = new string[] { $"{bar_no}", $"{process_no}", $"{do_time}", $"{test_item[i]}", $"{test_item_type[i]}", $"{test_item_up[i]}", $"{test_item_down[i]}", $"{test_item_value[i]}", "0" };
                    list.Add($"({string.Join(",", values.Select(t => $"'{t}'"))})");
                }

                sb.Append($"{string.Join(",", list.ToArray())}");
                sb.Append(';');

                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sb}");

                    return true;
                }
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sb.ToString());

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                    return false;
                }
            });
        }


        /// <summary>
        /// 批量上传三花点检/抽检表数据
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="bar_no"></param>
        /// <param name="process_no"></param>
        /// <param name="do_time"></param>
        /// <param name="test_item"></param>
        /// <param name="test_item_type"></param>
        /// <param name="test_item_up"></param>
        /// <param name="test_item_down"></param>
        /// <param name="test_item_value"></param>
        public static void UpdateSHPeriodOrCalibrationValues( string bar_no, string process_no, string do_time, List<SaveItem> saveItems)
        {
            //异步保存数据
            Task.Factory.StartNew(() =>
            {
                StringBuilder sb = new StringBuilder();

                sb.Append($"insert into GTProcessCalibration (bar_no,process_no,do_time,test_item,test_item_type,test_item_up,test_item_down,test_item_value,flag) values ");

                List<string> list = new List<string>();

                foreach (var item in saveItems)
                {
                    string[] values = new string[] { bar_no, process_no, do_time, item.Test_item_name, "Property", item.Test_item_up, item.Test_item_down, item.Test_item_value, item.Ok_flag };
                    list.Add($"({string.Join(",", values.Select(t => $"'{t}'"))})");
                }
                
                sb.Append($"{string.Join(",", list.ToArray())}");
                sb.Append(';');

                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sb}");

                    return true;
                }
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sb.ToString());

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                    return true;
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// 上传抽检
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="bar_no"></param>
        /// <param name="test_item_type"></param>
        /// <param name="test_item"></param>
        /// <param name="test_item_value"></param>
        /// <param name="do_time"></param>
        public static void UploadPeriodTestInspection(string process_no, string bar_no, string test_item_type, string test_item, string test_item_value, string do_time)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"insert into SHProcessPeriodTest (bar_no,process_no,do_time,test_item,test_item_type,test_item_up,test_item_down,test_item_value,flag) " +
                                                          $"values ('{bar_no}','{process_no}','{do_time}','{test_item}','{test_item_type}','','','{test_item_value}','0')";

                if (string.IsNullOrWhiteSpace(bar_no))
                {
                    DisplayLog.Info($"{sqlStr}");
                    return;
                }
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);

                    DisplayLog.Info($"产品码--{bar_no}--数据上传成功");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据上传失败", ex);
                }
            });
        }


        #endregion

        #region 查询语句

        /// <summary>
        /// 查询数据库（DataSet）
        /// </summary>
        public static DataSet SearchData(string processNo, DateTime? startTime, DateTime? endTime,
            string okFlag, string filterColumn, string filterValue)
        {
            // 动态字段（英文）
            string fields = GetDynamicFields(processNo);

            var sb = new StringBuilder();
            sb.Append($"SELECT bar_no, vou_no, ok_flag, ng_msg, do_time, user_id {fields} ");
            sb.Append("FROM GTProcessProperty WHERE process_no = @process_no ");

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@process_no", processNo)
            };

            if (!string.IsNullOrEmpty(filterColumn) && !string.IsNullOrEmpty(filterValue))
            {
                sb.Append($" AND {filterColumn} LIKE @filterValue");
                ps.Add(new SqlParameter("@filterValue", $"%{filterValue}%"));
            }

            if (startTime.HasValue && endTime.HasValue)
            {
                sb.Append(" AND do_time BETWEEN @start AND @end");
                ps.Add(new SqlParameter("@start", startTime.Value));
                ps.Add(new SqlParameter("@end", endTime.Value));
            }

            if (!string.IsNullOrEmpty(okFlag) && okFlag != "All")
            {
                sb.Append(" AND ok_flag = @ok_flag");
                ps.Add(new SqlParameter("@ok_flag", okFlag));
            }

            sb.Append(" ORDER BY do_time DESC");
            try
            {
                var ds = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sb.ToString(), ps.ToArray());

                return ds;
            }
            catch (Exception ex)
            {
                DisplayLog.Error("生产信息查询失败",ex);
                return null;
            }
        }

        

        /// <summary>
        /// 取动态字段（英文列名拼接字符串）“,data001,data002,...”
        /// </summary>
        private static string GetDynamicFields(string processNo)
        {
            string sql = @"
            SELECT ',' + CAST(field_name AS nvarchar(250))
            FROM GTProcessPropertyParse
            WHERE process_no = @process_no
            ORDER BY field_name FOR XML PATH('')";
            var ds = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sql,
                new SqlParameter("@process_no", processNo));
            return ds.Tables[0].Rows[0][0]?.ToString();
        }

        /// <summary>
        /// 返回 英文列名 -> 中文标题 映射
        /// </summary>
        public static Dictionary<string, string> GetColumnMap(string processNo)
        {
            // 固定列
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "bar_no",  "产品码" },
                { "vou_no",  "工单号" },
                { "ok_flag", "结果" },
                { "ng_msg",  "结果信息" },
                { "do_time", "测试时间" },
                { "user_id", "操作员" }
            };

            string sql = @"SELECT field_name, field_name_cn 
                       FROM GTProcessPropertyParse 
                       WHERE process_no = @process_no";
            var ds = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sql,
                new SqlParameter("@process_no", processNo));

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                string en = row["field_name"]?.ToString();
                string cn = row["field_name_cn"]?.ToString();
                if (!string.IsNullOrEmpty(en))
                {
                    map[en] = string.IsNullOrWhiteSpace(cn) ? en : cn;
                }
            }

            return map;
        }

        /// <summary>
        /// 查询工艺代码
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static string SelectNgCode(string bar_no)
        {
            string sqlStr = $"select ng_msg from GTProcessProperty where bar_no = '{bar_no}' order by do_time desc";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"产品码--{bar_no}--查询工艺数据--0");
                    return null;
                }
                else
                {
                    string NgCode = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"产品码--{bar_no}--查询工艺数据--{NgCode}");
                    return NgCode;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{bar_no}--工艺数据查询失败",ex);
                return "";
            }
        }

        /// <summary>
        /// 查询指定工序工艺代码
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static string SelectProcessNgCode(string bar_no, string process_no)
        {
            string sqlStr = $"select ng_msg from GTProcessProperty where bar_no = '{bar_no}' and process_no = '{process_no}' order by do_time desc";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"产品码--{bar_no}--查询--{process_no}--工艺数据--0");
                    return null;
                }
                else
                {
                    string NgCode = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"产品码--{bar_no}--查询--{process_no}--工艺数据--{NgCode}");
                    return NgCode;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{bar_no}--查询{process_no}--工艺数据失败", ex);
                return "";
            }
        }

        /// <summary>
        /// 查询点检配置项
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static CalibrationOrPeriodInspection SelectCalibrationItem(string inspection,string station_id)
        {
            CalibrationOrPeriodInspection calibrationOrPeriodInspection = new CalibrationOrPeriodInspection();
            List<string> LsTest_item = new List<string>();
            List<string> LsTest_item_type = new List<string>();
            List<string> LsTest_item_up = new List<string>();
            List<string> LsTest_item_down = new List<string>();
            string sqlStr = $"select test_item,inspection,test_item_type,test_item_up,test_item_down from SHProcessCalibrationInspection where inspection = '{inspection}' and station_id = '{station_id}'";
            DataSet dataSet = new DataSet();

            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"未查询到点检配置项");
                    return calibrationOrPeriodInspection;
                }
                else
                {
                    calibrationOrPeriodInspection.CalibrationOrPeriodStationId = station_id;
                    string NgCode = dataSet.Tables[0].Rows[0][0].ToString();
                    foreach (DataRow item in dataSet.Tables[0].Rows)
                    {
                        LsTest_item.Add(item["test_item"].ToString());
                        LsTest_item_type.Add(item["test_item_type"].ToString());
                        LsTest_item_up.Add(item["test_item_up"].ToString());
                        LsTest_item_down.Add(item["test_item_down"].ToString());
                    }
                    calibrationOrPeriodInspection.Test_item = LsTest_item.ToArray();
                    calibrationOrPeriodInspection.Test_item_type = LsTest_item_type.ToArray();
                    calibrationOrPeriodInspection.Test_item_up = LsTest_item_up.ToArray();
                    calibrationOrPeriodInspection.Test_item_down = LsTest_item_down.ToArray();
                    return calibrationOrPeriodInspection;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询点检配置项异常", ex);
                return calibrationOrPeriodInspection;
            }
        }

        /// <summary>
        /// 查询参数
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static List<ParamatesLimit> SelectParamatesLimit(string process_no)
        {
            string sqlStr = $"select up_limit,down_limit from GTProcessParamatesLimit where process_no = '{process_no}' ";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"未查询到参数配置");
                    return null;
                }
                else
                {
                    List<ParamatesLimit> list = new List<ParamatesLimit>();
                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        ParamatesLimit paramates = new ParamatesLimit();
                        paramates.Up_limit = Convert.ToDouble(dataSet.Tables[0].Rows[i][0].ToString());
                        paramates.Down_limit = Convert.ToDouble(dataSet.Tables[0].Rows[i][1].ToString());
                        list.Add(paramates);
                    }
                    DisplayLog.Info($"工序号:{process_no}--查询到参数配置");
                    return list;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询参数配置异常", ex);
                return null;
            }
        }

        /// <summary>
        /// 查询工艺代码
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static List<ResultCodeParse> SelectResultCodeParse(string process_no)
        {
            string sqlStr = $"select ng_code,ng_msg from GTProcessResultCodeParse where process_no = '{process_no}' ";
            DataSet dataSet = new DataSet();
            dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
            if (dataSet.Tables[0].Rows.Count == 0)
            {
                DisplayLog.Info($"工序号:{process_no}--未查询到工艺代码配置");
                return null;
            }
            else
            {
                List<ResultCodeParse> list = new List<ResultCodeParse>();
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    ResultCodeParse resultCodeParse = new ResultCodeParse()
                    {
                        Ng_code = row["ng_code"].ToString(),
                        Ng_msg = row["ng_code"] + "_" + row["ng_msg"].ToString(),
                    };
                    list.Add(resultCodeParse);
                }
               

                DisplayLog.Info($"工序号:{process_no}--查询到工艺代码配置");

                return list;
            }
        }

        #endregion

        #region 存储过程 生成 产品码

        /// <summary>
        /// 生成产品码
        /// </summary>
        /// <param name="process_no">工序号</param>
        /// <param name="product_code">产品代码</param>
        /// <param name="CodeRules">编码规则</param>
        /// <param name="origin_code">产地代码</param>
        public static string GenerateProductCode(string process_no, string LineNo, int ProductModel, int OP90_x, string ProductNo)
        {

            StringBuilder sb = new StringBuilder();
            DataSet dataSet = new DataSet();
            sb.Append("DECLARE @ProductCode VARCHAR(100);");
            sb.Append($"EXEC GTGenerateProductCode '{process_no}',{ProductModel},'{ProductNo}','{LineNo}',{OP90_x},@ProductCode OUTPUT;");
            sb.Append("SELECT @ProductCode;");

            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sb.ToString());
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    return "";
                }
                else
                {
                    var sn = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"生成产品码成功--{sn}");
                    return sn;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"生成产品码失败", ex);
                return "";
            }
        }

        #endregion

        /// <summary>
        /// 查询该产品码上传了几次 1返工0不返工
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static int QueryDataUploadCount(string process_no, string bar_no, int reworkCountLimit, out int reworkFlag, out int ngCode)
        {
            reworkFlag = 0;
            ngCode = 0;

            string sql = @"
                            SELECT 
                                COUNT(*) AS TotalCount,
                                MAX(ng_msg) AS LastNgMsg
                            FROM GTProcessProperty
                            WHERE process_no = @process_no
                              AND bar_no = @bar_no";

            try
            {
                using (SqlConnection conn = new SqlConnection(MSSqlHelper.Conn1))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@process_no", process_no));
                    cmd.Parameters.Add(new SqlParameter("@bar_no", bar_no));

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            DisplayLog.Warn($"产品码--{bar_no}--获取返工次数异常", true);

                            // 理论上不会发生
                            return 2;
                        }

                        int totalCount = reader.GetInt32(0);

                        if (totalCount == 0)
                        {
                            // 无历史记录
                            return 1;
                        }

                        string lastNgMsg = reader.IsDBNull(1) ? null : reader.GetString(1);

                        // 解析 NGCode
                        if (!string.IsNullOrEmpty(lastNgMsg))
                        {
                            string nsg_msg = lastNgMsg.Split('_').FirstOrDefault();
                            if (int.TryParse(nsg_msg, out int _ngCode))
                            {
                                ngCode = _ngCode;
                            }
                        }

                        // 业务规则
                        if (totalCount > reworkCountLimit)
                        {
                            DisplayLog.Warn($"产品码--{bar_no}--返工次数【{totalCount}】超限【{reworkCountLimit}】", true);

                            reworkFlag = 0;
                            return 2; // 异常
                        }
                        else
                        {
                            DisplayLog.Warn($"产品码--{bar_no}--返工代码【{lastNgMsg}】", true);

                            reworkFlag = 1;
                            return 1; // 返工
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{bar_no}--数据查询失败", ex);
                return 2;
            }
        }


        /// <summary>
        /// 选择某站某条sn的最新的一条ok_flag的值
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static int SelectResultData(string process_no, string bar_no)
        {
            string sqlStr = $"select ok_flag from GTProcessProperty where process_no ='{process_no}' AND bar_no = '{bar_no}' order by do_time desc";

            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"过站检测{bar_no}未查询到结果");
                    return 2;
                }
                else
                {
                    string ok_flag = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"过站检测{bar_no}上游来料--{ok_flag}");
                    return ok_flag == "OK" ? 1 : 2;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{bar_no}--数据查询失败", ex);
                return 2;
            }
           
        }

        /// <summary>
        /// 查询指定工站指定字段数值
        /// </summary>
        /// <param name="process_no"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static float SelectValueData(string process_no, string bar_no,string FieldName)
        {
            string sqlStr = $"select {FieldName} from GTProcessProperty where process_no ='{process_no}' AND bar_no = '{bar_no}' order by do_time desc";

            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);

                if (dataSet == null || dataSet.Tables.Count == 0)
                {
                    DisplayLog.Error($"产品码--{bar_no}--数据查询失败", null);
                    return 0f;//异常
                }

                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"产品码--{bar_no}--无记录");
                    return 0f;//异常
                }
                else
                {
                    string value = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"查询{bar_no}数据为--{value}");
                    return Convert.ToSingle(value);
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{bar_no}--数据查询失败", ex);
                return 0f;
            }

        }

        //  产品在某工序的生产时间（下料时间）到现在是否超过24小时
        public static bool IsProductProcessTimeout(string processNo, string barNo, int timeoutHours = 1)
        {
            string sql = @"
                            SELECT TOP 1
                                CASE 
                                    WHEN do_time < DATEADD(HOUR, -@timeout, GETDATE()) THEN 1
                                    ELSE 0
                                END
                            FROM GTProcessProperty
                            WHERE bar_no = @barNo
                              AND process_no = @processNo
                            ORDER BY do_time DESC;
                        ";

            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@barNo", barNo),
                    new SqlParameter("@processNo", processNo),
                    new SqlParameter("@timeout", timeoutHours)
                };

                object result = MSSqlHelper.ExecuteScalar(MSSqlHelper.Conn1, CommandType.Text, sql, parameters);

                if (result == null || result == DBNull.Value)
                    return false;

                return Convert.ToInt32(result) == 1;
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询超时失败 barNo={barNo}", ex);
                return false;
            }
        }
        public static DataSet CheckPreResultData(string sn)
        {
            //string sqlStr = $"select * from (select id,bar_no,process_no,do_time,ok_flag,ng_msg,ROW_NUMBER() over(partition by process_no order by do_time desc) as num from SHProcessProperty where bar_no='{sn}') t where t.num=1 and ok_flag='NG' and process_no in ('OP50','OP40','OP60-1','OP60-2')";
            string sqlStr = $"select * from (select * from(select id, bar_no, process_no, do_time, ok_flag, ng_msg, ROW_NUMBER() over(partition by process_no  order by do_time desc) as num from GTProcessProperty where bar_no = '{sn}' ) t where t.num = 1  and ok_flag = 'NG' and process_no in ('OP50', 'OP40'))  t1 FULL JOIN " +
                                                        $" (SELECT TOP(1) id, bar_no, process_no, do_time, ok_flag, ng_msg from GTProcessProperty where process_no LIKE 'OP60%' AND bar_no = '{sn}' ORDER BY do_time DESC)  t2 on t1.bar_no = t2.bar_no";
            DataSet dataSet = new DataSet();
            dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
            return dataSet;
        }

        /// <summary>
        /// 查询部件码
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="part_name"></param>
        /// <param name="process_no"></param>
        /// <returns></returns>
        public static string SelectQRData(string sn,string part_name,string process_no)
        {
            string sqlStr = $"select part_bar from GTProcessPart where main_bar = '{sn}' AND part_name='{part_name}' AND process_no='{process_no}'  order by id desc";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"未查到{sn}{part_name}部件码");
                    return null;
                }
                else
                {
                    string QRData = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"查到{sn}{part_name}部件码{QRData}");
                    return QRData;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码--{sn}--数据查询失败", ex);
                return null;
            }
        }

       /// <summary>
       /// 查询主码
       /// </summary>
       /// <param name="partNo"></param>
       /// <returns></returns>
        public static string SelectSn(string partNo,string process_no)
        {
            string sqlStr = $"select main_bar from GTProcessPart where part_bar = '{partNo}' and process_no = {process_no} order by id desc";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Info($"未查到{partNo}主码");
                    return null;
                }
                else
                {
                    string QRData = dataSet.Tables[0].Rows[0][0].ToString();
                    DisplayLog.Info($"查到{partNo}主码{QRData}");
                    return QRData;
                }
            }
            catch (Exception exp)
            {
                DisplayLog.Error($"产品码--{partNo}--数据上传失败", exp);
                return null;
            }
            
        }

        //  上传报警信息

        public static void UpdateBeatValue(string bar_no, string processno, double beat)
        {
            Task.Factory.StartNew(() =>
            {
                string sqlStr = $"INSERT INTO SHProcessNoBeat (bar_no,process_no,beat,do_time) " +
                $"VALUES" +
                $" ('{bar_no}','{processno}','{beat}',GETDATE())";

                MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
            });
        }

        /// <summary>
        /// 上传报警
        /// </summary>
        /// <param name="bar_no"></param>
        /// <param name="processno"></param>
        /// <param name="beat"></param>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="state"></param>
        public static void UpdateAlarmInfor(AlarmInfo info)
        {    
            Task.Factory.StartNew(() =>
            {
                string sqlStr1 = $"insert into GTProcessAlarm (process_no,create_time,event_type,event_code,event_name,event_content,close_time,andon_flag,flag)" +
                 $"values ('{info.ProcessNo}','{info.CreateTime}','SC','{info.PlcAddr}','{info.AlarmGrade}','{info.Description}','{info.CloseTime}',0,0)";
                try
                {
                    MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sqlStr1);
                }
                catch (Exception ex)
                {
                    DisplayLog.Error("上传报警信息失败", ex);
                }
            });
        }

        // 查询当前时间
        public static string GetSqlTime()
        {
            string sqlStr = $"SELECT FORMAT(GETDATE(), 'yyyy年MM月') AS FormattedDate;";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    return DateTime.Now.ToString("yyyy年MM月");
                  
                }
                else
                {
                    string sqlTime = dataSet.Tables[0].Rows[0][0].ToString();
                    return sqlTime;
                }
            }
            catch
            {
                return DateTime.Now.ToString("yyyy年MM月");
            }
        }

        /// <summary>
        /// 查询工序所有点检或抽检 station_id
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="process_no"></param>
        /// <returns></returns>
        public static string[] SelectStationId(string table_name,string process_no)
        {
            string sqlStr = $"select distinct station_id from {table_name} where inspection = '{process_no}'";
            DataSet dataSet = new DataSet();
            try
            {
                dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlStr);
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    DisplayLog.Warn($"未查到点检或抽检项");
                    return null;
                }
                else
                {
                    List<string> station_id = new List<string>();
                    for (int i = 0; i < dataSet.Tables[0].Rows.Count; i++)
                    {
                        station_id.Add(dataSet.Tables[0].Rows[i][0].ToString());
                    }
                    DisplayLog.Info($"查到点检或抽检项");
                    return station_id.ToArray();
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询工序所有点检或抽检异常", ex);
                return null;
            }

        }

        /// <summary>
        /// 查询工序所有点检或抽检 station_id
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="process_no"></param>
        /// <returns></returns>
        public static List<CalibrationOrPeriodInspection> SelectInspectionItem(string tableName, string[] stationIds)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("表名不能为空", nameof(tableName));

            if (stationIds == null || stationIds.Length == 0)
                throw new ArgumentException("站点ID数组不能为空", nameof(stationIds));

            var inspections = new List<CalibrationOrPeriodInspection>();

            try
            {
                foreach (var stationId in stationIds)
                {
                    if (string.IsNullOrWhiteSpace(stationId))
                        continue;

                    var sqlQuery = $@"SELECT test_item, test_item_type, test_item_up, test_item_down 
                            FROM {tableName} 
                            WHERE station_id = @stationId";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@stationId", stationId)
                    };

                    using (var dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlQuery, parameters))
                    {
                        if (dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                        {
                            DisplayLog.Info($"未找到站点 {stationId} 的点检或抽检项");
                            continue;
                        }

                        var table = dataSet.Tables[0];
                        var inspection = new CalibrationOrPeriodInspection
                        {
                            CalibrationOrPeriodStationId = stationId,
                            Test_item = new string[table.Rows.Count],
                            Test_item_type = new string[table.Rows.Count],
                            Test_item_up = new string[table.Rows.Count],
                            Test_item_down = new string[table.Rows.Count]
                        };

                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            var row = table.Rows[i];
                            inspection.Test_item[i] = row[0]?.ToString() ?? string.Empty;
                            inspection.Test_item_type[i] = row[1]?.ToString() ?? string.Empty;
                            inspection.Test_item_up[i] = row[2]?.ToString() ?? string.Empty;
                            inspection.Test_item_down[i] = row[3]?.ToString() ?? string.Empty;
                        }

                        inspections.Add(inspection);
                    }
                }

                return inspections.Count > 0 ? inspections : null;
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询点检抽检配置异常", ex);
                return null;
            }
        }


        /// <summary>
        /// 查询工序所有点检或抽检 station_id
        /// </summary>
        /// <param name="table_name"></param>
        /// <param name="process_no"></param>
        /// <returns></returns>
        public static DataSet SelectCalibration(string process_no, DateTime? start_time, DateTime? end_time)
        {
            try
            {
                var sqlQuery = $@"select bar_no,do_time,test_item,test_item_up,test_item_down,test_item_value,case when flag = 1 then 'OK' else 'NG' end as flag 
                            FROM GTProcessCalibration 
                            WHERE process_no = @process_no and do_time between @start_time and @end_time order by do_time desc";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@process_no", process_no),
                    new SqlParameter("@start_time", start_time.Value),
                    new SqlParameter("@end_time", end_time.Value),
                };

                var dataSet = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sqlQuery, parameters);

                return dataSet;
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询点检抽检配置异常", ex);
                return null;
            }
        }

        #region 报警

        public static double GetAlarmDurationMinutes()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("select cc.process_no, sum(cc.sumvalue) ccSum,sum(cc.num) ccCum from ");
            sb.Append("(");
            sb.Append("select aa.alarm_id, b.process_no, aa.sumvalue, aa.num from ");
            sb.Append("(");
            sb.Append("select alarm_id, count(alarm_id) num, sum(alarm_duration) as sumvalue ");
            sb.Append("from SHProcessAlarmSelf ");
            sb.Append($"where alarm_time between 'DATEADD(MINUTE, -10, GETDATE())' and 'GETDATE()'");
            sb.Append("group by alarm_id");
            sb.Append(")");
            sb.Append("aa LEFT JOIN SHProcessAlarmParse b ON aa.alarm_id = b.alarm_id");
            sb.Append(")cc group by process_no order by LEN(process_no)");


            DataSet data = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sb.ToString());

            double totalAlarmSeconds = 0;  // 新增：用于统计总报警时长(秒)

            foreach (DataRow row in data.Tables[0].Rows)
            {
                double processSeconds = Convert.ToDouble(row["ccSum"]);  // 每个工序报警秒数

                totalAlarmSeconds += processSeconds;   // 统计总时长

            }
            return totalAlarmSeconds;
        }

        #endregion

        #region 计算总数

        public static (int outputCount, int goodCount) GetProductionData()
        {
            string sql = @"
                            WITH LastRecord AS (
                                SELECT bar_no, ok_flag,
                                       ROW_NUMBER() OVER(PARTITION BY bar_no ORDER BY do_time DESC) AS rn
                                FROM GTProcessProperty
                                WHERE process_no='OP070' and do_time >= DATEADD(MINUTE, -10, GETDATE())
                            )
                            SELECT 
                                COUNT(*) AS OutputCount,
                                SUM(CASE WHEN ok_flag = 'OK' THEN 1 ELSE 0 END) AS GoodCount
                            FROM LastRecord
                            WHERE rn = 1;
                            ";

            DataSet data = MSSqlHelper.GetDataSet(MSSqlHelper.Conn1, CommandType.Text, sql);

            int totalOutputCount = 0;  // 总数
            int totalGoodCount = 0;  // OK数


            foreach (DataRow row in data.Tables[0].Rows)
            {
                int processOutputCount = Convert.ToInt32(row["OutputCount"]);  // 

                int processGoodCount = Convert.ToInt32(row["GoodCount"]);  // 

                totalOutputCount += processOutputCount;   // 
                totalGoodCount += processGoodCount;   // 

            }
            return (totalOutputCount, totalGoodCount);
        }
        #endregion

        /// <summary>
        /// 生成数据的带逗号的字符串
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static string TransArrayToStr(string[] valus)
        {
            List<string> list = new List<string>();

            for (int i = 0; i < valus.Length; i++)
            {
                list.Add(valus[i]);
            }
            return string.Format("'{0}'", string.Join(",", list.ToArray()).Replace(",", "','"));
        }

        /// <summary>
        /// 生成字段的带逗号的字符串 DataItem
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static string GenerateFieldStr(DataItem value)
        {
            List<string> list = new List<string>();

            for (int i = 1; i <= value.Data.Length * 4; i++)
            {
                list.Add($"data{i:d3}");
            }
            return string.Join(",", list.ToArray());
        }
               
        private static string GenerateFieldStr1(string[] value)
        {
            List<string> list = new List<string>();

            for (int i = 1; i <= value.Length; i++)
            {
                list.Add($"data{i:d3}");
                //list.Add($"data{i}");
            }
            return string.Join(",", list.ToArray());
        }

        public class ParamatesLimit
        {
            public double Up_limit { get; set; }

            public double Down_limit { get; set; }

        }

        public class ResultCodeParse
        {
            public string Ng_code { get; set; }

            public string Ng_msg { get; set; }

        }

        //  更新字段
        public static void SaveField(string processNo, string fieldName, string fieldNameCn)
        {
            string sql = @"
                            UPDATE GTProcessPropertyParse
                            SET field_name_cn=@field_name_cn,
                                data_type='varchar'
                            WHERE process_no=@process_no
                            AND field_name=@field_name;
                            
                            IF @@ROWCOUNT = 0
                            BEGIN
                                INSERT INTO GTProcessPropertyParse
                                (
                                    process_no,
                                    field_name,
                                    field_name_cn,
                                    data_type
                                )
                                VALUES
                                (
                                    @process_no,
                                    @field_name,
                                    @field_name_cn,
                                    'varchar'
                                )
                            END";

            using (SqlConnection conn = new SqlConnection(MSSqlHelper.Conn1))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@process_no", processNo));
                cmd.Parameters.Add(new SqlParameter("@field_name", fieldName));
                cmd.Parameters.Add(new SqlParameter("@field_name_cn", fieldNameCn));

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}
