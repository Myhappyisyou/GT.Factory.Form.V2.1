using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using GT.Factory.Station.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration
{
    /// <summary>
    /// 工站仪器配置。
    ///
    /// 该配置描述当前工站实际使用的仪器设备。
    ///
    /// 注意：
    /// 1. 配置文件属于具体工站，例如 OP070\Config\InstrumentConfig.json。
    /// 2. 该类属于 Station 公共配置模型。
    /// 3. 该类不负责创建具体 Instrument 实例。
    /// 4. 具体 Instrument 的通讯协议仍由对应 Instrument DLL 负责。
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InstrumentConfig :
        BaseParaFileConfiguration<InstrumentConfig>,
        IValidatableConfig
    {
        /// <summary>
        /// 创建默认仪器配置。
        ///
        /// 保证配置对象创建后 Instruments 不为 null，
        /// 便于配置界面和运行时代码直接使用。
        /// </summary>
        public InstrumentConfig()
        {
            Instruments = new List<InstrumentDeviceConfig>();
        }

        /// <summary>
        /// 当前工站配置的所有仪器。
        ///
        /// 每一个元素代表一台实际设备。
        /// </summary>
        [DisplayName("仪器列表")]
        [Description("当前工站实际使用的所有仪器设备")]
        [JsonProperty]
        public List<InstrumentDeviceConfig> Instruments { get; set; }

        /// <summary>
        /// 根据仪器名称获取配置。
        ///
        /// Name 是当前工站内部识别某一台设备的唯一名称。
        /// </summary>
        /// <param name="name">仪器名称。</param>
        /// <returns>找到的仪器配置；找不到返回 null。</returns>
        public InstrumentDeviceConfig Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            if (Instruments == null)
                return null;

            return Instruments.FirstOrDefault(
                x => x != null &&
                     string.Equals(
                         x.Name,
                         name,
                         StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 判断当前工站是否已经配置指定名称的仪器。
        /// </summary>
        /// <param name="name">仪器名称。</param>
        /// <returns>存在返回 true，否则返回 false。</returns>
        public bool Exists(string name)
        {
            return Get(name) != null;
        }

        /// <summary>
        /// 添加一台仪器配置。
        ///
        /// 添加前会检查：
        /// 1. 配置对象不能为空。
        /// 2. 仪器名称不能为空。
        /// 3. 仪器名称不能重复。
        /// 4. 仪器自身配置必须合法。
        ///
        /// 注意：
        /// Add 只修改当前运行时配置对象，
        /// 不会自动写入 JSON 文件。
        /// 修改完成后调用 Save()。
        /// </summary>
        /// <param name="config">仪器配置。</param>
        public void Add(InstrumentDeviceConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Name))
                throw new ArgumentException(
                    "仪器名称不能为空。",
                    nameof(config));

            if (Exists(config.Name))
            {
                throw new InvalidOperationException(
                    "仪器名称已经存在：" + config.Name);
            }

            config.Validate();

            if (Instruments == null)
                Instruments = new List<InstrumentDeviceConfig>();

            Instruments.Add(config);
        }

        /// <summary>
        /// 修改一台已经存在的仪器配置。
        ///
        /// 根据 config.Name 查找原配置并替换。
        ///
        /// 修改只发生在当前运行时配置对象，
        /// 不会自动写入 JSON 文件。
        /// 修改完成后调用 Save()。
        /// </summary>
        /// <param name="config">新的仪器配置。</param>
        public void Update(InstrumentDeviceConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrWhiteSpace(config.Name))
                throw new ArgumentException(
                    "仪器名称不能为空。",
                    nameof(config));

            if (Instruments == null)
                throw new InvalidOperationException(
                    "仪器配置列表尚未初始化。");

            config.Validate();

            int index = Instruments.FindIndex(
                x => x != null &&
                     string.Equals(
                         x.Name,
                         config.Name,
                         StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                throw new InvalidOperationException(
                    "未找到需要修改的仪器：" + config.Name);
            }

            Instruments[index] = config;
        }

        /// <summary>
        /// 删除指定名称的仪器配置。
        ///
        /// 删除只发生在当前运行时配置对象，
        /// 不会自动写入 JSON 文件。
        /// 删除完成后调用 Save()。
        /// </summary>
        /// <param name="name">仪器名称。</param>
        /// <returns>成功删除返回 true，否则返回 false。</returns>
        public bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (Instruments == null)
                return false;

            int index = Instruments.FindIndex(
                x => x != null &&
                     string.Equals(
                         x.Name,
                         name,
                         StringComparison.OrdinalIgnoreCase));

            if (index < 0)
                return false;

            Instruments.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 对当前仪器配置进行完整合法性校验。
        ///
        /// Reload 时 BaseParaFileConfiguration 会自动调用此方法。
        /// 校验失败时，新配置不会覆盖当前运行时配置。
        /// </summary>
        public void Validate()
        {
            if (Instruments == null)
            {
                throw new InvalidOperationException(
                    "仪器配置列表不能为空。");
            }

            HashSet<string> names =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (InstrumentDeviceConfig instrument in Instruments)
            {
                if (instrument == null)
                {
                    throw new InvalidOperationException(
                        "仪器配置列表中存在空配置项。");
                }

                instrument.Validate();

                if (!names.Add(instrument.Name))
                {
                    throw new InvalidOperationException(
                        "存在重复的仪器名称：" +
                        instrument.Name);
                }
            }
        }

    }
}
