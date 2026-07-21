using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Aynettek
{
    public interface IRfidService
    {
        Task StartListening(Action<string> onUidReceived, Action<bool> TagOnline, CancellationToken token);
        bool Init(string addr, int port);
        void Stop();
    }
}
