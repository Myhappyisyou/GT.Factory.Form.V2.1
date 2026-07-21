using GT_Common.MyEnum;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Keyence;
using HslCommunication.Profinet.Siemens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    public static class PlcFactory
    {
        public static NetworkDeviceBase Create(PlcConfig config)
        {
            switch (config.PlcType)
            {
                case PlcType.SiemensS1500:
                    return new SiemensS7Net(SiemensPLCS.S1500, config.Ip);

                case PlcType.SiemensS1200:
                    return new SiemensS7Net(SiemensPLCS.S1200, config.Ip);

                case PlcType.KeyenceNano:
                    return new KeyenceNanoSerialOverTcp(config.Ip);

                default:
                    throw new NotSupportedException($"不支持的PLC类型: {config.PlcType}");
            }
        }
    }
}
