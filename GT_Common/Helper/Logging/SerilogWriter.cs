using System;
using System.Collections.Generic;
using Serilog;

namespace GT_Common.Helper.Logging
{
    public class SerilogWriter : ILogWriter
    {
        private readonly ILogger _logger;

        public SerilogWriter()
        {
            _logger = Serilog.Log.Logger; // 使用全局已初始化 Logger
        }

        public SerilogWriter(ILogger logger)
        {
            _logger = logger;
        }


        public void Log(LogLevel level, string message, Exception ex = null)
        {
            switch (level)
            {
                case LogLevel.Info:
                    _logger.Information(message);
                    break;

                case LogLevel.Warning:
                    if (ex != null)
                        _logger.Warning(ex, message);
                    else
                        _logger.Warning(message);
                    break;

                case LogLevel.Error:
                    if (ex != null)
                        _logger.Error(ex, message);
                    else
                        _logger.Error(message);
                    break;
            }
        }
    }
}
