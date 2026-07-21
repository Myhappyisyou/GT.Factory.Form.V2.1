using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.Model;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var user =  _localUserService.GetUserByLogin(username, password);
            if (user == null)
                return null;

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
            var user = await _userService.GetUserByCard(uid)
                       ?? _localUserService.GetUserByCard(uid);

            if (user == null)
                return null;

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
