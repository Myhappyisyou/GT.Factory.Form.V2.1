using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
using System;

namespace GT.Factory.RFID.Aynettek
{
    /// <summary>
    /// MES配置管理器。
    /// </summary>
    public class UIDConfigProvider : IConfigProvider
    {
        private static readonly Lazy<UIDConfigProvider> lazy =
            new Lazy<UIDConfigProvider>(
                delegate
                {
                    return new UIDConfigProvider();
                });

        private UIDConfig config;

        private UIDConfigProvider()
        {
            Load();
        }

        public static UIDConfigProvider Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public string Name
        {
            get
            {
                return "MES配置";
            }
        }

        public object Config
        {
            get
            {
                return config;
            }
        }

        public UIDConfig Settings
        {
            get
            {
                return config;
            }
        }

        private void Load()
        {
            config = new UIDConfig();

            try
            {
                UIDConfig loaded =
                    SerializeHelper.LoadXml<UIDConfig>(
                        PathCenter.ConfigFile("UIDConfig.xml"));

                if (loaded != null)
                {
                    config = loaded;
                }
            }
            catch
            {
                // 读取失败使用默认配置。
            }
        }

        public void Save()
        {
            SerializeHelper.SaveXml<UIDConfig>(
                config,
                PathCenter.ConfigFile("MesConfig.xml"));
        }

        public void Reset()
        {
            config = new UIDConfig();
        }
    }
}