using GT.Factory.Common.Interfaces;
using GT.Factory.Common.Models.Mes;

namespace GT.Factory.Station.Helper.BydMes
{
    public class MesService : IMesService
    {
        public bool ValidateUser(string userName, string passWord, out string errorMessage)
        {
            BydMesCom.用户验证(userName, passWord, out bool ok, out string msg, out string _);
            errorMessage = msg;
            return ok;
        }

        public ShopOrderInfo ValidateOrderInfor(string order, string resource, string sn, out bool 验证结果)
        {
            验证结果 = BydMesCom.工单信息查询(order, resource, sn, out ShopOrderInfo 工单信息, out string MES反馈, out string XMLOUT);
            return 工单信息;
        }
    }

}
