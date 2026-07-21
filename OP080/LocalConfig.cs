using GT_Common.Helper;
using GT_Common.Model;
using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OP080
{

    public class LocalConfig
    {
        #region xml
        private static readonly Lazy<LocalConfig> lazy = new Lazy<LocalConfig>(
        () =>
        {
            LocalConfig setting = new LocalConfig();
            try
            {
                var loaded = SerializeHelper.LoadXml<LocalConfig>(PathCenter.ConfigFile("LocalConfig.xml"));
                if (loaded != null)
                    setting = loaded;
                //setting = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Setting.json"));
            }
            catch { }
            return setting;
        });

        private LocalConfig() { }

        public static LocalConfig Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public static void Save()
        {
            //SerializeHelper.SaveXmlEncode<Config>(Config.Instance, "LocalConfig.xml");
            SerializeHelper.SaveXml<LocalConfig>(LocalConfig.Instance, PathCenter.ConfigFile("LocalConfig.xml"));
            //File.WriteAllText("Setting.json", JsonConvert.SerializeObject(Instance));
        }
        #endregion
       
        public string userdbPath = @"D:\BYD_Users\Users_Data.mdb"; // 目标电脑 IP 和共享名
        public string datadbPath = @"D:\生产数据"; // 目标电脑 IP 和共享名 StationName

        #region title信息
        public string ProcessNo { get; set; } = "工位8";
        public string StationName { get; set; } = "工位8";

        public string ProcessName { get; set; } = "OP080高压充气&小球焊接";
        public string Vison { get; set; } = "V1.0";

        #endregion

        #region  过站检前站及当站名

        public string CurrProcessNo { get; set; } = "OP080";

        public string FrontProcessNo { get; set; } = "10";

        #endregion

        #region  UID

        // UID
        public string UIDIP { get; set; } = "192.168.4.121"; //    OP070

        public int UIDPort { get; set; } = 1030;

        #endregion

        #region 电阻焊监控

        public string Thw_ST70_IP { get; set; } = "192.168.4.183";
        public int Thw_ST70_Port { get; set; } = 4002;

        public string Thw_ST80_IP { get; set; } = "192.168.4.183";
        public int Thw_ST80_Port { get; set; } = 4003;

        public string Thw_ST90_IP { get; set; } = "192.168.4.183";
        public int Thw_ST90_Port { get; set; } = 4004;


        #endregion

    }
}
