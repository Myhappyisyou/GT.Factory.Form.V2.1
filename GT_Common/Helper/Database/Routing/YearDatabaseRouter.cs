using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Routing
{
    /// <summary>
    /// 按年份分库路由策略
    /// 
    /// 示例：
    /// 原数据库名：BYD
    /// 路由后：BYD_2024
    /// </summary>
    public class YearDatabaseRouter : IDatabaseRouter
    {
        private readonly string _baseDatabaseName;

        public YearDatabaseRouter(string baseDatabaseName)
        {
            _baseDatabaseName = baseDatabaseName;
        }

        /// <summary>
        /// 根据当前年份生成数据库名称
        /// </summary>
        public string GetDatabaseName()
        {
            return $"{_baseDatabaseName}_{DateTime.Now.Year}";
        }
    }
}
