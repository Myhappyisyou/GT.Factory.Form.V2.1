using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Protocol;
using System;


namespace GT.Factory.LIN.Core
{


    /// <summary>
    /// LIN接收工作器
    ///
    /// 负责处理LIN总线收到的数据。
    ///
    ///
    /// 工作流程:
    ///
    /// ILinAdapter
    ///
    ///       ↓
    ///
    /// MessageReceived事件
    ///
    ///       ↓
    ///
    /// LinReceiveWorker
    ///
    ///       ↓
    ///
    /// Signal Decode
    ///
    ///       ↓
    ///
    /// LinRuntime更新
    ///
    ///       ↓
    ///
    /// UI / Trace事件
    ///
    ///
    /// 注意:
    ///
    /// 这里不再创建接收线程。
    ///
    /// 原因:
    ///
    /// 真实LIN硬件:
    ///
    /// Vector
    /// Kvaser
    /// USB LIN
    ///
    /// 驱动层通常已经提供RX事件。
    ///
    /// Adapter负责:
    ///
    /// 硬件线程
    ///
    /// Worker负责:
    ///
    /// 协议解析
    ///
    /// 数据更新
    ///
    /// </summary>
    public class LinReceiveWorker
    {


        /// <summary>
        /// LIN硬件适配器
        ///
        /// 例如:
        ///
        /// MockLinAdapter
        /// VectorLinAdapter
        ///
        /// </summary>
        private readonly ILinAdapter _adapter;




        /// <summary>
        /// LIN运行环境
        ///
        /// 保存:
        ///
        /// Signal当前值
        ///
        /// </summary>
        private readonly LinRuntime _runtime;





        /// <summary>
        /// 是否已经启动
        ///
        /// 控制事件订阅生命周期
        ///
        /// </summary>
        private bool _started;





        /// <summary>
        /// 收到LIN消息事件
        ///
        /// 用于:
        ///
        /// 1. Trace显示
        ///
        /// 2. 调试工具
        ///
        /// 3. UI刷新
        ///
        /// </summary>
        public event Action<LinMessage> MessageReceived;





        /// <summary>
        /// 构造函数
        /// </summary>
        public LinReceiveWorker(
            ILinAdapter adapter,
            LinRuntime runtime)
        {

            _adapter = adapter;

            _runtime = runtime;

        }






        /// <summary>
        /// 启动接收处理
        ///
        /// 注意:
        ///
        /// 不创建线程。
        ///
        /// 只是监听Adapter事件。
        ///
        /// </summary>
        public void Start()
        {


            if (_started)
                return;



            //
            // 注册LIN RX事件
            //
            _adapter.MessageReceived +=
                OnAdapterMessageReceived;



            _started = true;


        }







        /// <summary>
        /// 停止接收处理
        ///
        /// 取消事件订阅。
        ///
        /// </summary>
        public void Stop()
        {


            if (!_started)
                return;



            //
            // 取消LIN RX事件
            //
            _adapter.MessageReceived -=
                OnAdapterMessageReceived;



            _started = false;


        }







        /// <summary>
        /// Adapter收到消息
        ///
        /// </summary>
        private void OnAdapterMessageReceived(
            LinMessage message)
        {


            if (message == null)
                return;



            ProcessMessage(
                message);


        }









        /// <summary>
        /// 处理LIN消息
        ///
        /// 包括:
        ///
        /// 1. 查找Frame
        ///
        /// 2. Signal解析
        ///
        /// 3. 更新Runtime
        ///
        /// 4. 通知外部
        ///
        /// </summary>
        private void ProcessMessage(
            LinMessage message)
        {



            //
            // 根据Frame ID查找定义
            //
            LinFrame frame =
                _runtime.Network.Frames
                .Find(
                    x => x.Id == message.Id);





            if (frame != null)
            {


                //
                // 遍历Frame中的Signal
                //
                foreach (var frameSignal in frame.Signals)
                {


                    //
                    // 找到Signal定义
                    //
                    LinSignal signal =
                        _runtime.Network.Signals
                        .Find(
                            x => x.Name == frameSignal.Signal.Name);



                    if (signal == null)
                        continue;




                    //
                    // Data Byte
                    //
                    // Decode Signal
                    //
                    object value =
                        LinSignalCode.Decode(
                            frame,
                            signal,
                            message.Data);





                    //
                    // 更新运行值
                    //
                    _runtime.SetSignalValue(
                        signal.Name,
                        value);



                }



            }






            //
            // 通知外部
            //
            // Trace/UI使用
            //
            MessageReceived?
                .Invoke(
                    message);


        }






        /// <summary>
        /// 释放资源
        ///
        /// </summary>
        public void Dispose()
        {


            Stop();


        }


    }


}