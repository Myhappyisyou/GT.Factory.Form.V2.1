using GT_Common.DriverForm.Login;
using GT_Common.Helper;
using GT_Common.Helper.Database.Core;
using GT_Common.Model;
using GT_Common.MyEnum;
using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GT_Common
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
                var loaded = SerializeHelper.LoadXml<Config>(PathCenter.ConfigFile("CommonConfig.xml"));
                if (loaded != null)
                    setting = loaded;
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
            SerializeHelper.SaveXml<Config>(Config.Instance, PathCenter.ConfigFile("CommonConfig.xml"));
        }

        #endregion

        #region 登陆缓存

        // ✅ 新增登录缓存控制参数
        [Category("登陆系统配置"), DisplayName("启用登录超时")]
        public bool EnableLoginTimeout { get; set; } = true;

        [Category("登陆系统配置"), DisplayName("登录超时(分钟)")]
        public int LoginTimeoutMinutes { get; set; } = 5;

        #endregion

        #region title信息

        public string ProcessName { get; set; } = "OP070摩擦焊接";

        public string Vison { get; set; } = "V1.0";

        #endregion

        #region 公司信息

        public string SupplierInformation { get; set; } = "均普";

        public string AfterSaleCall { get; set; } = "*******";

        #endregion

        #region plc配方数据信息

        [Category("PLC配置"), DisplayName("MES触发地址")]
        [Browsable(false)]
        public string MES_Trigger { get; set; } = "DB5.DBW0";
        
        [Category("PLC配置"), DisplayName("配方号地址")]
        [Browsable(false)]
        public string MES_Recipe_Code { get; set; } = "DB5.DBW2";
        
        [Category("PLC配置"), DisplayName("结果反馈地址")]
        [Browsable(false)] 
        public string PLC_Feedback_Result { get; set; } = "DB5.DBW4";
        
        [Category("PLC配置"), DisplayName("换型完成地址")]
        [Browsable(false)]
        public string PLC_Changeover_Finished { get; set; } = "DB5.DBW6";
       
        [Category("PLC配置"), DisplayName("当前型号地址")]
        [Browsable(false)]
        public string PLC_Current_Type { get; set; } = "DB5.DBW8";

        #endregion

        #region Access

        //public string userdbPath { get; set; } = @"\\192.168.4.239\Access\Users_Data.mdb"; // 目标电脑 IP 和共享名
        //public string datadbPath { get; set; } = @"\\192.168.4.239\Access\生产数据"; // 目标电脑 IP 和共享名

        //public string sharedbPath { get; set; } = @"\\127.0.0.1\Access"; // 目标电脑 IP 和共享名
        //public string remoteDbUserdbPath { get; set; } = @"\\127.0.0.1\Access\Users_Data.mdb"; // 目标电脑 IP 和共享名
        //public string remoteDbDatadbPath { get; set; } = @"\\127.0.0.1\Access\生产数据"; // 目标电脑 IP 和共享名
        //public string serverDbtask { get; set; } = @"http://127.0.0.1:4444/dbtask/"; // 目标电脑 IP 和共享名
        //public string serverApi { get; set; } = @"http://127.0.0.1:4443/"; // 目标电脑 IP 和共享名


        //public string sharedbPath { get; set; } = @"\\192.168.1.232\生产数据"; // 目标电脑 IP 和共享名
        //public string remoteDbUserdbPath { get; set; } = @"\\192.168.1.232\BYD_Users\Users_Data.mdb"; // 目标电脑 IP 和共享名
        //public string remoteDbDatadbPath { get; set; } = @"\\192.168.1.232\生产数据"; // 目标电脑 IP 和共享名
        [Category("服务配置"), DisplayName("用户信息数据库")]
        [Browsable(false)]
        public string UserdbPath { get; set; } = @"D:\BYD_Users\Users_Data.mdb"; // 目标电脑 IP 和共享名

        [Category("服务配置"), DisplayName("DB任务地址")]
        [Browsable(false)]
        public string ServerDbtask { get; set; } = @"http://192.168.1.232:4444/dbtask/"; // 目标电脑 IP 和共享名
       
        [Category("服务配置"), DisplayName("API地址")]
        [Browsable(false)]
        public string ServerApi { get; set; } = @"http://192.168.1.232:4443/"; // 目标电脑 IP 和共享名

        [Category("数据库配置"), DisplayName("SQL连接字符串"),]
        [Browsable(false)]
        public string ServerIP { get; set; } = "192.168.1.232"; //服务器IP 
        //public string ServerIP { get; set; } = "127.0.0.1"; //服务器IP 

        #endregion

        //public List<ActionTipModel> lsActionTipModels { get; set; }


        #region  过站检前站及当站名

        public string CurrProcessNo { get; set; } = "OP110";

        public string FrontProcessNo { get; set; } = "10";

        #endregion

        #region  UID

        public bool EnableCardReader { get; set; } = false;

        // UID
        public string UIDIP { get; set; } = "192.168.4.91"; //    OP070

        public int UIDPort { get; set; } = 1030;

        #endregion

        #region csv保存

        //  保存天数
        [Category("数据保存配置"), DisplayName("保留天数")]
        [Range(0, int.MaxValue, ErrorMessage = "保留天数不能0")]
        public int RretainDays { get; set; } = 60;

        //  保存天数
        [Category("返工配置"), DisplayName("返工次数")]
        [Range(0, int.MaxValue, ErrorMessage = "返工次数不能小于0")]
        public int ReworkCountLimit { get; set; } = 3;

        #endregion

        #region 样件配置

        //  水爆件sn标识
        [Category("样件配置"), DisplayName("水爆标识")]
        public string WaterBurstMark { get; set; } = "HB000";

        //  切割件sn标识
        [Category("样件配置"), DisplayName("切割标识")]
        public string CutPieceMark { get; set; } = "WSA00";

        #endregion


        #region 数据库

        [Category("本地Access数据配置"), DisplayName("数据存储路径")]
        [Browsable(false)]
        public string DatadbPath { get; set; } = @"D:\生产数据"; // 目标电脑 IP 和共享名

        //public string SqlServerConn { get; set; } = "Server=.;database=BYD;uid=sa;pwd=root";
        [Category("数据库配置"), DisplayName("SQL连接字符串"),]
        [Browsable(false)]
        public string SqlServerConn { get; set; } = "Server=.;database=BYD;uid=sa;pwd=root";

        #endregion

        #region BYD-MES参数

        [Category("MES配置"), DisplayName("服务器IP")]
        public string  IP { get; set; } = "127.0.0.1"; //服务器IP 

        [Category("MES配置"), DisplayName("端口")]
        public string PORT { get; set; } = "50000"; //服务器端口号 
        
        [Category("MES配置"), DisplayName("URL")]
        public string URL { get; set; } = "/manufacturing/IntegrationServlet?InterType=XML"; //URL 
        
        [Category("MES配置"), DisplayName("站点")]
        public string Site { get; set; } = "X150"; //站点 MES 工厂代码
        
        [Category("MES配置"), DisplayName("用户名")]
        [Browsable(false)]
        public string UserName { get; set; } = "4111348"; //用户 
        
        [Category("MES配置"), DisplayName("密码"), PasswordPropertyText(true)]
        [Browsable(false)]
        public string Password { get; set; } = "4111348"; //用户密码 
        
        [Category("MES配置"), DisplayName("资源号")]
        public string Resource { get; set; } = "Z1-SAB-01-ZP"; //资源号 MES 中对应资源
        
        [Category("MES配置"), DisplayName("工序")]
        public string Operation { get; set; } = "SAB-ZP"; //操作号 MES 中对应工序
        
        [Category("MES配置"), DisplayName("不良代码")]
        public string NcCode { get; set; } = "XX_FALL"; //不良代码 不合格代码 
        
        [Category("MES配置"), DisplayName("超时(ms)")]
        public int TimeOut { get; set; } = 3000; //请求服务器超时
        
        [Category("MES配置"), DisplayName("版本号")]
        public string MesVison { get; set; } = "V1.0"; //请求服务器超时

        [Category("MES配置"), DisplayName("物料版本")]
        public string MeterVison { get; set; } = "V1.0"; //请求服务器超时

        [Category("MES_API配置"), DisplayName("接口Url")]
        public string ApiUrl { get; set; } = "http://qngcpms.byd.com:2080"; //API接口路由

        [Category("MES_API配置"), DisplayName("Appid")]
        public string AppId { get; set; } = "L1120"; //API-AppID

        [Category("MES_API配置"), DisplayName("Token")]
        public string Token { get; set; } = "9f33ddd9e93fbabd70c317242dd9c1d9"; //API-AppID

        [Category("MES_API配置"), DisplayName("根据零部件查询总成信息")]
        public string GetAssemblySnTestInfo { get; set; } = "pms-api/getAssemblySnTestInfo"; //API-AppID

        [Category("MES_API配置"), DisplayName("工站编码")]
        public string StationCode { get; set; } = "SL-LGG-ZP-ZD-01-ZDLH"; //API-AppID

        [Category("MES_API配置"), DisplayName("测试项")]
        public string TestItem { get; set; } = "成品重量"; //API-AppID

        #endregion

        #region 端口号使用情况

        //  看板后端WebApiPort
        [Category("端口配置"), DisplayName("WebApi端口")]
        [Browsable(false)]
        public int WebApiPort { get; set; } = 4441;
        
        //  看板前端
        [Category("端口配置"), DisplayName("前端端口")]
        [Browsable(false)]
        public int WebPort { get; set; } = 4442;
        
        //  API接口，登录
        [Category("端口配置"), DisplayName("API端口")]
        [Browsable(false)]
        public int ApiPort { get; set; } = 4443;
        
        //  测试数据上传
        [Category("端口配置"), DisplayName("数据上传端口")]
        [Browsable(false)]
        public int DbTaskQueuePort { get; set; } = 4444;
        
        //  切换型号
        [Category("端口配置"), DisplayName("型号切换端口")]
        [Browsable(false)]
        public int ModelSenderPort { get; set; } = 5555;
        
        //  监听客户端
        [Category("端口配置"), DisplayName("监听端口")]
        [Browsable(false)]
        public int MonitorPort { get; set; } = 8881;
        
        //  服务端心跳
        [Category("端口配置"), DisplayName("心跳端口")]
        [Browsable(false)]
        public int HeartbeatPort { get; set; } = 6666;

        #endregion

        #region 登录配置

        /// <summary>
        /// 登录模块配置
        /// </summary>
        [Category("登录页面配置"), DisplayName("登录页面")]
        public LoginConfig Login { get; set; } = new LoginConfig
        {
            IdentityTypes = new List<IdentityType>
            {
                IdentityType.RoleSelect
            },
            NeedMesValidation = false,
            Roles = new List<UserLevel>
            {
                UserLevel.ADM,
                 UserLevel.OP,
                 UserLevel.ME,
            }
        };

        #endregion


        #region 数据库

        /// <summary>
        /// 数据库表名前缀
        /// 例如：GT、SH
        /// </summary>
        [Category("数据库配置"), DisplayName("表名前缀")]
        [Description("数据库表名前缀，例如 GT 或 SH")]
        public TablePrefixType TablePrefix { get; set; } = TablePrefixType.GT;

       
        [Category("数据库配置"), DisplayName("数据库配置")]
        public MultiDatabaseOptions DatabaseOptions { get; set; } = new MultiDatabaseOptions
        {
            Primary = new DatabaseNodeOptions
            {
                Enabled=true,
                DbType = DatabaseType.SqlServer,
                ConnectionString = "Server=.;database=BYD;uid=sa;pwd=root"
            },
            LocalCache = new DatabaseNodeOptions
            {
                Enabled = true,
                DbType = DatabaseType.SQLite,
                ConnectionString = "Data Source=XMOilPumpData.sqlite"
            }
        };

      
        #endregion
    }
}
