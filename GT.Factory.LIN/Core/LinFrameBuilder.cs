using System;
using System.Collections.Generic;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Protocol;


namespace GT.Factory.LIN.Core
{

    /// <summary>
    /// LIN Frame构造器
    ///
    /// 负责:
    ///
    /// Signal值
    ///
    ///     ↓
    ///
    /// Frame Data
    ///
    ///     ↓
    ///
    /// LinMessage
    ///
    ///
    /// 相当于LIN协议栈中的:
    ///
    /// Signal Packing层
    ///
    /// </summary>
    public class LinFrameBuilder
    {

        /// <summary>
        /// 当前LIN网络
        /// </summary>
        private readonly LinNetwork _network;

        /// <summary>
        /// 创建Frame构造器
        ///
        /// </summary>
        public LinFrameBuilder(
            LinNetwork network)
        {

            _network = network;

        }

        /// <summary>
        /// 根据Frame名称生成消息
        ///
        /// </summary>
        public LinMessage Build(
            string frameName,
            LinRuntime runtime)
        {

            //
            // 查找Frame
            //
            LinFrame frame =
                _network.Frames
                .Find(
                x => x.Name == frameName);

            if (frame == null)
            {
                throw new System.Exception(
                    $"不存在Frame:{frameName}");
            }

            return Build(
                frame,
                runtime);

        }

        /// <summary>
        /// 根据Frame生成消息
        ///
        /// </summary>
        public LinMessage Build(
            LinFrame frame,
            LinRuntime runtime)
        {

     //       Console.WriteLine(
     //$"Build Frame:{frame.Name}");



            //Console.WriteLine(
            //    $"Signal Count:{frame.Signals.Count}");
            //
            // 创建数据
            //
            byte[] data =
                new byte[frame.Length];

            //
            // 遍历Frame中的Signal
            //
            foreach (var frameSignal
                in frame.Signals)
            {


                //
                // 找Signal定义
                //
                //LinSignal signal =
                //    _network.Signals
                //    .Find(
                //    x => x.Name
                //    ==
                //    frameSignal.Signal.Name);

                var signal =
       frameSignal.Signal;

                if (signal == null)
                    continue;

                //
                // 获取当前值
                //
                object value =
                    runtime.GetSignalValue(
                        signal.Name);

                //
                // 写入Data
                //
                LinSignalCode.Encode(
                    frame,
                    signal,
                    value,
                    data);

            }

            //
            // 创建LIN消息
            //
            LinMessage message =
                new LinMessage(
                    frame.Id,
                    frame.Length);

            message.Data = data;

            //
            // 判断是谁发布这个Frame
            //
            //
            // 判断是谁发布这个Frame
            //
            if (frame.Publisher != null)
            {

                if (frame.Publisher.Type
                    == LinNodeType.Master)
                {
                    message.FrameDirection =
                        LinFrameDirection.MasterToSlave;
                }
                else
                {
                    message.FrameDirection =
                        LinFrameDirection.SlaveToMaster;
                }

            }
            else
            {

                //
                // 未解析Publisher
                //
                // 默认Master发送
                //
                message.FrameDirection =
                    LinFrameDirection.MasterToSlave;

            }



            //
            // 计算校验
            //
            message.Checksum =
                LinChecksum.CalculateEnhanced(
                    frame.Id,
                    data);

            message.IsChecksumValid = true;

            return message;

        }

    }

}

