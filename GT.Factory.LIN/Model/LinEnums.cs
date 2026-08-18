namespace GT.Factory.LIN.Model
{


    /// <summary>
    /// LIN节点类型
    ///
    /// LIN网络中：
    ///
    /// 一个Master
    ///
    /// 多个Slave
    ///
    /// </summary>
    public enum LinNodeType
    {


        /// <summary>
        /// 主节点
        ///
        /// 负责：
        /// 1. Schedule执行
        /// 2. Header发送
        /// 3. 总线调度
        ///
        /// </summary>
        Master,



        /// <summary>
        /// 从节点
        ///
        /// 根据PID响应数据
        ///
        /// </summary>
        Slave

    }







    /// <summary>
    /// LIN帧方向
    ///
    /// </summary>
    public enum LinFrameDirection
    {


        /// <summary>
        /// Slave发送给Master
        ///
        /// Response由Slave提供
        ///
        /// </summary>
        SlaveToMaster,



        /// <summary>
        /// Master发送给Slave
        ///
        /// </summary>
        MasterToSlave

    }



    /// <summary>
    /// 调试工具显示方向
    ///
    /// 从当前设备角度看
    ///
    /// </summary>
    public enum LinTraceDirection
    {


        /// <summary>
        /// Master发送
        /// Header发送
        /// </summary>
        Tx,


        /// <summary>
        /// Slave反馈
        /// Response接收
        /// </summary>
        Rx

    }




    /// <summary>
    /// Signal数据类型
    ///
    /// </summary>
    public enum LinSignalDataType
    {


        /// <summary>
        /// 无符号
        /// </summary>
        UInt,



        /// <summary>
        /// 有符号
        /// </summary>
        Int



    }



    public enum LinSignalValueType
    {
        Raw,
        Logical,
        Physical
    }

}
