using GT.Factory.PLC.Enums;
using HslCommunication.Core.Net;
using HslCommunication.ModBus;
using HslCommunication.Profinet.Keyence;
using HslCommunication.Profinet.Melsec;
using HslCommunication.Profinet.Omron;
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

                case PlcType.Melsec:
                    return new MelsecMcNet(Ip, 6000);

                case PlcType.Omron:
                    return new OmronFinsNet(Ip, 9600);

                case PlcType.Modbus:
                    return new ModbusTcpNet(Ip, 502);

                default:
                    throw new NotSupportedException($"不支持的PLC类型: {plcType}");
            }
        }
    }
}
