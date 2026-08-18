using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.Simulator;
using GT.Factory.LIN.Test.Engine;
using GT.Factory.LIN.Test.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace GT.Factory.LIN.Tests.Engine
{


    /// <summary>
    ///
    /// LIN V0.3.1 Test Engine测试
    ///
    /// 验证:
    ///
    /// 1.
    /// LinTestRunner测试流程
    ///
    /// 2.
    /// LinActionExecutor动作执行
    ///
    /// 3.
    /// LinSignalChecker比较逻辑
    ///
    /// 4.
    /// LinRuntime数据交互
    ///
    /// 5.
    /// LinTestReport输出
    ///
    ///
    /// 当前:
    ///
    /// 模拟LIN运行环境
    ///
    /// 不连接真实LIN硬件
    ///
    /// 不发送真实Frame
    ///
    /// </summary>
    [TestClass]
    public class LinTestEngineTests
    {



        #region Test Environment



        /// <summary>
        ///
        /// 创建模拟LIN Runtime
        ///
        /// 提供:
        ///
        /// WRITE_SPEED
        ///
        /// READ_ACTUAL_SPEED
        ///
        /// 两个Signal
        ///
        /// </summary>
        private LinRuntime CreateRuntime()
        {


            var network =
                new LinNetwork();



            network.Signals.Add(

                new LinSignal
                {

                    Name =
                    "WRITE_SPEED",


                    InitialValue =
                    0

                });



            network.Signals.Add(

                new LinSignal
                {

                    Name =
                    "READ_ACTUAL_SPEED",


                    InitialValue =
                    0

                });



            return new LinRuntime(network);

        }







        /// <summary>
        ///
        /// 创建Test Runner
        ///
        /// 包含:
        ///
        /// LinRuntime
        ///
        /// LinController
        ///
        /// LinSignalChecker
        ///
        /// LinActionExecutor
        ///
        /// </summary>
        private LinTestRunner CreateRunner(
            LinRuntime runtime)
        {


            var config =
                new LinSimulationConfig();



            var adapter =
                new MockLinAdapter();



            var controller =
                new LinController(

                    runtime,

                    config,

                    adapter);

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



            return new LinTestRunner(
                executor);

        }



        #endregion






        #region SetSignal



        /// <summary>
        ///
        /// 测试:
        ///
        /// LinTestCase
        ///
        /// Step:
        ///
        /// SetSignal
        ///
        ///
        /// 验证:
        ///
        /// WRITE_SPEED
        /// 是否写入Runtime
        ///
        /// </summary>
        [TestMethod]
        public void Test_Run_SetSignal_TestCase()
        {


            var runtime =
                CreateRuntime();



            var runner =
                CreateRunner(runtime);



            var testCase =
                new LinTestCase
                {

                    Name =
                    "SetSpeed_Test"

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



            var result =
                runner.Run(testCase);


            PrintResult(result);


            //Console.WriteLine(
            //    result.State);



            Assert.AreEqual(

                TestResultState.Pass,

                result.State);



            Assert.AreEqual(

                3000,

              Convert.ToInt32(runtime.GetSignalValue(
                    "WRITE_SPEED")));

        }



        #endregion






        #region CheckSignal



        /// <summary>
        ///
        /// CheckSignal通过测试
        ///
        /// Actual:
        ///
        /// 3000
        ///
        /// Expect:
        ///
        /// >2800
        ///
        /// </summary>
        [TestMethod]
        public void Test_Run_CheckSignal_Pass()
        {


            var runtime =
                CreateRuntime();



            runtime.SetSignalValue(

                "READ_ACTUAL_SPEED",

                3000);



            var runner =
                CreateRunner(runtime);



            var testCase =
                new LinTestCase
                {

                    Name =
                    "CheckSpeed_Test"

                };



            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 1,


                    Name =
                    "Check Speed",


                    Action =
                    LinTestAction.CheckSignal,


                    Checks =
                    {

                        new LinSignalCheck
                        {

                            Signal =
                            "READ_ACTUAL_SPEED",


                            Compare =
                            CompareType.Greater,


                            Value =
                            2800

                        }

                    }

                });



            var result =
                runner.Run(testCase);



            //Console.WriteLine(
            //    result.State);



            Assert.AreEqual(

                TestResultState.Pass,

                result.State);

        }







        /// <summary>
        ///
        /// CheckSignal失败测试
        ///
        /// Actual:
        ///
        /// 2000
        ///
        /// Expect:
        ///
        /// >2800
        ///
        /// </summary>
        [TestMethod]
        public void Test_Run_CheckSignal_Fail()
        {


            var runtime =
                CreateRuntime();



            runtime.SetSignalValue(

                "READ_ACTUAL_SPEED",

                2000);



            var runner =
                CreateRunner(runtime);



            var testCase =
                new LinTestCase
                {

                    Name =
                    "CheckSpeed_Fail"

                };



            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 1,


                    Name =
                    "Check Speed",


                    Action =
                    LinTestAction.CheckSignal,


                    Checks =
                    {

                        new LinSignalCheck
                        {

                            Signal =
                            "READ_ACTUAL_SPEED",


                            Compare =
                            CompareType.Greater,


                            Value =
                            2800

                        }

                    }

                });



            var result =
                runner.Run(testCase);



            Console.WriteLine(
                result.State);



            Assert.AreEqual(

                TestResultState.Fail,

                result.State);



            Assert.AreEqual(

                2000,

                result.Steps[0]
                .ActualValue);

        }



        #endregion






        #region Complete TestCase



        /// <summary>
        ///
        /// 完整Motor测试
        ///
        /// 流程:
        ///
        /// Step1:
        /// Set Speed
        ///
        /// Step2:
        /// Wait Motor
        ///
        /// Step3:
        /// Check Speed
        ///
        /// </summary>
        [TestMethod]
        public void Test_Run_Motor_TestCase()
        {


            var runtime =
                CreateRuntime();



            //
            // 模拟反馈
            //
            runtime.SetSignalValue(

                "READ_ACTUAL_SPEED",

                3000);



            var runner =
                CreateRunner(runtime);



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



            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 2,


                    Name =
                    "Wait Motor",


                    Action =
                    LinTestAction.Wait,


                    Timeout =
                    100

                });



            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 3,


                    Name =
                    "Check Speed",


                    Action =
                    LinTestAction.CheckSignal,


                    Checks =
                    {

                        new LinSignalCheck
                        {

                            Signal =
                            "READ_ACTUAL_SPEED",


                            Compare =
                            CompareType.Greater,


                            Value =
                            2800

                        }

                    }

                });



            var result =
                runner.Run(testCase);



            PrintResult(result);



            Assert.AreEqual(

                TestResultState.Pass,

                result.State);



            Assert.AreEqual(

                3,

                result.Steps.Count);


        }



        #endregion



        /// <summary>
        ///
        /// 验证TestStep执行顺序
        ///
        /// 流程:
        ///
        /// Step1:
        /// Set WRITE_SPEED=3000
        ///
        /// Step2:
        /// Check WRITE_SPEED > 2000
        ///
        ///
        /// 验证:
        ///
        /// 1.
        /// Step按照添加顺序执行
        ///
        /// 2.
        /// 前一步设置的数据
        /// 可以被后一步读取
        ///
        /// </summary>
        [TestMethod]
        public void Test_Run_Step_Order()
        {


            var runtime =
                CreateRuntime();



            var runner =
                CreateRunner(runtime);




            var testCase =
                new LinTestCase
                {

                    Name =
                    "Step_Order_Test"

                };




            //
            // Step1
            //
            // 设置速度
            //
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
            // Step2
            //
            // 检查刚才设置的值
            //
            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 2,


                    Name =
                    "Check Speed",


                    Action =
                    LinTestAction.CheckSignal,


                    Checks =
                    {

                new LinSignalCheck
                {

                    Signal =
                    "WRITE_SPEED",


                    Compare =
                    CompareType.Greater,


                    Value =
                    2000

                }

                    }

                });





            //
            // 执行TestCase
            //
            var result =
                runner.Run(testCase);





            //
            // 输出日志
            //
            foreach (var step in result.Steps)
            {

                Console.WriteLine(

                    $"Step:{step.StepNo} " +
                    $"{step.StepName} " +
                    $"{step.Success} " +
                    $"{step.Message}");

            }





            //
            // 验证总结果
            //
            Assert.AreEqual(

                TestResultState.Pass,

                result.State);





            //
            // 验证Step数量
            //
            Assert.AreEqual(

                2,

                result.Steps.Count);





            //
            // 验证执行顺序
            //
            Assert.AreEqual(

                1,

                result.Steps[0]
                .StepNo);



            Assert.AreEqual(

                2,

                result.Steps[1]
                .StepNo);





            //
            // 验证第一步确实执行成功
            //
            Assert.IsTrue(

                result.Steps[0]
                .Success);




            //
            // 验证第二步读取到了第一步写入的数据
            //
            Assert.IsTrue(

                result.Steps[1]
                .Success);



            Assert.AreEqual(

                3000,

               Convert.ToInt32(runtime.GetSignalValue(
                    "WRITE_SPEED")));

        }


        #region Report



        /// <summary>
        ///
        /// 测试报告输出
        ///
        /// </summary>
        [TestMethod]
        public void Test_Report_Print()
        {


            var result =
                new LinTestResult
                {

                    TestName =
                    "Motor_Test",


                    State =
                    TestResultState.Pass,


                    StartTime =
                    DateTime.Now.AddSeconds(-1),


                    EndTime =
                    DateTime.Now

                };



            result.Steps.Add(

                new LinStepResult
                {

                    StepNo = 1,


                    StepName =
                    "Set Speed",


                    Success =
                    true,


                    Message =
                    "Signal设置完成"

                });



            var report =
                new LinTestReport();



            //report.Print(result);



            Assert.AreEqual(

                1,

                result.Steps.Count);

        }



        #endregion




        /// <summary>
        /// 输出测试结果日志
        ///
        /// 用于调试阶段观察:
        ///
        /// Test状态
        /// Step执行结果
        /// ActualValue
        /// Message
        ///
        /// </summary>
        private void PrintResult(
            LinTestResult result)
        {

            Console.WriteLine("======================");

            Console.WriteLine(
                $"Test:{result.TestName}");

            Console.WriteLine(
                $"State:{result.State}");


            foreach (var step in result.Steps)
            {

                Console.WriteLine(

                    $"Step:{step.StepNo} " +
                    $"Name:{step.StepName} " +
                    $"Success:{step.Success}");



                if (step.ActualValue != null)
                {
                    Console.WriteLine(
                        $"Actual:{step.ActualValue}");
                }


                Console.WriteLine(
                    $"Message:{step.Message}");

            }


            Console.WriteLine("======================");

        }

    }

}