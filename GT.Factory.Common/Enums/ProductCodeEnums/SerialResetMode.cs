using System;

namespace GT.Factory.Common.Enums.ProductCodeEnums
{
    /// <summary>
    /// 产品码流水号重置周期。
    ///
    /// 不同产品可能有不同流水规则。
    ///
    /// 例如：
    ///
    /// 每天：
    /// 000001
    /// 000002
    ///
    /// 第二天重新：
    /// 000001
    ///
    /// </summary>
    public enum SerialResetMode
    {
        /// <summary>
        /// 不自动重置。
        ///
        /// 永久递增。
        /// </summary>
        None = 0,


        /// <summary>
        /// 每天重置。
        ///
        /// 常用于生产日期码。
        /// </summary>
        Daily = 1,


        /// <summary>
        /// 每月重置。
        /// </summary>
        Monthly = 2,


        /// <summary>
        /// 每年重置。
        /// </summary>
        Yearly = 3,


        /// <summary>
        /// 每班次重置。
        ///
        /// 例如：
        ///
        /// 白班
        /// 夜班
        ///
        /// </summary>
        Shift = 4

    }
}