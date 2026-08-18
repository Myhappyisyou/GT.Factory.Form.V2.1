using Newtonsoft.Json;
using System;
using System.IO;


namespace GT.Factory.LIN.Config
{

    /// <summary>
    /// LIN配置管理器
    ///
    /// 负责:
    ///
    /// Json读取
    ///
    /// Json保存
    ///
    /// </summary>
    public class SimulationConfigManager
    {

        /// <summary>
        /// 加载配置
        ///
        /// </summary>
        public static LinSimulationConfig Load(
            string file)
        {

            if (!File.Exists(file))
            {

                throw new FileNotFoundException(
                    "找不到LIN配置文件",
                    file);

            }

            string json =
                File.ReadAllText(
                    file);

            return
                JsonConvert.DeserializeObject<LinSimulationConfig>(
                    json);

        }

        /// <summary>
        /// 保存配置
        ///
        /// </summary>
        public static void Save(
            string file,
            LinSimulationConfig config)
        {

            string json =
                JsonConvert.SerializeObject(
                    config,
                    Formatting.Indented);

            File.WriteAllText(
                file,
                json);

        }

    }

}

