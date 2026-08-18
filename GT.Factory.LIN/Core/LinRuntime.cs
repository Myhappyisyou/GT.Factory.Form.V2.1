using System;
using System.Collections.Generic;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Core
{

    /// <summary>
    /// LIN运行环境
    ///
    /// 保存LIN模拟运行过程中的所有状态。
    ///
    ///
    /// 注意：
    ///
    /// LinNetwork:
    ///
    ///     描述网络结构
    ///
    ///
    /// LinRuntime:
    ///
    ///     描述当前运行状态
    ///
    ///
    /// 两者不要混合。
    ///
    /// </summary>
    public class LinRuntime
    {

        #region 基础网络

        /// <summary>
        /// LIN网络定义
        ///
        /// 来自LDF解析结果
        ///
        /// </summary>
        public LinNetwork Network
        {
            get;
            private set;
        }

        #endregion

        #region Schedule状态

        /// <summary>
        /// 当前运行的Schedule
        ///
        /// Master根据Schedule发送Frame
        ///
        /// </summary>
        public LinSchedule CurrentSchedule
        {
            get;
            set;
        }

        /// <summary>
        /// 当前Schedule索引
        ///
        /// 表示执行到第几个Frame
        ///
        /// </summary>
        public int ScheduleIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 下一次执行时间
        ///
        /// 单位:
        ///
        /// DateTime
        ///
        /// </summary>
        public DateTime NextExecuteTime
        {
            get;
            set;
        }

        #endregion

        #region Frame数据缓存

        /// <summary>
        /// Frame当前数据
        ///
        /// Key:
        ///
        /// Frame ID
        ///
        ///
        /// Value:
        ///
        /// 当前8字节数据
        ///
        /// </summary>
        private Dictionary<byte, LinMessage>
            _frameCache;

        /// <summary>
        /// Signal当前值
        ///
        /// Key:
        ///
        /// Signal名称
        ///
        ///
        /// Value:
        ///
        /// Raw值
        ///
        /// </summary>
        private Dictionary<string, object>
            _signalValues;

        #endregion

        /// <summary>
        /// LIN是否运行
        ///
        /// </summary>
        public bool IsRunning
        {
            get;
            set;
        }

        /// <summary>
        /// 创建运行环境
        ///
        /// </summary>
        public LinRuntime(
            LinNetwork network)
        {


            if (network == null)
            {
                throw new ArgumentNullException(
                    nameof(network));
            }

            Network = network;

            _frameCache =
                new Dictionary<byte, LinMessage>();

            _signalValues =
                new Dictionary<string, object>();

            InitializeFrames();

            InitializeSignals();

        }

        #region 初始化

        /// <summary>
        /// 初始化Frame缓存
        ///
        /// 根据LDF中的Frame创建空报文
        ///
        /// </summary>
        private void InitializeFrames()
        {

            foreach (var frame
                in Network.Frames)
            {

                LinMessage message =
                    new LinMessage(
                        frame.Id,
                        frame.Length);

                _frameCache.Add(
                    frame.Id,
                    message);

            }

        }

        /// <summary>
        /// 初始化Signal值
        ///
        /// 默认使用:
        ///
        /// InitialValue
        ///
        /// </summary>
        private void InitializeSignals()
        {

            foreach (var signal
                in Network.Signals)
            {

                _signalValues.Add(
                    signal.Name,
                    signal.InitialValue);

            }

        }

        #endregion

        #region Frame操作

        /// <summary>
        /// 获取Frame当前数据
        ///
        /// </summary>
        public LinMessage GetFrame(
            byte id)
        {

            if (_frameCache.ContainsKey(id))
            {

                return _frameCache[id];

            }

            return null;

        }

        /// <summary>
        /// 更新Frame
        ///
        /// </summary>
        public void UpdateFrame(
            LinMessage message)
        {

            if (message == null)
                return;

            _frameCache[message.Id] = message;

        }

        #endregion

        #region Signal操作

        /// <summary>
        /// 设置Signal值
        ///
        /// 例如:
        ///
        /// WRITE_SPEED = 3000rpm
        ///
        /// </summary>
        public void SetSignalValue(
            string name,
            object value)
        {

            _signalValues[name] = value;



        }

        /// <summary>
        /// 获取Signal值
        ///
        /// </summary>
        public object GetSignalValue(
            string name)
        {

            if (_signalValues.ContainsKey(name))
            {
                object value = _signalValues[name];
                return _signalValues[name];

            }

            return null;

        }

        #endregion

    }

}

