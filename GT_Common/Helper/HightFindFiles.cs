using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class HightFindFiles
    {
        /// <summary>
        /// 查询文件路径下文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchString"></param>
        /// <param name="fileExtensions"></param>
        /// <returns></returns>
        public static ConcurrentBag<string> FindFilesContainingStringParallel(string folderPath, string searchString, string[] fileExtensions)
        {
            ConcurrentBag<string> result = new ConcurrentBag<string>();

            foreach (var pattern in fileExtensions)
            {
                string[] files = Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories);

                // 使用并行处理文件
                Parallel.ForEach(files, (file) =>
                {
                    if (FileContainsString(file, searchString))
                    {
                        result.Add(file); // 并发安全集合
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// 查询文件路径下文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchString"></param>
        /// <param name="fileExtensions"></param>
        /// <returns></returns>
        public static ConcurrentBag<string> FindFilesContainingStringParallel(string folderPath, string searchString)
        {
            ConcurrentBag<string> result = new ConcurrentBag<string>();
        
            string[] files = Directory.GetFiles(folderPath);
            // 使用并行处理文件
            Parallel.ForEach(files, (file) =>
            {
                if (file.Contains(searchString))
                {

                    result.Add(file); // 并发安全集合
                }
            });
            //Console.WriteLine(2);
            return result;
        }

        /// <summary>
        /// 判断文件包含指定字符
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public static bool FileContainsString(string filePath, string searchString)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Contains(searchString))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // 忽略文件访问异常
            }
            return false;
        }

        /// <summary>
        /// 判断当前日期和前一天日期是否包含文件
        /// </summary>
        /// <param name="count"></param>
        /// <param name="ftpBasePath"></param>
        /// <param name="targetFileName"></param>
        /// <param name="ftpUsername"></param>
        /// <param name="ftpPassword"></param>
        /// <returns></returns>
        public static List<string> CheckFileInCurrentAndPreviousDay( string folderPath, string searchString)
        {
            List<string> matchingFiles = new List<string>();
            // Get current and previous date
            DateTime currentDate = DateTime.Now;
            string currentDateFolder = Path.Combine(folderPath, currentDate.ToString("yyyyMMdd"));
            string previousDateFolder = Path.Combine(folderPath, currentDate.AddDays(-1).ToString("yyyyMMdd"));

            // 检查当前日期文件夹
            foreach (var item in FindFilesContainingStringParallel(currentDateFolder, searchString))
            {
                matchingFiles.Add(item);
            }

            // 如果当前日期没有找到，检查前一天日期文件夹
            if (matchingFiles.Count == 0)
            {
                foreach (var item in FindFilesContainingStringParallel(previousDateFolder, searchString))
                {
                    matchingFiles.Add(item);
                }
            }

            return matchingFiles;
        }

        /// <summary>
        /// 获取最新文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string GetFileInCurrentAndPreviousDay(string folderPath)
        {
            // 获取所有子文件夹
            var directories = Directory.GetDirectories(folderPath)
                                       .OrderByDescending(d => d)
                                       .ToList();

            if (directories.Count == 0)
            {
                throw new DirectoryNotFoundException($"文件夹不存在：{folderPath}");
            }

            // 获取最新的日期文件夹
            string latestDateFolder = directories.First();
      
            return latestDateFolder;
        }

        /// <summary>
        /// 获取最新文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string GetLatestFile(string folderPath)
        {
            // 检查文件夹是否存在
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"文件夹不存在：{folderPath}");
            }

            // 获取文件夹中所有文件
            var files = Directory.GetFiles(folderPath);

            // 如果文件夹为空，返回空字符串
            if (files.Length == 0)
            {
                return string.Empty;
            }

            // 查找最新的文件（按最后修改时间）
            var latestFile = files.OrderByDescending(File.GetLastWriteTime).FirstOrDefault();

            return latestFile;
        }
    }
}
