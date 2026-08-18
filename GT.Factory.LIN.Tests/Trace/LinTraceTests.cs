using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Simulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;


namespace GT.Factory.LIN.Tests.Trace
{

    [TestClass]
    public class LinTraceTests
    {


        private LinController CreateController(
            out MockLinAdapter adapter,
            out LinTraceLogger trace)
        {


            var network =
                new LinNetwork();



            #region Node


            var master =
                new LinNode
                {
                    Name =
                    "Master",

                    Type =
                    LinNodeType.Master
                };


            var motor =
                new LinNode
                {
                    Name =
                    "Motor",

                    Type =
                    LinNodeType.Slave
                };


            network.Nodes.Add(master);

            network.Nodes.Add(motor);


            #endregion





            #region Signal


            var speed =
                new LinSignal
                {
                    Name =
                    "WRITE_SPEED",

                    Length = 16,

                    InitialValue = 0
                };


            network.Signals.Add(speed);




            var actualSpeed =
                new LinSignal
                {
                    Name =
                    "ACTUAL_SPEED",

                    Length = 16,

                    InitialValue = 0
                };


            network.Signals.Add(actualSpeed);



            #endregion





            #region Master Frame


            var command =
                new LinFrame
                {
                    Name =
                    "MotorCommand",

                    Id = 0x10,

                    Length = 8,

                    Publisher = master

                };


            command.Signals.Add(

                new LinFrameSignal
                {
                    Signal = speed,

                    StartBit = 16
                });



            network.Frames.Add(command);



            #endregion





            #region Slave Response Frame


            var status =
                new LinFrame
                {
                    Name =
                    "MotorStatus",

                    Id = 0x20,

                    Length = 8,

                    Publisher = motor
                };



            status.Signals.Add(

                new LinFrameSignal
                {
                    Signal = actualSpeed,

                    StartBit = 16
                });



            network.Frames.Add(status);



            #endregion






            #region Schedule


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
                    command,

                    FrameName =
                    command.Name,

                    DelayMs = 20,

                    Order = 1
                });



            schedule.Items.Add(

                new LinScheduleItem
                {
                    Frame =
                    status,

                    FrameName =
                    status.Name,

                    DelayMs = 20,

                    Order = 2
                });


            network.Schedules.Add(schedule);

            #endregion

            //
            // Runtime
            //
            var runtime =
                new LinRuntime(
                    network);



            adapter =
                new MockLinAdapter();



            //
            // Virtual Bus
            //
            var bus =
                new LinVirtualBus(
                    network,
                    runtime);



            adapter.AttachBus(
                bus);



            var config =
                new LinSimulationConfig();



            var controller =
                new LinController(
                    runtime,
                    config,
                    adapter);



            //
            // Trace
            //
            var localTrace = new LinTraceLogger(); 

            trace = localTrace;


            //
            // TX Trace
            //
            controller.Engine.FrameSent +=
                message =>
                {

                    var txFrame =
                        network.Frames
                        .FirstOrDefault(
                            x =>
                            x.Id == message.Id);



                    if (txFrame == null)
                        return;



                    localTrace.Log(
                        "TX",
                        txFrame,
                        message);

                };



            //
            // RX Trace
            //
            adapter.MessageReceived +=
                message =>
                {

                    var rxFrame =
                        network.Frames
                        .FirstOrDefault(
                            x =>
                            x.Id == message.Id);



                    if (rxFrame == null)
                        return;



                    localTrace.Log(
                        "RX",
                        rxFrame,
                        message);

                };



            return controller;

        }





        [TestMethod]
        public void Test_Trace_TX()
        {

            var controller =
                CreateController(
                    out var adapter,
                    out var trace);



            controller.Start(
                "MotorSchedule");


            Thread.Sleep(
                100);


            controller.Stop();



            foreach (var item in trace.Items)
            {
                Console.WriteLine(item);
            }



            Assert.IsTrue(
                trace.Items.Count > 0);



            var tx =
                trace.Items
                .FirstOrDefault(
                    x =>
                    x.Direction == "TX");



            Assert.IsNotNull(
                tx);



            Assert.AreEqual(
                0x10,
                tx.Id);



        }



        [TestMethod]
        public void Test_Trace_RX()
        {


            var controller =
                CreateController(
                    out var adapter,
                    out var trace);



            controller.Start(
                "MotorSchedule");


            Thread.Sleep(
                200);


            controller.Stop();



            foreach (var item in trace.Items)
            {
                Console.WriteLine(item);
            }



            var rx =
                trace.Items
                .FirstOrDefault(
                    x =>
                    x.Direction == "RX");



            //
            // 当前如果没有Slave Simulator挂入VirtualBus
            //
            // RX不存在是架构连接问题
            //
            // 不是Trace问题
            //
            Assert.IsNotNull(
                rx);



        }



    }

}
