using GT.Factory.MES.BydMes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;

namespace GT.Factory.MES.BydMes
{
    /// <summary>
    /// MES配置管理器。
    /// </summary>
    public class MesConfigProvider : IConfigProvider
    {
        private static readonly Lazy<MesConfigProvider> lazy =
            new Lazy<MesConfigProvider>(
                delegate
                {
                    return new MesConfigProvider();
                });

        private MesConfig config;

        private MesConfigProvider()
        {
            Load();
        }

        public static MesConfigProvider Instance
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

        public MesConfig Settings
        {
            get
            {
                return config;
            }
        }

        private void Load()
        {
            config = new MesConfig();

            try
            {
                MesConfig loaded =
                    SerializeHelper.LoadXml<MesConfig>(
                        PathCenter.ConfigFile("MesConfig.xml"));

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
            SerializeHelper.SaveXml<MesConfig>(
                config,
                PathCenter.ConfigFile("MesConfig.xml"));
        }

        public void Reset()
        {
            config = new MesConfig();
        }
    }
}