using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.Helper.LanModelSync
{
    public class MasterHeartbeatMonitor
    {
        private readonly string _masterIP;
        private readonly int _port;
        private DateTime _lastPingTime = DateTime.MinValue;
        private CancellationTokenSource _cts;
        public event Action<bool> OnConnectionStateChanged;

        private bool _lastState = false;
        private int _missedCount = 0;
        private readonly int _missThreshold = 3; // 连续丢 3 次才算离线
        private readonly int _timeoutSeconds = 5; // 每次最多允许多久没收到
        private bool _firstCheck = true;
        private bool _everReceivedHeartbeat = false;

        public MasterHeartbeatMonitor(string masterIP, int port)
        {
            _masterIP = masterIP;
            _port = port;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() => Listen());
            //Task.Run(() => MonitorTimeout());
            Task.Factory.StartNew(() => MonitorTimeout().Wait(),
                    _cts.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
        }

        private async Task Listen()
        {
            // ✅ 客户端监听本机所有可用 IP 
            TcpListener listener = new TcpListener(IPAddress.Any, _port);

            // 如果你只想监听 192.168.4.99
            //TcpListener listener = new TcpListener(IPAddress.Parse("192.168.4.199"), _port);

            //TcpListener listener = new TcpListener(IPAddress.Parse(_masterIP), _port);
            listener.Start();
            Console.WriteLine("开始监听6666端口...");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();
                    _lastPingTime = DateTime.Now;
                    _missedCount = 0;
                    _everReceivedHeartbeat = true; // ✅ 收到过心跳
                    client.Close();
                }
                catch {
                    Console.WriteLine(1111111111111);
                }
            }

            listener.Stop();
        }

        //private async Task MonitorTimeout()
        //{
        //    try
        //    {
        //        while (!_cts.Token.IsCancellationRequested)
        //        {
        //            if (_everReceivedHeartbeat)
        //            {
        //                var span = DateTime.Now - _lastPingTime;

        //                if (span.TotalSeconds > _timeoutSeconds)
        //                    _missedCount++;
        //                else
        //                    _missedCount = 0;
        //            }
        //            else
        //            {
        //                // 从未收到心跳，直接认为断线
        //                _missedCount = _missThreshold;
        //            }

        //            bool isConnected = _missedCount < _missThreshold;

        //            if (_firstCheck || isConnected != _lastState)
        //            {
        //                _lastState = isConnected;
        //                OnConnectionStateChanged?.Invoke(isConnected);
        //                _firstCheck = false;
        //            }
        //            await Task.Delay(1000);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[MonitorTimeout] 异常: {ex.Message}");
        //    }
        //}

        private async Task MonitorTimeout()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_everReceivedHeartbeat)
                    {
                        var span = DateTime.Now - _lastPingTime;
                        if (span.TotalSeconds > _timeoutSeconds)
                            _missedCount++;
                        else
                            _missedCount = 0;
                    }
                    else
                    {
                        // 等待首次心跳，初始不认为离线
                        _missedCount = 0;
                    }

                    bool isConnected = _everReceivedHeartbeat && _missedCount < _missThreshold;

                    if (_firstCheck || isConnected != _lastState)
                    {
                        _lastState = isConnected;
                        OnConnectionStateChanged?.Invoke(isConnected);
                        _firstCheck = false;
                    }

                    await Task.Delay(500); // 检查间隔改小，更快感知
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MonitorTimeout] 异常: {ex.Message}");
            }
        }

        public void Stop() => _cts?.Cancel();
    }
}
