using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.UI.Enum
{
    public enum UIPageState
    {
        Loading,
        Empty,
        Data,
        Error
    }

    public enum UIStatusType
    {
        Info,
        Success,
        Warning,
        Error
    }

    #region 产品状态
    public enum ProductStatus
    {
        Standby,    // 待机
        OK,         // 通过
        NG          // 失败
    }

    #endregion

}
