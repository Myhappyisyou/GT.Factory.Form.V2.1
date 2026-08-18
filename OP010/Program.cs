using GT_Common;
using GT_Common.DriverForm.Aynettek;
using GT_Common.DriverForm.Login;
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
                //using (var loginForm =
                //    new LoginForm(
                //        new RfidService(),
                //        new UserApiService(
                //            Config.Instance.ServerApi,
                //            Config.Instance.ServerDbtask),
                //        new MesService(),
                //        new LocalUserService(),
                //        LocalConfig.Instance.UIDIP,
                //        LocalConfig.Instance.UIDPort))
                var localConfig = Config.Instance;

                // 校验登录配置
                localConfig.Login.Validate();

                // 初始化 RFID（如果启用）
                if (localConfig.EnableCardReader)
                {
                    //rfidService.Init(localConfig.UIDIP, localConfig.UIDPort);
                }

                var loginService = new LoginService(
                    AppRuntime.Current.LocalUserService,
                    AppRuntime.Current.UserApiService,
                    AppRuntime.Current.MesService);

                var providers = IdentityProviderFactory.CreateProviders(
                    localConfig.Login,
                    loginService,
                    AppRuntime.Current.RfidService);

                var loginForm = new GT_Common.DriverForm.Login.LoginForm(providers);

                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        AuthHelper.RecordLogin(loginForm.CurrentUser);

                        Application.Run(new MainForm());
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
