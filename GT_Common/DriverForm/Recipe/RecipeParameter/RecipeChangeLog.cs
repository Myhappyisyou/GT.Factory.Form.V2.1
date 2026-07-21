using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Recipe.RecipeParameter
{
    public class RecipeChangeLog
    {
        public DateTime ChangeTime { get; set; }
        public string UserName { get; set; }      // 修改人
        public string RecipeName { get; set; }    // 配方名
        public string ParameterPath { get; set; } // 参数路径（如：Limit.Temperature.Max）
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Source { get; set; }        // "Manual", "Import", "MES" 等
    }

    public static class RecipeChangeLogReader
    {
        public static List<RecipeChangeLog> ReadLogs(DateTime date)
        {
            var list = new List<RecipeChangeLog>();

            string filePath = Path.Combine(
                PathCenter.Log,
                "RecipeChangeLogs",
                date.ToString("yyyy"),
                date.ToString("MM"),
                date.ToString("dd"),
                $"RecipeChange-{date:yyyy-MM-dd}.txt"
            );

            if (!File.Exists(filePath))
                return list;

            foreach (var line in File.ReadLines(filePath))
            {
                try
                {
                    string decoded = Encoding.UTF8.GetString(
                        Convert.FromBase64String(line));

                    var parts = decoded.Split('\t');

                    if (parts.Length < 7) continue;

                    list.Add(new RecipeChangeLog
                    {
                        ChangeTime = DateTime.Parse(parts[0]),
                        UserName = parts[1],
                        RecipeName = parts[2],
                        ParameterPath = parts[3],
                        OldValue = parts[4],
                        NewValue = parts[5],
                        Source = parts[6]
                    });
                }
                catch
                {
                    // 忽略坏数据
                }
            }

            return list;
        }
    }


    public static class RecipeChangeLogger
    {
       
        //private static readonly string LogFilePath =
        //    Path.Combine("D:\\MESLog", "RecipeChangeLogs", $"{DateTime.Now:yyyy-MM}RecipeChange.log");

        private static readonly string LogFilePath =
             Path.Combine(
                      PathCenter.Log,   // D:\GT_System\MES_OP010\Data\History
                      "RecipeChangeLogs",
                      DateTime.Now.ToString("yyyy"),
                      DateTime.Now.ToString("MM"),
                      DateTime.Now.ToString("dd"),
                      $"RecipeChange-{DateTime.Now:yyyy-MM-dd}.txt"
                  );

        private static string GetLogFilePath()
        {
            DateTime now = DateTime.Now;

            return Path.Combine(
                PathCenter.Log,
                "RecipeChangeLogs",
                now.ToString("yyyy"),
                now.ToString("MM"),
                now.ToString("dd"),
                $"RecipeChange-{now:yyyy-MM-dd}.txt"
            );
        }

        public static void LogChange(RecipeChangeLog log)
        {
            // 创建目录
            string dir = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string line = string.Format(
                "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                log.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                log.UserName,
                log.RecipeName,
                log.ParameterPath,
                log.OldValue,
                log.NewValue,
                log.Source
            );
            string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(line));
            File.AppendAllLines(GetLogFilePath(), new[] { encoded });

        }
    }
}
