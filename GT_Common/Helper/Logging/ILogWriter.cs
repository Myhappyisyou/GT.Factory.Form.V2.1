using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Logging
{
    public interface ILogWriter
    {
        void Log(LogLevel level, string message, Exception ex = null);
    }
}
