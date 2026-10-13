using Serilog;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.LanModelSync
{
    public class HeartbeatBroadcaster
    {
        private readonly string[] _clientIPs;
        private readonly int _heartbeatPort;
        private System.Timers.Timer _timer;

        public HeartbeatBroadcaster(string[] clientIPs, int heartbeatPort)
        {
            _clientIPs = clientIPs;
            _heartbeatPort = heartbeatPort;
        }

        public void Start()
        {
            _timer = new System.Timers.Timer(3000); // 每 3 秒
            _timer.Elapsed += (s, e) => SendHeartbeat();
            _timer.AutoReset = true;
            _timer.Start();
        }

        private async void SendHeartbeat()
        {
            var tasks = _clientIPs.Select(ip => SendHeartbeatToClient(ip));
            await Task.WhenAll(tasks);
        }

        private async Task SendHeartbeatToClient(string ip)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ip, _heartbeatPort);

                    // 等待连接，最多 1 秒
                    if (await Task.WhenAny(connectTask, Task.Delay(1000)) != connectTask)
                    {
                        //DisplayLog.ShowLogError($"心跳超时：{ip}");
                        //Log.Error($"心跳超时：{ip}");

                        return;
                    }

                   // DisplayLog.ShowLogInfo($"心跳成功：{ip}");
                }
            }
            catch (Exception exp)
            {
                Log.Error($"心跳失败：{ip} - {exp.Message}");
                //DisplayLog.ShowLogError($"心跳失败：{ip} - {ex.Message}");
            }
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
