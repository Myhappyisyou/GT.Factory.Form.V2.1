using GT_Common;
using GT_Common.Helper.Logging;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace GT_Common.Helper
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
        }        /// <summary>
                 /// 月度初始化（只执行一次）
                 /// </summary>
        private static void InitializeMonth(
            AccessMdbHelper db,
            bool isNew,
            List<Consumables> oldConsumables)
        {
            try
            {
                if (isNew && oldConsumables != null)
                {
                    foreach (var item in oldConsumables)
                    {
                        UploadSql.InsertOrUpdateConsumablesInfor(db, item);
                    }

                    DisplayLog.Info("跨月 Consumables 数据迁移完成");
                }

                DisplayLog.Info($"数据库初始化完成：{db.DatabasePath}");
            }
            catch (Exception ex)
            {
                DisplayLog.Error("数据库初始化失败", ex, true);
                throw;
            }
        }
    }
}
