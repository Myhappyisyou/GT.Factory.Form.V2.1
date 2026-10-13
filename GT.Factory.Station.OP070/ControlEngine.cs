using GT.Factory.Instrument.ItechPower;
using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.ProcessConfig.Instrument;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using System;

namespace GT.Factory.Station.OP070
{
    public class ControlEngine : ControlEngineBase
    {
        /// <summary>
        /// OP070 使用的艾德克斯电源。
        ///
        /// 电源属于站级外部设备，因此由 OP070 控制引擎负责创建和释放。
        /// </summary>
        private ItechPowerInstrument _itechPower;

        protected override bool NeedReadBatch() => true;



        protected override void RegisterInstrumentHandlers(
            IInstrumentDispatcher dispatcher)
        {

            base.RegisterInstrumentHandlers(dispatcher);
        }

        /// <summary>
        /// 注册 OP070 使用的仪器处理器。
        ///
        /// Handler 负责把 Station 流程与具体仪器驱动隔离开。
        /// ControlEngine 只负责创建仪器并注册 Handler。
        /// </summary>
        //protected override void RegisterInstrumentHandlers(
        //    IInstrumentDispatcher dispatcher)
        //{
        //    // ------------------------------------------------------------
        //    // 1. 注册原有扭力工具。
        //    // ------------------------------------------------------------
        //    string torqueToolAddress = "192.168.1.11";

        //    dispatcher.Register(
        //        "TorqueTester",
        //        new TorqueTesterHandler(torqueToolAddress));

        //    // ------------------------------------------------------------
        //    // 2. 注册原有氦检仪。
        //    // ------------------------------------------------------------
        //    string heliumTesterAddress = "192.168.1.10";

        //    dispatcher.Register(
        //        "HeliumTester",
        //        new HeliumTesterHandler(heliumTesterAddress));

        //    // ------------------------------------------------------------
        //    // 3. 注册第二台氦检仪。
        //    // ------------------------------------------------------------
        //    string heliumTesterAddress2 = "192.168.1.12";

        //    dispatcher.Register(
        //        "HeliumTester2",
        //        new HeliumTesterHandler(heliumTesterAddress2));

        //    // ------------------------------------------------------------
        //    // 4. 从 InstrumentConfig.json 获取 OP070 电源配置。
        //    //
        //    // Name 是当前工站内的物理设备唯一标识。
        //    // ------------------------------------------------------------
        //    InstrumentDeviceConfig powerConfig =
        //        instrumentConfig.Get("OP070-POWER");

        //    if (powerConfig == null)
        //    {
        //        throw new InvalidOperationException(
        //            "未找到 OP070 电源仪器配置：OP070-POWER");
        //    }

        //    if (powerConfig.Communication == null)
        //    {
        //        throw new InvalidOperationException(
        //            "OP070 电源通讯配置为空：OP070-POWER");
        //    }

        //    // ------------------------------------------------------------
        //    // 5. 创建 ItechPower 仪器。
        //    //
        //    // IP、端口、型号、序列号全部来自配置文件。
        //    // ------------------------------------------------------------
        //    _itechPower =
        //        new ItechPowerInstrument(
        //            powerConfig.Communication.Ip,
        //            powerConfig.Communication.Port,
        //            powerConfig.Model,
        //            powerConfig.SerialNumber);

        //    // ------------------------------------------------------------
        //    // 6. 设置通讯超时时间。
        //    //
        //    // 超时时间同样来自仪器配置文件。
        //    // ------------------------------------------------------------
        //    _itechPower.SendTimeout =
        //        powerConfig.Communication.SendTimeout;

        //    _itechPower.ReceiveTimeout =
        //        powerConfig.Communication.ReceiveTimeout;


        //    // ------------------------------------------------------------
        //    // 7. 初始化并连接电源。
        //    //
        //    // InstrumentBase 会统一管理仪器状态。
        //    // ------------------------------------------------------------
        //    _itechPower.Initialize();
        //    _itechPower.Connect();

        //    // ------------------------------------------------------------
        //    // 8. 创建 Station 层 Handler。
        //    //
        //    // Handler 不负责 PLC/MES，只负责调用电源并返回测量值。
        //    // ------------------------------------------------------------
        //    ItechPowerHandler powerHandler =
        //        new ItechPowerHandler(
        //            _itechPower,
        //            24.0,
        //            false);

        //    // ------------------------------------------------------------
        //    // 9. 注册到仪器 Dispatcher。
        //    // ------------------------------------------------------------
        //    dispatcher.Register(
        //        powerConfig.Name,
        //        powerHandler);
        //}


        protected override void AfterInit()
        {

            Shared.ProcessName = Config.Instance.ProcessName.Replace("&", "");

            Shared.orderNub = Shared.shopOrder != null ? Shared.shopOrder.OrderNum : "1000";

            Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "111111111111";

            Shared.monitor = monitor;
        }

        /// <summary>
        /// 释放 OP070 使用的外部仪器。
        ///
        /// ControlEngineBase.Shutdown() 会自动调用该方法。
        /// </summary>
        protected override void ShutdownExternalDevices()
        {
            if (_itechPower != null)
            {
                _itechPower.Dispose();
                _itechPower = null;
            }
        }
    }
}
