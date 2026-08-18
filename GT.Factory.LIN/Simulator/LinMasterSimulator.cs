using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Simulator
{


    /// <summary>
    /// LIN Master模拟器
    ///
    /// 负责:
    ///
    /// 1. Schedule控制
    ///
    /// 2. Header发送
    ///
    ///
    /// 注意:
    ///
    /// 实际调度由:
    ///
    /// LinEngine
    ///
    /// 完成。
    ///
    /// </summary>
    public class LinMasterSimulator
        : ILinNodeSimulator
    {



        public LinNode Node
        {
            get;
            private set;
        }





        public LinMasterSimulator(
            LinNode node)
        {

            Node = node;

        }





        /// <summary>
        /// Master收到自己的Header
        ///
        /// 不需要处理
        ///
        /// </summary>
        public void OnHeaderReceived(
            LinFrame frame,
            LinRuntime runtime)
        {

            // Master不响应

        }



        /// <summary>
        /// Master周期
        ///
        /// 暂无逻辑
        ///
        /// </summary>
        public void Cycle(
            LinRuntime runtime)
        {

        }


    }

}
