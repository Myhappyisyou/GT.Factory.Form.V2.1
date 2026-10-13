using GT.Factory.Station;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.OP090
{
    public class ControlEngine : ControlEngineBase
    {

        protected override void AfterInit()
        {

            Shared.ProcessName = Config.Instance.ProcessName.Replace("&", "");

            Shared.orderNub = Shared.shopOrder != null ? Shared.shopOrder.OrderNum : "1000";

            Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "111111111111";

            Shared.monitor = monitor;
        }
    }
}
