using GT.Factory.LIN.Model;
using GT.Factory.LIN.Protocol;
using System;
using System.Xml.Linq;


namespace GT.Factory.LIN.Core
{

    /// <summary>
    /// 
    /// LIN Signal运行管理器
    ///
    /// V0.4.1新增
    ///
    ///
    /// 负责:
    ///
    /// 1.
    /// Signal运行值管理
    ///
    /// 2.
    /// Signal -> Frame编码
    ///
    /// 3.
    /// 更新LinRuntime中的FrameCache
    ///
    ///
    /// 不负责:
    ///
    /// 1. LIN调度
    ///
    /// 2. Frame发送
    ///
    /// 3. Adapter通信
    ///
    ///
    /// 生命周期:
    ///
    /// LinRuntime创建后创建
    ///
    /// </summary>
    public class LinSignalRuntime
    {

        /// <summary>
        ///
        /// LIN运行环境
        ///
        /// 保存:
        ///
        /// Network
        ///
        /// FrameCache
        ///
        /// Signal状态
        ///
        /// </summary>
        private readonly LinRuntime _runtime;

        /// <summary>
        ///
        /// 构造函数
        ///
        /// </summary>
        public LinSignalRuntime(
            LinRuntime runtime)
        {

            if (runtime == null)
                throw new ArgumentNullException(
                    nameof(runtime));


            _runtime = runtime;

        }


        #region Signal写入

        /// <summary>
        ///
        /// 设置Signal运行值
        ///
        ///
        /// 示例:
        ///
        /// WRITE_SPEED=3000
        ///
        ///
        /// 执行:
        ///
        /// 1 更新Runtime Signal
        ///
        /// 2 找到对应Frame
        ///
        /// 3 Encode
        ///
        /// 4 更新FrameCache
        ///
        /// </summary>
        public void SetValue(
            string signalName,
            object value)
        {

            Console.WriteLine(
$"SignalRuntime Set!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!:{signalName}={value}");
            //
            // 查找Signal定义
            //
            LinSignal signal =
                FindSignal(
                    signalName);

            if (signal == null)
            {

                throw new Exception(
                    $"Signal不存在:{signalName}");

            }

            //
            // 更新Signal缓存
            //
            _runtime.SetSignalValue(
                signalName,
                value);

            //
            // 找对应Frame
            //
            foreach (var frame
                in _runtime.Network.Frames)
            {

                var frameSignal =
                    FindFrameSignal(
                        frame,
                        signalName);


                if (frameSignal == null)
                    continue;

                //
                // 获取当前Frame数据
                //
                var message =
                    _runtime.GetFrame(
                        frame.Id);

                if (message == null)
                    continue;

                //
                // 编码
                //
                LinSignalCode.Encode(

                    frame,

                    signal,

                    value,

                    message.Data);

                //
                // 更新时间
                //
                message.Timestamp =
                    DateTime.Now;

                //
                // 更新Frame缓存
                //
                _runtime.UpdateFrame(
                    message);

    //            Console.WriteLine(
    //$"Frame Update:{frame.Name} Data={BitConverter.ToString(message.Data)}");
                break;

            }

        }

        #endregion


        #region 查询

        /// <summary>
        ///
        /// 查找Signal定义
        ///
        /// </summary>
        private LinSignal FindSignal(
            string name)
        {

            foreach (var signal
                in _runtime.Network.Signals)
            {

                if (signal.Name == name)
                    return signal;

            }

            return null;

        }

        /// <summary>
        ///
        /// 查找Frame中的Signal
        ///
        /// </summary>
        private LinFrameSignal FindFrameSignal(
            LinFrame frame,
            string signalName)
        {

            foreach (var item
                in frame.Signals)
            {

                if (item.Signal.Name ==
                    signalName)
                {
                    return item;
                }

            }

            return null;

        }

        #endregion

    }


}