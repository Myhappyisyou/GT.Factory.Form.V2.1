using GT_Common;
using GT_Common.DriverForm.Aynettek;
using GT_Common.Helper;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.Helper.Logging;
using System;
using System.Windows.Forms;

namespace OP010
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]

        static void Main()
        {
            // 单实例检查
            if (!AppManager.CheckSingleInstance("OP010"))
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


            try
            {
                using (var loginForm =
                    new LoginForm(
                        new RfidService(),
                        new UserApiService(
                            Config.Instance.ServerApi,
                            Config.Instance.ServerDbtask),
                        new MesService(),
                        new LocalUserService(),
                        LocalConfig.Instance.UIDIP,
                        LocalConfig.Instance.UIDPort))
                {

                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        AuthHelper.RecordLogin(loginForm.currentUser);

                        Application.Run(new MainForm());
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Error("OP010启动异常", ex);
                MessageBox.Show(
                    ex.Message,
                    "系统异常",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
