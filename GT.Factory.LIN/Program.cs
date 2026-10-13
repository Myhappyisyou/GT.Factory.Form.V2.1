using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.LIN
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var host =
                new LinDebugHost();

            //
            // Project Debug正式入口：
            //
            // 不再硬编码具体.linproj。
            //
            // 由Host负责选择并加载工程。
            //
            host.Start(
                new MockLinAdapter());
        }
    }
}
