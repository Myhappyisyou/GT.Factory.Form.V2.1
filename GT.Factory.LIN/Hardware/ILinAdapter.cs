using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using System;


namespace GT.Factory.LIN.Hardware
{


    /// <summary>
    /// LIN通信适配器接口
    ///
    /// 负责屏蔽底层LIN硬件差异。
    ///
    /// 上层模块:
    ///
    ///     LinEngine
    ///
    /// 不需要关心:
    ///
    ///     Vector
    ///     Kvaser
    ///     USB LIN
    ///     模拟总线
    ///
    ///
    /// 统一通过:
    ///
    ///     Send()
    ///
    /// 发送LIN消息。
    ///
    /// 通过:
    ///
    ///     MessageReceived
    ///
    /// 获取LIN响应。
    ///
    ///
    /// 设计原则:
    ///
    ///     Engine只关注LIN协议流程。
    ///
    ///     Adapter负责具体通信实现。
    ///
    /// </summary>
    public interface ILinAdapter
    {


        /// <summary>
        /// LIN通道打开
        ///
        /// 真实环境:
        ///
        ///     初始化LIN硬件
        ///     打开Channel
        ///     设置BaudRate
        ///
        ///
        /// 模拟环境:
        ///
        ///     初始化虚拟总线
        ///
        /// </summary>
        void Open();



        /// <summary>
        /// LIN通道关闭
        ///
        /// 释放:
        ///
        ///     硬件资源
        ///     线程
        ///     缓存
        ///
        /// </summary>
        void Close();



        /// <summary>
        /// 发送LIN消息
        ///
        /// 对于Master:
        ///
        ///     发送Header
        ///
        ///     +
        ///
        ///     Data
        ///
        ///
        /// 注意:
        ///
        /// LIN协议中:
        ///
        /// Header由Master产生。
        ///
        /// Response由Publisher节点提供。
        ///
        /// </summary>
        /// <param name="message">
        /// LIN消息
        /// </param>
        void Send(
            LinMessage message);



        /// <summary>
        /// LIN消息接收事件
        ///
        /// 当LIN总线上出现Response时触发。
        ///
        /// 来源:
        ///
        ///     真实硬件:
        ///         LIN Controller
        ///
        ///     模拟:
        ///         LinVirtualBus
        ///
        ///
        /// 参数:
        ///
        ///     LinMessage
        ///
        /// 包含:
        ///
        ///     Frame ID
        ///     Data
        ///     Checksum
        ///
        /// </summary>
        event Action<LinMessage> MessageReceived;



        /// <summary>
        /// 当前LIN通道状态
        ///
        /// true:
        ///     已打开
        ///
        /// false:
        ///     未打开
        ///
        /// </summary>
        bool IsOpen
        {
            get;
        }


    }


}
