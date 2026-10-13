using GT.Factory.Common.Helpers;
using GT.Factory.PLC.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.ProcessConfig
{
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(SortedExpandableObjectConverter))]
    public class PlcConnectionConfig
    {
        #region 基本配置

        [DisplayName("PLC类型"), PropertyOrder(1)]
        [Description("PLC类型")]
        [JsonProperty]
        public PlcType PlcType { get; set; }

        //IP
        [DisplayName("Ip"), PropertyOrder(2)]
        [Description("Plc-Ip")]
        [JsonProperty]
        public string Ip { get; set; }

        //读取地址标识
        [DisplayName("地址标识"), PropertyOrder(3)]
        [Description("读取地址标识")]
        [JsonProperty]
        public string SectionName { get; set; }

        //心跳地址
        [DisplayName("心跳地址"), PropertyOrder(4)]
        [Description("心跳地址")]
        [JsonProperty]
        public string HeartbeatAddr { get; set; }

        //读取开始地址
        [DisplayName("读取开始地址"), PropertyOrder(5)]
        [Description("读取开始地址")]
        [JsonProperty]
        public int StartIndex { get; set; }

        //读取地址长度
        [DisplayName("读取地址长度"), PropertyOrder(6)]
        [Description("读取地址长度")]
        [JsonProperty]
        public ushort Length { get; set; }

        //工序号
        [DisplayName("工序号"), PropertyOrder(7)]
        [Description("工序号")]
        [JsonProperty]
        public string ProcessNo { get; set; }


        public override string ToString() => $"基本配置信息";

        #endregion

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Ip))
                throw new InvalidOperationException("IP地址不能为空");
            if (PlcType != PlcType.Modbus)
            {
                if (string.IsNullOrWhiteSpace(SectionName))
                    throw new ArgumentException("SectionName不能为空", nameof(SectionName));

            }

        }
    }
}
