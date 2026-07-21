using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.EditJson
{
    public interface IApplyChanges
    {
        void ApplyChanges(); // 将控件数据写回绑定的对象
    }
}
