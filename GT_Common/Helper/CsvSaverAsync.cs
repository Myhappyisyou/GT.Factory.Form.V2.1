using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class CsvSaverAsync
    {
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public static async Task SaveListToMonthlyCsvAsync<T>(
            List<T> list,
            string baseFolder,
            bool append = false,
            int retainDays = 0
        )
        {
            if (list == null || list.Count == 0)
                throw new ArgumentException("List 为空");

            if (string.IsNullOrWhiteSpace(baseFolder))
                throw new ArgumentException("基础路径无效");

            var table = ConvertToDataTable(list);
            await SaveDataTableToMonthlyCsvAsync(table, baseFolder, append, retainDays);
        }

        public static async Task SaveDataTableToMonthlyCsvAsync(
            DataTable table,
            string baseFolder,
            bool append = false,
            int retainDays = 0
        )
        {
            if (table == null || table.Rows.Count == 0)
                throw new ArgumentException("DataTable 为空");

            string monthFolder = DateTime.Now.ToString("yyyy-MM");
            string fullFolderPath = Path.Combine(baseFolder, monthFolder);
            string fileName = $"{DateTime.Now:yyyy-MM-dd}.csv";
            string filePath = Path.Combine(fullFolderPath, fileName);

            await _fileLock.WaitAsync(); // 异步锁定
            try
            {
                if (!Directory.Exists(fullFolderPath))
                    Directory.CreateDirectory(fullFolderPath);

                bool fileExists = File.Exists(filePath);

                using (var fs = new FileStream(
                                                 filePath,
                                                 append ? FileMode.Append : FileMode.Create,
                                                 FileAccess.Write,
                                                 FileShare.ReadWrite))   // 允许其他程序（如 Excel）同时读取
                using (var writer = new StreamWriter(fs, new UTF8Encoding(true)))
                {
                    if (!append || !fileExists)
                    {
                        // 写表头
                        var header = string.Join(",", table.Columns.Cast<DataColumn>().Select(c => $"\"{c.ColumnName}\""));
                        await writer.WriteLineAsync(header);
                    }

                    // 写内容
                    foreach (DataRow row in table.Rows)
                    {
                        var fields = table.Columns.Cast<DataColumn>()
                            .Select(col => $"\"{(row[col]?.ToString()?.Replace("\"", "\"\"") ?? "")}\"");

                        string line = string.Join(",", fields);
                        await writer.WriteLineAsync(line);
                    }
                }

                Console.WriteLine($"✅ {(append && fileExists ? "追加" : "写入")} CSV 成功：{filePath}");

                // 清理旧文件
                if (retainDays > 0)
                    CleanOldFiles(fullFolderPath, retainDays);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private static void CleanOldFiles(string folderPath, int retainDays)
        {
            try
            {
                var cutoff = DateTime.Today.AddDays(-retainDays);
                var files = Directory.GetFiles(folderPath, "*.csv");

                foreach (var file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    if (DateTime.TryParse(name, out DateTime fileDate))
                    {
                        if (fileDate < cutoff)
                        {
                            File.Delete(file);
                            Console.WriteLine($"🗑 已删除旧文件: {file}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ 清理旧文件失败: {ex.Message}");
            }
        }

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
