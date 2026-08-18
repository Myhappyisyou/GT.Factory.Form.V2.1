using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Simulator
{


    /// <summary>
    /// Slave业务行为接口
    ///
    /// 一个Slave对应一个ECU逻辑。
    ///
    /// 例如:
    ///
    /// 电机
    /// 门锁
    /// 空调
    ///
    /// 都实现这个接口。
    ///
    /// </summary>
    public interface ILinSlaveBehavior
    {



        /// <summary>
        /// 收到Master请求
        ///
        /// 例如:
        ///
        /// ELOP_Cmd
        ///
        /// </summary>
        void OnFrameReceived(
            LinFrame frame,
            LinRuntime runtime);




        /// <summary>
        /// 周期更新
        ///
        /// 模拟:
        ///
        /// 电机转速变化
        ///
        /// 温度变化
        ///
        /// 故障
        ///
        /// </summary>
        void OnCycle(
            LinRuntime runtime);


    }

}

