using GT.Factory.Common.Interfaces;
using System;
using GT.Factory.Common.Helpers;

namespace GT.Factory.Login.Login
{
    /// <summary>
    /// Login模块配置管理器。
    ///
    /// 负责：
    /// 1. 单例配置
    /// 2. XML加载
    /// 3. XML保存
    /// 4. 默认配置
    /// </summary>
    public class WebApiConfigProvider : IConfigProvider
    {
        private static readonly Lazy<WebApiConfigProvider> lazy =
            new Lazy<WebApiConfigProvider>(
                delegate
                {
                    return new WebApiConfigProvider();
                });

        private WebApiConfig config;

        private WebApiConfigProvider()
        {
            Load();
        }

        /// <summary>
        /// 当前Login配置管理器。
        /// </summary>
        public static WebApiConfigProvider Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        /// <summary>
        /// 配置名称。
        /// </summary>
        public string Name
        {
            get
            {
                return "WebApi配置";
            }
        }

        /// <summary>
        /// 当前配置对象。
        /// </summary>
        public object Config
        {
            get
            {
                return config;
            }
        }

        /// <summary>
        /// 强类型访问当前配置。
        /// Login业务代码使用这个属性。
        /// </summary>
        public WebApiConfig Settings
        {
            get
            {
                return config;
            }
        }

        /// <summary>
        /// 加载XML配置。
        ///
        /// XML不存在、损坏或者读取异常时，
        /// 自动使用默认配置。
        /// </summary>
        private void Load()
        {
            config = new WebApiConfig();

            try
            {
                WebApiConfig loaded =
                    SerializeHelper.LoadXml<WebApiConfig>(
                        PathCenter.ConfigFile("WebApiConfig.xml"));

                if (loaded != null)
                {
                    config = loaded;
                }
            }
            catch
            {
                // 配置读取失败时使用默认配置。
            }
        }

        /// <summary>
        /// 保存XML配置。
        /// </summary>
        public void Save()
        {
            SerializeHelper.SaveXml<WebApiConfig>(
                config,
                PathCenter.ConfigFile("WebApiConfig.xml"));
        }

        /// <summary>
        /// 恢复默认配置。
        /// </summary>
        public void Reset()
        {
            config = new WebApiConfig();
        }
    }
}