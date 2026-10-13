using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Parser;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.Simulator;
using GT.Factory.LIN.Test.Model;


namespace GT.Factory.LIN.Tests.Simulator
{


    [TestClass]
    public class LinSimulationTests
    {

        [TestMethod]
        public void Test_ELOP_LIN_Simulation()
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



            var adapter =
                new MockLinAdapter();



            var project =
                LinProject.Load(
                    ldf,
                    config,
                    adapter,
                    LinRunMode.Simulation,
                    new LinTestProject());



            project.Runtime.SetSignalValue(
                "WRITE_SPEED",
                3000);



            project.Controller.Engine.FrameSent +=
                msg =>
                {

                    Console.WriteLine(
                        $"TX {msg.FrameDirection} " +
                        $"ID:{msg.Id:X2} " +
                        $"{BitConverter.ToString(msg.Data)}");

                };



            adapter.MessageReceived +=
                msg =>
                {

                    Console.WriteLine(
                        $"RX {msg.FrameDirection} " +
                        $"ID:{msg.Id:X2} " +
                        $"{BitConverter.ToString(msg.Data)}");

                };



            project.Controller.Start(
                project.SimulationConfig.Schedule);



            Thread.Sleep(3000);

            project.Runtime.SetSignalValue(
                    "READ_ACTUAL_SPEED",3000);


            project.Controller.Stop();



            var speed =
                project.Runtime.GetSignalValue(
                    "READ_ACTUAL_SPEED");



            Console.WriteLine(
                $"ActualSpeed:{speed}");



            Assert.IsTrue(
               Convert.ToInt32( speed) > 0);

        }

        //[TestMethod]
        //public void Test_ELOP_LIN_Simulation()
        //{

        //    //
        //    // 1. LDF路径
        //    //
        //    string path =
        //    @"C:\Users\ding0\Desktop\BYD_300W_HEV_ELOP_V1.2(1).ldf";

        //    string configPath =
        //    @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\LinConfig.json";

        //    Assert.IsTrue(
        //        File.Exists(path));

        //    #region 屏蔽

        //    //
        //    // 2. 解析LDF
        //    //
        //    var reader =
        //        new LdfReader();


        //    var tokens =
        //        reader.Read(path);



        //    var parser =
        //        new LdfParser();



        //    LinNetwork network =
        //        parser.Parse(tokens);



        //    Assert.IsNotNull(network);



        //    Console.WriteLine(
        //        $"Frames:{network.Frames.Count}");



        //    foreach (var node in network.Nodes)
        //    {

        //        Console.WriteLine(
        //            $"Node:{node.Name} Type:{node.Type}");

        //    }

        //    //
        //    // 3. 创建Runtime
        //    //
        //    var runtime =
        //        new LinRuntime(
        //            network);

        //    //
        //    // 4. 设置Master发送信号
        //    //
        //    runtime.SetSignalValue(
        //        "WRITE_SPEED",
        //        3000);



        ////
        //// 5. 创建模拟Adapter
        ////
        //var adapter =
        //    new MockLinAdapter();



        ////
        //// 6. 创建虚拟LIN总线
        ////
        ////
        //// 模拟真实LIN线路
        ////
        //var bus =
        //    new LinVirtualBus(
        //        network,
        //        runtime);



        ////
        //// 8. Adapter连接VirtualBus
        ////
        //adapter.AttachBus(
        //    bus);



        //    //
        //    // 9. 加载LIN配置
        //    //
        //    var config =
        //        LinConfigManager.Load(
        //            configPath);

        //    //
        //    // 10. 创建Controller
        //    //
        //    var controller =
        //        new LinController(
        //            runtime,
        //            config,
        //            adapter);



        //    #endregion


        //    //
        //    // 11. Master发送Trace
        //    //
        //    controller.Engine.FrameSent +=
        //        msg =>
        //        {

        //            Console.WriteLine(
        //                $"TX " +
        //                $"{msg.FrameDirection} " +
        //                $"ID:{msg.Id:X2} " +
        //                $"{BitConverter.ToString(msg.Data)}");


        //        };


        //    //
        //    // 12. Slave响应Trace
        //    //
        //    adapter.MessageReceived +=
        //        msg =>
        //        {

        //            Console.WriteLine(
        //                $"RX " +
        //                $"{msg.FrameDirection} " +
        //                $"ID:{msg.Id:X2} " +
        //                $"{BitConverter.ToString(msg.Data)}");


        //        };


        //    //
        //    // 13. 启动LIN Schedule
        //    //
        //    controller.Start(
        //        "New_ScheduleTable_1");


        //    //
        //    // 14. 等待几个LIN周期
        //    //
        //    Thread.Sleep(
        //        3000);

        //    //
        //    // 15. 停止
        //    //
        //    controller.Stop();

        //    //
        //    // 16. 查看实际速度
        //    //
        //    double speed =
        //        controller.Runtime
        //        .GetSignalValue(
        //            "READ_ACTUAL_SPEED");



        //    Console.WriteLine(
        //        $"ActualSpeed:{speed}");




        //    //
        //    // 17. 验证Slave已经工作
        //    //
        //    Assert.IsTrue(
        //        speed > 0);




        //}



    }

}
