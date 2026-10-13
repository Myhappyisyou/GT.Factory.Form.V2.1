using GT.Factory.Common.Enums;
using GT.Factory.Common.Models;
using GT.Factory.Login.Login;
using System;
using System.Windows.Forms;

namespace GT.Factory.Station.Helper
{
    public static class AuthHelper
    {
        /// <summary>
        /// 当前是否启用超时验证
        /// </summary>
        private static bool EnableTimeout => LoginConfigProvider.Instance.Settings.EnableLoginTimeout;

        /// <summary>
        /// 当前登录超时时间
        /// </summary>
        private static TimeSpan _timeout => TimeSpan.FromMinutes(LoginConfigProvider.Instance.Settings.LoginTimeoutMinutes);


        private static User _lastUser;                 // 最近一次登录用户
        private static DateTime _lastLoginTime;        // 最近一次登录时间
        //private static readonly TimeSpan _timeout = TimeSpan.FromMinutes(5); // 登录缓存有效期


        public static void RecordLogin(User user)
        {
            if (user == null) return;
            _lastUser = user;
            _lastLoginTime = DateTime.Now;
        }

        // ✅ 提取公共方法：生成配置好的登录窗体
        public static LoginForm CreateLoginForm(string ip = null,
    int port = 8080)
        {
            var localConfig = Config.Instance;
            localConfig.Login.Validate();

            var loginService = new LoginService(
                AppRuntime.Current.LocalUserService,
                AppRuntime.Current.UserApiService,
                AppRuntime.Current.MesService);

            var providers = IdentityProviderFactory.CreateProviders(
                localConfig.Login,
                loginService,
                AppRuntime.Current.RfidService);

            return new LoginForm(providers);
        }

        /// <summary>
        /// 统一权限验证方法
        /// </summary>
        /// <param name="minLevel">所需最低权限</param>
        /// <param name="onSuccess">验证成功后的操作</param>
        public static void RequireLogin(UserLevel minLevel, Action<User> onSuccess)
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
            // ✅ 优雅调用，并使用 using 释放资源
            using (var loginForm = CreateLoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var user = loginForm.CurrentUser;
                    if (user.LevelEnum >= minLevel)
                    {
                        RecordLogin(user); // 记录登录状态
                        onSuccess?.Invoke(user);
                    }
                    else
                    {
                        MessageBox.Show($"权限不足，需要 {minLevel} 级及以上权限！", "验证失败");
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
