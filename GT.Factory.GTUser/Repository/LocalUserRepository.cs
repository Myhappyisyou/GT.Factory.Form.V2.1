using GT.Factory.Common.Models;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.GTUser.Repository
{
    /// <summary>
    /// Access 用户仓储
    /// 负责 UsersInfor 表操作
    /// 
    /// 仅使用 LocalCache 或指定 Access 数据库
    /// </summary>
    public class LocalUserRepository: ILocalUserService
    {
        private readonly IDatabase _db;

        public LocalUserRepository(DatabaseManager manager)
        {
            //_db = manager.Master 
            //    ?? throw new Exception("Access 本地数据库未配置");
            _db = manager.Primary;
        }


        #region ✅ 记录一次登录（插入日志）

        public async Task RecordLoginAsync(User user, string loginIp = null)
        {
            // 1. 确保用户存在（按 UID 查）
            var existSql = @"SELECT ID FROM GTUsersInfor WHERE UID = @UID";
            var userId = await _db.QuerySingleAsync<int?>(existSql, new { user.UID });

            // 2. 用户不存在，先插入用户信息
            if (userId == null)
            {
                await InsertNewUsersInforAsync(user);
                userId = await _db.QuerySingleAsync<int>(
                    existSql, new { user.UID });
            }

            // 3. 插入一条登录日志
            const string logSql = @"
        INSERT INTO GTUsers (UserID, UserName, LoginIP)
        VALUES (@UserID, @UserName, @LoginIP)";

            await _db.ExecuteAsync(logSql, new
            {
                UserID = userId.Value,
                UserName = user.UserName,
                LoginIP = loginIp
            });
        }

        #endregion



        #region ✅ 插入新用户

        public async Task InsertNewUsersInforAsync(User user)
        {
            const string insertSql = @"
        INSERT INTO GTUsersInfor
            (UserName, UserPassword, UserRole,
             UID, JobNum, MesAccount, MesPassword)
        VALUES
            (@UserName, @UserPassword, @UserRole,
             @UID, @JobNum, @MesAccount, @MesPassword)";

            await _db.ExecuteAsync(insertSql, new
            {
                user.UserName,
                user.UserPassword,
                user.UserRole,
                user.UID,
                user.JobNum,
                user.MesAccount,
                user.MesPassword
            });
        }

        #endregion


        #region ✅ 查询

        // 按 UID 查
        public async Task<User> GetByUIDAsync(string uid)
        {
            const string sql = @"
        SELECT ID, UserName, UserPassword, UserRole,
               UID, JobNum, MesAccount, MesPassword, IsEnabled
        FROM GTUsersInfor
        WHERE UID = @uid";

            return await _db.QuerySingleAsync<User>(sql, new { uid });
        }

        // 按工号 + 密码查
        public async Task<User> GetByJobAsync(string jobNum, string password)
        {
            const string sql = @"
        SELECT ID, UserName, UserPassword, UserRole,
               UID, JobNum, MesAccount, MesPassword, IsEnabled
        FROM GTUsersInfor
        WHERE JobNum = @jobNum
          AND UserPassword = @password";

            return await _db.QuerySingleAsync<User>(sql, new { jobNum, password });
        }

        // 按用户名 + 密码查
        public async Task<User> GetByRoleIdAsync(string userName, string password)
        {
            const string sql = @"
        SELECT ID, UserName, UserPassword, UserRole,
               UID, JobNum, MesAccount, MesPassword, IsEnabled
        FROM GTUsersInfor
        WHERE UserName = @userName
          AND UserPassword = @password";

            return await _db.QuerySingleAsync<User>(sql, new { userName, password });
        }

        // 全部用户
        public async Task<List<User>> GetAllAsync()
        {
            const string sql = @"
        SELECT ID, UserName, UserPassword, UserRole,
               UID, JobNum, MesAccount, MesPassword, IsEnabled
        FROM GTUsersInfor";

            return await _db.QueryAsync<User>(sql);
        }

        #endregion


        #region ✅ 更新

        public async Task UpdateAsync(User user)
        {
            const string sql = @"
        UPDATE GTUsersInfor
        SET UserName     = @UserName,
            UserPassword = @UserPassword,
            UserRole     = @UserRole,
            UID          = @UID,
            JobNum       = @JobNum,
            MesAccount   = @MesAccount,
            MesPassword  = @MesPassword,
            UpdateTime   = CURRENT_TIMESTAMP
        WHERE ID = @ID";

            await _db.ExecuteAsync(sql, user);
        }

        #endregion


        #region ✅ 删除

        public async Task DeleteAsync(string jobNum)
        {
            const string sql = @"DELETE FROM GTUsersInfor WHERE JobNum = @jobNum";
            await _db.ExecuteAsync(sql, new { jobNum });
        }

        #endregion


        #region ✅ 登录统计

        // 某用户登录次数
        public async Task<int> GetLoginCountAsync(int userId)
        {
            const string sql = @"
        SELECT COUNT(*) FROM GTUsers WHERE UserID = @userId";
            return await _db.QuerySingleAsync<int>(sql, new { userId });
        }

        // 某用户最后登录时间
        public async Task<DateTime?> GetLastLoginTimeAsync(int userId)
        {
            const string sql = @"
        SELECT MAX(LoginTime) FROM GTUsers WHERE UserID = @userId";
            return await _db.QuerySingleAsync<DateTime?>(sql, new { userId });
        }

        // 用户 + 登录次数 + 最后登录（列表页常用）
        public async Task<List<UserLoginStat>> GetUserLoginStatsAsync()
        {
            const string sql = @"
        SELECT u.ID, u.UserName, u.UserRole, u.JobNum,
               COUNT(l.ID)      AS LoginCount,
               MAX(l.LoginTime) AS LastLoginTime
        FROM GTUsersInfor u
        LEFT JOIN GTUsers l ON l.UserID = u.ID
        GROUP BY u.ID, u.UserName, u.UserRole, u.JobNum";

            return await _db.QueryAsync<UserLoginStat>(sql);
        }

        #endregion


    }
}
