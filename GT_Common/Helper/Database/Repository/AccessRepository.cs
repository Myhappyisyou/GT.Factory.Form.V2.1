using GT_Common.Helper.Alarms;
using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Core;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Repository
{
    /// <summary>
    /// Access 用户仓储
    /// 负责 UsersInfor 表操作
    /// 
    /// 仅使用 LocalCache 或指定 Access 数据库
    /// </summary>
    public class AccessUserRepository
    {
        private readonly IDatabase _db;

        public AccessUserRepository(DatabaseManager manager)
        {
            _db = manager.LocalCache
                ?? throw new Exception("Access 本地数据库未配置");
        }

        #region 生产数据

        #region ✅ 插入测试数据（事务）

        public async Task<bool> InsertProcessPropertyAsync(
         string processNo,
         string orderNo,
         string barNo,
         string operatorNo,
         string[] okFlag,
         string testBeat,
         string[] testItem,
         string[] testItemUp,
         string[] testItemDown,
         string[] testItemValue,
         string doTime = null)
        {
            if (string.IsNullOrWhiteSpace(barNo))
                return false;

            if (testItemValue == null || testItemValue.Length == 0)
                return false;

            var tx = await _db.BeginTransactionAsync();
            try
            {
                string sql = @"
            INSERT INTO 生产信息
            (工位名称, 当前工单号, 产品条码,
             操作人员, 测试时间,
             测试结果, 测试节拍,
             测试项名称, 测试项上限,
             测试项下限, 测试项实际值,
             更新标识)
            VALUES
            (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                for (int i = 0; i < testItemValue.Length; i++)
                {
                    await _db.ExecuteAsync(sql, new
                    {
                        processNo,
                        orderNo,
                        barNo,
                        operatorNo,
                        doTime = doTime ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        okFlag = okFlag[i],
                        testBeat,
                        testItem = testItem[i],
                        testItemUp = testItemUp[i],
                        testItemDown = testItemDown[i],
                        testItemValue = testItemValue[i],
                        更新标识 = "F"
                    });
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            finally
            {
                if (tx != null)
                    await tx.DisposeAsync();
            }
        }

        #endregion

        #region ✅ 查询生产数据

        public async Task<List<dynamic>> SearchAccessDataAsync(
            string processNo,
            DateTime? startTime,
            DateTime? endTime,
            string okFlag,
            string filterColumn,
            string filterValue)
        {
            var sql = new StringBuilder();
            sql.Append("SELECT * FROM 生产信息 WHERE 工位名称 = ?");

            var parameters = new List<object> { processNo };

            if (!string.IsNullOrEmpty(filterColumn) &&
                !string.IsNullOrEmpty(filterValue))
            {
                sql.Append($" AND {filterColumn} = ?");
                parameters.Add(filterValue);
            }

            if (startTime.HasValue && endTime.HasValue)
            {
                sql.Append(" AND 测试时间 >= ? AND 测试时间 <= ?");
                parameters.Add(startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                parameters.Add(endTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (!string.IsNullOrEmpty(okFlag) && okFlag != "All")
            {
                sql.Append(" AND 测试结果 = ?");
                parameters.Add(okFlag);
            }

            sql.Append(" ORDER BY ID");

            return await _db.QueryAsync<dynamic>(sql.ToString(), parameters.ToArray());
        }

        #endregion

        #endregion

        #region 用户

        #region ✅ 插入或更新登录次数

        public async Task InsertOrUpdateUserAsync(User user)
        {
            const string updateSql = @"
                UPDATE UsersInfor
                SET 登录次数 = 登录次数 + 1,
                    最后登录时间 = Now()
                WHERE 厂牌UID = @UID";

            int affected = await _db.ExecuteAsync(updateSql, new
            {
                user.UID
            });

            if (affected == 0)
            {
                await InsertNewUserAsync(user);
            }
        }

        #endregion

        #region ✅ 插入新用户

        public async Task InsertNewUserAsync(User user)
        {
            const string insertSql = @"
                INSERT INTO UsersInfor
                (用户名, 用户密码, 用户权限,
                 厂牌UID, 最后登录时间,
                 登录次数, 工号, MES账号, MES密码)
                VALUES
                (@UserName, @UserPassword, @UserRole,
                 @UID, Now(),
                 1, @JobNub, @MesAccount, @MesPassword)";

            await _db.ExecuteAsync(insertSql, user);
        }

        #endregion

        #region ✅ 查询

        public async Task<User> GetByUIDAsync(string uid)
        {
            const string sql = @"
                SELECT *
                FROM UsersInfor
                WHERE 厂牌UID = @uid";

            return await _db.QuerySingleAsync<User>(sql, new { uid });
        }

        public async Task<User> GetByJobAsync(string jobNub, string password)
        {
            const string sql = @"
                SELECT *
                FROM UsersInfor
                WHERE 工号 = @jobNub
                  AND 用户密码 = @password";

            return await _db.QuerySingleAsync<User>(sql,
                new { jobNub, password });
        }

        public async Task<List<User>> GetAllAsync()
        {
            const string sql = "SELECT * FROM UsersInfor";
            return await _db.QueryAsync<User>(sql);
        }

        #endregion

        #region ✅ 更新

        public async Task UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE UsersInfor
                SET 用户名 = @UserName,
                    用户密码 = @UserPassword,
                    用户权限 = @UserRole,
                    厂牌UID = @UID,
                    工号 = @JobNub,
                    MES账号 = @MesAccount,
                    MES密码 = @MesPassword
                WHERE ID = @ID";

            await _db.ExecuteAsync(sql, user);
        }

        #endregion

        #region ✅ 删除

        public async Task DeleteAsync(string jobNub)
        {
            const string sql = @"
                DELETE FROM UsersInfor
                WHERE 工号 = @jobNub";

            await _db.ExecuteAsync(sql, new { jobNub });
        }

        #endregion

        #endregion

        #region 易损件

        #region ✅ 更新使用次数（不存在则插入）

        public async Task UpdateConsumableUsageAsync(Consumables consumable)
        {
            const string updateSql = @"
                UPDATE 易损件信息
                SET
                    易损件已使用次数 = 易损件已使用次数 + 1,
                    易损件剩余使用次数 = 易损件剩余使用次数 - 1
                WHERE 易损件所在工位 = @ProcessName
                  AND 机台名称 = @StationName
                  AND 易损件所在位置 = @Location
                  AND 易损件名称 = @Name";

            int affected = await _db.ExecuteAsync(updateSql, new
            {
                consumable.ProcessName,
                consumable.StationName,
                consumable.Location,
                consumable.Name
            });

            if (affected == 0)
            {
                await InsertConsumableAsync(consumable);
            }
        }

        #endregion

        #region ✅ 插入

        public async Task InsertConsumableAsync(Consumables consumable)
        {
            const string insertSql = @"
                INSERT INTO 易损件信息
                (易损件所在工位, 机台名称,
                 易损件所在位置, 易损件名称,
                 易损件理论使用次数,
                 易损件已使用次数,
                 易损件剩余使用次数)
                VALUES
                (@ProcessName, @StationName,
                 @Location, @Name,
                 @TheoreticalCount,
                 @UsedCount,
                 @RemainderCount)";

            await _db.ExecuteAsync(insertSql, consumable);
        }

        #endregion

        #region ✅ 查询

        public async Task<List<Consumables>> GetByStationAsync(string stationName)
        {
            const string sql = @"
                SELECT *
                FROM 易损件信息
                WHERE 易损件所在工位 = @stationName";

            return await _db.QueryAsync<Consumables>(sql, new { stationName });
        }

        public async Task<Consumables> GetSingleAsync(
            string processName,
            string location,
            string name)
        {
            const string sql = @"
                SELECT TOP 1 *
                FROM 易损件信息
                WHERE 机台名称 = @processName
                  AND 易损件所在位置 = @location
                  AND 易损件名称 = @name";

            return await _db.QuerySingleAsync<Consumables>(sql,
                new { processName, location, name });
        }

        public async Task<List<Consumables>> GetAllConsumableAsync()
        {
            const string sql = "SELECT * FROM 易损件信息";
            return await _db.QueryAsync<Consumables>(sql);
        }

        #endregion

        #region ✅ 删除

        public async Task DeleteAsync(int id)
        {
            const string sql = @"
                DELETE FROM 易损件信息
                WHERE ID = @id";

            await _db.ExecuteAsync(sql, new { id });
        }

        #endregion

        #endregion

        #region 报警

        #region ✅ 插入报警信息

        public async Task InsertAlarmAsync(AlarmInfo alarm)
        {
            string sql = @"
                INSERT INTO 故障信息
                (故障发生工位, 机台名称, 故障类型,
                 故障描述, 发生时间, 结束时间)
                VALUES
                (?, ?, ?, ?, ?, ?)";

            await _db.ExecuteAsync(sql, new
            {
                alarm.AlarmStation,
                alarm.ProcessName,
                alarm.AlarmGrade,
                alarm.Description,
                alarm.CreateTime,
                alarm.CloseTime
            });
        }

        #endregion

        #endregion

    }
}
