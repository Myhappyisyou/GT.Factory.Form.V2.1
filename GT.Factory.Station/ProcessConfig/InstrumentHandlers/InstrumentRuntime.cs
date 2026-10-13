using GT.Factory.Logging.Logging;
using System;


namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers
{
    /// <summary>
    /// 仪器运行实例。
    ///
    /// 一个 Runtime 对应一台实际仪器。
    ///
    /// 包含：
    /// 1. Handler（流程调用）
    /// 2. Device（底层通讯资源）
    ///
    /// Dispatcher负责统一生命周期管理。
    /// </summary>
    public class InstrumentRuntime : IDisposable
    {

        /// <summary>
        /// 仪器处理器。
        ///
        /// Process执行时调用。
        /// </summary>
        public IInstrumentHandler Handler
        {
            get;
            private set;
        }


        /// <summary>
        /// 底层仪器资源。
        ///
        /// 例如：
        /// ItechPowerInstrument
        /// HeliumTesterClient
        /// CameraDevice
        /// </summary>
        private IDisposable _device;


        /// <summary>
        /// 创建仪器运行实例。
        /// </summary>
        public InstrumentRuntime(
            IInstrumentHandler handler,
            IDisposable device)
        {
            if (handler == null)
                throw new ArgumentNullException(
                    nameof(handler));


            Handler = handler;
            _device = device;
        }



        /// <summary>
        /// 释放底层仪器资源。
        ///
        /// Handler不负责释放。
        /// 防止多个Handler共享设备时重复释放。
        /// </summary>
        public void Dispose()
        {
            if (_device != null)
            {
                try
                {
                    _device.Dispose();
                }
                catch (Exception ex)
                {
                    DisplayLog.Error(
                        "释放仪器资源失败",
                        ex);
                }


                _device = null;
            }
        }
    }
}