using GT.Factory.Common.Enums.ProductCodeEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.ProductCode.Domain.Config
{
    /// <summary>
    /// 流水号配置。
    ///
    /// 对应：
    ///
    /// CodeSegmentType.SerialNumber
    ///
    /// </summary>
    public class SerialConfig
    {

        /// <summary>
        /// 流水长度。
        ///
        /// 例如：
        ///
        /// 6
        ///
        /// 输出：
        ///
        /// 000001
        ///
        /// </summary>
        public int Length
        {
            get;
            set;
        } = 4;


        /// <summary>
        /// 补位字符。
        ///
        /// 默认：
        ///
        /// 0
        /// </summary>
        public string PaddingChar
        {
            get;
            set;
        }


        /// <summary>
        /// 重置模式。
        /// </summary>
        public string ResetMode
        {
            get;
            set;
        }

        
    }
}
