using GT.Factory.Common.Helpers;
using Serilog;
using System;
using System.IO;

namespace GT.Factory.Logging.Logging
{
    public static class MesLog
    {
        public static readonly ILogger SendLogger;
        public static readonly ILogger ReceiveLogger;

        static MesLog()
        {
            
            SendLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(PathCenter.LogFile("Send_TBYD"), $"{DateTime.Now:yyyy-MM}", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30
                )
                .CreateLogger();

            ReceiveLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(PathCenter.LogFile("Receive_FBYD"), $"{DateTime.Now:yyyy-MM}", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30
                )
                .CreateLogger();
        }
    }
}
