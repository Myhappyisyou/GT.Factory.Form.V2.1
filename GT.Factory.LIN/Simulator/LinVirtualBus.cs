using System;
using System.Collections.Generic;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;


namespace GT.Factory.LIN.Simulator
{


    /// <summary>
    /// LIN虚拟总线
    ///
    /// 模拟真实LIN Bus。
    ///
    ///
    /// 真实环境:
    ///
    ///     LinEngine
    ///          |
    ///          |
    ///     LIN Controller
    ///          |
    ///          |
    ///     LIN Bus
    ///          |
    ///          |
    ///     Slave ECU
    ///
    ///
    /// 模拟环境:
    ///
    ///     LinEngine
    ///          |
    ///          |
    ///     MockLinAdapter
    ///          |
    ///          |
    ///     LinVirtualBus
    ///          |
    ///          |
    ///     LinSlaveSimulator
    ///
    ///
    /// 作用:
    ///
    /// 1. 接收Master发送
    ///
    /// 2. 分发Header
    ///
    /// 3. 驱动Slave响应
    ///
    /// 4. 返回Response
    ///
    /// </summary>
    public class LinVirtualBus
    {


        /// <summary>
        /// LIN网络定义
        ///
        /// 包含:
        ///
        /// Frame
        /// Node
        /// Schedule
        ///
        /// </summary>
        private readonly LinNetwork _network;



        /// <summary>
        /// LIN运行环境
        ///
        /// 保存:
        ///
        /// Signal值
        ///
        /// Frame缓存
        ///
        /// </summary>
        private readonly LinRuntime _runtime;



        /// <summary>
        /// 总线上的节点
        ///
        /// Master不放这里。
        ///
        /// 只保存Slave。
        ///
        /// </summary>
        private readonly List<ILinNodeSimulator> _nodes;

        private LinMessage _response;

        /// <summary>
        /// 构造函数
        ///
        /// </summary>
        public LinVirtualBus(
            LinNetwork network,
            LinRuntime runtime)
        {

            _network = network;

            _runtime = runtime;


            _nodes =
                new List<ILinNodeSimulator>();

        }



        /// <summary>
        /// 添加LIN节点
        ///
        /// 例如:
        ///
        /// Slave_1
        /// Slave_2
        ///
        /// </summary>
        public void Attach(
            ILinNodeSimulator node)
        {

            if (node == null)
                return;


            if (_nodes.Contains(node))
                return;


            _nodes.Add(node);

            if (node is LinSlaveSimulator slave)
            {

                slave.ResponseGenerated +=
                    OnSlaveResponse;

            }
        }

        private void OnSlaveResponse(
     LinMessage message)
        {

            _response = message;

        }


        /// <summary>
        /// Master发送消息
        ///
        /// 模拟:
        ///
        /// Master Header + Data
        ///
        /// 到达LIN总线。
        ///
        /// </summary>
        /// <param name="message">
        /// Master发送消息
        /// </param>
        public LinMessage Send(
            LinMessage message)
        {


            //
            // 根据ID查找Frame
            //
            LinFrame frame =
                _network.Frames.Find(
                    x => x.Id == message.Id);



            if (frame == null)
            {

                return null;

            }

            //
            // 清空上一次响应
            //
            _response = null;


            //
            // 通知所有Slave
            //
            foreach (var node in _nodes)
            {

                node.OnHeaderReceived(
                    frame,
                    _runtime);

            }



            //
            // 检查当前Frame是否属于Slave响应
            //
            //if (frame.Publisher != null &&
            //   frame.Publisher.Type == LinNodeType.Slave)
            //{


            //    //
            //    // 从Runtime获取Slave生成的数据
            //    //
            //    LinMessage response =
            //        _runtime.GetFrame(
            //            frame.Id);




            //    return response;

            //}

            //
            // 返回Slave Response
            //
            LinMessage response =
                _response;



            _response = null;




            return response;

        }





        /// <summary>
        /// LIN周期调用
        ///
        /// 对应真实ECU:
        ///
        /// 任务周期
        ///
        /// </summary>
        public void Cycle()
        {


            foreach (var node in _nodes)
            {

                node.Cycle(
                    _runtime);

            }


        }


    }

}
