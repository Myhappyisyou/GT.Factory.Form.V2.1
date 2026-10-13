using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.ProjectDebug;
using GT.Factory.LIN.Test.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;


namespace GT.Factory.LIN.Tests.ProjectDebug
{


    /// <summary>
    /// LIN Debug Session测试
    ///
    /// 验证:
    ///
    /// 1. Debug数据模型创建
    ///
    /// 2. Frame列表生成
    ///
    /// 3. Signal列表生成
    ///
    /// 4. Trace链路
    ///
    /// 5. Signal读写接口
    ///
    ///
    /// 不测试:
    ///
    /// 1. LIN协议
    ///
    /// 2. Schedule算法
    ///
    /// 3. Simulator行为
    ///
    /// </summary>
    [TestClass]
    public class LinDebugSessionTests
    {


        /// <summary>
        /// 创建测试工程
        ///
        /// 使用已经验证通过的Simulation模式
        ///
        /// </summary>
        private LinProject CreateProject()
        {


            //
            // LDF文件
            //
            string ldf =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\BYD_300W_HEV_ELOP_V1.2.ldf";



            //
            // LIN配置
            //
            string config =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\SimulationConfig.json";




            //
            // 使用虚拟Adapter
            //
            var adapter =
                new MockLinAdapter();





            //
            // 创建LIN工程
            //
            var project =
                LinProject.Load(
                    ldf,
                    config,
                    adapter,
                    LinRunMode.Simulation,
                    new LinTestProject());



            return project;


        }







        /// <summary>
        /// 测试:
        ///
        /// LinDebugSession可以正常创建
        ///
        /// </summary>
        [TestMethod]
        public void Test_DebugSession_Create()
        {


            var project =
                CreateProject();



            var session =
                new LinDebugSession(
                    project);



            Assert.IsNotNull(
                session);



            //
            // Trace必须存在
            //
            Assert.IsNotNull(
                session.Trace);

        }


        /// <summary>
        /// 测试:
        ///
        /// LDF Frame是否生成Debug模型
        ///
        /// </summary>
        [TestMethod]
        public void Test_DebugSession_Build_Frame()
        {


            var project =
                CreateProject();

            var session =
                new LinDebugSession(
                    project);


            //
            // Frame数量必须大于0
            //
            Assert.IsTrue(
                session.Frames.Count > 0);

            var frame =
                session.Frames[0];

            Assert.IsFalse(
                string.IsNullOrEmpty(
                    frame.Name));

            Console.WriteLine(
                $"Frame:{frame.Name} ID:{frame.Id}");

        }



        /// <summary>
        /// 测试:
        ///
        /// Signal模型生成
        ///
        /// </summary>
        [TestMethod]
        public void Test_DebugSession_Build_Signal()
        {


            var project =
                CreateProject();


            var session =
                new LinDebugSession(
                    project);


            Assert.IsTrue(
                session.Signals.Count > 0);


            var signal =
                session.Signals[0];


            Assert.IsFalse(
                string.IsNullOrEmpty(
                    signal.Name));



            Console.WriteLine(
                $"Signal:{signal.Name}");

        }

        [TestMethod]
        public void Test_Trace_FullDecode()
        {

            var project =
                CreateProject();

            var session =
                new LinDebugSession(
                    project);

            project.Controller.Start(
                project.SimulationConfig.Schedule);

            //
            // 等待几个Schedule周期
            //
            Thread.Sleep(500);

            //
            // Trace应该有数据
            //
            Assert.IsTrue(
                session.Trace.Items.Count > 0);


            foreach (var item in session.Trace.Items)
            {

                Console.WriteLine(
                    item.ToString());


                Console.WriteLine(
                    $"Frame={item.FrameName}");


                Console.WriteLine(
                    $"Length={item.Length}");


                Console.WriteLine(
                    $"Data={BitConverter.ToString(item.Data)}");


                Console.WriteLine(
                    $"Checksum={item.ChecksumOK}");



                foreach (var signal in item.Signals)
                {

                    Console.WriteLine(
                        $"{signal.Name}={signal.Value}");

                }


                Console.WriteLine(
                    "--------------------");

            }

        }

