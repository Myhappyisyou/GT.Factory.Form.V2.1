using GT_Common.MyEnum;
using GT_Common.DriverForm.Aynettek;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.Model;
using System;
using System.Windows.Forms;

namespace GT_Common.Helper
{
    public static class AuthHelper
    {
        /// <summary>
        /// 当前是否启用超时验证
        /// </summary>
        private static bool EnableTimeout => Config.Instance.EnableLoginTimeout;

        /// <summary>
        /// 当前登录超时时间
        /// </summary>
        private static TimeSpan _timeout => TimeSpan.FromMinutes(Config.Instance.LoginTimeoutMinutes);


        private static User _lastUser;                 // 最近一次登录用户
        private static DateTime _lastLoginTime;        // 最近一次登录时间
        //private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(5); // 登录缓存有效期


        public static void RecordLogin(User user)
        {
            if (user == null) return;
            _lastUser = user;
            _lastLoginTime = DateTime.Now;
        }

        /// <summary>
        /// 统一权限验证方法
        /// </summary>
        /// <param name="minLevel">所需最低权限</param>
        /// <param name="onSuccess">验证成功后的操作</param>
        public static void RequireLogin(UserLevel minLevel, string addr, int port, Action<User> onSuccess)
        {
            bool isValid = _lastUser != null &&
                     (!EnableTimeout || DateTime.Now - _lastLoginTime < _timeout);


            // 如果已有登录，且未超时
            if (isValid)
            {
                if (_lastUser.LevelEnum >= minLevel)
                {
                    onSuccess?.Invoke(_lastUser);
                    return;
                }
                else
                {
                    MessageBox.Show($"权限不足，需要 {minLevel} 级及以上权限！当前用户等级：{_lastUser.UserRole}", "权限验证失败");
                    return;
                }
            }

            // 否则要求重新登录
            using (var loginForm = new LoginForm(new RfidService(), new UserApiService(Config.Instance.ServerApi, Config.Instance.ServerDbtask), new MesService(), new LocalUserService(), addr, port))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var user = loginForm.currentUser;
                    if (user.LevelEnum >= minLevel)
                    {
                        _lastUser = user;
                        _lastLoginTime = DateTime.Now;
                        onSuccess?.Invoke(user);
                    }
                    else
                    {
                        MessageBox.Show($"权限不足，需要 {minLevel} 级及以上权限！当前用户等级：{user.UserRole}", "权限验证失败");
                    }
                }
                else
                {
                    MessageBox.Show("用户未登录，操作已取消。", "提示");
                }
            }
        }

        /// <summary>
        /// 主动清除缓存（比如用户主动退出）
        /// </summary>
        public static void Logout()
        {
            _lastUser = null;
            _lastLoginTime = DateTime.MinValue;
        }

        /// <summary>
        /// 当前是否已登录（即便禁用超时也有效）
        /// </summary>
        public static bool IsLoggedIn =>
            _lastUser != null &&
            (!EnableTimeout || DateTime.Now - _lastLoginTime < _timeout);

        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        public static User CurrentUser => _lastUser;
    }

}
