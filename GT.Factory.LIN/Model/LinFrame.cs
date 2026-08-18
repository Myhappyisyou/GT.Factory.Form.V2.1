using System.Collections.Generic;


namespace GT.Factory.LIN.Model
{

    /// <summary>
    /// LIN Frame模型
    ///
    /// Frame是LIN通信中的基本数据单元。
    ///
    /// 一个Frame包含：
    ///
    /// Header:
    ///     PID
    ///
    /// Response:
    ///     Data
    ///
    /// 示例：
    ///
    /// ELOP_Rsp1
    ///
    /// ID:
    ///     26
    ///
    /// DLC:
    ///     8
    ///
    /// </summary>
    public class LinFrame
    {


        /// <summary>
        /// Frame名称
        ///
        /// 对应LDF：
        ///
        /// ELOP_Rsp1
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
        /// 范围：
        ///
        /// 0 ~ 63
        ///
        /// 注意：
        ///
        /// LIN真正总线上传输的是PID
        /// PID由ID + Parity组成。
        ///
        /// </summary>
        public byte Id
        {
            get;
            set;
        }




        /// <summary>
        /// 发布该Frame的节点
        ///
        /// 例如：
        ///
        /// ELOP_Rsp1
        /// 
        /// Publisher:
        ///     Slave_1
        ///
        /// </summary>
        public LinNode Publisher
        {
            get;
            set;
        }




        /// <summary>
        /// Frame数据长度
        ///
        /// 单位:
        /// Byte
        ///
        /// LIN 2.x:
        ///
        /// 最大8 Byte
        ///
        /// </summary>
        public byte Length
        {
            get;
            set;
        }



        /// <summary>
        /// Frame中的Signal映射
        ///
        /// 描述：
        ///
        /// 哪些Signal位于该Frame。
        ///
        /// </summary>
        public List<LinFrameSignal> Signals
        {
            get;
            set;
        }
        =
        new List<LinFrameSignal>();



        /// <summary>
        /// 获取PID
        ///
        /// LIN总线实际发送：
        ///
        /// Protected Identifier
        ///
        /// 由:
        ///
        /// ID
        /// +
        /// Parity
        ///
        /// 组成。
        ///
        /// 目前只预留，
        /// 后续Protocol层实现。
        ///
        /// </summary>
        public byte? ProtectedId
        {
            get;
            set;
        }


        /// <summary>
        /// Frame方向
        ///
        /// 根据Publisher判断也可以，
        /// 但是单独保存更方便。
        ///
        /// </summary>
        public LinFrameDirection Direction
        {
            get;
            set;
        }



        public override string ToString()
        {
            return $"{Name} ({Id})";
        }


    }


    /// <summary>
    /// Frame中的Signal映射关系
    ///
    /// LDF中：
    ///
    /// Frame:
    ///
    /// ELOP_Rsp1
    ///
    /// {
    ///
    /// READ_SPEED_CRC,0;
    ///
    /// READ_ACTUAL_SPEED,8;
    ///
    /// }
    ///
    /// 这里保存：
    ///
    /// Signal名称
    /// 起始Bit
    ///
    /// </summary>
    public class LinFrameSignal
    {


        /// <summary>
        /// 对应Signal
        ///
        /// 来自:
        ///
        /// LinNetwork.Signals
        ///
        /// </summary>
        public LinSignal Signal
        {
            get;
            set;
        }



        /// <summary>
        /// 起始Bit位置
        ///
        /// 示例：
        ///
        /// READ_ACTUAL_SPEED:
        ///
        /// StartBit=8
        ///
        /// </summary>
        public ushort StartBit
        {
            get;
            set;
        }


        /// <summary>
        /// Signal长度
        ///
        /// 从LinSignal复制过来
        ///
        /// </summary>
        public ushort Length
        {
            get;
            set;
        }

    }

}

