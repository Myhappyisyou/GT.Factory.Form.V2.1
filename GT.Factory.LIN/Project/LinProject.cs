using GT.Factory.LIN.Config;
using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Parser;
using GT.Factory.LIN.Simulator;
using System;
using System.Collections.Generic;


namespace GT.Factory.LIN.Project
{

    /// <summary>
    /// LIN工程
    ///
    /// 一个LDF对应一个工程
    ///
    /// 负责:
    ///
    /// LDF加载
    /// Config加载
    /// Runtime创建
    /// Controller创建
    ///
    /// Simulation:
    ///     创建虚拟总线
    ///     创建Slave模拟器
    ///
    /// Hardware:
    ///     使用真实Adapter
    ///
    /// </summary>
    public class LinProject
    {

        /// <summary>
        /// LIN网络
        /// </summary>
        public LinNetwork Network
        {
            get;
            private set;
        }

        /// <summary>
        /// Runtime
        /// </summary>
        public LinRuntime Runtime
        {
            get;
            private set;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public LinSimulationConfig SimulationConfig
        {
            get;
            private set;
        }

        /// <summary>
        /// Controller
        /// </summary>
        public LinController Controller
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前运行模式
        /// </summary>
        public LinRunMode Mode
        {
            get;
            private set;
        }

        /// <summary>
        /// 虚拟总线
        ///
        /// Simulation模式使用
        ///
        /// </summary>
        private LinVirtualBus _virtualBus;

        /// <summary>
        /// Slave模拟器集合
        /// </summary>
        private readonly List<ILinNodeSimulator> _simulators =
            new List<ILinNodeSimulator>();

        private LinProject()
        {

        }

        public static LinProject Load(
            string ldfFile,
            string simulationFile,
            ILinAdapter adapter,
            LinRunMode mode)
        {

            var project =
                new LinProject();

            project.Mode = mode;

            //
            // 1.解析LDF
            //
            var reader =
                new LdfReader();

            var tokens =
                reader.Read(
                    ldfFile);

            var parser =
                new LdfParser();

            project.Network =
                parser.Parse(
                    tokens);

            //
            // 2.Runtime
            //
            project.Runtime =
                new LinRuntime(
                    project.Network);

            //
            // 3.Config
            //
            project.SimulationConfig =
                SimulationConfigManager.Load(
                    simulationFile);

            //
            // 4. Simulation处理
            //
            if (mode == LinRunMode.Simulation)
            {

                project.SetupSimulation();

            }

            //
            // 5.Controller
            //
            project.Controller =
                new LinController(
                    project.Runtime,
                    project.SimulationConfig,
                    adapter);

            //
            // 6. Simulation Adapter连接
            //
            if (mode == LinRunMode.Simulation)
            {

                var simAdapter = adapter as IVirtualLinAdapter;


                if (simAdapter == null)
                    throw new Exception(
                        "Adapter不支持Simulation");

                simAdapter.AttachBus(
                    project._virtualBus);

            }

            return project;

        }

        /// <summary>
        /// 创建模拟环境
        ///
        /// </summary>
        private void SetupSimulation()
        {

            //
            // 创建虚拟LIN总线
            //
            _virtualBus =
                new LinVirtualBus(
                    Network,
                    Runtime);

            //
            // 根据LDF节点创建Slave
            //
            foreach (var node in Network.Nodes)
            {

                //
                // Master不用模拟
                //
                if (node.Type == LinNodeType.Master)
                {
                    continue;
                }

                //
                // 查找行为配置
                //
                var slaveConfig =
                    SimulationConfig.Slaves
                    .Find(
                        x => x.NodeName == node.Name);

                if (slaveConfig == null)
                {
                    continue;
                }

                ILinSlaveBehavior behavior =
                    CreateBehavior(
                        slaveConfig.Behavior);

                var simulator =
                    new LinSlaveSimulator(
                        node,
                        Network,
                        behavior);

                _simulators.Add(
                    simulator);

                _virtualBus.Attach(
                    simulator);

            }

        }

        /// <summary>
        /// 创建Slave行为
        /// 
        /// 后续扩展:
        ///
        /// Motor
        /// Door
        /// Seat
        ///
        /// </summary>
        private ILinSlaveBehavior CreateBehavior(
            string behavior)
        {

            switch (behavior)
            {

                case "Motor":

                    return new MotorSlaveBehavior();

                default:

                    throw new Exception(
                        $"未知Slave行为:{behavior}");

            }

        }

    }

}
