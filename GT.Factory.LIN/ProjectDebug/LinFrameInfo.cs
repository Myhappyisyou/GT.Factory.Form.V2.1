using GT.Factory.LIN.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.ProjectDebug
{

    /// <summary>
    /// LIN Frame调试信息
    ///
    /// 对应LDF中的Frame定义
    ///
    /// 用于调试软件显示:
    ///
    /// Frame列表
    ///
    /// Frame包含哪些Signal
    ///
    /// </summary>
    public class LinFrameInfo
    {


        /// <summary>
        /// Frame名称
        ///
        /// 例如:
        ///
        /// ELOP_Cmd
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// LIN Frame ID
        ///
        /// 例如:
        ///
        /// 0x1C
        ///
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Data长度
        ///
        /// LIN:
        ///
        /// 最大8Byte
        ///
        /// </summary>
        public int Length
        {
            get;
            set;
        }

        /// <summary>
        /// Frame发布节点
        ///
        /// Master
        /// Slave
        ///
        /// </summary>
        public string Publisher
        {
            get;
            set;
        }


        /// <summary>
        /// Frame方向
        ///
        /// Master -> Slave
        ///
        /// Slave -> Master
        ///
        /// </summary>
        public LinFrameDirection Direction
        {
            get;
            set;
        }


        /// <summary>
        /// 当前Frame包含的Signal
        ///
        /// </summary>
        public List<LinSignalInfo> Signals
        {
            get;
            set;
        }



        /// <summary>
        /// 当前Frame数据
        ///
        /// 来源:
        ///
        /// LinRuntime
        ///
        /// 表示LIN总线上当前8Byte数据。
        ///
        /// 示例:
        ///
        /// ID:
        /// 28
        ///
        /// Data:
        ///
        /// 00 12 FF 00 00 00 00 00
        ///
        /// 用于:
        ///
        /// 1. 调试界面显示
        ///
        /// 2. Frame分析
        ///
        /// 3. Signal解析
        ///
        /// </summary>
        public byte[] Data
        {
            get;
            internal set;
        }

    }


}
