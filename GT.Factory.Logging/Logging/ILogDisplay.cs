using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Logging.Logging
{
    public interface ILogDisplay
    {
        void Show(LogLevel level, string message);
    }
}
