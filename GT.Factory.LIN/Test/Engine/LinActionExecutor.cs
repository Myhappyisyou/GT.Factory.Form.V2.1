using GT.Factory.LIN.Core;
using GT.Factory.LIN.Test.Model;
using System;
using System.Threading;

namespace GT.Factory.LIN.Test.Engine
{

    /// <summary>
    /// LIN测试动作执行器
    ///
    /// 负责:
    ///
    /// 根据:
    ///     LinTestStep.Action
    ///
    /// 执行具体测试动作。
    ///
    ///
    /// 当前支持:
    ///
    ///     SetSignal
    ///     Wait
    ///     CheckSignal
    ///
    ///
    /// 后续扩展:
    ///
    ///     SendFrame
    ///     WaitFrame
    ///     ReadSignal
    ///
    ///
    /// 注意:
    ///
    /// 本类不负责:
    ///
    ///     LIN协议解析
    ///     Frame打包
    ///     Schedule调度
    ///
    /// 这些由:
    ///
    ///     LinRuntime
    ///     LinController
    ///
    /// 完成。
    ///
    /// </summary>
    public class LinActionExecutor
    {

        /// <summary>
        /// Signal检查器
        ///
        /// 负责:
        ///
        /// Signal实际值
        /// 与
        /// 期望值
        ///
        /// 比较
        ///
        /// </summary>
        private readonly LinSignalChecker _checker;

        /// <summary>
        /// LIN运行时
        ///
        /// 负责:
        ///
        /// Signal读写
        ///
        /// </summary>
        private readonly LinRuntime _runtime;

        private readonly LinSignalRuntime _signalRuntime;

        /// <summary>
        /// LIN控制器
        ///
        /// 负责:
        ///
        /// Schedule启动
        /// Frame发送
        ///
        /// </summary>
        private readonly LinController _controller;

        /// <summary>
        /// 构造函数
        ///
        /// </summary>
        /// <param name="runtime">
        /// LIN Runtime
        /// </param>
        /// <param name="controller">
        /// LIN Controller
        /// </param>
        /// <param name="checker">
        /// Signal检查器
        /// </param>
        public LinActionExecutor(

            LinRuntime runtime,

            LinController controller,

            LinSignalChecker checker,

            LinSignalRuntime signalRuntime)

        {
            _runtime = runtime;

            _controller = controller;

            _checker = checker;

            _signalRuntime = signalRuntime;

        }

        /// <summary>
        /// 执行一个测试步骤
        ///
        /// </summary>
        public LinStepResult Execute(
            LinTestStep step)
        {

            switch (step.Action)
            {

                //
                // 设置Signal
                //
                case LinTestAction.SetSignal:

                    return ExecuteSetSignal(step);

                //
                // 等待
                //
                case LinTestAction.Wait:

                    return ExecuteWait(step);

                //
                // 检查Signal
                //
                case LinTestAction.CheckSignal:

                    return ExecuteCheck(step);

                default:

                    return new LinStepResult
                    {

                        StepNo =
                            step.StepNo,

                        StepName =
                            step.Name,

                        Success =
                            false,

                        Message =
                            $"不支持Action:{step.Action}"

                    };

            }

        }

        /// <summary>
        /// 设置Signal值
        ///
        /// 示例:
        ///
        /// WRITE_SPEED=3000
        ///
        /// 最终调用:
        ///
        /// LinRuntime.SetSignalValue
        ///
        /// </summary>
        private LinStepResult ExecuteSetSignal(
            LinTestStep step)
        {

            foreach (var value in step.Values)
            {

                //
                // 写入LIN Runtime
                //
                _runtime.SetSignalValue(
                    value.Signal,
                    value.Value);

                _signalRuntime.SetValue(
                    value.Signal,
                    value.Value);

                //Console.WriteLine(
                //    $"Set {value.Signal}={value.Value}");

            }

            return new LinStepResult
            {

                StepNo =
                    step.StepNo,

                StepName =
                    step.Name,

                Success =
                    true,

                Message =
                    "Signal设置完成"

            };

        }

        /// <summary>
        /// 等待动作
        ///
        /// 用于:
        ///
        /// 电机启动等待
        /// 状态稳定等待
        ///
        /// </summary>
        private LinStepResult ExecuteWait(
            LinTestStep step)
        {

            Thread.Sleep(
                step.Timeout);

            return new LinStepResult
            {

                StepNo =
                    step.StepNo,

                StepName =
                    step.Name,

                Success =
                    true,

                Message =
                    "Wait完成"

            };

        }

        /// <summary>
        /// 检查Signal
        ///
        /// 例如:
        ///
        /// READ_ACTUAL_SPEED > 2800
        ///
        /// </summary>
        private LinStepResult ExecuteCheck(
            LinTestStep step)
        {

            return _checker.Check(step);

        }

    }

}