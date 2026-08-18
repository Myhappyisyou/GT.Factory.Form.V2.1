using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.ProjectDebug
{

    /// <summary>
    /// LIN调试会话
    ///
    /// 是LIN运行和调试软件之间的桥梁。
    ///
    ///
    /// 上:
    ///
    ///     LinRuntime
    ///
    ///
    /// 下:
    ///
    ///     UI
    ///     测试流程
    ///     产线程序
    ///
    ///
    /// 不负责:
    ///
    ///     LIN通信
    ///
    ///     Frame发送
    ///
    ///
    /// 只负责:
    ///
    ///     提供调试数据
    ///
    /// </summary>
    public class LinDebugSession
    {

        #region 私有字段


        /// <summary>
        /// LIN工程
        ///
        /// 包含:
        ///
        /// Network
        /// Runtime
        /// Controller
        ///
        /// </summary>
        private readonly LinProject _project;


        /// <summary>
        /// Signal运行管理器
        ///
        /// 负责:
        ///
        /// Signal修改
        ///     ↓
        /// Encode
        ///     ↓
        /// Frame Data更新
        ///
        /// </summary>
        private readonly LinSignalRuntime _signalRuntime;

        #endregion

        #region 调试数据


        /// <summary>
        /// Frame调试列表
        ///
        /// UI显示Frame树
        ///
        /// </summary>
        public List<LinFrameInfo> Frames
        {
            get;
            private set;
        }


        /// <summary>
        /// Signal调试列表
        ///
        /// UI显示Signal
        ///
        /// </summary>
        public List<LinSignalInfo> Signals
        {
            get;
            private set;
        }

        /// <summary>
        /// LIN Trace记录
        ///
        /// 用于:
        ///
        /// TX/RX显示
        ///
        /// </summary>
        public LinTraceLogger Trace
        {
            get;
            private set;
        }

        #endregion

        #region Event


        /// <summary>
        /// Signal修改通知
        /// 
        /// UI刷新
        /// Trace记录
        /// 日志
        ///
        /// </summary>
        public event Action<string, object> SignalChanged;


        /// <summary>
        /// Frame数据变化通知
        ///
        /// 来源:
        /// LinRuntime.FrameCache
        ///
        /// 用于:
        /// FrameDataPanel刷新
        /// Trace联动
        /// </summary>
        public event Action<LinFrameInfo> FrameChanged;


        /// <summary>
        /// Trace新增事件
        ///
        /// 来源:
        ///
        /// LinTraceLogger.ItemAdded
        ///
        /// 用于:
        ///
        /// LinTrace实时显示
        /// 
        /// </summary>
        public event Action<LinTraceItem> TraceAdded;


        #endregion


        #region 构造


        /// <summary>
        /// 创建LIN调试会话
        ///
        /// 根据LinProject创建。
        ///
        /// </summary>
        public LinDebugSession(
            LinProject project)
        {


            if (project == null)
            {
                throw new ArgumentNullException(
                    nameof(project));
            }

            _project =
                project;

            //
            // Signal运行管理器
            //
            // 负责:
            // Signal
            // ↓
            // Encode
            // ↓
            // Frame.Data
            //
            _signalRuntime =
                new LinSignalRuntime(
                    _project.Runtime);

            Frames =
                new List<LinFrameInfo>();

            Signals =
                new List<LinSignalInfo>();

            Trace =
                new LinTraceLogger();

            //
            // Trace事件转发
            //
            // Core
            //   |
            // LinTraceLogger
            //   |
            // LinDebugSession
            //   |
            // UI
            //
            // UI不直接访问Logger
            //
            Trace.ItemAdded +=
                item =>
                {
                    TraceAdded?.Invoke(item);
                };

            //
            // 根据LDF生成调试数据
            //
            Build();

            //
            // 绑定通信Trace
            //
            BindTrace();

        }


        #endregion

        /// <summary>
        /// 根据LinNetwork创建调试数据
        ///
        /// 来源:
        ///
        /// LDF解析结果
        ///
        /// </summary>
        private void Build()
        {

            var network =
                _project.Runtime.Network;

            foreach (var frame in network.Frames)
            {

                var frameInfo =
                    new LinFrameInfo();

                frameInfo.Name =
                    frame.Name;

                frameInfo.Id =
                    frame.Id;

                frameInfo.Length =
                    frame.Length;

                frameInfo.Publisher =
                    frame.Publisher.Name;

                frameInfo.Signals =
                    new List<LinSignalInfo>();

                //
                // 判断方向
                //
                // Master发布:
                //     TX
                //
                // Slave发布:
                //     RX
                //
                frameInfo.Direction =
                    frame.Publisher.Type
                    ==
                    LinNodeType.Master
                    ?
                    LinFrameDirection.MasterToSlave
                    :
                    LinFrameDirection.SlaveToMaster;

                //
                // 初始化Frame缓存
                //
                // LIN最大8Byte
                //
                // 后续由Runtime刷新
                //
                //frameInfo.Data =
                //    _project.Runtime
                //    .GetFrame(frame.Id)
                //    .Data;


                var runtimeFrame =
                    _project.Runtime.GetFrame(frame.Id);

                if (runtimeFrame != null)
                {
                    frameInfo.Data =
                        new byte[runtimeFrame.Data.Length];

                    Array.Copy(
                        runtimeFrame.Data,
                        frameInfo.Data,
                        runtimeFrame.Data.Length);
                }
                else
                {
                    frameInfo.Data =
                        new byte[frame.Length];
                }


                foreach (var signal in frame.Signals)
                {


                    var signalInfo =
                        new LinSignalInfo();

                    signalInfo.Name =
                        signal.Signal.Name;

                    signalInfo.FrameName =
                        frame.Name;

                    signalInfo.Publisher =
                        frame.Publisher.Name;

                    //
                    // 原始LDF Signal引用
                    //
                    signalInfo.Source =
                        signal.Signal;

                    //
                    // Signal所属Frame对象
                    //
                    // 方便UI:
                    //
                    // Frame
                    //   |
                    //   + Signal
                    //
                    signalInfo.Frame =
                        frameInfo;

                    //
                    // 当前运行值
                    //
                    signalInfo.Value =
                        _project.Runtime.GetSignalValue(
                            signal.Signal.Name);

                    frameInfo.Signals.Add(
                        signalInfo);

                    Signals.Add(
                        signalInfo);

                }

                Frames.Add(
                    frameInfo);

            }

        }

        #region Trace


        /// <summary>
        /// 绑定LIN通信事件
        ///
        /// TX:
        ///
        /// Master发送
        ///
        ///
        /// RX:
        ///
        /// Slave响应
        ///
        /// </summary>
        private void BindTrace()
        {

            //
            // TX记录
            //
            _project.Controller.Engine.FrameSent +=
                message =>
                {
                    var frame =
                        _project.Network.Frames
                        .FirstOrDefault(
                            x =>
                            x.Id == message.Id);

                    if (frame == null)
                        return;


                    Trace.Log(
                        "TX",
                        frame,
                        message);


                    RefreshFrame(message.Id);
                };


            //
            // RX记录
            //
            _project.Controller.Receiver.MessageReceived +=
                message =>
                {

                    var frame =
                        _project.Network.Frames
                        .FirstOrDefault(
                            x =>
                            x.Id == message.Id);


                    if (frame == null)
                        return;


                    Trace.Log(
                        "RX",
                        frame,
                        message);


                    RefreshFrame(message.Id);
                };

        }

        #endregion

        /// <summary>
        /// 设置发送Signal
        ///
        /// 调试软件修改值入口
        ///
        /// </summary>
        public void SetSignalValue(
            string name,
            object value)
        {

            //Console.WriteLine(
            //    $"SetSignalValue:{name}={value}");


            //
            // 不直接修改Runtime
            //
            // 必须经过:
            //
            // LinSignalRuntime
            //
            // 保证:
            //
            // Signal
            // Frame
            // 同步
            //
            _signalRuntime.SetValue(
                name,
                value);

            //
            // 更新Debug显示数据
            //
            UpdateDebugSignalValue(
                name,
                value);


            ////
            //// 更新Frame缓存
            ////
            //RefreshFrames();

            //
            // 找到Signal所属Frame
            //
            // 不刷新全部Frame
            //
            var signal =
                Signals.FirstOrDefault(
                    x =>
                    x.Name == name);


            if (signal != null)
            {

                //
                // 通知Frame变化
                //
                // FrameDataPanel刷新
                //
                FrameChanged?.Invoke(
                    signal.Frame);

            }

            //
            // 通知调试界面
            //
            SignalChanged?.Invoke(
                name,
                value);

        }

        private void UpdateDebugSignalValue(
            string name,
            object value)
        {

            var signal =
                Signals.FirstOrDefault(
                    x =>
                    x.Name == name);

            if (signal == null)
                return;

            signal.Value = value;

        }


        /// <summary>
        /// 获取Signal当前值
        ///
        /// 调试软件读取入口
        ///
        /// </summary>
        public object GetSignalValue(
            string name)
        {

            return
                _project.Runtime.GetSignalValue(
                    name);

        }

        /// <summary>
        /// 刷新所有Signal当前值
        /// 
        /// 调试界面周期调用
        /// 
        /// </summary>
        public void RefreshSignals()
        {

            foreach (var signal in Signals)
            {

                var value =
             _project.Runtime.GetSignalValue(
                 signal.Name);

                //Console.WriteLine(
                //    $"Refresh======:{signal.Name}={value}");


                signal.Value =
                    value;

            }

        }


        /// <summary>
        /// 刷新Frame当前数据
        ///
        /// 数据来源:
        ///
        /// LinRuntime.FrameCache
        ///
        /// 更新:
        ///
        /// LinFrameInfo.Data
        ///
        /// 用于:
        ///
        /// 1. Debug UI显示Frame Data
        /// 2. Frame树查看当前总线数据
        ///
        /// 注意:
        ///
        /// 不修改Runtime。
        ///
        /// Runtime是唯一数据源。
        /// </summary>
        public void RefreshFrames()
        {

            foreach (var frameInfo in Frames)
            {

                //
                // 从Runtime获取真实Frame
                //
                var message =
                    _project.Runtime.GetFrame(
                        (byte)frameInfo.Id);

                if (message == null)
                    continue;

                //
                // 更新Debug数据
                //
                if (frameInfo.Data == null ||
                   frameInfo.Data.Length != message.Data.Length)
                {
                    frameInfo.Data =
                        new byte[message.Data.Length];
                }


                Array.Copy(
                    message.Data,
                    frameInfo.Data,
                    message.Data.Length);

                Console.WriteLine(
    $"Frame Update:{frameInfo.Name} Data={BitConverter.ToString(frameInfo.Data)}");

                //
                // 通知UI
                //
                FrameChanged?.Invoke(
                    frameInfo);

            }

        }


        /// <summary>
        /// 刷新单个Frame数据
        ///
        /// 数据来源:
        ///
        /// LinRuntime.FrameCache
        ///
        /// 流程:
        ///
        /// Runtime.Frame
        ///       ↓
        /// LinFrameInfo.Data
        ///       ↓
        /// FrameChanged事件
        ///       ↓
        /// UI刷新
        ///
        /// 用途:
        ///
        /// 1. LIN RX收到Frame后刷新
        /// 2. TX发送Frame后刷新
        /// 3. Signal修改后刷新所属Frame
        ///
        /// 注意:
        ///
        /// 不修改Runtime。
        ///
        /// Runtime始终是唯一数据源。
        ///
        /// </summary>
        private void RefreshFrame(
            byte id)
        {

            //
            // 根据Frame ID找到调试Frame对象
            //
            // LinFrameInfo是UI显示模型
            //
            var frameInfo =
                Frames.FirstOrDefault(
                    x =>
                    x.Id == id);



            if (frameInfo == null)
                return;



            //
            // 从Runtime获取真实Frame数据
            //
            // Runtime维护当前LIN总线状态
            //
            var message =
                _project.Runtime.GetFrame(
                    id);



            if (message == null)
                return;



            //
            // 确保Debug缓存长度正确
            //
            if (frameInfo.Data == null ||
               frameInfo.Data.Length != message.Data.Length)
            {

                frameInfo.Data =
                    new byte[message.Data.Length];

            }



            //
            // Runtime Frame数据
            //
            // 拷贝到Debug显示缓存
            //
            Array.Copy(
                message.Data,
                frameInfo.Data,
                message.Data.Length);



            //
            // 通知UI:
            //
            // FrameDataPanel刷新
            //
            FrameChanged?.Invoke(
                frameInfo);

        }
    }

}
