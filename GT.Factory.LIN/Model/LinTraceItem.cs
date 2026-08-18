using System;
using System.Collections.Generic;


namespace GT.Factory.LIN.Model
{

    /// <summary>
    /// LIN通讯记录
    ///
    /// 对应调试工具中的一行Trace
    ///
    /// </summary>
    public class LinTraceItem
    {

        /// <summary>
        /// 时间
        ///
        /// 记录Frame产生时间
        ///
        /// </summary>
        public DateTime Time
        {
            get;
            set;
        }

        /// <summary>
        /// 方向
        ///
        /// Tx:
        /// Master发送
        ///
        /// Rx:
        /// Slave响应
        ///
        /// </summary>
        public string Direction
        {
            get;
            set;
        }

        /// <summary>
        /// Frame名称
        ///
        /// </summary>
        public string FrameName
        {
            get;
            set;
        }

        /// <summary>
        /// Frame ID
        ///
        /// </summary>
        public byte Id
        {
            get;
            set;
        }

        /// <summary>
        /// 数据长度
        ///
        /// </summary>
        public int Length
        {
            get;
            set;
        }

        /// <summary>
        /// 数据
        ///
        /// 显示:
        ///
        /// 11-22-33
        ///
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// 校验结果
        ///
        /// </summary>
        public bool ChecksumOK
        {
            get;
            set;
        }


        /// <summary>
        /// Signal解析结果
        /// 
        /// 用于Trace显示
        /// 
        /// </summary>
        public List<LinSignalValueItem> Signals
        {
            get;
            set;
        }


        public override string ToString()
        {

            return
                $"{Time:HH:mm:ss.fff} " +
                $"{Direction} " +
                $"ID:{Id} " +
                $"{BitConverter.ToString(Data)}";

        }


    }

}