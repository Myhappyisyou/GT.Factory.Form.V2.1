using GT_Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public class PlcConsumables
    {
        [DisplayName("易损件名称")]
        public string Name { get; set; }

        [DisplayName("易损件理论使用次数")]
        public string TheoreticalCountAddress { get; set; }

        [DisplayName("易损件已使用次数")]
        public string UsedCountAddress { get; set; }

        [DisplayName("易损件剩余使用次数")]
        public string RemainderCountAddress { get; set; }
    }
    public class ConsumablesConfigManager
    {
        //private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlcConsumables.json");
        private static readonly string FilePath = PathCenter.ConfigFile("PlcConsumables.json");

        public static void Save(List<PlcConsumables> lsPlcConsumables)
        {
            var json = JsonConvert.SerializeObject(lsPlcConsumables, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static List<PlcConsumables> Load()
        {
            if (!File.Exists(FilePath)) return new List<PlcConsumables>();

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<PlcConsumables>>(json);
        }
    }
}
