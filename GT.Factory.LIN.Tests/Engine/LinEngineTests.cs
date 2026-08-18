using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Simulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Tests.Engine
{


    [TestClass]
    public class LinEngineTests
    {


        #region Environment


        private LinController CreateController(
            out MockLinAdapter adapter)
        {


            var network =
                new LinNetwork();



            //
            // Signal
            //
            var signal =
                new LinSignal
                {

                    Name =
                    "TEST_VALUE",

                    Length = 8,

                    InitialValue = 0

                };


            network.Signals.Add(
                signal);



            //
            // Frame1
            //
            var frame1 =
                new LinFrame
                {

                    Name =
                    "Frame1",

                    Id = 0x10,

                    Length = 8

                };


            frame1.Signals.Add(

                new LinFrameSignal
                {

                    Signal =
                    signal,

                    StartBit = 0

                });


            network.Frames.Add(
                frame1);



            //
            // Frame2
            //
            var frame2 =
                new LinFrame
                {

                    Name =
                    "Frame2",

                    Id = 0x11,

                    Length = 8

                };


            network.Frames.Add(
                frame2);



            //
            // Schedule
            //
            var schedule =
                new LinSchedule
                {

                    Name =
                    "TestSchedule"

                };


            schedule.Items.Add(

                new LinScheduleItem
                {

                    Frame =
                    frame1,

                    FrameName =
                    frame1.Name,

                    DelayMs = 20,

                    Order = 1

                });



            schedule.Items.Add(

                new LinScheduleItem
                {

                    Frame =
                    frame2,

                    FrameName =
                    frame2.Name,

                    DelayMs = 20,

                    Order = 2

                });



            network.Schedules.Add(
                schedule);



            var runtime =
                new LinRuntime(
                    network);



            adapter =
                new MockLinAdapter();



            var bus =
                new LinVirtualBus(
                    network,
                    runtime);



            adapter.AttachBus(
                bus);



            var config =
                new LinSimulationConfig();



            return new LinController(

                runtime,

                config,

                adapter);

        }


        #endregion




        #region Start Stop



        /// <summary>
        ///
        /// 验证:
        ///
        /// Engine启动停止
        ///
        /// </summary>
        [TestMethod]
        public void Test_Engine_Start_Stop()
        {


            var controller =
                CreateController(
                    out var adapter);



            controller.Start(
                "TestSchedule");



            Thread.Sleep(
                100);



            Assert.IsTrue(
                adapter.IsOpen);



            controller.Stop();



            Assert.IsFalse(
                adapter.IsOpen);


        }



        #endregion




        #region Schedule Order



        /// <summary>
        ///
        /// 验证:
        ///
        /// Schedule顺序执行
        ///
        ///
        /// Frame1
        ///
        /// Frame2
        ///
        /// Frame1
        ///
        /// 
        /// </summary>
        [TestMethod]
        public void Test_Schedule_Execute_Order()
        {


            var controller =
                CreateController(
                    out var adapter);



            controller.Start(
                "TestSchedule");



            Thread.Sleep(
                150);



            controller.Stop();



            Assert.IsTrue(

                adapter.SentMessages.Count
                >=
                2);



            Console.WriteLine(
                "TX Trace");



            foreach (var msg
                in adapter.SentMessages)
            {

                Console.WriteLine(
                    $"ID:{msg.Id:X2}");

            }



            Assert.AreEqual(

                0x10,

                adapter.SentMessages[0].Id);



            Assert.AreEqual(

                0x11,

                adapter.SentMessages[1].Id);



        }


        #endregion




        #region Frame Sent Event



        /// <summary>
        ///
        /// 验证:
        ///
        /// FrameSent事件
        ///
        /// </summary>
        [TestMethod]
        public void Test_FrameSent_Event()
        {


            var controller =
                CreateController(
                    out var adapter);



            int count = 0;



            controller.Engine.FrameSent +=
                msg =>
                {

                    count++;

                    Console.WriteLine(
                        $"Event ID:{msg.Id:X2}");

                };



            controller.Start(
                "TestSchedule");



            Thread.Sleep(
                80);



            controller.Stop();



            Assert.IsTrue(
                count > 0);


        }



        #endregion


        /// <summary>
        /// 
        /// 验证：
        ///
        /// Engine.Stop()之后
        ///
        /// RunLoop已经真正停止。
        ///
        ///
        /// 验证流程：
        ///
        /// Start
        ///     ↓
        /// Schedule发送
        ///     ↓
        /// Stop
        ///     ↓
        /// 等待
        ///     ↓
        /// 发送数量不再增加
        ///
        /// </summary>
        [TestMethod]
        public void Test_Stop_Stops_Engine()
        {


            var controller =
                CreateController(
                    out var adapter);



            //
            // 启动
            //
            controller.Start(
                "TestSchedule");



            //
            // 等待Engine运行
            //
            Thread.Sleep(
                100);



            //
            // 停止
            //
            controller.Stop();



            //
            // 记录停止后的发送数量
            //
            int countAfterStop =
                adapter.SentMessages.Count;



            Console.WriteLine(
                $"Messages After Stop:{countAfterStop}");



            //
            // 再等待一段时间
            //
            Thread.Sleep(
                100);



            //
            // 停止后不应该继续发送
            //
            int countLater =
                adapter.SentMessages.Count;



            Console.WriteLine(
                $"Messages Later:{countLater}");



            Assert.AreEqual(

                countAfterStop,

                countLater);


        }


    }

}
