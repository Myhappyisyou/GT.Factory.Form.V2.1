using GT.Factory.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.Common.Helpers
{

    /// <summary>
    /// 全局配置管理器。
    /// 
    /// 注意：
    /// 这里只负责注册和访问。
    /// 不保存具体业务配置。
    /// </summary>
    public static class ConfigManager
    {

        private static readonly List<IConfigProvider> providers
            = new List<IConfigProvider>();


        /// <summary>
        /// 注册配置模块。
        /// </summary>
        public static void Register(
            IConfigProvider provider)
        {
            providers.Add(provider);
        }


        /// <summary>
        /// 获取所有配置模块。
        /// </summary>
        public static IList<IConfigProvider> Providers
        {
            get
            {
                return providers;
            }
        }

    }

}