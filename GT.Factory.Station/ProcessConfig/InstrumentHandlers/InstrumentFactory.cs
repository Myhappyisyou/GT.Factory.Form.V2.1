using GT.Factory.Instrument.ItechPower;
using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.ProcessConfig.Instrument;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.Station.ProcessConfig.InstrumentHandlers
{
    /// <summary>
    /// 仪器创建工厂。
    ///
    /// 根据 InstrumentConfig 创建对应的仪器实例和Handler。
    ///
    /// ControlEngine 不再直接依赖具体仪器类型。
    /// </summary>
    public class InstrumentFactory
    {
        /// <summary>
        /// 创建单个仪器Handler。
        /// </summary>
        public InstrumentCreateResult Create(
            InstrumentDeviceConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(
                    nameof(config));

            if (string.IsNullOrWhiteSpace(
                config.DriverKey))
            {
                throw new InvalidOperationException(
                    "仪器DriverKey不能为空："
                    + config.Name);
            }

            switch (config.DriverKey)
            {
                case "ItechPower":

                    return CreateItechPower(config);

                default:

                    throw new NotSupportedException(
                        "未支持的仪器类型："
                        + config.DriverKey);
            }
        }


        /// <summary>
        /// 创建艾德克斯电源。
        /// </summary>
        private InstrumentCreateResult CreateItechPower(
            InstrumentDeviceConfig config)
        {

            if (config.Communication == null)
            {
                throw new InvalidOperationException(
                    "仪器通讯配置为空："
                    + config.Name);
            }

            ItechPowerInstrument instrument =
                new ItechPowerInstrument(
                    config.Communication.Ip,
                    config.Communication.Port,
                    config.Model,
                    config.SerialNumber);

            instrument.SendTimeout =
                config.Communication.SendTimeout;

            instrument.ReceiveTimeout =
                config.Communication.ReceiveTimeout;

            instrument.Initialize();

            instrument.Connect();

            ItechPowerHandler handler =
                new ItechPowerHandler(
                    instrument,
                    24.0,
                    false);

            return new InstrumentCreateResult
            {
                Handler = handler,
                Instrument = instrument
            };

        }
    }


    public class InstrumentCreateResult
    {

        /// <summary>
        /// 流程调用Handler。
        /// </summary>
        public IInstrumentHandler Handler
        {
            get;
            set;
        }

        /// <summary>
        /// 底层仪器资源。
        /// </summary>
        public IDisposable Instrument
        {
            get;
            set;
        }

    }
}