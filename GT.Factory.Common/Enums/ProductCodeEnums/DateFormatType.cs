using System;
using System.ComponentModel;

namespace GT.Factory.Common.Enums.ProductCodeEnums
{
    /// <summary>
    /// 日期格式类型。
    /// </summary>
    public enum DateFormatType
    {

        /// <summary>
        /// 年月日。
        ///
        /// yyyyMMdd
        ///
        /// 示例：
        /// 20260916
        /// </summary>
        [Description("yyyyMMdd")]
        YearMonthDay = 1,


        /// <summary>
        /// 年月。
        ///
        /// yyyyMM
        /// </summary>
        [Description("yyyyMM")]
        YearMonth = 2,


        /// <summary>
        /// 年月日时分秒。
        /// </summary>
        [Description("年月日时分秒")]
        FullDateTime = 3,

        /// <summary>
        /// 一年中第几天。
        /// </summary>
        [Description("一年中第几天")]
        DayOfYear = 4,

        /// <summary>
        /// 自定义日期格式。
        ///
        /// 年、月、日可以分别指定编码方式。
        ///
        /// 例如：
        /// 2026 -> 26
        /// 09   -> 9
        /// 20   -> 20
        ///
        /// 结果：
        /// 26920
        /// </summary>
        [Description("自定义日期格式")]
        Custom = 5,
    }
}