using GT.Factory.Common.Interfaces;
using GT.Factory.Data.Config;
using GT.Factory.Data.Core;
using GT.Factory.GTUser.Repository;
using GT.Factory.Login.Login;
using GT.Factory.Product.Repository;
using GT.Factory.Product.Service;
using GT.Factory.ProductCode.Repository;
using GT.Factory.ProductCode.Service;
using GT.Factory.RFID.Aynettek;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.WebApi.QueryClient;
using System;

namespace GT.Factory.Server
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

        public ProductCodeRuleService ProductCodeRuleService { get; }

        public ProductModelService ProductModelService { get; }

        public ProductPartService ProductPartRepository { get; }

        public AppRuntime()
        {
            // 数据运行环境
            DataRuntime =
                new DataRuntime(DatabaseConfigProvider.Instance.Settings);



            // 用户服务
            LocalUserService =
                new LocalUserService(new LocalUserRepository(DataRuntime.Database));



            // 远程用户接口
            UserApiService =
                new UserApiService(
                    WebApiConfigProvider.Instance.Settings.ServerApi,
                    WebApiConfigProvider.Instance.Settings.ServerDbtask);



            // MES
            MesService =
                new MesService();



            // RFID
            RfidService =
                new RfidService();


            ProductCodeRuleService =
                  new ProductCodeRuleService(new ProductCodeRuleRepository(DataRuntime.Database),
                  new ProductCodeRepository(DataRuntime.Database));

            ProductModelService =
               new ProductModelService(new ProductModelRepository(DataRuntime.Database));

            ProductPartRepository =
               new ProductPartService(new ProductPartRepository(DataRuntime.Database));

            _current = this;
        }
    }
}
