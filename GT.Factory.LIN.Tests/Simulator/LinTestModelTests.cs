using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.Test.Engine;
using GT.Factory.LIN.Test.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;


namespace GT.Factory.LIN.Tests.Simulator
{

    /// <summary>
    /// LIN V0.2 Test Model测试
    ///
    /// 验证:
    ///
    /// 1. TestCase模型是否完整
    ///
    /// 2. Step是否可以描述测试流程
    ///
    /// 3. JSON是否可以保存测试配置
    ///
    /// </summary>
    [TestClass]
    public class LinTestModelTests
    {



        /// <summary>
        /// 创建一个完整LIN测试案例
        ///
        /// 模拟:
        ///
        /// 电机测试
        ///
        /// 流程:
        ///
        /// 1 设置速度3000rpm
        ///
        /// 2 等待
        ///
        /// 3 检查实际速度
        ///
        /// </summary>
        [TestMethod]
        public void Test_Create_LIN_TestProject()
        {


            string ldf =
                @"C:\Users\ding0\Desktop\BYD_300W_HEV_ELOP_V1.2(1).ldf";



            //
            // 创建项目
            //
            var project =
                new LinTestProject
                {

                    Name =
                    "BYD_300W_HEV_ELOP",


                    LdfFile =
                    ldf

                };

            Console.WriteLine(
                $"Project:{project.Name}");

            Console.WriteLine(
                $"LDF:{project.LdfFile}");


            //
            // 创建测试Case
            //
            var testCase =
                new LinTestCase
                {

                    Name =
                    "Motor_Test",

                    Description =
                    "电机速度测试"

                };

            Console.WriteLine(
                $"Create Case:{testCase.Name}");


            //
            // Step1:
            //
            // 设置目标速度
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


                            Value = 3000,


                            ValueType =
                            LinValueType.Physical

                        }

                    }

                });



            //
            // Step2:
            //
            // 等待
            //
            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 2,

                    Name =
                    "Wait Motor",


                    Action =
                    LinTestAction.Wait,


                    Timeout = 3000

                });



            //
            // Step3:
            //
            // 检查反馈
            //
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


                            Value = 2800,


                            ValueType =
                            LinValueType.Physical

                        }

                    }

                });




            //
            // Case加入Project
            //
            project.Cases.Add(
                testCase);


            //
            // 输出完整测试结构
            //
            foreach (var tc in project.Cases)
            {


                Console.WriteLine(
                    $"Case:{tc.Name}");



                foreach (var step in tc.Steps)
                {

                    Console.WriteLine(
                        $" Step:{step.StepNo} " +
                        $"{step.Name} " +
                        $"Action:{step.Action}");



                    foreach (var value in step.Values)
                    {

                        Console.WriteLine(
                            $"  Set " +
                            $"{value.Signal}=" +
                            $"{value.Value} " +
                            $"Type:{value.ValueType}");

                    }



                    foreach (var check in step.Checks)
                    {

                        Console.WriteLine(
                            $"  Check " +
                            $"{check.Signal} " +
                            $"{check.Compare} " +
                            $"{check.Value}");

                    }

                }

            }



            //
            // 验证
            //
            Assert.AreEqual(
                1,
                project.Cases.Count);



            Assert.AreEqual(
                3,
                project.Cases[0].Steps.Count);



            Assert.AreEqual(
                "WRITE_SPEED",
                project.Cases[0]
                .Steps[0]
                .Values[0]
                .Signal);


        }


        /// <summary>
        ///
        /// 测试TestCase JSON保存
        ///
        /// 产线实际:
        ///
        /// 测试配置文件
        ///
        /// Motor_Test.json
        ///
        /// </summary>
        [TestMethod]
        public void Test_TestCase_Json()
        {


            var testCase =
                new LinTestCase
                {

                    Name =
                    "Motor_Test"

                };



            //
            // Step1:
            //
            // 设置目标速度
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


                            Value = 3000,


                            ValueType =
                            LinValueType.Physical

                        }

                    }

                });



            //
            // Step2:
            //
            // 等待
            //
            testCase.Steps.Add(

                new LinTestStep
                {

                    StepNo = 2,

                    Name =
                    "Wait Motor",


                    Action =
                    LinTestAction.Wait,


                    Timeout = 3000

                });



            //
            // Step3:
            //
            // 检查反馈
            //
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


                            Value = 2800,


                            ValueType =
                            LinValueType.Physical

                        }

                    }

                });



            //
            // 序列化
            //
            string json =
                JsonConvert.SerializeObject(
                    testCase,
                    Formatting.Indented,
                    new StringEnumConverter());



            Console.WriteLine(json);



            //
            // 反序列化
            //
            var result =
                JsonConvert.DeserializeObject<LinTestCase>(
                    json);



            Assert.IsNotNull(
                result);



            Assert.AreEqual(
                "Motor_Test",
                result.Name);



            Assert.AreEqual(
                3,
                result.Steps.Count);



            Assert.AreEqual(
                "WRITE_SPEED",
                result.Steps[0]
                .Values[0]
                .Signal);


        }





        /// <summary>
        ///
        /// 验证测试项目结构
        ///
        /// </summary>
        [TestMethod]
        public void Test_LinTestProject()
        {
            string ldf =
               @"C:\Users\ding0\Desktop\BYD_300W_HEV_ELOP_V1.2(1).ldf";


            var project =
                new LinTestProject
                {

                    Name =
                    "BYD_300W_HEV_ELOP",


                    LdfFile = ldf

                };

            project.Cases.Add(

                new LinTestCase
                {

                    Name =
                    "Motor Start",

                    Steps = new List<LinTestStep> {
                       new LinTestStep
                        {

                            StepNo = 1,

                            Name ="Set Speed",

                            Action =  LinTestAction.SetSignal,


                            Values =
                                {

                                    new LinSignalValue
                                    {

                                        Signal =
                                        "WRITE_SPEED",

                                        Value = 3000

                                    }

                                }


                        }
                    }
                });

            Assert.AreEqual(
                "BYD_300W_HEV_ELOP",
                project.Name);

            Assert.AreEqual(
                1,
                project.Cases.Count);

        }


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
                @"C:\Users\ding0\Desktop\BYD_300W_HEV_ELOP_V1.2(1).ldf";



            //
            // LIN配置
            //
            string config =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\LinConfig.json";




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
                    LinRunMode.Simulation);



            return project;


        }


        [TestMethod]
        public void Test_MotorECU_WriteSpeed_Response()
        {

            var project =
                CreateProject();


            //
            // 启动LIN
            //
            project.Controller.Start(
                project.SimulationConfig.Schedule);




            //
            // 设置目标速度
            //
            project.Runtime.SetSignalValue(
                "WRITE_SPEED",
                3000);



            Console.WriteLine(
                "WRITE_SPEED=3000");


            //
            // 观察20个周期
            //
            for (int i = 0; i < 40; i++)
            {

                Thread.Sleep(10);


                var speed =
                    project.Runtime.GetSignalValue(
                        "READ_ACTUAL_SPEED");


                Console.WriteLine(
                    $"Cycle:{i}  READ_ACTUAL_SPEED={speed}");

            }




            //
            // 等待几个LIN周期
            //
            Thread.Sleep(1000);



            var actual =
                project.Runtime.GetSignalValue(
                    "READ_ACTUAL_SPEED");



            Console.WriteLine(
                $"READ_ACTUAL_SPEED={actual}");



            Assert.IsTrue(
                Convert.ToDouble(actual) > 0);



            Assert.IsTrue(
                Convert.ToDouble(actual) <= 3000);



            project.Controller.Stop();

        }



        [TestMethod]
        public void Test_Motor_EndToEnd()
        {

            //
            // 创建LIN工程
            //
            var project =
                CreateProject();


            //
            // 启动LIN
            //
            project.Controller.Start(
                project.SimulationConfig.Schedule);



            //
            // 创建Test Runner
            //
            var runner =
                CreateRunner(
                    project.Runtime);



            //
            // 创建测试Case
            //
            var testCase =
                new LinTestCase
                {
                    Name = "Motor_EndToEnd"
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

                    Name = "Set Speed",

                    Action =
                    LinTestAction.SetSignal,


                    Values =
                    {
                new LinSignalValue
                {
                    Signal="WRITE_SPEED",

                    Value=3000
                }
                    }

                });



            //
            // Step2
            //
            // 等待LIN周期
            //
            testCase.Steps.Add(
                new LinTestStep
                {

                    StepNo = 2,

                    Name = "Wait Motor",

                    Action =
                    LinTestAction.Wait,


                    Timeout = 1000

                });



            //
            // Step3
            //
            // 判断反馈
            //
            testCase.Steps.Add(
                new LinTestStep
                {

                    StepNo = 3,

                    Name = "Check Speed",

                    Action =
                    LinTestAction.CheckSignal,


                    Checks =
                    {
                new LinSignalCheck
                {
                    Signal=
                    "READ_ACTUAL_SPEED",


                    Compare=
                    CompareType.Greater,


                    Value=2800
                }
                    }

                });



            //
            // 执行
            //
            var result =
                runner.Run(testCase);



            PrintResult(result);



            Assert.AreEqual(
                TestResultState.Pass,
                result.State);



            project.Controller.Stop();

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