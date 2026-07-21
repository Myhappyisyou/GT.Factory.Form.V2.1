using GT_Common;

namespace OP070
{
    public class ControlEngine_new : ControlEngineBase
    {

        protected override void AfterInit()
        {

            Shared.ProcessName = LocalConfig.Instance.ProcessName.Replace("&", "");

            Shared.orderNub = Shared.shopOrder != null ? Shared.shopOrder.OrderNum : "1000";

            Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "111111111111";

            Shared.monitor = monitor;
        }
    }
}
