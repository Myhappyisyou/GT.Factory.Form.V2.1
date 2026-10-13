using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
using GT.Factory.Data.Core;
using GT.Factory.GTUser.Repository;
using GT.Factory.Logging.Logging;
using GT.Factory.Login;
using GT.Factory.RFID.Aynettek;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.WebApi.QueryClient;
using Serilog;
using System;
using System.IO;
using System.Windows.Forms;

namespace GT.Factory.Station.DriverForm.Aynettek
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!HslCommunication.Authorization.SetAuthorizationCode("123456"))
            {
                MessageBox.Show("激活失败");
            }


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: Path.Combine(
                            Path.Combine(PathCenter.LogFile("MesLog")),
                            $"{DateTime.Now:yyyy-MM}",
                            "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 30
                 )
                 .CreateLogger();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DataRuntime dataRuntime =
    new DataRuntime(Config.Instance.DatabaseOptions);

            // 初始化应用环境
            var runtime = new AppRuntime();

            ILocalUserService localUserService =
    new LocalUserService(
        new LocalUserRepository(dataRuntime.Database));

            IUserService userApiService =
                new UserApiService(
                    Config.Instance.ServerApi,
                    Config.Instance.ServerDbtask);

            IMesService mesService =
                new MesService();

            IRfidService rfidService =
                new RfidService();

            new LoginRuntime(
                localUserService,
                userApiService,
                mesService,
                rfidService);

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

                        Application.Run(new BaseMainForm());
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
                DisplayLog.Error("OP070启动异常", ex);
                MessageBox.Show(
                    ex.Message,
                    "系统异常",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}
