using GT.Factory.Common.Helpers;
using GT.Factory.Station.Model;
using System;

namespace GT.Factory.Station.OP010
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
        
        #region title信息

        public string ProcessNo { get; set; } = "OP010";
        public string StationName { get; set; } = "工位1"; 
        public string ProcessName { get; set; } = "OP010管壳上料&激光刻码";
        public string vison { get; set; } = "V1.0";

        #endregion

        public ActionTipModel[] actionTipModels { get; set; } =
        {
            new ActionTipModel{ Code=1,Tips="自动模式" },
            new ActionTipModel{ Code=2,Tips="手动模式" },
        };
      
        #region  过站检前站及当站名

        public string CurrProcessNo { get; set; } = "1100";
        public string FrontProcessNo { get; set; } = "10";

        #endregion

    }
}
