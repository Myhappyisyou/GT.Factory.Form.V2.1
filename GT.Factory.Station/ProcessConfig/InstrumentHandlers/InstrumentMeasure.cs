using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers
{
    /// <summary>
    /// 仪器单个测试项。
    /// 用于描述仪器返回的一项实际测试数据。
    /// </summary>
    public class InstrumentMeasure
    {
        /// <summary>
        /// 测试项业务主键。
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 测试项显示名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 测试项单位。
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 测试实际值。
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 测试项上限。
        /// 仪器没有上限时允许为空。
        /// </summary>
        public object Upper { get; set; }

        /// <summary>
        /// 测试项下限。
        /// 仪器没有下限时允许为空。
        /// </summary>
        public object Lower { get; set; }
    }

    /// <summary>
    /// 仪器返回的完整测试结果。
    /// 一个仪器可以包含多个测试项，但通常只有一个最终测试结果。
    /// </summary>
    public class InstrumentResult
    {
        /// <summary>
        /// 仪器测试项集合。
        /// </summary>
        public List<InstrumentMeasure> Measures { get; set; }

        /// <summary>
        /// 仪器最终测试结果。
        /// 例如：OK、NG、PASS、FAIL、1、0。
        /// </summary>
        public object Result { get; set; }
    }
}
