using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Simulator
{


    /// <summary>
    /// LIN节点模拟器接口
    ///
    /// 所有LIN节点:
    ///
    /// Master
    /// Slave
    ///
    /// 都实现此接口。
    ///
    ///
    /// 一个LIN网络中:
    ///
    /// Master负责调度
    ///
    /// Slave负责响应
    ///
    /// </summary>
    public interface ILinNodeSimulator
    {



        /// <summary>
        /// 当前节点
        ///
        /// 对应:
        ///
        /// LinNode
        ///
        /// </summary>
        LinNode Node
        {
            get;
        }





        /// <summary>
        /// 收到LIN Header
        ///
        /// Master发送Frame ID
        ///
        /// 节点判断是否响应
        ///
        /// </summary>
        /// <param name="frame">
        /// 当前Frame
        /// </param>
        /// <param name="runtime">
        /// 运行环境
        /// </param>
        void OnHeaderReceived(
            LinFrame frame,
            LinRuntime runtime);



        /// <summary>
        /// 周期运行
        ///
        /// 每个LIN周期调用。
        ///
        /// 用于:
        ///
        /// 1. 模拟ECU内部状态
        ///
        /// 2. 更新Signal
        ///
        /// </summary>
        void Cycle(
            LinRuntime runtime);



    }

}

