using System;

namespace GT.Factory.Common.Enums.ProductCodeEnums
{
    /// <summary>
    /// 产品码组成段类型。
    ///
    /// 产品码不是一个字符串，
    /// 而是由多个不同类型的段组合而成。
    ///
    /// 例如：
    ///
    /// GT-A01-20260916-000001
    ///
    /// 可以拆分为：
    ///
    /// FixedText       GT
    /// ProductModel    A01
    /// Date            20260916
    /// SerialNumber    000001
    ///
    /// 数据库存储枚举值，
    /// 避免直接保存中文或字符串导致数据污染。
    /// </summary>
    public enum CodeSegmentType
    {
        /// <summary>
        /// 固定字符。
        ///
        /// 例如：
        /// GT
        /// ABC
        /// XM
        /// </summary>
        FixedText = 1,


        /// <summary>
        /// 日期。
        ///
        /// 格式由 SegmentFormat 指定。
        ///
        /// 例如：
        /// yyyyMMdd
        /// yyyyMM
        /// </summary>
        Date = 2,


        /// <summary>
        /// 班次。
        ///
        /// 例如：
        /// DAY
        /// NIGHT
        /// </summary>
        Shift = 3,


        /// <summary>
        /// 流水号。
        ///
        /// 由SerialService生成。
        ///
        /// 例如：
        /// 000001
        /// </summary>
        SerialNumber = 4
    }
}
