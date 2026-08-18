using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Simulator;
using GT.Factory.LIN.Test.Engine;
using GT.Factory.LIN.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Tests.Engine
{


    [TestClass]
    public class LinTestEngineCommunicationTests
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
            var speed =
                new LinSignal
                {

                    Name =
                    "WRITE_SPEED",

                    Length = 16,

                    InitialValue = 0

                };


            network.Signals.Add(
                speed);



            //
            // Frame
            //
            var frame =
                new LinFrame
                {

                    Name =
                    "MotorCommand",

                    Id = 0x10,

                    Length = 8

                };


            frame.Signals.Add(
                new LinFrameSignal
                {

                    Signal = speed,

                    StartBit = 16

                });


            network.Frames.Add(
                frame);



            //
            // Schedule
            //
            var schedule =
                new LinSchedule
                {

                    Name =
                    "MotorSchedule"

                };


            schedule.Items.Add(

                new LinScheduleItem
                {

                    Frame =
                    frame,

                    FrameName =
                    frame.Name,

                    DelayMs = 20,

                    Order = 1

                });


            network.Schedules.Add(
                schedule);



            var runtime =
                new LinRuntime(
                    network);



            adapter =
                new MockLinAdapter();


            var bus =
    new LinVirtualBus(network, runtime);


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



        /// <summary>
        ///
        /// 验证:
        ///
        /// TestCase
        /// 
        /// SetSignal
        ///
        /// 可以最终发送LIN Frame
        ///
        /// </summary>
        [TestMethod]
        public void Test_Run_TestCase_Send_Frame()
        {


            var controller =
                CreateController(
                    out var adapter);



            //
            // 获取Runtime
            //
            var runtime =
                controller.Runtime;



            var signalRuntime =
                new LinSignalRuntime(
                    runtime);



            var checker =
                new LinSignalChecker(
                    runtime);



            var executor =
                new LinActionExecutor(

                    runtime,

                    controller,

                    checker,

                    signalRuntime);



            var runner =
                new LinTestRunner(
                    executor);



            //
            // TestCase
            //
            var testCase =
                new LinTestCase
                {

                    Name =
                    "Motor_Test"

                };



            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 1,

                    Name =
                    "Set Speed",

                    Action =
                    LinTestAction.SetSignal,


                    Values =
                    {

                    new LinSignalValue
                    {

                        Signal =
                        "WRITE_SPEED",

                        Value =
                        3000

                    }

                    }

                });



            //
            // 执行测试
            //
            var result =
                runner.Run(
                    testCase);



            Assert.AreEqual(

                TestResultState.Pass,

                result.State);



            //
            // 启动LIN
            //
            controller.Start(
                "MotorSchedule");



            //
            // 等待一个周期
            //
            Thread.Sleep(
                100);



            controller.Stop();



            //
            // 验证发送
            //
            Assert.IsTrue(

                adapter.SentMessages.Count > 0);



            var message =
                adapter.SentMessages[0];



            Console.WriteLine(
                message);



            Assert.AreEqual(

                0x10,

                message.Id);



            //
            // 3000=0x0BB8
            //
            Assert.AreEqual(

                0xB8,

                message.Data[2]);



            Assert.AreEqual(

                0x0B,

                message.Data[3]);


        }


    }

}
