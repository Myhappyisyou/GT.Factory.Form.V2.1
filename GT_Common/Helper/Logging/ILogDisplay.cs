using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Logging
{
    public interface ILogDisplay
    {
        void Show(LogLevel level, string message);
    }
}
