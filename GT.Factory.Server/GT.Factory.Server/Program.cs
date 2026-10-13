using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
using GT.Factory.Data.Config;
using GT.Factory.Data.Core;
using GT.Factory.GTUser.Repository;
using GT.Factory.Logging.Logging;
using GT.Factory.Login;
using GT.Factory.Login.Login;
using GT.Factory.RFID.Aynettek;
using GT.Factory.Server.Help;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.WebApi.QueryClient;
using GT.Factory.WebApi.QueryServer;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace GT.Factory.Server
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process[] pro = Process.GetProcessesByName("Server");

            if (pro == null || pro.Length >= 2)
            {
                MessageBox.Show("软件已经打开，请勿重复打开!!", "提示！", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Exit();
                return;
            }

            //LogBootstrapper.Init("Server");

            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              .Enrich.FromLogContext()
              .WriteTo.File(
                  path: Path.Combine(
                          Path.Combine(PathCenter.LogFile("ServerLog")),
                          $"{DateTime.Now:yyyy-MM}",
                          "log-.txt"),
                  rollingInterval: RollingInterval.Day,
                  rollOnFileSizeLimit: true,
                  retainedFileCountLimit: 30
               )
               .CreateLogger();

            //string userDbPath = @"D:\BYD_Users\Users_Data.mdb";
            //RuntimeContext.UserDb = new AccessMdbHelper(userDbPath);


            // 初始化应用环境
            var runtime = new AppRuntime();

            new LoginRuntime(
                runtime.LocalUserService,
                runtime.UserApiService,
                runtime.MesService,
                runtime.RfidService);

            // 启动 HTTP 接收任务
           //var  _httpReceiver = new HttpTaskReceiver($"http://+:4444/dbtask/", DbTaskQueueInstance.Instance);
           // _httpReceiver.Start();

            // 启动 Api任务

            var services = new WebApiServiceCollection();

            services.Add<ILocalUserService>(runtime.LocalUserService);

            var server = new EmbeddedApiServer(
                services,
                "http://+:4443/");

            server.Start();

            //_apiServer = new EmbeddedApiServer($"http://+:4443/");
            //_apiServer.Start();


            try
            {
                // ✅ 只有简单干脆的一处 using
                using (var loginForm = AuthHelper.CreateLoginForm())
                {
                    // ✅ 只调用一次 ShowDialog
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        AuthHelper.RecordLogin(loginForm.CurrentUser);


                        Application.Run(new MainForm(runtime));
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


            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());
        }
    }
}
