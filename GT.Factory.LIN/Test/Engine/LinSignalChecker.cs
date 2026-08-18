using GT.Factory.LIN.Core;
using GT.Factory.LIN.Test.Model;
using System;

namespace GT.Factory.LIN.Test.Engine
{

    /// <summary>
    /// LIN Signal检查器
    ///
    /// 负责:
    ///
    /// 获取Runtime中的Signal值
    ///
    /// 并根据:
    ///
    /// CompareType
    ///
    /// 判断是否满足条件。
    ///
    ///
    /// 示例:
    ///
    /// READ_ACTUAL_SPEED > 2800
    ///
    /// </summary>
    public class LinSignalChecker
    {

        /// <summary>
        /// LIN Runtime
        ///
        /// 用于读取当前Signal值
        ///
        /// </summary>
        private readonly LinRuntime _runtime;

        /// <summary>
        /// 构造函数
        ///
        /// </summary>
        public LinSignalChecker(
            LinRuntime runtime)
        {

            _runtime = runtime;

        }

        /// <summary>
        /// 执行Signal检查
        ///
        /// </summary>
        public LinStepResult Check(
            LinTestStep step)
        {

            foreach (var check in step.Checks)
            {

                //
                // 获取实际值
                //
                object actual =
                    _runtime.GetSignalValue(
                        check.Signal);

                //
                // 比较
                //
                bool pass =
                    Compare(
                        actual,
                        check);

                if (!pass)
                {

                    return new LinStepResult
                    {

                        StepNo =
                            step.StepNo,

                        StepName =
                            step.Name,

                        Success =
                            false,

                        ActualValue =
                            actual,

                        Message =
                        $"检查失败 {check.Signal}={actual}"

                    };

                }

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
                    "Signal Check OK"

            };

        }

        /// <summary>
        /// 比较逻辑
        ///
        /// 支持:
        ///
        /// Equal
        /// Greater
        /// Less
        ///
        /// </summary>
        private bool Compare(
            object actual,
            LinSignalCheck check)
        {

            if (actual == null)
                return false;

            if (check.Value == null)
                return false;

            double a =
                Convert.ToDouble(actual);

            double b =
                Convert.ToDouble(check.Value);

            switch (check.Compare)
            {

                case CompareType.Equal:

                    return
                        Math.Abs(a - b)
                        <= check.Tolerance;

                case CompareType.Greater:

                    return a > b;

                case CompareType.GreaterOrEqual:

                    return a >= b;

                case CompareType.Less:

                    return a < b;

                case CompareType.LessOrEqual:

                    return a <= b;

                default:

                    return false;

            }

        }

    }

}