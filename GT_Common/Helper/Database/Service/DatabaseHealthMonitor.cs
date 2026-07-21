using GT_Common.Helper.Database.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Service
{
    public class DatabaseHealthMonitor
    {
        private readonly IDatabase _primary;

        private bool _isHealthy = true;

        private DateTime _lastCheckTime = DateTime.MinValue;

        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15);

        public DatabaseHealthMonitor(IDatabase primary)
        {
            _primary = primary;
        }

        public async Task<bool> IsHealthyAsync()
        {
            if (DateTime.Now - _lastCheckTime < _checkInterval)
                return _isHealthy;

            _lastCheckTime = DateTime.Now;

            _isHealthy = await _primary.PingAsync();

            return _isHealthy;
        }
    }
}
