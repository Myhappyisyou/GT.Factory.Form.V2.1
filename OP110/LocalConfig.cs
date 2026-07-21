using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OP110
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
            //SerializeHelper.SaveXml<LocalConfig>(LocalConfig.Instance, "LocalConfig.xml");
            SerializeHelper.SaveXml<LocalConfig>(LocalConfig.Instance, PathCenter.ConfigFile("LocalConfig.xml"));

            //File.WriteAllText("Setting.json", JsonConvert.SerializeObject(Instance));
        }
        #endregion
        
        //  查询字段名
        public string LengthProcessNo { get; set; } = "OP070";

        public string LengthFieldName { get; set; } = "data031";


        #region title信息

        public string StationName { get; set; } = "工位11";
        public string ProcessNo { get; set; } = "工位11";

        public string ProcessName { get; set; } = "OP110装短路环测电阻";
        public string Vison { get; set; } = "V1.0";

        #endregion

        #region  过站检前站及当站名

        public string CurrProcessNo { get; set; } = "1100";

        public string FrontProcessNo { get; set; } = "10";

        #endregion

        #region  UID

        // UID
        public string UIDIP { get; set; } = "192.168.6.98"; //    OP070

        public int UIDPort { get; set; } = 1030;

        #endregion

        #region 电测仪器

        // 电测仪器1
        public string ST40_1_IP = "192.168.6.97"; 

        public int ST40_1_Port { get; set; } = 1030;

        // 电测仪器2
        public string ST40_2_IP = "192.168.6.97"; 

        public int ST40_2_Port { get; set; } = 1031;

        #endregion

    }
}
