using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core
{
    public interface IPageLifecycle
    {
        void OnPageShown();   // 页面显示
        void OnPageHidden();  // 页面隐藏
    }

}
