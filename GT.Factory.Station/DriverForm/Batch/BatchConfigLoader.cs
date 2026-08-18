using GT.Factory.Common.Helper;
using GT.Factory.Station.Helper;
using GT.Factory.Station.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.DriverForm.Batch
{
   
    public static class BatchConfigLoader
    {
        private static readonly string FilePath = Path.Combine(PathCenter.ConfigFile("batchConfig.json"));

        public static void Save(List<ClientBatchConfig> clientBatches)
        {
            var json = JsonConvert.SerializeObject(clientBatches, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static List<ClientBatchConfig> Load()
        {
            if (!File.Exists(FilePath)) return new List<ClientBatchConfig>();

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<ClientBatchConfig>>(json);
        }
    }
}
