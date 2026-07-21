using GT_Common.Helper.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class MonthlyAccessDbManager
    {
        private readonly static string _appDataFolder = Config.Instance.DatadbPath;

        private readonly static string _templateDbPath = Path.Combine(_appDataFolder, "Template.mdb");

        /// <summary>
        /// 获取当前月份的数据库路径，如果不存在则创建
        /// </summary>

        public static (bool isNew, string dbPath) GetCurrentMonthDbPath(string month)
        {
            string dbName = $"{month}生产数据.mdb";
            string dbPath = Path.Combine(Config.Instance.DatadbPath, dbName);

            bool isNew = false;

            if (!File.Exists(dbPath))
            {
               
                CreateMonthlyDatabase(dbPath);
                isNew = true;
            }

            return (isNew, dbPath);
        }


        /// <summary>
        /// 从模板创建新的月度数据库
        /// </summary>
        private static void CreateMonthlyDatabase(string targetPath)
        {
            try
            {
                // 复制模板文件
                File.Copy(_templateDbPath, targetPath);
                // 记录日志
                DisplayLog.Info($"创建新的月度数据库: {targetPath}");
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"创建月度数据库失败",ex);
                throw;
            }
        }

        //  前一个月
        public static string GetPreviousMonthDbPath()
        {
            string previousMonth =
                DateTime.Now.AddMonths(-1)
                .ToString("yyyy年MM月");

            string dbName =
                $"{previousMonth}生产数据.mdb";

            return Path.Combine(_appDataFolder, dbName);
        }

        /// <summary>
        /// 在每月第一天凌晨检查并创建新数据库
        /// </summary>
        public static void ScheduleMonthlyCheck()
        {
            // 每天检查一次是否是新的月份
            System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromDays(1).TotalMilliseconds);
            timer.Elapsed += (s, e) =>
            {
                if (DateTime.Now.Day == 1)
                {
                    // 触发新月份数据库创建
                    //GetCurrentMonthDbPath();
                }
            };
            timer.Start();
        }
    }
}
