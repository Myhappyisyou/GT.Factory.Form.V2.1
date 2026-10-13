using System;
using System.Collections.Generic;
using System.Threading;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Core
{

    /// <summary>
    /// LIN核心引擎
    ///
    /// 负责LIN Master运行。
    ///
    /// 工作流程：
    ///
    /// Schedule
    ///
    ///      ↓
    ///
    /// Frame
    ///
    ///      ↓
    ///
    /// Header
    ///
    ///      ↓
    ///
    /// Signal打包
    ///
    ///      ↓
    ///
    /// Adapter发送
    ///
    ///
    /// </summary>
    public class LinEngine
    {

        /// <summary>
        /// LIN运行环境
        ///
        /// 保存当前Signal值
        ///
        /// </summary>
        private readonly LinRuntime _runtime;

        /// <summary>
        /// LIN硬件适配器
        ///
        /// 真实:
        /// Vector
        ///
        /// 模拟:
        /// Mock
        ///
        /// </summary>
        private readonly ILinAdapter _adapter;

        /// <summary>
        /// 当前Schedule
        ///
        /// </summary>
        private  LinSchedule _schedule;

        /// <summary>
        /// 是否运行
        ///
        /// </summary>
        private bool _running;

        /// <summary>
        /// 工作线程
        ///
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// Header发送事件
        ///
        /// 当Master发送Frame Header时触发。
        ///
        /// 用于:
        ///
        /// Slave模拟器监听
        ///
        /// </summary>
        public event Action<LinFrame> HeaderSent;

        /// <summary>
        /// Frame发送事件
        ///
        /// 完整LIN消息发送后触发。
        ///
        /// 用于:
        ///
        /// Trace
        ///
        /// </summary>
        public event Action<LinMessage> FrameSent;

        /// <summary>
        /// 周期事件
        ///
        /// 每个LIN周期调用。
        ///
        /// 用于:
        ///
        /// Slave模拟
        ///
        /// 状态刷新
        ///
        /// </summary>
        public event Action Cycle;


        private readonly LinFrameBuilder _builder;

        /// <summary>
        /// 构造函数
        ///
        /// </summary>
        public LinEngine(
            LinRuntime runtime,
            ILinAdapter adapter)
        {

            _runtime = runtime;

            _adapter = adapter;

            _builder =
      new LinFrameBuilder(
          runtime.Network);
        }

        /// <summary>
        /// 启动LIN
        ///
        /// </summary>
        public void Start(
            string scheduleName)
        {


            if (_running)
                return;

            //
            // 根据名称查找Schedule
            //
            _schedule =
                _runtime.Network.Schedules
                .Find(
                    x => x.Name == scheduleName);

            if (_schedule == null)
            {

                throw new System.Exception(
                    $"找不到LIN Schedule:{scheduleName}");

            }

            _running = true;

            //
            // 创建后台线程
            //
            _thread =
                new Thread(
                    RunLoop);

            _thread.IsBackground = true;

            _thread.Start();

        }

        /// <summary>
        /// 停止LIN Engine
        ///
        /// 负责：
        ///
        /// 1. 请求RunLoop停止
        /// 2. 等待后台线程退出
        /// 3. 清理线程引用
        ///
        /// </summary>
        public void Stop()
        {
            if (!_running)
                return;


            //
            // 通知RunLoop停止
            //
            _running = false;


            //
            // 等待后台线程退出
            //
            if (_thread != null &&
                _thread.IsAlive)
            {
                _thread.Join(1000);
            }


            //
            // 清理线程引用
            //
            _thread = null;
        }

        /// <summary>
        /// LIN主循环
        ///
        /// 执行Schedule
        ///
        /// </summary>
        private void RunLoop()
        {
       //     Console.WriteLine(
       //"LIN Engine Loop Start");


            int index = 0;

            while (_running)
            {

                try
                {
       //             Console.WriteLine(
       //$"Execute Schedule Index:{index}");

                    //
                    // 当前Schedule Item
                    //
                    LinScheduleItem item =
                        _schedule.Items[index];


              //      Console.WriteLine(
              //$"Frame:{item.Frame.Name}");


                    //
                    // 执行Frame
                    //
                    ExecuteFrame(
                        item.Frame);

                    //
                    // 周期事件
                    //
                    Cycle?
                        .Invoke();

                    //
                    // 等待Delay
                    //
                    Thread.Sleep(
                        item.DelayMs);

                    index++;

                    if (index >= _schedule.Items.Count)
                    {

                        index = 0;

                    }

                }
                catch (Exception ex)
                {

                    Console.WriteLine(
                        "LIN Engine Exception:");

                    Console.WriteLine(
                        ex.ToString());

                }

            }

        }

        /// <summary>
        /// 执行一个LIN Frame
        ///
        /// </summary>
        private void ExecuteFrame(
            LinFrame frame)
        {

    //        Console.WriteLine(
    //$"ExecuteFrame:{frame.Name}");

            //
            // 1.
            // Master发送Header
            //
            HeaderSent?
                .Invoke(
                    frame);


            //Console.WriteLine(
            //    "Before Build");

            //
            // 2.
            // 根据Signal生成Data
            //
            LinMessage message =
     _builder.Build(
         frame,
         _runtime);

      //      Console.WriteLine(
      //"After Build");

      //      Console.WriteLine(
      //$"Build Message ID:{message.Id:X2}");


            ////
            //// LIN方向
            ////
            //message.FrameDirection =
            //    frame.Publisher.Type == LinNodeType.Master
            //    ?
            //    LinFrameDirection.MasterToSlave
            //    :
            //    LinFrameDirection.SlaveToMaster;


      //      Console.WriteLine(
      //"Before Adapter.Send");


            //
            // 3.
            // 发送到LIN总线
            //
            _adapter.Send(
                message);


            //
            // 保存已经交给Adapter的最终TX报文。
            // 不重新构建或发送，只同步Runtime状态。
            //
            _runtime.UpdateFrame(
                message);


            //Console.WriteLine(
            //    "After Adapter.Send");

            //
            // 4.
            // 通知外部
            //
            FrameSent?
                .Invoke(
                    message);

        }

        ///// <summary>
        ///// 创建LIN消息
        /////
        ///// Frame
        /////
        ///// +
        /////
        ///// Signal值
        /////
        ///// =
        /////
        ///// Data Bytes
        /////
        ///// </summary>
        //private LinMessage BuildMessage(
        //    LinFrame frame)
        //{

        //    LinMessage message =
        //        new LinMessage();

        //    message.Id =
        //        frame.Id;

        //    message.Data =
        //        new byte[
        //            frame.Length];

        //    //
        //    // 后续这里调用:
        //    //
        //    // LinSignalCodec
        //    //
        //    // 完成:
        //    //
        //    // Signal
        //    // Bit
        //    // Packing
        //    //
        //    //
        //    return message;

        //}

    }

}

