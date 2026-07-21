using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.BydMes
{
    public interface IMesService
    {
        bool ValidateUser(string userName, string passWord, out string error);

        ShopOrderInfo ValidateOrderInfor(string order, string resource, string sn, out bool 验证结果);

    }
}
