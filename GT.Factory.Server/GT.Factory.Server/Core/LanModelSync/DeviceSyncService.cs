using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.LanModelSync
{
    public class DeviceSyncService
    {
        public DeviceStateMonitor Monitor { get; private set; }
        public HeartbeatBroadcaster Heartbeat { get; private set; }

        private readonly string[] _clientIPs;
        private readonly int _monitorPort;
        private readonly int _heartbeatPort;

        public DeviceSyncService(string[] clientIPs, int monitorPort = 8881, int heartbeatPort = 6666)
        {
            _clientIPs = clientIPs;
            _monitorPort = monitorPort;
            _heartbeatPort = heartbeatPort;
        }

        public void Start()
        {
            Monitor = new DeviceStateMonitor(_monitorPort, _clientIPs);
            Monitor.Start();

            Heartbeat = new HeartbeatBroadcaster(_clientIPs, _heartbeatPort);
            Heartbeat.Start();
        }

        public void Stop()
        {
            Heartbeat?.Stop();
            Monitor?.Stop();
        }
    }

}
