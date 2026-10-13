using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.MES.BydMes
{
    /// <summary>
    /// MES模块配置。
    /// </summary>
    public class MesConfig
    {
        #region BYD MES

        [Category("MES配置")]
        [DisplayName("服务器IP")]
        public string IP { get; set; }

        [Category("MES配置")]
        [DisplayName("端口")]
        public string PORT { get; set; }

        [Category("MES配置")]
        [DisplayName("URL")]
        public string URL { get; set; }

        [Category("MES配置")]
        [DisplayName("站点")]
        public string Site { get; set; }

        [Category("MES配置")]
        [DisplayName("用户名")]
        [Browsable(false)]
        public string UserName { get; set; }

        [Category("MES配置")]
        [DisplayName("密码")]
        [PasswordPropertyText(true)]
        [Browsable(false)]
        public string Password { get; set; }

        [Category("MES配置")]
        [DisplayName("资源号")]
        public string Resource { get; set; }

        [Category("MES配置")]
        [DisplayName("工序")]
        public string Operation { get; set; }

        [Category("MES配置")]
        [DisplayName("不良代码")]
        public string NcCode { get; set; }

        [Category("MES配置")]
        [DisplayName("超时(ms)")]
        public int TimeOut { get; set; }

        [Category("MES配置")]
        [DisplayName("版本号")]
        public string MesVison { get; set; }

        [Category("MES配置")]
        [DisplayName("物料版本")]
        public string MeterVison { get; set; }

        [Category("MES配置")]
        [DisplayName("前道资源号")]
        public string FrontResource { get; set; }

        [Category("MES配置")]
        [DisplayName("前道工序")]
        public string FrontOperation { get; set; }

        [Category("MES配置")]
        [DisplayName("前道查询项")]
        public string FrontTestItem { get; set; }

        #endregion


        #region MES API

        [Category("MES_API配置")]
        [DisplayName("接口Url")]
        public string ApiUrl { get; set; }

        [Category("MES_API配置")]
        [DisplayName("Appid")]
        public string AppId { get; set; }

        [Category("MES_API配置")]
        [DisplayName("Token")]
        public string Token { get; set; }

        [Category("MES_API配置")]
        [DisplayName("根据零部件查询总成信息")]
        public string GetAssemblySnTestInfo { get; set; }

        [Category("MES_API配置")]
        [DisplayName("工站编码")]
        public string StationCode { get; set; }

        [Category("MES_API配置")]
        [DisplayName("测试项")]
        public string TestItem { get; set; }

        #endregion


        public MesConfig()
        {
            IP = "127.0.0.1";
            PORT = "50000";
            URL = "/manufacturing/IntegrationServlet?InterType=XML";

            Site = "X150";

            UserName = "";
            Password = "";

            Resource = "";
            Operation = "";

            NcCode = "XX_FALL";

            TimeOut = 3000;

            MesVison = "V1.0";
            MeterVison = "V1.0";

            FrontResource = "";
            FrontOperation = "";
            FrontTestItem = "";

            ApiUrl = "";
            AppId = "";
            Token = "";

            GetAssemblySnTestInfo =
                "pms-api/getAssemblySnTestInfo";

            StationCode = "";
            TestItem = "";
        }
    }
}