        /// <summary>
        /// 测试:
        ///
        /// Trace是否能够收到TX/RX
        ///
        /// </summary>
        [TestMethod]
        public void Test_DebugSession_Trace()
        {

            var project =
                CreateProject();


            var session =
                new LinDebugSession(
                    project);

            //
            // 启动LIN
            //
            project.Controller.Start(
                project.SimulationConfig.Schedule);

            //
            // 等待几个周期
            //
            Thread.Sleep(
                300);


            var item =
                session.Trace.Items
                .FirstOrDefault(
                    x =>
                    x.FrameName
                    ==
                    "ELOP_Cmd");


            Assert.IsNotNull(item);


            Assert.IsNotNull(
                item.Signals);

            Assert.IsTrue(
                item.Signals.Count > 0);


            var speed =
                item.Signals
                .FirstOrDefault(
                    x =>
                    x.Name
                    ==
                    "WRITE_SPEED");


            Assert.IsNotNull(speed);


            Console.WriteLine(
                $"WRITE_SPEED={speed.Value}");

            //
            // 停止
            //
            project.Controller.Stop();


            foreach (var item1 in session.Trace.Items)
            {

                Console.WriteLine(
                    item1);

            }


            //
            // Trace应该存在
            //
            Assert.IsTrue(
                session.Trace.Items.Count > 0);


            //
            // 应该至少存在TX
            //
            bool hasTx =
                false;


            foreach (var item2 in session.Trace.Items)
            {

                if (item2.Direction == "TX")
                {

                    hasTx = true;

                    break;

                }

            }

            Assert.IsTrue(
                hasTx);

        }


        [TestMethod]
        public void Test_Debug_SetSignal()
        {

            var project =
               CreateProject();


            var session =
                new LinDebugSession(
                    project);

            session.SetSignalValue(
                "WRITE_SPEED",
                3000);


            var value =
                session.GetSignalValue(
                    "WRITE_SPEED");

            Assert.AreEqual(
                3000,
                Convert.ToInt32(value));

        }


        [TestMethod]
        public void Test_Trace_SignalDecode()
        {
            var project =
               CreateProject();


            var session =
                new LinDebugSession(
                    project);

            //
            // 设置发送值
            //
            session.SetSignalValue(
                "WRITE_SPEED",
                3000);


            project.Controller.Start(
                project.SimulationConfig.Schedule);


            Thread.Sleep(300);



            var item =
                session.Trace.Items
                .FirstOrDefault(
                    x =>
                    x.FrameName
                    ==
                    "ELOP_Cmd");


            Assert.IsNotNull(item);



            Assert.IsNotNull(
                item.Signals);



            Assert.IsTrue(
                item.Signals.Count > 0);



            var speed =
                item.Signals
                .FirstOrDefault(
                    x =>
                    x.Name
                    ==
                    "WRITE_SPEED");



            Assert.IsNotNull(speed);



            Console.WriteLine(
                $"WRITE_SPEED={speed.Value}");




            var item2 =
                session.Trace.Items
                .Last();



            Assert.IsNotNull(
                item2.Signals);



            Assert.IsTrue(
                item2.Signals.Count > 0);

        }



        [TestMethod]
        public void Test_DebugSession_SignalModify_UpdateFrame()
        {

            var project =
                CreateProject();


            var session =
                new LinDebugSession(
                    project);



            //      var signal =
            //session.Signals
            //.First(x =>
            //    x.Frame.Direction ==
            //    LinFrameDirection.MasterToSlave);



            var signal =
    session.Signals
    .First(x =>
        x.Name == "WRITE_SPEED");


            Console.WriteLine(
        $"Signal:{signal.Name}");

            Console.WriteLine(
                $"Frame:{signal.FrameName}");

          
            //var runtimeFrame =
            //    project.Runtime.GetFrame(
            //        (byte)signal.Frame.Id);


            var runtimeFrame =
      signal.Frame;


            Console.WriteLine(
                "Debug Before:" +
                BitConverter.ToString(
                    runtimeFrame.Data));


            Console.WriteLine(
    $"Runtime Before:{BitConverter.ToString(
        project.Runtime.GetFrame(
            (byte)runtimeFrame.Id).Data)}");


            session.SetSignalValue(
                signal.Name,
                3000);


            //
            // 刷新Debug数据
            //
            session.RefreshFrames();

            Assert.AreEqual(0x00, runtimeFrame.Data[0]);
            Assert.AreEqual(0x00, runtimeFrame.Data[1]);
            Assert.AreEqual(0xB8, runtimeFrame.Data[2]);
            Assert.AreEqual(0x0B, runtimeFrame.Data[3]);

            Assert.AreEqual(0xB8, runtimeFrame.Data[2]);
            Assert.AreEqual(0x0B, runtimeFrame.Data[3]);

            Console.WriteLine(
                $"Debug After:{BitConverter.ToString(runtimeFrame.Data)}");


            Console.WriteLine(
                $"Runtime After:{BitConverter.ToString(
                    project.Runtime
                    .GetFrame((byte)runtimeFrame.Id)
                    .Data)}");


            //runtimeFrame =
            //    project.Runtime.GetFrame(
            //        (byte)signal.Frame.Id);



