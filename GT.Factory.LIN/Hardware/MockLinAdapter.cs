using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Simulator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace GT.Factory.LIN.Hardware
{


    /// <summary>
    /// LIN模拟适配器
    ///
    /// 用于:
    ///
    ///     1. 无LIN硬件环境测试
    ///
    ///     2. 单元测试
    ///
    ///     3. UI调试工具开发
    ///
    ///
    /// 当前阶段:
    ///
    ///     模拟LIN通信入口。
    ///
    /// 后续:
    ///
    ///     通过LinVirtualBus连接Slave模拟器。
    ///
    /// </summary>
    public class MockLinAdapter
        :
        ILinAdapter,
    IVirtualLinAdapter
    {


        /// <summary>
        /// 虚拟LIN总线
        ///
        /// 模拟:
        ///
        ///     LIN物理线路
        ///
        /// </summary>
        private LinVirtualBus _bus;



        /// <summary>
        /// 当前通道状态
        /// </summary>
        public bool IsOpen
        {
            get;
            private set;
        }



        /// <summary>
        /// 接收事件
        ///
        /// 模拟Slave响应时触发。
        ///
        /// 对应真实硬件:
        ///
        /// LIN Controller接收回调。
        ///
        /// </summary>
        public event Action<LinMessage> MessageReceived;



        /// <summary>
        /// 构造函数
        ///
        /// </summary>
        public MockLinAdapter()
        {

        }


        // <summary>
        /// 绑定虚拟总线
        ///
        /// 注意:
        ///
        /// Adapter本身不知道Slave存在。
        ///
        /// 它只连接Bus。
        ///
        /// </summary>
        public void AttachBus(
            LinVirtualBus bus)
        {

            _bus = bus;

        }



        /// <summary>
        /// 打开模拟LIN通道
        ///
        /// </summary>
        public void Open()
        {

            IsOpen = true;

        }



        /// <summary>
        /// 关闭模拟LIN通道
        ///
        /// </summary>
        public void Close()
        {

            IsOpen = false;

        }



        /// <summary>
        /// 发送LIN消息
        ///
        /// 当前阶段:
        ///
        ///     仅作为模拟发送入口。
        ///
        /// 下一阶段:
        ///
        ///     转交:
        ///
        ///         LinVirtualBus
        ///
        ///     由SlaveSimulator生成Response。
        ///     
        /// 流程:
        ///
        /// LinEngine
        ///       |
        ///       |
        /// Send
        ///       |
        ///       |
        /// VirtualBus
        ///       |
        ///       |
        /// Slave响应
        ///
        ///
        /// </summary>
        /// <param name="message">
        /// LIN发送消息
        /// </param>
        public void Send(
            LinMessage message)
        {

     //       Console.WriteLine(
     //$"Mock Send Open={IsOpen}");

            if (!IsOpen)
            {
                //Console.WriteLine(
                //    "Adapter not open");

                return;
            }

            //
            // Trace发送
            //
            SentMessages.Add(message);

            if (_bus == null)
            {

                throw new Exception(
                    "MockLinAdapter未绑定LinVirtualBus");

            }


            //
            // 交给虚拟总线处理
            //
            LinMessage response =
                _bus.Send(
                    message);



            //
            // 如果Slave有响应
            //
            if (response != null)
            {

                RaiseMessageReceived(
                    response);

            }
        }



        public List<LinMessage> SentMessages
        {
            get;
            private set;
        }
        =
        new List<LinMessage>();


        /// <summary>
        /// 模拟接收到Slave响应
        ///
        /// 该方法供:
        ///
        ///     LinVirtualBus
        ///
        /// 调用。
        ///
        /// 模拟真实硬件:
        ///
        ///     MessageReceived事件
        ///
        /// </summary>
        /// <param name="message">
        /// Slave返回消息
        /// </param>
        internal void RaiseMessageReceived(
            LinMessage message)
        {

            if (!IsOpen)
                return;


            MessageReceived?
                .Invoke(
                    message);

        }


        /// <summary>
        /// 模拟硬件收到LIN消息
        ///
        /// 用于:
        /// 
        /// 1. 单元测试
        /// 2. UI模拟接收
        /// 3. Slave模拟
        ///
        /// 真实硬件不会调用。
        /// </summary>
        public void SimulateReceive(
            LinMessage message)
        {
            RaiseMessageReceived(message);
        }


        private LinMessage Clone(
    LinMessage source)
        {

            return new LinMessage
            {

                Id =
                source.Id,


                Length =
                source.Length,


                Direction =
                source.Direction,


                FrameDirection =
                source.FrameDirection,


                Data =
                (byte[])source.Data.Clone(),


                Timestamp =
                source.Timestamp,


                Checksum =
                source.Checksum,


                IsChecksumValid =
                source.IsChecksumValid

            };

        }

    }


}