using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Enums.ProductCodeEnums
{

    /// <summary>
    /// 产品码年份转换方式。
    ///
    /// 用于：
    ///
    /// 2026
    ///
    /// 如何转换。
    ///
    /// </summary>
    public enum DateYearMode
    {

        /// <summary>
        /// 完整年份。
        ///
        /// 2026
        ///
        /// </summary>
        FullYear = 1,

        /// <summary>
        /// 年后两位。
        ///
        /// 2026
        ///
        /// 输出:
        ///
        /// 26
        ///
        /// </summary>
        LastTwo = 2,

        /// <summary>
        /// 自定义映射。
        ///
        /// 2026 -> A
        ///
        /// </summary>
        Mapping = 3

    }


    /// <summary>
    /// 月份编码方式。
    ///
    /// </summary>
    public enum DateMonthMode
    {

        /// <summary>
        /// 数字月份。
        ///
        /// 01
        /// 02
        /// ...
        ///
        /// </summary>
        TwoDigit = 1,


        /// <summary>
        /// 去掉前导0。
        ///
        /// 01 -> 1
        ///
        /// 09 -> 9
        ///
        /// </summary>
        Number = 2,


        /// <summary>
        /// 10以上使用字母。
        ///
        /// 01 -> 1
        ///
        /// 09 -> 9
        ///
        /// 10 -> A
        ///
        /// 11 -> B
        ///
        /// 12 -> C
        ///
        /// </summary>
        Alpha = 3

    }
}