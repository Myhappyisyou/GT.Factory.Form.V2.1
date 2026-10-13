using GT.Factory.Common.Interfaces;
using GT.Factory.GTUser.Repository;
using GT.Factory.RFID.Aynettek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Login
{
    /// <summary>
    /// Login 模块运行时环境。
    ///
    /// 负责集中保存 Login 模块运行期间需要使用的服务。
    /// Login 模块不依赖 GT.Factory.Station.AppRuntime。
    /// </summary>
    public class LoginRuntime
    {
        private static LoginRuntime _current;

        /// <summary>
        /// 当前 Login 运行时实例。
        /// </summary>
        public static LoginRuntime Current
        {
            get
            {
                if (_current == null)
                    throw new Exception("LoginRuntime未初始化");

                return _current;
            }
        }

        /// <summary>
        /// 本地用户服务。
        /// </summary>
        public ILocalUserService LocalUserService { get; private set; }

        /// <summary>
        /// 远程用户服务。
        /// </summary>
        public IUserService UserApiService { get; private set; }

        /// <summary>
        /// MES 服务。
        /// </summary>
        public IMesService MesService { get; private set; }

        /// <summary>
        /// RFID 服务。
        /// </summary>
        public IRfidService RfidService { get; private set; }

        public LoginRuntime(
            ILocalUserService localUserService,
            IUserService userApiService,
            IMesService mesService,
            IRfidService rfidService)
        {
            LocalUserService = localUserService;
            UserApiService = userApiService;
            MesService = mesService;
            RfidService = rfidService;

            _current = this;
        }
    }
}
