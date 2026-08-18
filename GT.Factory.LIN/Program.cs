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


            var projectFile =
       @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\BYD_300W.linproj";

            host.Start(projectFile, new MockLinAdapter());
        }
    }
}
