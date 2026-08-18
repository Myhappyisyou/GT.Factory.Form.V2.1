using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Simulator
{


    /// <summary>
    /// 默认Slave行为
    ///
    /// 没有特殊逻辑的ECU
    ///
    /// 只保持通信。
    ///
    /// </summary>
    public class DefaultSlaveBehavior
        :
        ILinSlaveBehavior
    {



        /// <summary>
        /// 收到Frame
        ///
        /// 默认不处理
        ///
        /// </summary>
        public void OnFrameReceived(
            LinFrame frame,
            LinRuntime runtime)
        {

        }




        /// <summary>
        /// 周期运行
        ///
        /// 默认不动作
        ///
        /// </summary>
        public void OnCycle(
            LinRuntime runtime)
        {

        }


    }

}

