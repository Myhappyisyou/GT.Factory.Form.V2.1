using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class AccessDbManager
    {
        private static readonly object _lock = new object();
        private static AccessMdbHelper _db;
        private static string _currentPath;

        public static AccessMdbHelper GetCurrentDb()
        {
            var value = MonthlyAccessDbManager
                .GetCurrentMonthDbPath(DateTime.Now.ToString("yyyy年MM月"));
            string path = value.dbPath;
            lock (_lock)
            {
                if (_db == null)
                {
                    _db = new AccessMdbHelper(path);
                    _currentPath = path;
                }
                else if (!string.Equals(_currentPath, path, StringComparison.OrdinalIgnoreCase))
                {
                    _db.Dispose();                  // ⭐ 释放旧连接
                    _db = new AccessMdbHelper(path);
                    _currentPath = path;
                }

                return _db;
            }
        }

        /// <summary>
        /// 应用退出时调用
        /// </summary>
        public static void Shutdown()
        {
            lock (_lock)
            {
                _db?.Dispose();
                _db = null;
                _currentPath = null;
            }
        }
    }

}
