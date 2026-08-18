using GT.Factory.LIN.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Engine
{

    /// <summary>
    /// LIN测试执行器
    ///
    /// 负责:
    ///
    /// 1. 执行TestCase
    /// 2. 管理Step顺序
    /// 3. 生成测试结果
    ///
    /// 不负责:
    ///
    /// LIN协议
    /// Frame发送
    /// Signal解析
    ///
    /// </summary>
    public class LinTestRunner
    {

        private readonly LinActionExecutor _executor;

        /// <summary>
        /// 构造函数
        ///
        /// </summary>
        public LinTestRunner(
            LinActionExecutor executor)
        {
            _executor = executor;
        }

        /// <summary>
        /// 执行测试
        /// </summary>
        public LinTestResult Run(
            LinTestCase testCase)
        {

            var result =
                new LinTestResult
                {
                    TestName = testCase.Name,

                    StartTime = DateTime.Now,

                    State = TestResultState.Running
                };

            foreach (var step in testCase.Steps)
            {

                if (!step.Enabled)
                    continue;

                var stepResult =
                    _executor.Execute(step);

                result.Steps.Add(
                    stepResult);

                if (!stepResult.Success)
                {

                    result.State =
                        TestResultState.Fail;

                    break;

                }

            }

            if (result.State ==
                TestResultState.Running)
            {

                result.State =
                    TestResultState.Pass;

            }

            result.EndTime =
                DateTime.Now;

            return result;

        }

    }

}
