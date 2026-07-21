using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Core.Net;
using HslCommunication.Profinet.Keyence;
using HslCommunication.Profinet.Melsec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.Helper.PlcComm
{
    public class PatchReader
    {
        public NetworkDeviceBase Plc { get; }
        public bool IsConnected { get; private set; }

        public string SectionName { get; }
        public int StartIndex { get; }
        public ushort ReadDLength { get; }
        public string HeartbeatAddr { get; }
        public Int16Block Data { get; }

        public event EventHandler<Int16Block> ValueUpdateEvent;

        private ushort _heartbeatValue;
        private CancellationTokenSource _cts;

        /// <summary>
        /// 读取间隔，默认 50 毫秒
        /// </summary>
        public TimeSpan ReadInterval { get; set; } = TimeSpan.FromMilliseconds(50);

        public PatchReader(NetworkDeviceBase plc, string sectionName, string heartbeatAddr, int startIndex, ushort length)
        {
            Plc = plc ?? throw new ArgumentNullException(nameof(plc));
            SectionName = sectionName;
            StartIndex = startIndex;
            ReadDLength = length;
            HeartbeatAddr = heartbeatAddr;

            Data = new Int16Block(plc.ByteTransform) { ValueStartIndex = startIndex };

            Plc.ReceiveTimeOut = 2000;
            Plc.ConnectTimeOut = 2000;
            Plc.SetPersistentConnection();
        }

        /// <summary>
        /// 启动异步读取
        /// </summary>
        public void Start()
        {
            if (_cts != null) return; // 已经运行
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => ReadLoopAsync(_cts.Token));
        }

        /// <summary>
        /// 停止读取
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }

        private async Task ReadLoopAsync(CancellationToken token)
        {
            string readAddr = $"{SectionName}{StartIndex}";

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 读取数据
                    var readResult = Plc.Read(readAddr, ReadDLength);
                    if (!readResult.IsSuccess)
                    {
                        IsConnected = false;
                        TryReconnect();
                        await Task.Delay(200, token);
                        continue;
                    }

                    // 更新数据（线程安全）
                    lock (Data)
                    {
                        Data.AllValueRead = readResult.Content;
                    }

                    // 触发事件
                    ValueUpdateEvent?.Invoke(this, Data);

                    // 写心跳
                    var writeResult = Plc.Write(HeartbeatAddr, _heartbeatValue);
                    if (!writeResult.IsSuccess)
                    {
                        IsConnected = false;
                        TryReconnect();
                        await Task.Delay(200, token);
                        continue;
                    }

                    _heartbeatValue = (ushort)((_heartbeatValue + 1) % ushort.MaxValue);
                    IsConnected = true;
                }
                catch
                {
                    IsConnected = false;
                    await Task.Delay(500, token);
                }

                await Task.Delay(ReadInterval, token);
            }
        }

        private void TryReconnect()
        {
            try
            {
                Plc.ConnectClose();
                var result = Plc.ConnectServer();
                IsConnected = result.IsSuccess;
            }
            catch
            {
                IsConnected = false;
            }
        }
    }
}
