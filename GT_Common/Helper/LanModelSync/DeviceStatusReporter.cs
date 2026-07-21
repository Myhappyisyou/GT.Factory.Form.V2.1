using GT_Common.Helper.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.Helper.LanModelSync
{
    public sealed class DeviceStatusReporter : IDisposable
    {
        private readonly string _masterIP;
        private readonly int _reportPort;
        private readonly Func<DeviceStatus> _getStatus;

        private CancellationTokenSource _cts;
        private Task _worker;

        private readonly int _intervalMs = 3000;
        private readonly int _connectTimeoutMs = 500;

        public DeviceStatusReporter(string masterIP, int reportPort, Func<DeviceStatus> getStatus)
        {
            _masterIP = masterIP;
            _reportPort = reportPort;
            _getStatus = getStatus;
        }

        #region Public API

        public void Start()
        {
            if (_worker != null) return;

            _cts = new CancellationTokenSource();
            _worker = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            // ⚠ UI 线程只做一件事：Cancel
            _cts?.Cancel();
        }

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region Worker Loop

        private async Task LoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    DeviceStatus status = null;

                    try
                    {
                        status = _getStatus();
                        //status.Time = DateTime.Now;
                        await SendStatusAsync(status, token);
                    }
                    catch
                    {
                        // 心跳失败不抛
                    }

                    // 可被 Cancel 立即打断的延迟
                    await Task.Delay(_intervalMs, token);
                }
            }
            catch (TaskCanceledException)
            {
                // 正常退出
            }
            finally
            {
                // 程序退出前，尝试发送 Offline
                try
                {
                    var offline = new DeviceStatus
                    {
                        Status = "Offline",
                        ModelName = _getStatus()?.ModelName
                    };

                    await SendStatusAsync(offline, CancellationToken.None);
                }
                catch
                {
                    // 离线失败不影响退出
                }
            }
        }

        #endregion

        #region TCP Send

        //private async Task SendStatusAsync(DeviceStatus status, CancellationToken token)
        //{
        //    using (var client = new TcpClient())
        //    {
        //        // 关键：连接超时控制
        //        var connectTask = client.ConnectAsync(_masterIP, _reportPort);

        //        if (await Task.WhenAny(connectTask, Task.Delay(_connectTimeoutMs, token)) != connectTask)
        //            return;

        //        string json = JsonConvert.SerializeObject(status);
        //        byte[] data = Encoding.UTF8.GetBytes(json);

        //        using (var stream = client.GetStream())
        //        {
        //            await stream.WriteAsync(data, 0, data.Length, token);
        //        }
        //    }
        //}

        private async Task SendStatusAsync(DeviceStatus status, CancellationToken token)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // 连接超时控制
                    var connectTask = client.ConnectAsync(_masterIP, _reportPort);

                    if (await Task.WhenAny(connectTask, Task.Delay(_connectTimeoutMs, token)) != connectTask)
                        return;

                    using (var stream = client.GetStream())
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        string json = JsonConvert.SerializeObject(status);

                        // 关键：WriteLine
                        await writer.WriteLineAsync(json);

                        await writer.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Warn($"SendStatusAsync Error: {ex.Message}");
            }
        }

        #endregion
    }
}
