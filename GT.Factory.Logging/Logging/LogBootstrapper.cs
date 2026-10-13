using GT.Factory.Common.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Logging.Logging
{
    public static class LogBootstrapper
    {
        public static void Init(string stationName)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // 最低日志级别为 Debug，意味着 Debug、Info、Warn、Error 都会被记录
                .Enrich.FromLogContext() // 允许从当前上下文（如 using 块）中动态添加额外属性
                .Enrich.WithProperty("Station", stationName) // 给所有日志打上 "Station" 标签，值为传入的 "OP070"
                .WriteTo.File(
                    path: Path.Combine(
                        PathCenter.LogFile("MesLog"), // 获取 MesLog 的根目录
                        $"{DateTime.Now:yyyy-MM}",    // 按年月建子文件夹，如 "2026-10"
                        "log-.txt"),                  // 文件名前缀，Serilog 会自动在后面加上日期，如 "log-20261010.txt"
                    rollingInterval: RollingInterval.Day, // 每天生成一个新的日志文件
                    rollOnFileSizeLimit: true)            // 如果单个文件太大，自动切分新文件
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
