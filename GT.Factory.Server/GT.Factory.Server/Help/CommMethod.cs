using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Server.Help
{
    public static class CommMethod
    {
        public static void SafeInvoke(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
            //if (control.IsHandleCreated && !control.IsDisposed)
            //{
            //    control.Invoke(action);
            //}
        }
    }
}
