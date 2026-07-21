using GT_Common.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class ActionTipModel
    {
        public  int Code { get; set; }
        public  string Tips { get; set; }

    }

    public static class ActionTipModelConfigManager
    {
        //private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"actionTips\ActionTips.json");
        private static readonly string FilePath = PathCenter.ConfigFile(Path.Combine("actionTips", "ActionTips.json"));

        public static void Save(List<ActionTipModel> lsAlarms)
        {
            var json = JsonConvert.SerializeObject(lsAlarms, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static List<ActionTipModel> Load()
        {
            if (!File.Exists(FilePath)) return new List<ActionTipModel>();

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<ActionTipModel>>(json);
        }
    }
}
