using GT.Factory.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskContracts;

namespace GT.Factory.Server.Help
{
    public static class LocalCache
    {
        //private static readonly string CacheFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
        //private static readonly string CacheFolder = PathCenter.HistoryFile(Path.Combine("PendingTasks", "PendingTasks.json"));
        static LocalCache()
        {
            Directory.CreateDirectory(PathCenter.HistoryFile("Cache"));
        }

        public static void Save(DbTask task)
        {
            //string file = Path.Combine(CacheFolder, $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{task.TaskType}.json");
            string file = PathCenter.HistoryFile(Path.Combine("Cache", $"{DateTime.Now:yyyyMMdd_HHmmssfff}_{task.TaskType}.json"));

            File.WriteAllText(file, JsonConvert.SerializeObject(task));
        }
    }
}
