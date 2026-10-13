
using GT.Factory.Common.Helpers;
using GT.Factory.Server.Help;
using System;

namespace GT.Factory.Server
{
    public class Config
    {
        #region xml

        private static readonly Lazy<Config> lazy = new Lazy<Config>(
        () =>
        {
            Config setting = new Config();
            try
            {
                SerializeHelper.LoadXml<Config>(PathCenter.ConfigFile("CommonConfig.xml"));
            }
            catch { }
            return setting;
        });

        private Config() { }

        public static Config Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public static void Save()
        {
            //SerializeHelper.SaveXmlEncode<Config>(Config.Instance, "LocalConfig.xml");
            SerializeHelper.SaveXml<Config>(Config.Instance, PathCenter.ConfigFile("CommonConfig.xml"));
            //File.WriteAllText("Setting.json", JsonConvert.SerializeObject(Instance));
        }
        #endregion

        #region Access

        public string userdbPath = @"D:\BYD_Users\Users_Data.mdb"; // 目标电脑 IP 和共享名

        public string DatadbPath { get; set; } = @"D:\生产数据"; // 目标电脑 IP 和共享名

        public string sharedbPath { get; set; } = @"\\127.0.0.1\Access"; // 目标电脑 IP 和共享名
        public string remoteDbUserdbPath { get; set; } = @"\\127.0.0.1\Access\Users_Data.mdb"; // 目标电脑 IP 和共享名
        public string remoteDbDatadbPath { get; set; } = @"\\127.0.0.1\Access\生产数据"; // 目标电脑 IP 和共享名
        public string serverDbtask { get; set; } = @"http://127.0.0.1:4444/dbtask/"; // 目标电脑 IP 和共享名

        //public string sharedbPath { get; set; } = @"\\192.168.1.232\生产数据"; // 目标电脑 IP 和共享名
        //public string remoteDbUserdbPath { get; set; } = @"\\192.168.1.232\BYD_Users\Users_Data.mdb"; // 目标电脑 IP 和共享名
        //public string remoteDbDatadbPath { get; set; } = @"\\192.168.1.232\生产数据"; // 目标电脑 IP 和共享名
        //public string serverDbtask { get; set; } = @"http://192.168.1.232:4444/dbtask/"; // 目标电脑 IP 和共享名

        #endregion

        #region 端口号使用情况

        //  看板后端WebApiPort
        public int WebApiPort { get; set; } = 4441;
        //  看板前端
        public int WebPort { get; set; } = 4442;
        //  API接口，登录
        public int ApiPort { get; set; } = 4443;
        //  测试数据上传
        public int DbTaskQueuePort { get; set; } = 4444;
        //  切换型号
        public int ModelSenderPort { get; set; } = 5555;
        //  监听客户端
        public int MonitorPort { get; set; } = 8881;
        //  服务端心跳
        public int HeartbeatPort { get; set; } = 6666;

        #endregion

        #region  UID

        // UID
        public string UIDIP = "192.168.4.91"; 

        public int UIDPort { get; set; } = 1030;

        #endregion

        #region server 

        //public string ServerIP { get; set; } = "192.168.1.232"; //服务器IP 
        public string ServerIP { get; set; } = "127.0.0.1"; //服务器IP 

        #endregion

        #region Sqlserver

        public string SqlServerConn { get; set; } = "Server=127.0.0.1;database=BYD;uid=sa;pwd=root";

        //public string SqlServerConn { get; set; } = "Server=192.168.1.232;database=BYD;uid=sa;pwd=GETLOON#2025";

        #endregion

        #region BYD-MES参数

        public string  IP { get; set; } = "127.0.0.1"; //服务器IP 
        public string PORT { get; set; } = "50000"; //服务器端口号 
        public string URL { get; set; } = "/manufacturing/IntegrationServlet?InterType=XML"; //URL 
        public string Site { get; set; } = "X150"; //站点 MES 工厂代码
        public string UserName { get; set; } = "4111348"; //用户 
        public string Password { get; set; } = "4111348"; //用户密码 
        public string Resource { get; set; } = "Z1-SAB-01-ZP"; //资源号 MES 中对应资源
        public string Operation { get; set; } = "SAB-ZP"; //操作号 MES 中对应工序
        public string NcCode { get; set; } = "XX_FALL"; //不良代码 不合格代码 
        public int TimeOut { get; set; } = 3000; //请求服务器超时

        #endregion
    }
}
