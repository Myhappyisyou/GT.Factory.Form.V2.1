using System;


namespace GT.Factory.LIN.Model
{


    /// <summary>
    /// LIN消息
    ///
    /// 对应LIN总线上的一帧数据
    ///
    /// 包含：
    ///
    /// 1. Frame ID
    /// 2. DLC
    /// 3. Data
    /// 4. 校验类型
    ///
    ///
    /// 示例:
    ///
    /// ELOP_Rsp1
    ///
    /// ID:
    /// 26
    ///
    /// DATA:
    ///
    /// 11 22 33 44 55 66 77 88
    ///
    /// </summary>
    public class LinMessage
    {
        /// <summary>
        /// 通信方向
        ///
        /// TX:
        /// Master发送Header
        ///
        /// RX:
        /// Slave返回数据
        ///
        /// </summary>
        public LinTraceDirection Direction
        {
            get;
            set;
        }


        /// <summary>
        /// LIN协议方向
        ///
        /// MasterToSlave:
        /// Master发送数据
        ///
        /// SlaveToMaster:
        /// Slave反馈数据
        ///
        /// </summary>
        public LinFrameDirection FrameDirection
        {
            get;
            set;
        }


        /// <summary>
        /// LIN Frame ID
        ///
        /// 范围:
        ///
        /// 0~63
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
        /// LIN:
        ///
        /// Classic:
        /// 2~8 Byte
        ///
        /// </summary>
        public byte Length
        {
            get;
            set;
        }

        /// <summary>
        /// 数据
        ///
        /// 最大8字节
        ///
        /// </summary>
        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// 时间戳
        ///
        /// 调试工具显示需要
        ///
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// 校验和
        ///
        /// LIN:
        ///
        /// Classic checksum
        ///
        /// Enhanced checksum
        ///
        /// </summary>
        public byte Checksum
        {
            get;
            set;
        }

        /// <summary>
        /// 是否校验通过
        ///
        /// </summary>
        public bool IsChecksumValid
        {
            get;
            set;
        }

        /// <summary>
        /// 创建一个空消息
        ///
        /// </summary>
        public LinMessage()
        {

            Data =
                new byte[8];


            Timestamp =
                DateTime.Now;


        }


        /// <summary>
        /// 创建指定长度消息
        ///
        /// </summary>
        public LinMessage(
            byte id,
            byte length)
        {


            Id = id;


            Length = length;


            Data =
                new byte[length];


            Timestamp =
                DateTime.Now;


        }

        public override string ToString()
        {

            return
                $"{Direction} " +
                $"ID:{Id:X2} " +
                $"LEN:{Length} " +
                $"DATA:{BitConverter.ToString(Data, 0, Length)}";

        }


    }

}

