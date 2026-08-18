using System;
using GT.Factory.LIN.Test.Model;


namespace GT.Factory.LIN.Test.Engine
{

    /// <summary>
    /// LIN测试报告
    ///
    /// 负责:
    ///
    /// 1. 格式化测试结果
    /// 2. 输出日志
    /// 3. 保存报告
    ///
    /// </summary>
    public class LinTestReport
    {

        /// <summary>
        /// 输出测试报告
        ///
        /// </summary>
        public void Print(
            LinTestResult result)
        {

            Console.WriteLine(
                "====================");

            Console.WriteLine(
                $"Test:{result.TestName}");

            Console.WriteLine(
                $"Result:{result.State}");

            foreach (var step in result.Steps)
            {

                Console.WriteLine(
                    $"{step.StepNo} " +
                    $"{step.StepName} " +
                    $"{step.Success} " +
                    $"{step.Message}");

            }

            Console.WriteLine(
                "====================");

        }

    }

}
