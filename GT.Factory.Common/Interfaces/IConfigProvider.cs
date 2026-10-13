using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.Common.Interfaces
{
    /// <summary>
    /// 配置模块提供者接口。
    ///
    /// 每个独立模块负责自己的：
    /// 1. 配置对象
    /// 2. XML加载
    /// 3. XML保存
    /// 4. 默认值恢复
    ///
    /// ConfigCenter只通过这个接口访问配置。
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// 配置页面显示名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取当前配置对象。
        /// PropertyGrid使用该对象显示和编辑。
        /// </summary>
        object Config { get; }

        /// <summary>
        /// 保存配置。
        /// </summary>
        void Save();

        /// <summary>
        /// 恢复默认配置。
        /// </summary>
        void Reset();
    }
}