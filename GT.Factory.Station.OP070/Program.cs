using GT.Factory.Station;
using GT.Factory.Station.DriverForm.Aynettek;
using GT.Factory.Station.Helper;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.Station.Helper.BydUser;
using GT.Factory.Station.Helper.Logging;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GT.Factory.Station.OP070
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序主入口
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 单实例检查
            if (!AppManager.CheckSingleInstance("OP070"))
            {
                MessageBox.Show(
                    "软件已经打开，请勿重复打开!!",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }


            // 初始化公共环境
            AppManager.Initialize();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 初始化应用环境
            var runtime = new AppRuntime();

            try
            {
                // ✅ 只有简单干脆的一处 using
                using (var loginForm = AuthHelper.CreateLoginForm())
                {
                    // ✅ 只调用一次 ShowDialog
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        AuthHelper.RecordLogin(loginForm.CurrentUser);

                        Shared.user = AuthHelper.CurrentUser;

                        Application.Run(new MainForm());
                    }
                    else
                    {
                        // 用户点了右上角 X 退出，直接结束程序
                        Application.Exit();
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error("OP070启动异常",ex);
                MessageBox.Show(
                    ex.Message,
                    "系统异常",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
