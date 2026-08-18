using System.Collections.Generic;

namespace GT.Factory.LIN.Model
{

    /// <summary>
    /// LIN节点
    ///
    /// LIN网络中的一个ECU节点。
    ///
    /// 一个LIN网络包含：
    ///
    /// Master节点
    ///     负责Schedule管理以及Header发送
    ///
    /// Slave节点
    ///     根据ID响应数据
    ///
    /// 示例：
    ///
    /// Master:
    ///     LIN_Master
    ///
    /// Slave:
    ///     Slave_1
    ///
    /// </summary>
    public class LinNode
    {

        /// <summary>
        /// 节点名称
        ///
        /// 对应LDF中的节点名称。
        ///
        /// 示例：
        /// LIN_Master
        /// Slave_1
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 节点类型
        ///
        /// Master / Slave
        /// </summary>
        public LinNodeType Type
        {
            get;
            set;
        }

        /// <summary>
        /// LIN诊断地址
        ///
        /// NAD(Network Address for Diagnostics)
        ///
        /// 主要用于LIN诊断协议。
        ///
        /// LDF:
        ///
        /// configured_NAD = 0x27
        ///
        /// </summary>
        public byte? Nad
        {
            get;
            set;
        }

        /// <summary>
        /// 节点LIN协议版本
        ///
        /// 示例：
        /// 2.1
        ///
        /// </summary>
        public string ProtocolVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 节点描述信息
        ///
        /// 用于显示或日志。
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 该节点发布的Frame
        ///
        /// Slave模拟时使用
        ///
        /// </summary>
        public List<LinFrame> PublishFrames
        {
            get;
            set;
        }
        =
        new List<LinFrame>();

        /// <summary>
        /// 该节点接收的Frame
        ///
        /// </summary>
        public List<LinFrame> SubscribeFrames
        {
            get;
            set;
        }
        =
        new List<LinFrame>();

        /// <summary>
        /// 重写显示
        ///
        /// TreeView/DataGrid显示节点名称。
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

    }

}