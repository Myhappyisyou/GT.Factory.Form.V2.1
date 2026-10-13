using GT.Factory.Common.Helpers;
using GT.Factory.Data.Config;
using GT.Factory.Data.Service;
using GT.Factory.Login;
using GT.Factory.Login.Login;
using GT.Factory.Management.UI.ConfigCenter;
using GT.Factory.Management.UI.GTUser;
using GT.Factory.Management.UI.ProductCode.Forms;
using GT.Factory.MES.BydMes;
using GT.Factory.Server.Core;
using GT.Factory.Server.Core.ClientInfor;
using GT.Factory.Server.Core.LanModelSync;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GT.Factory.Server
{
    /// <summary>
    /// GT.Factory Server 主窗体。
    /// 只负责服务器 UI 框架、一级导航、二级导航和页面切换；
    /// ServerRuntime、数据库、TaskQueue、API 等运行时服务不要放到这里。
    /// </summary>
    public class MainForm : Form
    {
        private readonly Panel _topBar;
        private readonly Panel _subNavigation;
        private readonly Panel _contentPanel;
        private readonly Label _pageTitle;
        private readonly Label _statusLabel;
        private readonly FlowLayoutPanel _moduleBar;
        private readonly FlowLayoutPanel _subMenuBar;

        private readonly Dictionary<string, List<NavigationItem>> _navigation;
        private readonly Dictionary<string, Control> _pageCache;

        private string _currentModule;
        private string _currentPage;

        private readonly AppRuntime Runtime;


        private Control _currentControl;

        private DeviceSyncService _syncService;
        private List<ProcessClient> lsProcessClients;

        /// <summary>
        /// 创建 Server 主界面。
        /// </summary>
        public MainForm(AppRuntime runtime)
        {
            Runtime = runtime;
            _navigation = CreateNavigation();
            _pageCache = new Dictionary<string, Control>();

            Text = "GT.Factory Server";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1100, 700);
            Size = new Size(1440, 900);
            BackColor = UITheme.Background;
            Font = UITheme.FontNormal;

            InitServer();

            ConfigManager.Register(
                LoginConfigProvider.Instance);

            ConfigManager.Register(
                MesConfigProvider.Instance);

            ConfigManager.Register(
                DatabaseConfigProvider.Instance);

            ConfigManager.Register(
                WebApiConfigProvider.Instance);

            _moduleBar = BuildModuleBar();

            _topBar = BuildTopBar();
            _subNavigation = BuildSubNavigation();
            _contentPanel = BuildContentPanel();
            _pageTitle = BuildPageTitle();
            _statusLabel = BuildStatusBar();
            _subMenuBar = BuildSubMenuBar();

            BuildLayout();
            BuildModules();

            ShowModule("生产");
        }

        private void InitServer()
        {
            // ✅ 提前启动监听和心跳
            lsProcessClients= new List<ProcessClient>
            {
                new ProcessClient
                {
                    ID = 1,
                    ClientName = "默认客户端",
                    ClientDetail = "默认描述",
                    ClientIp = "127.0.0.1"
                }
            };

            _syncService = new DeviceSyncService(lsProcessClients.Select(t => t.ClientIp).ToArray());
            _syncService.Start();
        }

        /// <summary>
        /// 建立主窗体整体布局。
        /// TopBar → 左侧二级导航 → 内容区 → 底部状态栏。
        /// </summary>
        private void BuildLayout()
        {
           
            _topBar.Dock = DockStyle.Top;
            _topBar.Height = 120;

            _subNavigation.Dock = DockStyle.Left;
            _subNavigation.Width = 190;

            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Padding = new Padding(18, 12, 18, 12);

            _statusLabel.Dock = DockStyle.Bottom;
            _statusLabel.Height = 28;

            //_topBar.Controls.Add(_moduleBar);

            Controls.Add(_contentPanel);
            Controls.Add(_statusLabel);
            Controls.Add(_subNavigation);

            Controls.Add(_topBar);
        }


        /// <summary>
        /// 创建顶部区域。
        /// 上层：Logo + 用户信息
        /// 下层：一级导航按钮
        /// </summary>
        private Panel BuildTopBar()
        {
            Panel panel = new Panel();
            panel.BackColor = Color.FromArgb(25, 65, 98);
            panel.Dock = DockStyle.Top;
            panel.Height = 64 + 52;   // 上层 64 + 下层 52

            // ============================================================
            // 第一层：Logo + 用户信息
            // ============================================================
            Panel titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 64;
            titleBar.BackColor = Color.FromArgb(25, 65, 98);
            titleBar.Padding = new Padding(18, 0, 12, 0);

            Label logo = UILabel.Title("GT.Factory Server");
            logo.ForeColor = Color.White;
            logo.Font = new Font(UITheme.FontNormal.FontFamily, 14, FontStyle.Bold);
            logo.AutoSize = false;
            logo.Width = 260;
            logo.Height = 64;
            logo.TextAlign = ContentAlignment.MiddleLeft;
            logo.Dock = DockStyle.Left;
            titleBar.Controls.Add(logo);

            Label user = new Label();
            user.Text = "当前用户：管理员    " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            user.ForeColor = Color.White;
            user.Font = UITheme.FontNormal;
            user.AutoSize = false;
            user.Width = 320;
            user.Height = 64;
            user.TextAlign = ContentAlignment.MiddleRight;
            user.Dock = DockStyle.Right;
            titleBar.Controls.Add(user);

            // ============================================================
            // 第二层：一级导航栏
            // ============================================================
            Panel navBar = new Panel();
            navBar.Dock = DockStyle.Fill;
            navBar.BackColor = Color.FromArgb(32, 78, 115);  // 略浅一点，区分两层
            navBar.Padding = new Padding(12, 0, 12, 0);

            _moduleBar.Dock = DockStyle.Fill;
            _moduleBar.FlowDirection = FlowDirection.LeftToRight;
            _moduleBar.WrapContents = false;
            _moduleBar.AutoScroll = false;
            _moduleBar.Padding = new Padding(4, 6, 0, 0);
            _moduleBar.BackColor = Color.Transparent;
            navBar.Controls.Add(_moduleBar);

            // ============================================================
            // 把两层加到顶部容器
            // Dock 规则：先加 Fill，再加 Top，最后 Top 才会在最上面。
            // 这里希望 titleBar 在上、navBar 在下：
            // 先加 navBar（Fill，占剩余），再加 titleBar（Top，占顶部）。
            // ============================================================
            panel.Controls.Add(navBar);      // Fill：占据 titleBar 之外的空间
            panel.Controls.Add(titleBar);    // Top：占据最上方 64px

            return panel;
        }

        /// <summary>
        /// 创建一级模块按钮容器。
        /// </summary>
        private FlowLayoutPanel BuildModuleBar()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = false;
            panel.AutoScroll = false;
            panel.Padding = new Padding(10, 11, 0, 0);
            panel.BackColor = Color.Transparent;
            return panel;
        }

        /// <summary>
        /// 创建左侧二级导航区域。
        /// </summary>
        private Panel BuildSubNavigation()
        {
            Panel panel = new Panel();
            panel.BackColor = Color.FromArgb(25, 65, 98); ;
            panel.Padding = new Padding(10, 12, 10, 10);
            return panel;
        }

        /// <summary>
        /// 创建二级导航按钮容器。
        /// </summary>
        private FlowLayoutPanel BuildSubMenuBar()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.TopDown;
            panel.WrapContents = false;
            panel.AutoScroll = true;
            panel.Padding = new Padding(0, 8, 0, 0);
            panel.BackColor = Color.Transparent;
            return panel;
        }

        /// <summary>
        /// 创建主内容区域。
        /// </summary>
        private Panel BuildContentPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = UITheme.Background;
            return panel;
        }

        /// <summary>
        /// 创建页面标题。
        /// </summary>
        private Label BuildPageTitle()
        {
            Label title = UILabel.Title("生产总览");
            title.Dock = DockStyle.Top;
            title.Height = 42;
            title.TextAlign = ContentAlignment.MiddleLeft;
            return title;
        }

        /// <summary>
        /// 创建底部状态栏。
        /// </summary>
        private Label BuildStatusBar()
        {
            Label status = new Label();
            status.Text = "● Server 运行中    |    API：正常    |    数据库：正常    |    TaskQueue：正常";
            status.BackColor = Color.FromArgb(225, 238, 247);
            status.ForeColor = Color.FromArgb(40, 90, 120);
            status.Font = UITheme.FontNormal;
            status.TextAlign = ContentAlignment.MiddleLeft;
            status.Padding = new Padding(12, 0, 0, 0);
            return status;
        }

        /// <summary>
        /// 注册所有一级模块。
        /// </summary>
        private void BuildModules()
        {
            string[] modules = new string[] { "生产", "设备", "数据", "服务", "系统" };

            foreach (string module in modules)
            {
                Button button = CreateModuleButton(module);
                _moduleBar.Controls.Add(button);
            }
        }

        /// <summary>
        /// 创建一级导航按钮。
        /// </summary>
        private Button CreateModuleButton(string text)
        {
            Button button = UIButton.Default(text);
            button.Width = 92;
            button.Height = 40;
            button.Margin = new Padding(4, 0, 4, 0);
            button.Tag = text;
            button.Click += ModuleButton_Click;
            return button;
        }

        /// <summary>
        /// 创建二级导航按钮。
        /// </summary>
        private Button CreateSubMenuButton(NavigationItem item)
        {
            Button button = UIButton.Ghost(item.Text);
            button.Width = 160;
            button.Height = 38;
            button.Margin = new Padding(0, 3, 0, 3);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 0, 0);
            button.Tag = item;
            button.Click += SubMenuButton_Click;
            return button;
        }

        /// <summary>
        /// 一级导航点击事件。
        /// </summary>
        private void ModuleButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            ShowModule(Convert.ToString(button.Tag));
        }

        /// <summary>
        /// 二级导航点击事件。
        /// </summary>
        private void SubMenuButton_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
                return;

            NavigationItem item = button.Tag as NavigationItem;
            if (item == null)
                return;

            ShowPage(_currentModule, item);
        }

        /// <summary>
        /// 切换一级模块并重建左侧二级菜单。
        /// </summary>
        private void ShowModule(string module)
        {
            if (!_navigation.ContainsKey(module))
                return;

            _currentModule = module;

            _subNavigation.Controls.Clear();
            _subNavigation.Controls.Add(_subMenuBar);

            Label title = UILabel.Title(module);
            title.Dock = DockStyle.Top;
            title.Height = 42;
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.Font = new Font(UITheme.FontNormal.FontFamily, 11, FontStyle.Bold);
            _subNavigation.Controls.Add(title);

            _subMenuBar.Controls.Clear();

            List<NavigationItem> items = _navigation[module];
            foreach (NavigationItem item in items)
            {
                _subMenuBar.Controls.Add(CreateSubMenuButton(item));
            }

            if (items.Count > 0)
                ShowPage(module, items[0]);
        }

        /// <summary>
        /// 添加控件到面板
        /// </summary>
        /// <param name="c"></param>
        //private void AddControlsToPanel(Control c)
        //{
        //    if (_currentControl == c)
        //        return;

        //    // 隐藏上一个控件生命周期
        //    if (_currentControl is IPageLifecycle prevPage)
        //    {
        //        prevPage.OnPageHidden();

        //    }

        //    pnlControls.SuspendLayout();
        //    pnlControls.Controls.Clear();
        //    c.Dock = DockStyle.Fill;
        //    pnlControls.Controls.Add(c);
        //    pnlControls.ResumeLayout();

        //    // 新控件生命周期
        //    if (c is IPageLifecycle page)
        //    {
        //        page.OnPageShown();

        //        LogInfo("页面切换", $"{c.Name} 显示");
        //    }

        //    _currentControl = c;
        //}

        /// <summary>
        /// 显示指定业务页面。
        /// 页面第一次打开时创建并缓存，后续只切换显示。
        /// </summary>
        private void ShowPage(string module, NavigationItem item)
        {

            // 隐藏上一个控件生命周期
            if (_currentControl is IPageLifecycle prevPage)
            {
                prevPage.OnPageHidden();

            }


            string key = module + "." + item.Key;
            Control page;

            if (!_pageCache.TryGetValue(key, out page))
            {
                page = CreatePage(module, item);
                _pageCache.Add(key, page);
            }

            _currentPage = key;
            _pageTitle.Text = item.Text;

            _contentPanel.SuspendLayout();
            _contentPanel.Controls.Clear();
            _contentPanel.Controls.Add(page);
            page.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(_pageTitle);
            _contentPanel.ResumeLayout();



            // 新控件生命周期
            if (page is IPageLifecycle ct)
            {
                ct.OnPageShown();

            }

            _currentControl = page;
        }

        /// <summary>
        /// 根据导航项创建页面。
        /// 当前先接入生产总览，其余页面使用占位页，后续分别实现。
        /// </summary>
        private Control CreatePage(string module, NavigationItem item)
        {
            if (module == "生产" && item.Key == "Dashboard")
                return new ProductionDashboardControl();

            if (module == "生产" && item.Key == "WorkOrder")
                return new UC_ChangeOrder(_syncService);

            if (module == "系统" && item.Key == "ServerInfo")
                return new ConfigCenterControl();

            if (module == "服务" && item.Key == "User")
                return new UserManageControl(LoginRuntime.Current.LocalUserService);

            if (module == "服务" && item.Key == "CodeRule")
                return new ProductCodeRuleManageControl(Runtime.ProductModelService, Runtime.ProductPartRepository, Runtime.ProductCodeRuleService);

            return new PlaceholderPageControl(module, item.Text);
        }


      

        /// <summary>
        /// 创建 Server 导航配置。
        /// </summary>
        private Dictionary<string, List<NavigationItem>> CreateNavigation()
        {
            Dictionary<string, List<NavigationItem>> result =
                new Dictionary<string, List<NavigationItem>>();

            result.Add("生产", new List<NavigationItem>
            {
                new NavigationItem("Dashboard", "总览"),
                new NavigationItem("WorkOrder", "工单 / 型号"),
                new NavigationItem("ProductionRecord", "生产记录")
            });

            result.Add("设备", new List<NavigationItem>
            {
                new NavigationItem("StationStatus", "Station 状态"),
                new NavigationItem("Alarm", "报警")
            });

            result.Add("数据", new List<NavigationItem>
            {
                new NavigationItem("Trace", "产品追溯"),
                new NavigationItem("NgRecord", "NG 记录"),
                new NavigationItem("ProductionRecord", "生产记录")

            });

            result.Add("服务", new List<NavigationItem>
            {
                new NavigationItem("Api", "API"),
                new NavigationItem("TaskQueue", "TaskQueue"),
                new NavigationItem("Database", "数据库"),
                new NavigationItem("User", "用户"),
                new NavigationItem("CodeRule", "编码规则")
            });

            result.Add("系统", new List<NavigationItem>
            {
                new NavigationItem("Log", "日志"),
                new NavigationItem("ServerInfo", "Server 信息")
            });

            return result;
        }

        /// <summary>
        /// 一级/二级导航数据对象。
        /// </summary>
        private class NavigationItem
        {
            public string Key { get; private set; }
            public string Text { get; private set; }

            public NavigationItem(string key, string text)
            {
                Key = key;
                Text = text;
            }
        }

        /// <summary>
        /// 当前尚未接入实际业务实现的页面占位控件。
        /// </summary>
        private class PlaceholderPageControl : Panel
        {
            public PlaceholderPageControl(string module, string pageName)
            {
                BackColor = UITheme.Background;

                Label label = UILabel.Title(module + " / " + pageName);
                label.AutoSize = false;
                label.Dock = DockStyle.Fill;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Font = new Font(UITheme.FontNormal.FontFamily, 16, FontStyle.Bold);
                label.ForeColor = UITheme.Text;

                Controls.Add(label);
            }
        }
    }
}
