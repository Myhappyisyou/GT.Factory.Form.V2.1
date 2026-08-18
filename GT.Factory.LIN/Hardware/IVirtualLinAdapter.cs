using GT.Factory.LIN.Simulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Hardware
{
    public interface IVirtualLinAdapter
    {

        void AttachBus(
            LinVirtualBus bus);

    }
}
