using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace GT_Common.Helper
{
    public static class CsvSaver
    {
        private static readonly object _fileLock = new object();

        /// <summary>
        /// 保存 List<T> 为 CSV，按月份文件夹、按日期文件名
        /// </summary>
        public static void SaveListToMonthlyCsv<T>(
            List<T> list,
            string baseFolder,
            bool append = false,
            int retainDays = 0 // 0 表示不清理
        )
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List 为空");
            if (string.IsNullOrWhiteSpace(baseFolder))
                throw new ArgumentException("基础路径无效");

            var table = ConvertToDataTable(list);
            SaveDataTableToMonthlyCsv(table, baseFolder, append, retainDays);
        }

        public static void BasicSave(string fileName, string stationNo, StringBuilder contentToAppend)
        {
            try
            {
                //string resultFolder = Path.Combine(
                //    @"D:\TWH",
                //    DateTime.Now.ToString("yyyy"),
                //    DateTime.Now.ToString("MM"),
                //    DateTime.Now.ToString("dd")
                //);

                string resultFolder = Path.Combine(
                      PathCenter.History,   // D:\GT_System\MES_OP010\Data\History
                      HistoryKind.TWH.GetFolderName(),
                      DateTime.Now.ToString("yyyy"),
                      DateTime.Now.ToString("MM"),
                      DateTime.Now.ToString("dd")
                  );

                // 如果目录不存在，先创建
                Directory.CreateDirectory(resultFolder);


                // 自动创建多级目录
                Directory.CreateDirectory(resultFolder);

                // 文件名带毫秒，避免高并发冲突
                string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string filePath = Path.Combine(resultFolder, $"{fileName}_{stationNo}_{timeStamp}.csv");

                lock (_fileLock)
                {
                    File.WriteAllText(filePath, contentToAppend.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"保存失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存 DataTable 为 CSV 文件，可追加，线程安全，支持旧文件自动清理
        /// </summary>
        public static void SaveDataTableToMonthlyCsv(
            DataTable table,
            string baseFolder,
            bool append = false,
            int retainDays = 0 // 0 表示不清理
        )
        {
            if (table == null || table.Rows.Count == 0)
                throw new ArgumentException("DataTable 为空");

            string monthFolder = DateTime.Now.ToString("yyyy-MM");
            string fullFolderPath = Path.Combine(baseFolder, monthFolder);
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.csv";
            string filePath = Path.Combine(fullFolderPath, fileName);

            lock (_fileLock) // 多线程写入锁定
            {
                if (!Directory.Exists(fullFolderPath))
                    Directory.CreateDirectory(fullFolderPath);

                bool fileExists = File.Exists(filePath);

                using (var writer = new StreamWriter(filePath, append, new UTF8Encoding(true)))
                {
                    try
                    {
                        if (!append || !fileExists)
                        {
                            // 写表头
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                writer.Write(table.Columns[i].ColumnName);
                                if (i < table.Columns.Count - 1)
                                    writer.Write(",");
                            }
                            writer.WriteLine();
                        }

                        // 写内容
                        foreach (DataRow row in table.Rows)
                        {
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                string value = row[i]?.ToString().Replace("\"", "\"\"") ?? "";
                                writer.Write($"\"{value}\"");
                                if (i < table.Columns.Count - 1)
                                    writer.Write(",");
                            }
                            writer.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"CSV 保存失败: {ex.Message}");
                    }
                }

                Log.Information($"{(append && fileExists ? "追加" : "写入")} CSV 文件成功：{filePath}");

                // 自动清理旧文件
                if (retainDays > 0)
                {
                    CleanOldFiles(fullFolderPath, retainDays);
                }
            }
        }

        /// <summary>
        /// 删除超出保留天数的旧文件（按文件名日期格式识别）
        /// </summary>
        private static void CleanOldFiles(string folderPath, int retainDays)
        {
            try
            {
                var cutoffDate = DateTime.Today.AddDays(-retainDays);

                var csvFiles = Directory.GetFiles(folderPath, "*.csv");

                foreach (var file in csvFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file); // e.g. 2025-07-10
                    if (DateTime.TryParse(fileName, out DateTime fileDate))
                    {
                        if (fileDate < cutoffDate)
                        {
                            File.Delete(file);
                            Log.Information($"已删除旧文件: {file}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"清理旧文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// List<T> 转换为 DataTable
        /// </summary>
        private static DataTable ConvertToDataTable<T>(List<T> list)
        {
            var table = new DataTable(typeof(T).Name);
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                var displayAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
                string columnName = displayAttr?.DisplayName ?? prop.Name;
                table.Columns.Add(columnName, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in list)
            {
                var values = props.Select(p => p.GetValue(item, null) ?? DBNull.Value).ToArray();
                table.Rows.Add(values);
            }

            return table;
        }
    }
}
