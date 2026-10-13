using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using GT.Factory.Logging.Logging;

namespace GT.Factory.Server.Core.LanModelSync
{
    public class DeviceStateMonitor
    {
        private readonly int _port;

        private TcpListener _listener;

        private CancellationTokenSource _cts;

        private readonly object _lock = new object();

        public Dictionary<string, DeviceStatus> DeviceStates { get; private set; }
            = new Dictionary<string, DeviceStatus>();

        private Dictionary<string, DateTime> LastSeen
            = new Dictionary<string, DateTime>();

        public event Action<string, DeviceStatus> OnDeviceStateChanged;

        private Timer _offlineTimer;

        public DeviceStateMonitor(int port, IEnumerable<string> knownClients)
        {
            _port = port;

            foreach (var ip in knownClients)
            {
                DeviceStates[ip] = new DeviceStatus
                {
                    Status = "Offline",
                    ModelName = ""
                };

                LastSeen[ip] = DateTime.Now;
            }

            StartOfflineChecker();
        }

        /// <summary>
        /// 启动监听
        /// </summary>
        public void Start()
        {
            _cts = new CancellationTokenSource();

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            Task.Run(AcceptLoop);
        }

        /// <summary>
        /// TCP监听循环
        /// </summary>
        private async Task AcceptLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();

                    _ = Task.Run(() => HandleClient(client));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"AcceptLoop Error: ", ex);
                }
            }
        }

        /// <summary>
        /// 处理客户端
        /// </summary>
        private void HandleClient(TcpClient client)
        {
            string ip = "";

            try
            {
                ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                Console.WriteLine($"监听到IP {ip}");

                // 过滤未知设备
                if (!DeviceStates.ContainsKey(ip))
                {
                    Console.WriteLine($"未知设备 {ip}");
                    return;
                }

                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream))
                {
                    // 客户端发送 JSON + 换行
                    string json = reader.ReadLine();

                    if (string.IsNullOrEmpty(json))
                        return;

                    var model = JsonConvert.DeserializeObject<DeviceStatus>(json);

                    if (model == null)
                        return;

                    lock (_lock)
                    {
                        DeviceStates[ip] = model;
                        LastSeen[ip] = DateTime.Now;
                    }

                    OnDeviceStateChanged?.Invoke(ip, model);
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"HandleClient Error {ip}: ", ex);
            }
            finally
            {
                client.Close();
            }
        }

        /// <summary>
        /// 掉线检测
        /// </summary>
        private void StartOfflineChecker()
        {
            _offlineTimer = new Timer(_ =>
            {
                var now = DateTime.Now;

                List<string> offlineList = new List<string>();

                lock (_lock)
                {
                    foreach (var kv in LastSeen)
                    {
                        var ip = kv.Key;

                        if ((now - kv.Value).TotalSeconds > 5 &&
                            DeviceStates[ip].Status != "Offline")
                        {
                            DeviceStates[ip].Status = "Offline";

                            offlineList.Add(ip);
                        }
                    }
                }

                foreach (var ip in offlineList)
                {
                    Console.WriteLine($"[掉线] {ip}");

                    DisplayLog.Warn($"[掉线] {ip}");

                    OnDeviceStateChanged?.Invoke(ip,
                        new DeviceStatus
                        {
                            Status = "Offline",
                            ModelName = ""
                        });
                }

            }, null, 1000, 1000);
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        public void Stop()
        {
            try
            {
                _cts?.Cancel();

                _listener?.Stop();

                _offlineTimer?.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 是否全部Ready
        /// </summary>
        public bool AllReady
        {
            get
            {
                lock (_lock)
                {
                    return DeviceStates.Values.All(x => x.Status == "Ready");
                }
            }
        }
    }

    //public class DeviceStateMonitor
    //{
    //    private readonly int _port;
    //    private TcpListener _listener;
    //    private CancellationTokenSource _cts;
    //    public Dictionary<string, DeviceStatus> DeviceStates { get; private set; } = new Dictionary<string, DeviceStatus>();
    //    private Dictionary<string, DateTime> LastSeen = new Dictionary<string, DateTime>();

    //    public event Action<string, DeviceStatus> OnDeviceStateChanged;

    //    public DeviceStateMonitor(int port, IEnumerable<string> knownClients)
    //    {
    //        _port = port;
    //        foreach (var ip in knownClients)
    //        {
    //            DeviceStates[ip] = new DeviceStatus { Status = "Offline", ModelName = "" };
    //            LastSeen[ip] = DateTime.Now;
    //        }
    //        StartOfflineChecker(); // 启动掉线检测
    //    }

    //    public void Start()
    //    {
    //        _cts = new CancellationTokenSource();
    //        Task.Run(() =>
    //        {
    //            _listener = new TcpListener(IPAddress.Any, _port);
    //            _listener.Start();
    //            while (!_cts.Token.IsCancellationRequested)
    //            {
    //                TcpClient client = _listener.AcceptTcpClient();
    //                string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
    //                Console.WriteLine($"监听到IP--{ip}");
    //                // 使用传统的 using 块替代 using var
    //                using (var stream = client.GetStream())
    //                {
    //                    using (var reader = new StreamReader(stream))
    //                    {
    //                        string state = reader.ReadToEnd();

    //                        // 反序列化JSON
    //                        var model = JsonConvert.DeserializeObject<DeviceStatus>(state);

    //                        // 添加大括号的 lock 语句
    //                        lock (DeviceStates)
    //                        {
    //                            DeviceStates[ip] = model;
    //                            LastSeen[ip] = DateTime.Now;
    //                        }
    //                        //DisplayLog.ShowLogInfo($"[监听到IP]--{ip}--{model}");

    //                        // 安全调用事件
    //                        var handler = OnDeviceStateChanged;
    //                        if (handler != null)
    //                        {
    //                            Console.WriteLine("安全安全调用事件");
    //                            handler(ip, model);
    //                        }
    //                    }
    //                }

    //                // 确保客户端关闭
    //                client.Close();
    //            }
    //        });
    //    }

    //    public void Stop()
    //    {
    //        _cts?.Cancel();
    //        try
    //        {
    //            _listener?.Stop();
    //        }
    //        catch { }
    //    }


    //    private void StartOfflineChecker()
    //    {
    //        Timer timer = new Timer(_ =>
    //        {
    //            var now = DateTime.Now;
    //            List<string> toMarkOffline = new List<string>();

    //            lock (DeviceStates)
    //            {
    //                foreach (var kvp in LastSeen.ToList())
    //                {
    //                    if ((now - kvp.Value).TotalSeconds > 3 && DeviceStates[kvp.Key].Status != "Offline")
    //                    {
    //                        DeviceStates[kvp.Key].Status = "Offline";
    //                        toMarkOffline.Add(kvp.Key);
    //                    }
    //                }
    //            }
    //            foreach (var ip in toMarkOffline)
    //            {
    //                Console.WriteLine($"[掉线] {ip} 被标记为 Offline");
    //                DisplayLog.Warn($"[掉线] {ip} 被标记为 Offline");
    //                OnDeviceStateChanged?.Invoke(ip, new DeviceStatus { Status = "Offline", ModelName = "" });
    //            }

    //        }, null, 1000, 1000); // 每 3 秒检查
    //    }


    //    public bool AllReady => DeviceStates.Values.All(s => s.Status == "Ready");


    //}

}