            //Console.WriteLine(
            //    "Runtime After:" +
            //    BitConverter.ToString(
            //        runtimeFrame.Data));

        }



        [TestMethod]
        public void Test_DebugSession_SignalModify_UpdateValue()
        {

            var project =
                CreateProject();



            var session =
                new LinDebugSession(
                    project);



            var signal =
                session.Signals
                .First(x =>
                    x.Name == "WRITE_SPEED");



            Console.WriteLine(
                $"Before:{signal.Value}");



            session.SetSignalValue(
                signal.Name,
                3000);

            Assert.AreEqual(
    3000,
    Convert.ToInt32(project.Runtime.GetSignalValue(signal.Name)));

            Assert.AreEqual(
                3000,
                Convert.ToInt32(signal.Value));



            Console.WriteLine(
                $"Runtime:{project.Runtime.GetSignalValue(signal.Name)}");



            Console.WriteLine(
                $"Debug:{signal.Value}");



        }



        [TestMethod]
        public void Test_DebugSession_RuntimeFrame_Update_DebugFrame()
        {


            var project =
                CreateProject();



            var session =
                new LinDebugSession(
                    project);



            //
            // 找一个Frame
            //
            var frameInfo =
                session.Frames
                .First(
                    x =>
                    x.Name == "ELOP_Rsp1");



            //
            // Runtime中的Frame
            //
            var runtimeFrame =
                project.Runtime.GetFrame(
                    (byte)frameInfo.Id);



            Assert.IsNotNull(
                runtimeFrame);



            Console.WriteLine(
                $"Before Debug:{BitConverter.ToString(frameInfo.Data)}");


            Console.WriteLine(
                $"Before Runtime:{BitConverter.ToString(runtimeFrame.Data)}");




            //
            // 模拟RX数据
            //
            // 假设Slave返回:
            //
            // ID:
            // 0x1A
            //
            // DATA:
            // 12 34 56 78
            //
            runtimeFrame.Data[0] = 0x12;

            runtimeFrame.Data[1] = 0x34;

            runtimeFrame.Data[2] = 0x56;

            runtimeFrame.Data[3] = 0x78;

            session.RefreshFrames();

            Console.WriteLine(
                $"After Runtime:{BitConverter.ToString(runtimeFrame.Data)}");


            Console.WriteLine(
                $"After Debug:{BitConverter.ToString(frameInfo.Data)}");



            //
            // 验证Debug同步
            //
            Assert.AreEqual(
                0x12,
                frameInfo.Data[0]);


            Assert.AreEqual(
                0x34,
                frameInfo.Data[1]);


            Assert.AreEqual(
                0x56,
                frameInfo.Data[2]);


            Assert.AreEqual(
                0x78,
                frameInfo.Data[3]);



        }



        [TestMethod]
        public void Test_DebugSession_RuntimeFrame_Update_SignalValue()
        {

            var project =
                CreateProject();


            var session =
                new LinDebugSession(
                    project);



            //
            // 找发送Frame中的Signal
            //
            var signal =
                session.Signals
                .First(x =>
                    x.Name == "WRITE_SPEED");



            var frame =
                signal.Frame;



            Console.WriteLine(
                $"Signal:{signal.Name}");

            Console.WriteLine(
                $"Before Signal:{signal.Value}");



            //
            // 获取Runtime Frame
            //
            var message =
                project.Runtime.GetFrame(
                    (byte)frame.Id);



            Assert.IsNotNull(
                message);



            //
            // 模拟收到LIN数据
            //
            // WRITE_SPEED
            //
            // StartBit=16
            //
            // 3000 = 0x0BB8
            //
            message.Data[0] = 0x00;
            message.Data[1] = 0x00;
            message.Data[2] = 0xB8;
            message.Data[3] = 0x0B;



            //
            // 更新Runtime Frame
            //
            project.Runtime.UpdateFrame(
                message);



            //
            // 关键：
            //
            // Frame -> Signal Decode
            //
            var decoded =
                Protocol.LinSignalCode.Decode(
                    project.Runtime.Network.Frames
                        .First(x => x.Id == frame.Id),
                    signal.Source,
                    message.Data);

            Console.WriteLine(
    $"Decoded:{decoded}");

            //
            // 更新Runtime Signal
            //
            project.Runtime.SetSignalValue(
                signal.Name,
                decoded);

            Console.WriteLine(
    $"Decoded:{decoded}");

            Console.WriteLine(
                $"Runtime Signal After Set:{project.Runtime.GetSignalValue(signal.Name)}");

            Console.WriteLine(
    $"Before Refresh Hash:{signal.GetHashCode()}");
    

session.RefreshSignals();


var newSignal =
    session.Signals
    .First(x =>
        x.Name=="WRITE_SPEED");


Console.WriteLine(
    $"After Refresh Hash:{newSignal.GetHashCode()}");


Console.WriteLine(
    $"After Signal:{newSignal.Value}");



            Console.WriteLine(
                $"After Signal:{signal.Value}");



            Assert.AreEqual(
                3000,
                Convert.ToInt32(signal.Value));

        }



    }


}
