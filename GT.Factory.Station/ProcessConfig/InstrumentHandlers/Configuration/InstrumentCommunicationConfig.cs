using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration
{
    /// <summary>
    /// 仪器通讯参数。
    ///
    /// 描述设备如何连接，
    /// 不负责真正执行通讯。
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InstrumentCommunicationConfig
    {
        /// <summary>
        /// 通讯类型。
        ///
        /// 例如：
        /// TCP
        /// SerialPort
        /// </summary>
        [DisplayName("通讯类型")]
        [Description("仪器通讯方式，例如 TCP、SerialPort")]
        [JsonProperty]
        public string Type { get; set; }

        /// <summary>
        /// TCP设备IP地址。
        /// </summary>
        [DisplayName("IP地址")]
        [Description("TCP仪器的IP地址")]
        [JsonProperty]
        public string Ip { get; set; }

        /// <summary>
        /// TCP设备端口。
        /// </summary>
        [DisplayName("端口")]
        [Description("TCP仪器通讯端口")]
        [JsonProperty]
        public int Port { get; set; }

        /// <summary>
        /// 发送超时时间，单位毫秒。
        /// </summary>
        [DisplayName("发送超时")]
        [Description("TCP发送超时时间，单位毫秒")]
        [JsonProperty]
        public int SendTimeout { get; set; }

        /// <summary>
        /// 接收超时时间，单位毫秒。
        /// </summary>
        [DisplayName("接收超时")]
        [Description("TCP接收超时时间，单位毫秒")]
        [JsonProperty]
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// 校验通讯配置。
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Type))
            {
                throw new InvalidOperationException(
                    "仪器通讯类型不能为空。");
            }

            if (string.Equals(
                Type,
                "TCP",
                StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(Ip))
                {
                    throw new InvalidOperationException(
                        "TCP仪器IP地址不能为空。");
                }

                if (Port <= 0 || Port > 65535)
                {
                    throw new InvalidOperationException(
                        "TCP仪器端口必须在1~65535之间。");
                }
            }

            if (SendTimeout <= 0)
            {
                throw new InvalidOperationException(
                    "仪器发送超时必须大于0。");
            }

            if (ReceiveTimeout <= 0)
            {
                throw new InvalidOperationException(
                    "仪器接收超时必须大于0。");
            }
        }
    }
}
