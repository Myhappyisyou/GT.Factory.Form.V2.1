using System;

namespace GT.Factory.Common.Enums.ProductCodeEnums
{
    /// <summary>
    /// 数字补位方向。
    /// </summary>
    public enum PaddingDirection
    {
        /// <summary>
        /// 左补。
        ///
        /// 1
        ///
        /// 变成：
        ///
        /// 000001
        /// </summary>
        Left = 1,


        /// <summary>
        /// 右补。
        ///
        /// 特殊客户使用。
        /// </summary>
        Right = 2
    }
}