using GT.Factory.LIN.Core;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using System;
using System.Threading;


namespace GT.Factory.LIN.Simulator
{


    /// <summary>
    /// LIN模拟器
    ///
    /// 用于模拟Master行为
    ///
    /// </summary>
    public class LinSimulator
    {

        private readonly ILinAdapter adapter;

        private Thread thread;

        private bool running;

        private readonly LinFrameBuilder builder;

        private readonly LinRuntime runtime;

        public LinSimulator(
            ILinAdapter adapter)
        {

            this.adapter = adapter;

        }

        /// <summary>
        /// 启动模拟
        /// </summary>
        public void Start(
            LinNetwork network)
        {

            running = true;

            thread =
                new Thread(() =>
                {


                    while (running)
                    {


                        foreach (
                            LinFrame frame
                            in network.Frames)
                        {


                            //
                            // Frame定义
                            //
                            // 转换成真实报文
                            //
                            LinMessage message =
                                builder.Build(
                                    frame,
                                    runtime);




                            //
                            // 发送LIN消息
                            //
                            adapter.Send(
                                message);

                            Thread.Sleep(100);

                        }

                    }

                });

            thread.IsBackground = true;

            thread.Start();

        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {

            running = false;

        }

    }

}