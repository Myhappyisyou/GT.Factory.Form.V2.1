using GT.Factory.Logging.Logging;
using HslCommunication;
using Serilog;
using System;
using System.Diagnostics;

namespace GT.Factory.Station
{
    public static class AppManager
    {

        public static ILogger Logger => Log.Logger;


        public static bool CheckSingleInstance(string name)
        {
            Process[] ps =
                Process.GetProcessesByName(name);

            return ps.Length <= 1;
        }

        public static void Initialize()
        {

            // HSL授权
            if (!Authorization.SetAuthorizationCode("123456"))
            {
                throw new Exception("PLC通讯库授权失败");
            }

            //  Station = OP070  （程序级）
            //  Module = PLC    （模块级）
            LogBootstrapper.Init("OP070");

            var moduleLogger = Log.Logger.ForContext("Module", "OP070");

            var writer = new SerilogWriter(moduleLogger);

            // UI display 暂时先传 null
            DisplayLog.Init(writer, null);

        }
    }
}