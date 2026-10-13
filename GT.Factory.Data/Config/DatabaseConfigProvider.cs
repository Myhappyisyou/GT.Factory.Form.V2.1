using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
using GT.Factory.Data.Core;
using System;

namespace GT.Factory.Data.Config
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
    public class DatabaseConfigProvider : IConfigProvider
    {
        private static readonly Lazy<DatabaseConfigProvider> lazy =
            new Lazy<DatabaseConfigProvider>(
                delegate
                {
                    return new DatabaseConfigProvider();
                });

        private MultiDatabaseOptions config;

        private DatabaseConfigProvider()
        {
            Load();
        }

        /// <summary>
        /// 当前Login配置管理器。
        /// </summary>
        public static DatabaseConfigProvider Instance
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
                return "数据库配置";
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
        public MultiDatabaseOptions Settings
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
            config = new MultiDatabaseOptions();

            try
            {
                MultiDatabaseOptions loaded =
                    SerializeHelper.LoadXml<MultiDatabaseOptions>(
                        PathCenter.ConfigFile("DatabaseConfig.xml"));

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
            SerializeHelper.SaveXml<MultiDatabaseOptions>(
                config,
                PathCenter.ConfigFile("DatabaseConfig.xml"));
        }

        /// <summary>
        /// 恢复默认配置。
        /// </summary>
        public void Reset()
        {
            config = new MultiDatabaseOptions();
        }
    }
}