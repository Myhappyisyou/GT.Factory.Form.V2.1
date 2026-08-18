using GT.Factory.Common.Models;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    /// <summary>
    /// 登录业务逻辑服务
    /// 统一处理账号登录、刷卡登录、MES校验
    /// </summary>
    public class LoginService
    {
        private readonly ILocalUserService _localUserService;
        private readonly IUserService _userService;
        private readonly IMesService _mesService;

        public LoginService(
            ILocalUserService localUserService,
            IUserService userService,
            IMesService mesService)
        {
            _localUserService = localUserService;
            _userService = userService;
            _mesService = mesService;
        }

        /// <summary>
        /// 账号密码登录
        /// </summary>
        public async Task<User> LoginByAccount(
            string username,
            string password,
            bool needMesValidation)
        {
            User user = null;

            try
            {
                // 1. 优先尝试远程服务器登录
                user = await _userService.GetUserByLogin(username, password);
            }
            catch (Exception ex)
            {
                // 远程调用崩溃（如断网、服务器500等），记录日志，但不让程序死掉
                // DisplayLog.Warn("连接主服务器失败，将尝试本地登录: " + ex.Message);
            }

            // 2. 如果远程查无此人（返回null），或者上面 catch 到了断网异常（user仍为null）
            if (user == null)
            {
                // 尝试本地数据库/缓存登录
                // 注意：这里的本地调用通常不需要 catch，因为如果连本地单机数据库都崩了，
                // 说明工控机系统坏了，应该让外层的 UI 弹窗报错。
                user = await _localUserService.GetUserByLogin(username, password);

                // 如果本地登录成功了，可以在这里标记一下当前是【离线模式】
                if (user != null)
                {
                    // Shared.isOffline = true; 
                }
                else
                {
                    return user;
                }
            }

            if (needMesValidation)
            {
                bool mesOk = _mesService.ValidateUser(
                    user.JobNub,
                    password,
                    out string err);

                if (!mesOk)
                    return null;

                Shared.isOffline = false;
            }
            else
            {
                Shared.isOffline = true;
            }

            return user;
        }

        /// <summary>
        /// 角色密码登录
        /// </summary>
        public async Task<User> LoginByRoleId(
            string username,
            string password,
            bool needMesValidation)
        {
            User user = null;

            try
            {
                // 1. 优先尝试远程服务器登录
                user = await _userService.GetUserByRoleId(username, password);
            }
            catch (Exception ex)
            {
                // 远程调用崩溃（如断网、服务器500等），记录日志，但不让程序死掉
                // DisplayLog.Warn("连接主服务器失败，将尝试本地登录: " + ex.Message);
            }

            // 2. 如果远程查无此人（返回null），或者上面 catch 到了断网异常（user仍为null）
            if (user == null)
            {
                // 尝试本地数据库/缓存登录
                // 注意：这里的本地调用通常不需要 catch，因为如果连本地单机数据库都崩了，
                // 说明工控机系统坏了，应该让外层的 UI 弹窗报错。
                user = await _localUserService.GetUserByRoleId(username, password);

                // 如果本地登录成功了，可以在这里标记一下当前是【离线模式】
                if (user != null)
                {
                    // Shared.isOffline = true; 
                }
                else
                {
                    return user;
                }
            }

            if (needMesValidation)
            {
                bool mesOk = _mesService.ValidateUser(
                    user.JobNub,
                    password,
                    out string err);

                if (!mesOk)
                    return null;

                Shared.isOffline = false;
            }
            else
            {
                Shared.isOffline = true;
            }

            return user;
        }

        /// <summary>
        /// 刷卡登录
        /// </summary>
        public async Task<User> LoginByCard(
            string uid,
            bool needMesValidation)
        {
            User user = null;

            try
            {
                // 1. 优先尝试远程服务器登录
                user = await _userService.GetUserByCard(uid);
            }
            catch (Exception ex)
            {
                // 远程调用崩溃（如断网、服务器500等），记录日志，但不让程序死掉
                // DisplayLog.Warn("连接主服务器失败，将尝试本地登录: " + ex.Message);
            }

            // 2. 如果远程查无此人（返回null），或者上面 catch 到了断网异常（user仍为null）
            if (user == null)
            {
                // 尝试本地数据库/缓存登录
                // 注意：这里的本地调用通常不需要 catch，因为如果连本地单机数据库都崩了，
                // 说明工控机系统坏了，应该让外层的 UI 弹窗报错。
                user = await _localUserService.GetUserByCard(uid);

                // 如果本地登录成功了，可以在这里标记一下当前是【离线模式】
                if (user != null)
                {
                    // Shared.isOffline = true; 
                }
                else
                {
                    return user;
                }
            }

            if (needMesValidation)
            {
                bool mesOk = _mesService.ValidateUser(
                    user.JobNub,
                    user.MesCredential?.Password,
                    out string err);

                if (!mesOk)
                    return null;

                Shared.isOffline = false;
            }
            else
            {
                Shared.isOffline = true;
            }

            return user;
        }
    }
}
