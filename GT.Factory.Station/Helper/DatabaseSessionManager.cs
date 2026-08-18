using GT.Factory.Station;
using GT.Factory.Station.Helper.Logging;
using GT.Factory.Station.Model;
using Consumables = GT.Factory.Common.Models.Consumables;
using System;
using System.Collections.Generic;
using System.IO;

namespace GT.Factory.Station.Helper
{
    public static class DatabaseSessionManager
    {
        private static readonly object _lock = new object();

        private static string _currentDbPath;
        private static string _lastMonth;
        private static bool _migrated = false;

        /// <summary>
        /// 获取“当前可用数据库”（唯一入口）
        /// </summary>
        public static void EnsureDatabase()
        {
            lock (_lock)
            {
                string month = DateTime.Now.ToString("yyyy年MM月");

                var result = MonthlyAccessDbManager.GetCurrentMonthDbPath(month);

                // ===== 1. 切库 =====
                if (DbContext.CurrentDb.DatabasePath != result.dbPath)
                {
                    DbContext.CurrentDb.DatabasePath = result.dbPath;

                    DbContext.Set(DbContext.CurrentDb, month);

                    _migrated = false; // 新库必须允许迁移
                }

                // ===== 2. 初始化迁移（只执行一次）=====
                if (!_migrated)
                {
                    DatabaseInitializer.InitMonthlyDatabase(DbContext.CurrentDb, result.isNew);
                    _migrated = true;
                }

                _lastMonth = month;
            }
        }        
      
    }
}
