using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
using GT.Factory.GTUser.Repository;
using GT.Factory.Logging.Logging;
using GT.Factory.RFID.Aynettek;
using GT.Factory.Server.Component.ModelConfig_UC;
using GT.Factory.Server.Core;
using GT.Factory.Server.Core.ClientInfor;
using GT.Factory.Server.Core.LanModelSync;
using GT.Factory.Server.Help;
using GT.Factory.Server.Help.Mssql;
using GT.Factory.Server.Queue;
using GT.Factory.Server.Services;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.WebApi.QueryServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Button = System.Windows.Forms.Button;

namespace GT.Factory.Server
{
    public partial class Form1 : Form
    {
        private static readonly Color NavNormalColor = Color.FromArgb(24, 30, 54);
        private static readonly Color NavActiveColor = Color.FromArgb(46, 51, 73);

        // 切工单工序
        private DeviceSyncService _syncService;
        private List<ProcessClient> lsProcessClients;

        private PluginManager _pluginManager;
        private TaskQueueService _taskService;
        private HttpTaskReceiver _httpReceiver;

        private EmbeddedApiServer _apiServer;

        private AccessMdbHelper db;

        string dbPath = @"D:\公共参数\GTDatabase.mdb"; // 目标电脑 IP 和共享名

        private Control _currentControl;

        private UC_ChangeOrder _ucChangeOrder;
        private UC_UserEdit uc_useredit;
        private UC_ModelConfig uC_ModelConfig;

        public Form1()
        {
            InitializeComponent();

            // 或者设置在屏幕中心
            this.StartPosition = FormStartPosition.CenterScreen;

            db = new AccessMdbHelper(dbPath);

            lsProcessClients = UploadSql.SelectProcessClients(db);

            DbShiftManager.EnsureShiftDb();

            // ✅ 提前启动监听和心跳
            _syncService = new DeviceSyncService(lsProcessClients.Select(t=>t.ClientIp).ToArray());
            _syncService.Start();

            ActivateNav(btnDashBoard);
            //  日志初始化
            DisplayLog.Init(
                    new SerilogWriter(),
                    new RichTextBoxLogDisplay(rtboxLog)
                    );

            ShowChangeOrder();
        }

        /// <summary>
        /// 显示日志信息到 UI 和日志文件
        /// </summary>
        /// <param name="source">信息来源，例如用户/控件名</param>
        /// <param name="message">日志内容</param>
        private void LogInfo(string source, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogInfo(source, message)));
                return;
            }

            string text = $"[{source}] {message}";
            DisplayLog.Info(text,true);
        }

        private void ActivateNav(Button activeBtn)
        {
            // 移动导航条
            pnlNav.Top = activeBtn.Top;
            pnlNav.Left = activeBtn.Left;
            pnlNav.Height = activeBtn.Height;

            // 重置所有按钮状态
            foreach (Control c in panel1.Controls)
            {
                Button b = c as Button;
                if (b != null && b.Tag?.ToString() == "Nav")
                    b.BackColor = NavNormalColor;
            }

            // 激活当前按钮
            activeBtn.BackColor = NavActiveColor;
        }

        /// <summary>
        /// 添加控件到面板
        /// </summary>
        /// <param name="c"></param>
        private void AddControlsToPanel(Control c)
        {
            if (_currentControl == c)
                return;

            // 隐藏上一个控件生命周期
            if (_currentControl is IPageLifecycle prevPage)
            {
                prevPage.OnPageHidden();

                LogInfo("页面切换", $"{_currentControl.Name} 隐藏");
            }

            pnlControls.SuspendLayout();
            pnlControls.Controls.Clear();
            c.Dock = DockStyle.Fill;
            pnlControls.Controls.Add(c);
            pnlControls.ResumeLayout();

            // 新控件生命周期
            if (c is IPageLifecycle page)
            {
                page.OnPageShown();

                LogInfo("页面切换", $"{c.Name} 显示");
            }

            _currentControl = c;
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            ActivateNav(btn);

            LogInfo("导航栏", $"{btn.Text} 被点击");

            switch (btn.Name)
            {
                case nameof(btnDashBoard):
                    ShowChangeOrder();
                    break;
                case nameof(btnParameter):
                    ShowParameter();
                    break;
                case nameof(btnUserEdit):
                    ShowUserEdit();
                    break;
                    break;
            }
        }

        private void ShowChangeOrder()
        {
            if (_ucChangeOrder == null)
                _ucChangeOrder = new UC_ChangeOrder(_syncService);

            AddControlsToPanel(_ucChangeOrder);

            DisplayLog.Info("进入选型界面",true);
        }

        private void ShowParameter()
        {
            if (uC_ModelConfig == null)
                uC_ModelConfig = new UC_ModelConfig();

            AddControlsToPanel(uC_ModelConfig);
        }

        private void ShowUserEdit()
        {
            if (uc_useredit == null)
                uc_useredit = new UC_UserEdit();

            AddControlsToPanel(uc_useredit);
        }



        /// <summary>
        /// 退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExit_Leave(object sender, EventArgs e)
        {
            btnExit.BackColor = Color.FromArgb(24, 30, 54);
        }

        private void btnMinimized_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnMaximized_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //string dbPath = @"D:\生产数据\Template.mdb"; // 目标电脑 IP 和共享名

            //AccessMdbHelper dbData = new AccessMdbHelper(dbPath);

            //string date = UploadSql.GetNetworkTime(dbData);

            //string path = MonthlyAccessDbManager.GetCurrentMonthDbPath(date);

            //dbData.DatabasePath = path;

            InitDatabases();

            DatabaseSessionManager.EnsureDatabase();

            var dbData = DbContext.CurrentDb;

            // 加载插件
            _pluginManager = new PluginManager();

            _pluginManager.LoadPlugins(PathCenter.Plugins);

            // 启动后台 Worker
            _taskService = new TaskQueueService(DbTaskQueueInstance.Instance, _pluginManager, dbData, LogInfo);
            _taskService.Start();

            //// 启动 HTTP 接收任务
            //_httpReceiver = new HttpTaskReceiver($"http://+:4444/dbtask/", DbTaskQueueInstance.Instance);
            //_httpReceiver.Start();

            //// 启动 Api任务

            //var services = new WebApiServiceCollection();

            //services.Add<ILocalUserService>(
            //    localUserService);

            //var server = new EmbeddedApiServer(
            //    services,
            //    "http://+:4443/");

            //server.Start();

            //_apiServer = new EmbeddedApiServer($"http://+:4443/");
            //_apiServer.Start();

            Text = $"Server";

 
        }

        private void InitDatabases()
        {
            string month = DateTime.Now.ToString("yyyy年MM月");

            var result = MonthlyAccessDbManager.GetCurrentMonthDbPath(month);

            db = new AccessMdbHelper(result.dbPath);

            DbContext.Set(db, month);

            DatabaseInitializer.InitMonthlyDatabase(db, result.isNew);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _syncService?.Monitor?.Stop();
            _syncService?.Heartbeat?.Stop();
            _apiServer.Dispose();
        }

    }
}
