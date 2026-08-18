using GT.Factory.Common.Helper;
using GT.Factory.Station.Helper;
using System;

namespace GT.Factory.Station.OP070
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
        public string datadbPath = @"D:\生产数据"; // 目标电脑 IP 和共享名

        #region title信息
        public string StationName { get; set; } = "工位7";
        public string ProcessNo { get; set; } = "工位7";

        public string ProcessName { get; set; } = "OP070摩擦焊接";
        public string Vison { get; set; } = "V1.0";

        #endregion

        #region  过站检前站及当站名

        public string CurrProcessNo { get; set; } = "1100";
        public string FrontProcessNo { get; set; } = "10";

        #endregion

        #region  UID

        // UID
        public string UIDIP { get; set; } = "192.168.4.91"; //    OP070

        public int UIDPort { get; set; } = 1030;
        #endregion

    }
}
