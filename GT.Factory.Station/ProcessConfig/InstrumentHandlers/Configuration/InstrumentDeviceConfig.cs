using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration
{
    /// <summary>
    /// 单台实际仪器设备配置。
    ///
    /// 该类描述工站现场安装的设备，
    /// 不属于具体仪器DLL。
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InstrumentDeviceConfig
    {
        /// <summary>
        /// 工站内部仪器名称。
        ///
        /// 例如：
        /// OP070-POWER
        /// OP070-HELIUM-01
        /// </summary>
        [DisplayName("仪器名称")]
        [Description("当前工站内部唯一的仪器名称")]
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// 仪器驱动标识。
        ///
        /// 用于匹配对应的 Handler。
        ///
        /// 例如：
        /// ItechPower
        /// HeliumTester
        /// </summary>
        [DisplayName("驱动标识")]
        [Description("用于匹配对应仪器Handler的标识")]
        [JsonProperty]
        public string DriverKey { get; set; }

        /// <summary>
        /// 仪器型号。
        /// </summary>
        [DisplayName("仪器型号")]
        [Description("实际设备型号")]
        [JsonProperty]
        public string Model { get; set; }

        /// <summary>
        /// 仪器序列号。
        /// </summary>
        [DisplayName("序列号")]
        [Description("实际设备序列号")]
        [JsonProperty]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 仪器通讯配置。
        /// </summary>
        [DisplayName("通讯配置")]
        [Description("仪器TCP或其他通讯参数")]
        [JsonProperty]
        public InstrumentCommunicationConfig Communication { get; set; }

        /// <summary>
        /// 校验当前仪器配置。
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidOperationException(
                    "仪器名称不能为空。");
            }

            if (string.IsNullOrWhiteSpace(DriverKey))
            {
                throw new InvalidOperationException(
                    "仪器驱动标识不能为空：" +
                    Name);
            }

            if (Communication == null)
            {
                throw new InvalidOperationException(
                    "仪器通讯配置不能为空：" +
                    Name);
            }

            Communication.Validate();
        }
    }
}
