using GT.Factory.Common.Helpers;
using GT.Factory.Station.Model;
using System;
using System.ComponentModel;

namespace GT.Factory.Station
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

        [Category("机台配置"), DisplayName("工序号")]
        [Browsable(false)]
        public string ProcessNo { get; set; } = "OP010";


        [Category("机台配置"), DisplayName("工序名")]
        [Browsable(false)]
        public string ProcessName { get; set; } = "OP010管壳上料&激光刻码";

        [Category("机台配置"), DisplayName("版本")]
        [Browsable(false)]
        public string vison { get; set; } = "V1.0";

        #endregion


        #region  过站检前站及当站名

        [Category("过站检"), DisplayName("前道工序号")]
        [Browsable(false)]

        public string FrontProcessNo { get; set; } = "10";

        #endregion

    }
}
