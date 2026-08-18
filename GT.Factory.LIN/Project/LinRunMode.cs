using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Project
{

    /// <summary>
    /// LIN运行模式
    ///
    /// Simulation:
    ///     PC模拟Slave ECU
    ///
    /// Hardware:
    ///     真实LIN硬件+ECU
    ///
    /// </summary>
    public enum LinRunMode
    {
        Simulation,

        Hardware
    }

}
