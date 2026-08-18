using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using System;
using System.Threading;


namespace GT.Factory.LIN.Simulator
{

    /// <summary>
    /// 电机Slave模拟
    ///
    /// 模拟:
    ///
    /// ELOP泵
    ///
    /// </summary>
    public class MotorSlaveBehavior
        :
        ILinSlaveBehavior
    {

        /// <summary>
        /// 当前目标转速
        /// </summary>
        private double _targetSpeed;

        /// <summary>
        /// 当前实际转速
        /// </summary>
        private double _actualSpeed;

        /// <summary>
        /// 收到Frame
        ///
        /// </summary>
        public void OnFrameReceived(
            LinFrame frame,
            LinRuntime runtime)
        {

      //      Console.WriteLine(
      //$"Motor Receive:{frame.Name}");

            //
            // 判断命令Frame
            //
            if (frame.Name == "ELOP_Cmd")
            {


                //
                // 读取目标速度
                //
                _targetSpeed =
                  Convert.ToDouble(
        runtime.GetSignalValue(
            "WRITE_SPEED"));

                //runtime.SetSignalValue(
                //    "READ_ACTUAL_SPEED",
                //    _targetSpeed);

            }

        }

        /// <summary>
        /// 周期运行
        ///
        /// 模拟电机响应
        ///
        /// </summary>
        public void OnCycle(
            LinRuntime runtime)
        {

            //
            // 简单模拟:
            //
            // 每周期靠近目标
            //
            if (_actualSpeed < _targetSpeed)
            {

                _actualSpeed += 100;

            }
            else if (_actualSpeed > _targetSpeed)
            {

                _actualSpeed -= 100;

            }

            Thread.Sleep(10);

            //
            // 更新反馈Signal
            //
            runtime.SetSignalValue(
                "READ_ACTUAL_SPEED",
                _actualSpeed);

            //
            // 状态正常
            //
            runtime.SetSignalValue(
                "READ_STATUS",
                1);

        }

    }

}

