using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.ProjectDebug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace GT.Factory.LIN.Tests.Runtime
{


    /// <summary>
    ///
    /// LIN V0.4.1 Signal Runtime测试
    ///
    /// 验证:
    ///
    /// 1.
    /// Signal是否可以写入LinSignalRuntime
    ///
    /// 2.
    /// Signal值是否同步到LinRuntime
    ///
    /// 3.
    /// Signal是否可以编码到对应Frame
    ///
    /// 4.
    /// FrameCache中的数据是否发生变化
    ///
    ///
    /// 当前测试:
    ///
    /// 使用模拟LIN Network
    ///
    /// 不连接真实LIN硬件
    ///
    /// 不发送真实Frame
    ///
    /// </summary>
    [TestClass]
    public class LinSignalRuntimeTests
    {


        #region Test Environment


        /// <summary>
        ///
        /// 创建测试用LIN Network
        ///
        /// 模拟:
        ///
        /// WRITE_SPEED
        ///
        /// 由Frame:
        ///
        /// MotorCommand
        ///
        /// 承载。
        ///
        /// </summary>
        private LinNetwork CreateNetwork()
        {


            var network =
                new LinNetwork();



            //
            // 创建Signal
            //
            var signal =
                new LinSignal
                {

                    Name =
                    "WRITE_SPEED",

                    InitialValue =
                    0,

                    Length =
                    16

                };



            //
            // 加入Network
            //
            network.Signals.Add(
                signal);



            //
            // 创建Frame
            //
            var frame =
                new LinFrame
                {

                    Name =
                    "MotorCommand",

                    Id =
                    0x10,

                    Length =
                    8

                };



            //
            // 将Signal加入Frame
            //
            frame.Signals.Add(

                new LinFrameSignal
                {

                    Signal =
                    signal,

                    StartBit =
                    16

                });



            //
            // 加入Frame
            //
            network.Frames.Add(
                frame);



            return network;

        }


        /// <summary>
        ///
        /// 创建LIN Runtime
        ///
        /// </summary>
        private LinRuntime CreateRuntime()
        {


            var network =
                CreateNetwork();



            return new LinRuntime(
                network);

        }


        #endregion



        #region Multi-Frame Cache

        [TestMethod]
        public void Test_SetSignalValue_Updates_All_Related_FrameCaches()
        {
            var network =
                new LinNetwork();

            var signal =
                new LinSignal
                {
                    Name = "SHARED_VALUE",
                    InitialValue = 0,
                    Length = 8
                };

            network.Signals.Add(signal);

            var frame1 =
                new LinFrame
                {
                    Name = "FrameOne",
                    Id = 0x31,
                    Length = 8
                };

            frame1.Signals.Add(
                new LinFrameSignal
                {
                    Signal = signal,
                    StartBit = 0
                });

            var frame2 =
                new LinFrame
                {
                    Name = "FrameTwo",
                    Id = 0x32,
                    Length = 8
                };

            frame2.Signals.Add(
                new LinFrameSignal
                {
                    Signal = signal,
                    StartBit = 8
                });

            network.Frames.Add(frame1);
            network.Frames.Add(frame2);

            var runtime =
                new LinRuntime(network);

            var signalRuntime =
                new LinSignalRuntime(runtime);

            signalRuntime.SetValue(
                "SHARED_VALUE",
                0x5A);

            Assert.AreEqual(
                0x5A,
                Convert.ToInt32(
                    runtime.GetSignalValue(
                        "SHARED_VALUE")));

            Assert.AreEqual(
                0x5A,
                runtime.GetFrame(0x31).Data[0]);

            Assert.AreEqual(
                0x5A,
                runtime.GetFrame(0x32).Data[1]);
        }

        #endregion



        #region Signal Value


        /// <summary>
        ///
        /// 测试:
        ///
        /// Signal写入
        ///
        ///
        /// 验证:
        ///
        /// LinSignalRuntime.SetValue
        ///
        /// 是否可以正常修改
        /// Runtime中的Signal值。
        ///
        /// </summary>
        [TestMethod]
        public void Test_SetSignalValue()
        {


            var runtime =
                CreateRuntime();



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            //
            // 设置Signal
            //
            signalRuntime.SetValue(

                "WRITE_SPEED",

                3000);



            //
            // 输出
            //
            double value =
                Convert.ToDouble(runtime.GetSignalValue(
                    "WRITE_SPEED"));



            Console.WriteLine(
                $"WRITE_SPEED={value}");



            //
            // 验证Signal值
            //
            Assert.AreEqual(

                3000,

                value);


        }


        #endregion



        #region Frame Encode


        /// <summary>
        ///
        /// 测试:
        ///
        /// Signal
        ///     ↓
        /// Encode
        ///     ↓
        /// Frame
        ///
        ///
        /// 验证:
        ///
        /// 设置WRITE_SPEED后，
        ///
        /// 对应Frame的数据
        /// 是否发生变化。
        ///
        /// </summary>
        [TestMethod]
        public void Test_Signal_Encode_To_Frame()
        {


            var runtime =
                CreateRuntime();



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            //
            // 获取初始Frame
            //
            var before =
    runtime.GetFrame(
        0x10);


            Assert.IsNotNull(
    before);



            //
            // 保存初始数据快照
            //
            var beforeData =
                (byte[])before.Data.Clone();




            //
            // 输出初始数据
            //
            Console.WriteLine(
                "Before:");



            Console.WriteLine(
                BitConverter.ToString(
                    beforeData));



            //
            // 设置Signal
            //
            signalRuntime.SetValue(

                "WRITE_SPEED",

                3000);



            //
            // 获取更新后的Frame
            //
            var after =
                runtime.GetFrame(
                    0x10);



            Assert.IsNotNull(
                after);



            //
            // 输出更新数据
            //
            Console.WriteLine(
                "After:");



            Console.WriteLine(
                BitConverter.ToString(
                    after.Data));



            //
            // Frame数据应该发生变化
            //
            Assert.IsFalse(

                AreEqual(

                    beforeData,

                    after.Data));


        }


        #endregion



        #region Frame Cache


        /// <summary>
        ///
        /// 测试:
        ///
        /// FrameCache
        ///
        ///
        /// 验证:
        ///
        /// Signal修改后，
        ///
        /// LinRuntime中的
        ///
        /// FrameCache
        ///
        /// 已经同步更新。
        ///
        /// </summary>
        [TestMethod]
        public void Test_FrameCache_Updated()
        {


            var runtime =
                CreateRuntime();



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            //
            // 设置Signal
            //
            signalRuntime.SetValue(

                "WRITE_SPEED",

                3000);



            //
            // 从Runtime获取Frame
            //
            var frame =
                runtime.GetFrame(
                    0x10);



            Assert.IsNotNull(
                frame);



            //
            // 根据:
            //
            // StartBit = 16
            //
            // Length = 16
            //
            // 3000 = 0x0BB8
            //
            //
            // Little Endian:
            //
            // Byte2 = B8
            //
            // Byte3 = 0B
            //
            //
            // 所以:
            //
            // DATA[2] = B8
            //
            // DATA[3] = 0B
            //


            Console.WriteLine(
                $"DATA[2]={frame.Data[2]:X2}");



            Console.WriteLine(
                $"DATA[3]={frame.Data[3]:X2}");



            Assert.AreEqual(

                0xB8,

                frame.Data[2]);



            Assert.AreEqual(

                0x0B,

                frame.Data[3]);


        }


        #endregion



        #region Signal Decode


        /// <summary>
        ///
        /// 测试:
        ///
        /// Frame
        ///     ↓
        /// Decode
        ///     ↓
        /// Signal
        ///
        ///
        /// 验证:
        ///
        /// Frame中的Signal数据
        ///
        /// 可以被正确解析。
        ///
        /// </summary>
        [TestMethod]
        public void Test_Frame_Decode_Signal()
        {


            var runtime =
                CreateRuntime();



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            //
            // 写入Signal
            //
            signalRuntime.SetValue(

                "WRITE_SPEED",

                3000);



            //
            // 获取Frame
            //
            var frame =
                runtime.GetFrame(
                    0x10);



            Assert.IsNotNull(
                frame);



            //
            // 获取Signal定义
            //
            var signal =
                runtime.Network.Signals[0];



            //
            // Decode
            //
            object value =
                Protocol.LinSignalCode.Decode(

                    frame:
                    runtime.Network.Frames[0],

                    signal:
                    signal,

                    data:
                    frame.Data);



            Console.WriteLine(
                $"Decoded WRITE_SPEED={value}");



            //
            // 验证
            //
            Assert.AreEqual(

                3000,

               Convert.ToInt32( value));


        }


        #endregion



        #region Complete Flow


        /// <summary>
        ///
        /// 完整V0.4.1闭环测试
        ///
        ///
        /// 流程:
        ///
        /// Test:
        ///
        /// WRITE_SPEED=3000
        ///
        ///       ↓
        ///
        /// LinSignalRuntime
        ///
        ///       ↓
        ///
        /// LinRuntime SignalValue
        ///
        ///       ↓
        ///
        /// LinSignalCode.Encode
        ///
        ///       ↓
        ///
        /// Frame.Data
        ///
        ///       ↓
        ///
        /// LinSignalCode.Decode
        ///
        ///       ↓
        ///
        /// WRITE_SPEED=3000
        ///
        /// </summary>
        [TestMethod]
        public void Test_Signal_Frame_Closed_Loop()
        {


            var runtime =
                CreateRuntime();



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            //
            // Step1
            //
            // 设置Signal
            //
            signalRuntime.SetValue(

                "WRITE_SPEED",

                3000);



            //
            // Step2
            //
            // Runtime读取Signal
            //
            double runtimeValue =
               Convert.ToDouble(runtime.GetSignalValue(

                    "WRITE_SPEED"));



            Console.WriteLine(
                $"Runtime Signal={runtimeValue}");



            Assert.AreEqual(

                3000,

                runtimeValue);



            //
            // Step3
            //
            // 获取Frame
            //
            var frame =
                runtime.GetFrame(
                    0x10);



            Assert.IsNotNull(
                frame);



            //
            // Step4
            //
            // Decode Frame
            //
            var signal =
                runtime.Network.Signals[0];



            object decoded =
                Protocol.LinSignalCode.Decode(

                    runtime.Network.Frames[0],

                    signal,

                    frame.Data);



            Console.WriteLine(
                $"Frame Decode={decoded}");



            //
            // 最终验证
            //
            Assert.AreEqual(

                3000,

                Convert.ToInt32(decoded));


        }


        #endregion


        #region Multiple Signal


        /// <summary>
        /// 
        /// 多Signal同步测试
        ///
        /// 验证:
        ///
        /// 1.
        /// 多个Signal可以同时写入
        ///
        /// 2.
        /// Frame数据正确更新
        ///
        /// 3.
        /// 不同bit区域互不影响
        ///
        /// 模拟:
        ///
        /// WRITE_SPEED
        ///     bit16-31
        ///
        /// WRITE_ENABLE
        ///     bit0
        ///
        /// WRITE_MODE
        ///     bit8-9
        ///
        /// Frame:
        ///
        /// MotorCommand
        ///
        /// </summary>
        [TestMethod]
        public void Test_Multiple_Signal_Update()
        {


            var network =
                new LinNetwork();



            //
            // Signal1
            //
            var speed =
                new LinSignal
                {

                    Name =
                    "WRITE_SPEED",

                    Length =
                    16,

                    InitialValue =
                    0

                };



            //
            // Signal2
            //
            var enable =
                new LinSignal
                {

                    Name =
                    "WRITE_ENABLE",

                    Length =
                    1,

                    InitialValue =
                    0

                };



            //
            // Signal3
            //
            var mode =
                new LinSignal
                {

                    Name =
                    "WRITE_MODE",

                    Length =
                    2,

                    InitialValue =
                    0

                };



            network.Signals.Add(speed);
            network.Signals.Add(enable);
            network.Signals.Add(mode);



            //
            // Frame
            //
            var frame =
                new LinFrame
                {

                    Name =
                    "MotorCommand",

                    Id =
                    0x10,

                    Length =
                    8

                };



            //
            // Signal布局
            //
            // ENABLE
            // bit0
            //
            // MODE
            // bit8-9
            //
            // SPEED
            // bit16-31
            //
            frame.Signals.Add(

                new LinFrameSignal
                {

                    Signal =
                    enable,

                    StartBit =
                    0

                });



            frame.Signals.Add(

                new LinFrameSignal
                {

                    Signal =
                    mode,

                    StartBit =
                    8

                });



            frame.Signals.Add(

                new LinFrameSignal
                {

                    Signal =
                    speed,

                    StartBit =
                    16

                });



            network.Frames.Add(frame);



            //
            // Runtime
            //
            var runtime =
                new LinRuntime(
                    network);



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            //
            // 写入多个Signal
            //
            signalRuntime.SetValue(

                "WRITE_ENABLE",

                1);



            signalRuntime.SetValue(

                "WRITE_MODE",

                2);



            signalRuntime.SetValue(

                "WRITE_SPEED",

                3000);



            //
            // 获取Frame
            //
            var result =
                runtime.GetFrame(
                    0x10);



            Assert.IsNotNull(
                result);



            Console.WriteLine(
                BitConverter.ToString(
                    result.Data));



            //
            // Decode Enable
            //
            object enableValue =
                Protocol.LinSignalCode.Decode(

                    frame,

                    enable,

                    result.Data);



            //
            // Decode Mode
            //
            object modeValue =
                Protocol.LinSignalCode.Decode(

                    frame,

                    mode,

                    result.Data);



            //
            // Decode Speed
            //
            object speedValue =
                Protocol.LinSignalCode.Decode(

                    frame,

                    speed,

                    result.Data);



            Console.WriteLine(
                $"Enable={enableValue}");

            Console.WriteLine(
                $"Mode={modeValue}");

            Console.WriteLine(
                $"Speed={speedValue}");



            //
            // 验证
            //
            Assert.AreEqual(

                1,

               Convert.ToInt32( enableValue));



            Assert.AreEqual(

                2,

                Convert.ToInt32(modeValue));



            Assert.AreEqual(

                3000,

                Convert.ToInt32(speedValue));


        }



        #endregion


        #region Helper


        /// <summary>
        ///
        /// 比较两个Frame数据是否完全一致
        ///
        /// </summary>
        private bool AreEqual(
            byte[] a,
            byte[] b)
        {


            if (a == null ||
               b == null)
            {
                return
                    a == b;
            }



            if (a.Length != b.Length)
                return false;



            for (int i = 0;
                i < a.Length;
                i++)
            {

                if (a[i] != b[i])
                    return false;

            }



            return true;

        }


        #endregion




    }


}
