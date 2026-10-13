using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Protocol;

namespace GT.Factory.LIN.Tests.Core
{


    /// <summary>
    /// 
    /// LinReceiveWorker接收链路测试
    ///
    /// 验证:
    ///
    /// Adapter RX
    ///      ↓
    /// ReceiveWorker
    ///      ↓
    /// Decode
    ///      ↓
    /// Runtime
    ///
    /// 
    /// 
    /// </summary>
    [TestClass]
    public class LinReceiveWorkerTests
    {


        #region Environment


        private LinRuntime CreateRuntime(
            out LinSignal signal,
            out LinFrame frame)
        {


            var network =
                new LinNetwork();



            //
            // Signal
            //
            signal =
                new LinSignal
                {

                    Name =
                    "READ_SPEED",

                    Length =
                    16,

                    InitialValue =
                    0

                };


            network.Signals.Add(
                signal);



            //
            // Frame
            //
            frame =
                new LinFrame
                {

                    Name =
                    "MotorStatus",

                    Id =
                    0x20,

                    Length =
                    8

                };



            frame.Signals.Add(

                new LinFrameSignal
                {

                    Signal = signal,

                    StartBit = 0

                });



            network.Frames.Add(
                frame);



            return new LinRuntime(
                network);

        }



        #endregion




        /// <summary>
        ///
        /// 验证:
        ///
        /// Adapter收到RX消息
        ///
        /// 可以更新Runtime Signal
        ///
        /// </summary>
        [TestMethod]
        public void Test_ReceiveWorker_Decode_Update_Runtime()
        {


            //
            // 创建Runtime
            //
            var runtime =
                CreateRuntime(
                    out var signal,
                    out var frame);



            //
            // 模拟Adapter
            //
            var adapter =
                new MockLinAdapter();



            //
            // 创建ReceiveWorker
            //
            var worker =
                new LinReceiveWorker(
                    adapter,
                    runtime);



            //
            // 记录RX事件
            //
            LinMessage receiveMessage =
                null;



            worker.MessageReceived +=
                msg =>
                {

                    receiveMessage = msg;

                };

            //
            // 开硬件
            //
            adapter.Open();


            //
            // 启动接收
            //
            worker.Start();



            //
            // 构造Slave返回数据
            //
            //
            // READ_SPEED = 3000
            //
            // 3000 = 0x0BB8
            //
            // Little Endian:
            //
            // DATA[0]=B8
            // DATA[1]=0B
            //
            var message =
                new LinMessage(
                    0x20,
                    8);



            message.Direction =
                LinTraceDirection.Rx;


            message.FrameDirection =
                LinFrameDirection.SlaveToMaster;



            message.Data[0] =
                0xB8;


            message.Data[1] =
                0x0B;



            //
            // 模拟硬件收到消息
            //
            //
            // 注意:
            //
            // MockLinAdapter内部没有公开Raise方法
            //
            // 所以这里直接通过事件触发
            //
            //
            adapter
                .GetType()
                .GetField(
                    "MessageReceived",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);



            //
            // 模拟收到Slave响应
            //
            adapter.SimulateReceive(
                message);



            //
            // 验证事件收到
            //
            Assert.IsNotNull(
                receiveMessage);



            //
            // 验证Runtime已经更新
            //
            var value =
                runtime.GetSignalValue(
                    "READ_SPEED");



            Assert.AreEqual(
                3000,
                Convert.ToInt32(value));

            // RX原始Frame也必须成为Runtime当前FrameCache。
            var cachedFrame =
                runtime.GetFrame(
                    0x20);

            Assert.AreSame(
                message,
                cachedFrame);

            CollectionAssert.AreEqual(
                message.Data,
                cachedFrame.Data);



            worker.Stop();

        }


    }

}
