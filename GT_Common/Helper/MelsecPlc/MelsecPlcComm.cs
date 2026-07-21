using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HslCommunication;
using HslCommunication.Profinet.Melsec;
using System.Data;

namespace GT_Common.Helper.MelsecPlc
{
    public class MelsecPlcComm
    {
        public string PlcName { get; private set; }

        public MelsecMcNet Plc;

        public string Ip { get; private set; }

        public int Port { get; private set; }

        public bool IsConnected { get; private set; } = false;

        private Thread readThread;

        public int StartIndex { get; private set; }

        public ushort ReadDLength { get; private set; }

        public string HeartbeatAddr { get; private set; }

        public Dvalue Data { get; private set; } = new Dvalue();


        private HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();


        public event EventHandler<Dvalue> ValueUpdateEvent;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="plcName">PLC命名</param>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="heartbeatAddr">心跳地址</param>
        /// <param name="startIndex">读取的起始位</param>
        /// <param name="length">读取长度</param>
        public MelsecPlcComm(string plcName, string ip, int port, string heartbeatAddr, int startIndex, ushort length)
        {
            PlcName = plcName;
            Ip = ip;
            Port = port;
            StartIndex = startIndex;
            ReadDLength = length;
            HeartbeatAddr = heartbeatAddr;

            Data.ValueStartIndex = startIndex;

            Plc = new MelsecMcNet(Ip, Port);

        }


        public void Init()
        {
            readThread = new Thread(new ThreadStart(ReadPlc));
            readThread.IsBackground = true;
            readThread.Start();
        }


        private void ReadPlc()
        {
            string readAddr = $"R{StartIndex}";

            ushort hearBearValue = 0;

            OperateResult<byte[]> readResult = new OperateResult<byte[]>();
            OperateResult writeResult = new OperateResult();

            Plc.SetPersistentConnection();

            while (true)
            {
                Thread.Sleep(10);

                //全部读取
                readResult = Plc.Read(readAddr, ReadDLength);
                IsConnected = readResult.IsSuccess;
                if (!IsConnected)
                {
                    goto Disconnect;
                }
                else
                {
                    Data.AllValueRead = readResult.Content;

                    ValueUpdateEvent?.Invoke(this, Data);
                }

                //心跳写入
                writeResult = Plc.Write(HeartbeatAddr, hearBearValue);
                IsConnected = writeResult.IsSuccess;
                if (!IsConnected)
                {
                    goto Disconnect;
                }
                hearBearValue++;

                continue;

            Disconnect:
                {
                    Thread.Sleep(50);
                    continue;
                }
            }
        }



        public char GetChar(int index)
        {
            return (char)Data.AllValueRead[(index - StartIndex) * 2];
        }

        public short GetShort(int index)
        {
            return baseOperator.TransInt16(Data.AllValueRead, (index - StartIndex) * 2);
        }

        public float GetFloat(int index)
        {
            return baseOperator.TransSingle(Data.AllValueRead, (index - StartIndex) * 2);
        }

        public double GetDouble(int index)
        {
            return baseOperator.TransDouble(Data.AllValueRead, (index - StartIndex) * 2);
        }

        public string GetString(int index, int length)
        {
            return baseOperator.TransString(Data.AllValueRead, (index - StartIndex) * 2, length, Encoding.ASCII);
        }





    }


    public class Dvalue
    {
        public byte[] AllValueRead { get; set; }

        public int ValueStartIndex { get; set; }


        private HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();

        public char C(int index)
        {
            return (char)AllValueRead[(index - ValueStartIndex) * 2];
        }

        public double D(int index)
        {
            return baseOperator.TransDouble(AllValueRead, (index - ValueStartIndex) * 2);
        }

        public short S(int index)
        {
            return baseOperator.TransInt16(AllValueRead, (index - ValueStartIndex) * 2);
        }

        public float F(int index)
        {
            return baseOperator.TransSingle(AllValueRead, (index - ValueStartIndex) * 2);
        }

        public string Str(int index, int length)
        {
            return baseOperator.TransString(AllValueRead, (index - ValueStartIndex) * 2, length, Encoding.ASCII);
        }
        public int I(int index)
        {
            return baseOperator.TransInt32(AllValueRead, (index - ValueStartIndex) * 2);
        }

    }

}
