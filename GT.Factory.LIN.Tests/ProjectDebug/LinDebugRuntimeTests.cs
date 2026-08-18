using Microsoft.VisualStudio.TestTools.UnitTesting;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.ProjectDebug;
using System;


namespace GT.Factory.LIN.Tests.ProjectDebug
{


    //[TestClass]
    //public class LinDebugRuntimeTests
    //{


    //    private LinRuntime CreateRuntime()
    //    {


    //        var network =
    //            new LinNetwork();



    //        var nodeMaster =
    //            new LinNode
    //            {
    //                Name = "Master",
    //                Type = LinNodeType.Master
    //            };


    //        var nodeSlave =
    //            new LinNode
    //            {
    //                Name = "Motor",
    //                Type = LinNodeType.Slave
    //            };


    //        network.Nodes.Add(nodeMaster);
    //        network.Nodes.Add(nodeSlave);



    //        var signal =
    //            new LinSignal
    //            {
    //                Name = "Speed",
    //                Length = 16,
    //                InitialValue = 0
    //            };


    //        network.Signals.Add(signal);



    //        var frame =
    //            new LinFrame
    //            {
    //                Name = "MotorCommand",
    //                Id = 0x10,
    //                Length = 8,
    //                Publisher = nodeMaster
    //            };


    //        frame.Signals.Add(

    //            new LinFrameSignal
    //            {
    //                Signal = signal,
    //                StartBit = 16
    //            });


    //        network.Frames.Add(frame);



    //        return new LinRuntime(network);

    //    }





    //    /// <summary>
    //    /// 验证:
    //    ///
    //    /// Runtime变化
    //    /// DebugSession可以读取最新值
    //    ///
    //    /// </summary>
    //    [TestMethod]
    //    public void Test_DebugSession_Read_Runtime_Value()
    //    {


    //        var runtime =
    //            CreateRuntime();



    //        var session =
    //            new LinDebugSession(
    //                runtime);



    //        runtime.SetSignalValue(
    //            "Speed",
    //            3000);



    //        var value =
    //            session.GetSignalValue(
    //                "Speed");



    //        Assert.IsNotNull(value);



    //        Assert.AreEqual(
    //            3000,
    //            Convert.ToInt32(value));


    //    }






    //    /// <summary>
    //    ///
    //    /// 验证:
    //    ///
    //    /// DebugSession SetSignal
    //    ///
    //    /// 可以反向修改Runtime
    //    ///
    //    /// </summary>
    //    [TestMethod]
    //    public void Test_DebugSession_SetSignal_Update_Runtime()
    //    {


    //        var runtime =
    //            CreateRuntime();



    //        var session =
    //            new LinDebugSession(
    //                runtime);



    //        session.SetSignalValue(
    //            "Speed",
    //            2500);



    //        var value =
    //            runtime.GetSignalValue(
    //                "Speed");



    //        Assert.IsNotNull(value);



    //        Assert.AreEqual(
    //            2500,
    //            Convert.ToInt32(value));


    //    }





    //    /// <summary>
    //    ///
    //    /// 验证Frame调试信息
    //    ///
    //    /// </summary>
    //    [TestMethod]
    //    public void Test_DebugSession_Frame_Info()
    //    {


    //        var runtime =
    //            CreateRuntime();



    //        var session =
    //            new LinDebugSession(
    //                runtime);



    //        Assert.AreEqual(
    //            1,
    //            session.Frames.Count);



    //        var frame =
    //            session.Frames[0];



    //        Assert.AreEqual(
    //            "MotorCommand",
    //            frame.Name);



    //        Assert.AreEqual(
    //            0x10,
    //            frame.Id);



    //        Assert.AreEqual(
    //            "Master",
    //            frame.Publisher);



    //        Assert.AreEqual(
    //            LinFrameDirection.MasterToSlave,
    //            frame.Direction);



    //    }


    //}

}
