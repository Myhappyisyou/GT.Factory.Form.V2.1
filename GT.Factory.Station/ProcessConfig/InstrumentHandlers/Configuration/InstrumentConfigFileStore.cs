using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration
{
    /// <summary>
    /// 仪器配置文件存储器。
    ///
    /// 负责将 InstrumentConfig 写入指定 JSON 文件。
    ///
    /// 注意：
    /// 1. 不修改 BaseParaFileConfiguration。
    /// 2. 不负责加载运行时 Instance。
    /// 3. 不负责创建 Instrument。
    /// 4. 仅负责配置文件的持久化。
    /// </summary>
    public class InstrumentConfigFileStore
    {
        private readonly string _configPath;

        /// <summary>
        /// 创建仪器配置文件存储器。
        /// </summary>
        /// <param name="configPath">仪器配置文件路径。</param>
        public InstrumentConfigFileStore(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
            {
                throw new ArgumentException(
                    "仪器配置文件路径不能为空。",
                    nameof(configPath));
            }

            _configPath = configPath;
        }

        /// <summary>
        /// 保存仪器配置。
        ///
        /// 保存前先执行 Validate，
        /// 防止非法配置写入正式配置文件。
        /// </summary>
        /// <param name="config">需要保存的配置。</param>
        public void Save(InstrumentConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // 保存前执行完整配置校验。
            config.Validate();

            string directory =
                Path.GetDirectoryName(_configPath);

            if (!string.IsNullOrWhiteSpace(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 先写临时文件，避免写文件过程中程序异常导致正式 JSON 损坏。
            string tempPath = _configPath + ".tmp";

            string json =
                JsonConvert.SerializeObject(
                    config,
                    Formatting.Indented);

            File.WriteAllText(
                tempPath,
                json,
                Encoding.UTF8);

            // 临时文件写成功以后，再覆盖正式配置文件。
            File.Copy(
                tempPath,
                _configPath,
                true);

            // 删除临时文件。
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
