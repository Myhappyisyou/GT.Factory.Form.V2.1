using AYNETTEK.HFModbus;
using GT_Common.Helper.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Aynettek
{
    public class RfidService : IRfidService
    {
        private readonly Reader _reader;
        private readonly AynettekMethode _aynettekMethode;

        public RfidService()
        {
            _reader = new Reader();
            _aynettekMethode = new AynettekMethode(_reader);
        }

        public bool Init(string addr, int port)
        {
            try
            {
                return _aynettekMethode.InitDriver( addr,  port);
            }
            catch (Exception ex)
            {
                DisplayLog.Error("RFID初始化失败", ex);

                return false;
            }
        }

        public Task StartListening(Action<string> onUidReceived, Action< bool> TagOnline, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        (string uid, bool tagSingle)  = _aynettekMethode.GetTagOnline();
                        if (!string.IsNullOrEmpty(uid))
                        {
                            onUidReceived?.Invoke(uid);
                        }
                        TagOnline?.Invoke(tagSingle);
                    }
                    catch (Exception ex)
                    {
                        DisplayLog.Error("刷卡监听异常", ex);

                    }

                    await Task.Delay(100, token);
                }
            });
        }

        public void Stop()
        {
            _reader.DisConnect();
        }
    }

}
