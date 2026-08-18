using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using System;
using System.Collections.Generic;


namespace GT.Factory.LIN.Simulator
{

    /// <summary>
    /// LIN Slave模拟器
    ///
    /// 模拟ECU行为
    ///
    ///
    /// 例如:
    ///
    /// Slave_1
    ///
    /// 收到:
    ///
    /// ELOP_Cmd
    ///
    /// 修改内部状态
    ///
    /// 下一周期:
    ///
    /// 返回:
    ///
    /// ELOP_Rsp1
    ///
    /// </summary>
    public class LinSlaveSimulator
        :
        ILinNodeSimulator
    {

        private readonly ILinSlaveBehavior _behavior;

        private readonly LinFrameBuilder _builder;


        public event Action<LinMessage> ResponseGenerated;


        /// <summary>
        /// 节点
        /// </summary>
        public LinNode Node
        {
            get;
            private set;
        }

        /// <summary>
        /// 响应Frame列表
        ///
        /// Slave只能响应:
        ///
        /// Publisher=自己
        ///
        /// </summary>
        private List<LinFrame> _responseFrames;

        public LinSlaveSimulator(
            LinNode node,
            LinNetwork network,
            ILinSlaveBehavior behavior)
        {

            Node = node;

            _behavior = behavior;

            _builder =
       new LinFrameBuilder(
           network);

            _responseFrames =
                new List<LinFrame>();

            //
            // 找出Slave发布的Frame
            //
            foreach (var frame
                in network.Frames)
            {

                if (frame.Publisher != null && frame.Publisher.Name == node.Name)
                {

                    _responseFrames.Add(
                        frame);

                }

            }

        }

        /// <summary>
        /// 收到Master Header
        ///
        /// </summary>
        public void OnHeaderReceived(
    LinFrame frame,
    LinRuntime runtime)
        {

            //Console.WriteLine(
            //    $"Slave:{Node.Name} Header:{frame.Name} Publisher:{frame.Publisher.Name}");



            //
            // 如果是Slave自己发布的Frame
            // 不调用业务
            //
            if (frame.Publisher != null
               && frame.Publisher.Name == Node.Name)
            {

                GenerateResponse(
                    frame,
                    runtime);

                return;
            }



            //
            // Master发送的命令
            //
            _behavior?
                .OnFrameReceived(
                    frame,
                    runtime);



        }

        public void Cycle(
    LinRuntime runtime)
        {

            _behavior?
                .OnCycle(runtime);

        }

        /// <summary>
        /// 生成Slave响应数据
        ///
        /// </summary>
        private void GenerateResponse(
            LinFrame frame,
            LinRuntime runtime)
        {
            //
            // 只有Slave发布的Frame才响应
            //
            if (frame.Publisher == null ||
               frame.Publisher.Name != Node.Name)
            {
                return;
            }

            //
            // 获取Frame缓存
            //
            //LinMessage msg =
            //    runtime.GetFrame(
            //        frame.Id);

            
            LinMessage msg =
      _builder.Build(
          frame,
          runtime);


            //
            // 这里暂时保持:
            //
            // 当前Signal值
            //
            // 后续增加:
            //
            // ECU逻辑
            //
            // 温度变化
            // 转速变化
            // 故障产生
            //

            runtime.UpdateFrame(
                msg);


            //
            // 通知VirtualBus
            //
            ResponseGenerated?
                .Invoke(msg);
        }



    }

}

