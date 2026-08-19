using GT.Factory.PLC.Enums;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Keyence;
using HslCommunication.Profinet.Siemens;
using System;

namespace GT.Factory.PLC
{
    public static class PlcFactory
    {
        public static NetworkDeviceBase Create(PlcType plcType, string Ip)
        {
            switch (plcType)
            {
                case PlcType.SiemensS1500:
                    return new SiemensS7Net(SiemensPLCS.S1500, Ip);

                case PlcType.SiemensS1200:
                    return new SiemensS7Net(SiemensPLCS.S1200, Ip);

                case PlcType.KeyenceNano:
                    return new KeyenceNanoSerialOverTcp(Ip);

                default:
                    throw new NotSupportedException($"不支持的PLC类型: {plcType}");
            }
        }
    }
}
