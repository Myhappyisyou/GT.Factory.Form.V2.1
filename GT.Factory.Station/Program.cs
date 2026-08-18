using GT.Factory.Common.Helper;
using GT.Factory.Station.Helper;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.Station.Helper.BydUser;
using GT.Factory.Station.Helper.ClientTask;
using GT.Factory.Station.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            // 初始化应用环境
            var runtime = new AppRuntime();

            // 首先显示登录窗口

            using (var loginForm = new LoginForm(new RfidService(), new UserApiService(Config.Instance.ServerApi, Config.Instance.ServerDbtask), new MesService(), new LocalUserService(), Config.Instance.UIDIP, Config.Instance.UIDPort))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    // 登录成功
                    var user = loginForm.currentUser;
                    AuthHelper.RecordLogin(user); // ✅ 记录用户到 AuthHelper
                    // 登录成功，显示主测试界面
                    Application.Run(new BaseMainForm());
                }
                else
                {
                    // 登录失败或取消，退出程序
                    Application.Exit();
                }
            }
        }

    }
}
