using GT.Factory.LIN.Config;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Simulator;
using System;
using System.Collections.Generic;


namespace GT.Factory.LIN.Core
{


    /// <summary>
    /// LIN控制器
    ///
    /// 是LIN系统的总入口。
    ///
    /// 负责：
    ///
    /// 1. 创建Runtime
    ///
    /// 2. 创建Engine
    ///
    /// 3. 创建Adapter
    ///
    /// 4. 创建节点模拟器
    ///
    /// 5. 管理启动停止
    ///
    ///
    /// 不负责：
    ///
    /// 1. LIN协议解析
    ///
    /// 2. ECU业务逻辑
    ///
    /// 3. UI显示
    ///
    /// </summary>
    public class LinController
    {

        /// <summary>
        /// LIN配置
        /// </summary>
        private readonly LinSimulationConfig _config;

        /// <summary>
        /// LIN网络定义
        /// </summary>
        public LinNetwork Network
        {
            get;
            private set;
        }

        /// <summary>
        /// LIN运行环境
        ///
        /// 保存Signal当前值
        ///
        /// </summary>
        public LinRuntime Runtime
        {
            get;
            private set;
        }

        /// <summary>
        /// LIN主引擎
        ///
        /// Schedule执行
        ///
        /// Frame发送
        ///
        /// </summary>
        public LinEngine Engine
        {
            get;
            private set;
        }

        /// <summary>
        /// LIN硬件适配器
        ///
        /// 可以是:
        ///
        /// MockLinAdapter
        ///
        /// VectorAdapter
        ///
        /// </summary>
        private readonly ILinAdapter _adapter;

        /// <summary>
        /// 接收线程
        /// </summary>
        private LinReceiveWorker _receiver;

        /// <summary>
        /// 节点模拟列表
        ///
        /// Master
        ///
        /// Slave
        ///
        /// </summary>
        private readonly List<ILinNodeSimulator> _nodes;


        /// <summary>
        /// 虚拟LIN总线
        ///
        /// Mock环境使用。
        ///
        /// 真实硬件不使用。
        /// </summary>
        private LinVirtualBus _virtualBus;


        /// <summary>
        /// 创建LIN控制器
        ///
        /// </summary>
        public LinController(
             LinRuntime runtime,
             LinSimulationConfig config,
             ILinAdapter adapter)
        {

            Network = runtime.Network;

            _config = config;

            //
            // 创建运行环境
            //
            Runtime = runtime;

            //
            // 创建硬件
            //
            // 当前默认模拟
            //
            _adapter = adapter;

            //
            // 创建LIN Engine
            //
            Engine =
                new LinEngine(
                    Runtime,
                    _adapter);

            //
            // 创建接收线程
            //
            _receiver =
                new LinReceiveWorker(
                    _adapter,
                    Runtime);

            _nodes =
                new List<ILinNodeSimulator>();

            //
            // Mock环境创建VirtualBus
            //
            CreateVirtualBus();



            ////
            //// 创建节点
            ////
            CreateNodes(
                runtime.Network);

            //
            // 绑定事件
            //
            BindEvents();

        }


        private void CreateVirtualBus()
        {

            if (_adapter is MockLinAdapter mock)
            {

                _virtualBus =
                    new LinVirtualBus(
                        Network,
                        Runtime);



                mock.AttachBus(
                    _virtualBus);

            }

        }

        /// <summary>
        /// 创建LIN节点
        ///
        /// 根据LDF中的Node定义。
        ///
        /// </summary>
        private void CreateNodes(
            LinNetwork network)
        {

            foreach (var node
                in network.Nodes)
            {

       //         Console.WriteLine(
       //$"Create Node:{node.Name} {node.Type}");

                //
                // Master
                //
                if (node.Type
                    == LinNodeType.Master)
                {

                    _nodes.Add(
                        new LinMasterSimulator(
                            node));

                }
                else
                {

                    //
                    // Slave
                    //
                    // 注意:
                    //
                    // Behavior属于Simulator
                    //
                    //
                    ILinSlaveBehavior behavior =
                        CreateSlaveBehavior(
                            node);

                    var slave =
    new LinSlaveSimulator(
        node,
        network,
        behavior);



                    _nodes.Add(
                        slave);


                    //
                    // Mock环境挂入VirtualBus
                    //
                    _virtualBus?.Attach(
                        slave);

                }

            }

        }

        /// <summary>
        /// 创建Slave行为
        ///
        /// 当前只是测试。
        ///
        /// 后续替换:
        ///
        /// LinBehaviorFactory
        ///
        /// </summary>
        private ILinSlaveBehavior CreateSlaveBehavior(
            LinNode node)
        {

            LinSlaveSimulationConfig config =
                _config.Slaves?
                .Find(
                    x => x.NodeName
                    == node.Name);

            if (config == null)
            {

                return new DefaultSlaveBehavior();

            }

            switch (config.Behavior)
            {

                case "Motor":

                    return new MotorSlaveBehavior();

                default:


                    return new DefaultSlaveBehavior();

            }

        }

        /// <summary>
        /// 绑定事件
        ///
        /// </summary>
        private void BindEvents()
        {

            //
            // Header发送
            //
            // 通知所有节点
            //
            Engine.HeaderSent +=
                frame =>
                {

                    foreach (var node
                        in _nodes)
                    {

                        node.OnHeaderReceived(
                            frame,
                            Runtime);

                    }

                };

            //
            // Frame发送
            //
            // Tx Trace
            //
            Engine.FrameSent +=
                message =>
                {


                    //
                    // 后续接Trace
                    //
                    //Console.WriteLine(
                    //    $"TX ID:{message.Id:X2}");

                };

            //
            // Frame接收
            //
            _receiver.MessageReceived +=
                message =>
                {

                    //Console.WriteLine(
                    //    $"RX ID:{message.Id:X2}");

                };

            // LIN周期
            //
            // 一个Schedule周期结束
            //
            // 通知ECU执行周期任务
            //
            Engine.Cycle +=
                OnEngineCycle;

        }

        private void OnEngineCycle()
        {

            foreach (var node in _nodes)
            {

                node.Cycle(
                    Runtime);

            }

        }

        /// <summary>
        /// 启动LIN
        ///
        /// </summary>
        public void Start(
            string schedule)
        {

            //
            // 打开Adapter
            //
            _adapter.Open();

            //
            // 启动接收
            //
            _receiver.Start();

            //
            // 启动Engine
            //
            Engine.Start(
                schedule);

        }

        /// <summary>
        /// 停止LIN
        ///
        /// </summary>
        public void Stop()
        {

            Engine.Stop();

            _receiver.Stop();

            _adapter.Close();

        }


        /// <summary>
        /// LIN接收处理器
        ///
        /// Debug/UI监听RX使用
        ///
        /// </summary>
        public LinReceiveWorker Receiver
        {
            get
            {
                return _receiver;
            }
        }
    }

}
