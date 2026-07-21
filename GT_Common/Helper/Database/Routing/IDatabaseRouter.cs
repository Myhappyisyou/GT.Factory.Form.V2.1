using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Routing
{
    /// <summary>
    /// 数据库路由策略接口
    /// 
    /// 作用：
    /// 根据当前时间 / 班次 / 规则
    /// 动态计算数据库名称
    /// </summary>
    public interface IDatabaseRouter
    {
        /// <summary>
        /// 获取目标数据库名称
        /// </summary>
        string GetDatabaseName();
    }
}
