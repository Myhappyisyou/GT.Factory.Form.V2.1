using GT.Factory.Data.Core;
using GT.Factory.Station.DriverForm.Aynettek;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.Station.Helper.BydUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station
{
    public class AppRuntime
    {
        private static AppRuntime _current;

        public static AppRuntime Current
        {
            get
            {
                if (_current == null)
                    throw new Exception("AppRuntime未初始化");

                return _current;
            }
        }


        public DataRuntime DataRuntime { get; }


        public ILocalUserService LocalUserService { get; }


        public IUserService UserApiService { get; }


        public IMesService MesService { get; }


        public IRfidService RfidService { get; }



        public AppRuntime()
        {
            // 数据运行环境
            DataRuntime =
                new DataRuntime(
                    Config.Instance.DatabaseOptions);



            // 用户服务
            LocalUserService =
                new LocalUserService();



            // 远程用户接口
            UserApiService =
                new UserApiService(
                    Config.Instance.ServerApi,
                    Config.Instance.ServerDbtask);



            // MES
            MesService =
                new MesService();



            // RFID
            RfidService =
                new RfidService();



            _current = this;
        }
    }
}
