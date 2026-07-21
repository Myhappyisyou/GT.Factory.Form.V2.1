using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Logging
{
    public static class LogBootstrapper
    {
        public static void Init(string stationName)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Station", stationName)
                .WriteTo.File(
                    path: Path.Combine(
                        PathCenter.LogFile("MesLog"),
                        $"{DateTime.Now:yyyy-MM}",
                        "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.ContainsKey("Channel") &&
                        e.Properties["Channel"].ToString().Contains("Send_TBYD"))
                    .WriteTo.File(
                        Path.Combine(
                            PathCenter.LogFile("Send_TBYD"),
                            $"{DateTime.Now:yyyy-MM}",
                            "log-.txt"),
                        rollingInterval: RollingInterval.Day))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.ContainsKey("Channel") &&
                        e.Properties["Channel"].ToString().Contains("Receive_FBYD"))
                    .WriteTo.File(
                        Path.Combine(
                            PathCenter.LogFile("Receive_FBYD"),
                            $"{DateTime.Now:yyyy-MM}",
                            "log-.txt"),
                        rollingInterval: RollingInterval.Day))
                .CreateLogger();
        }
    }
}
