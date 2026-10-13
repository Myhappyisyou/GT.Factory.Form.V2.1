using GT.Factory.LIN.Model;
using GT.Factory.LIN.ProjectDebug;
using GT.Factory.LIN.UI.Controls;
using GT.Factory.LIN.UI.Version;
using GT.Factory.UI.Component;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI
{

    /// <summary>
    /// LIN调试主窗口
    ///
    /// 模拟:
    ///
    /// Vector LIN Analyzer
    /// CANoe LIN窗口
    ///
    ///
    /// 功能:
    ///
    /// 1. Frame浏览
    ///
    /// 2. Trace监控
    ///
    /// 3. Signal查看
    ///
    ///
    /// 数据来源:
    ///
    /// LinDebugSession
    ///
    ///
    /// 注意:
    ///
    /// 本窗体不包含LIN协议逻辑
    ///
    /// 只是调试显示层
    ///
    /// </summary>
    public partial class LinDebugForm : Form
    {
        #region 事件（对外，保持不变）

        public event EventHandler ProjectOpenRequested;
        public event EventHandler ProjectCloseRequested;
        public event EventHandler RuntimeStartRequested;
        public event EventHandler RuntimeStopRequested;

        #endregion

        #region 字段

        /// <summary>
        /// LIN调试会话
        ///
        /// UI和Core之间唯一桥梁
        ///
        /// </summary>
        private LinDebugSession _session;

        /// <summary>
        /// UI刷新定时器
        /// 
        /// 测试阶段使用
        /// 后续替换为DebugRuntime
        /// </summary>
        private Timer _refreshTimer;

        /// <summary>
        /// “通信 → 启动”菜单项。
        ///
        /// 用于根据当前Project和Runtime状态控制菜单是否可用。
        /// </summary>
        private ToolStripMenuItem _startMenuItem;

        /// <summary>
        /// “通信 → 停止”菜单项。
        ///
        /// 用于根据当前Project和Runtime状态控制菜单是否可用。
        /// </summary>
        private ToolStripMenuItem _stopMenuItem;

        private ToolStripMenuItem _openProjectMenu;
        private ToolStripMenuItem _closeProjectMenu;

        // Tab 容器
        private TabControl _tabControl;
        private TabPage _debugTabPage;
        private TabPage _testTabPage;

        // 两个子面板
        private LinDebugMonitorPanel _monitorPanel;
        private LinTestStepPanel _testStepPanel;

        // 统一驱动 Session 绑定的视图集合
        private readonly List<ILinDebugSessionView> _sessionViews =
            new List<ILinDebugSessionView>();


        /// <summary>
        /// LIN调试菜单
        /// </summary>
        private MenuStrip _menuStrip;

        /// <summary>
        /// LIN状态栏
        ///
        /// 底部显示
        ///
        /// </summary>
        private LinStatusBar _statusBar;

        /// <summary>
        /// Runtime 状态
        /// </summary>
        private bool _runtimeRunning;

        #endregion


        #region 构造

        /// <summary>
        /// 创建空的LIN Debug窗口。
        ///
        /// 软件启动时使用。
        ///
        /// 此时可以没有打开任何LIN工程。
        /// </summary>
        public LinDebugForm()
        {
            Initialize();

            StartRefreshTimer();
        }


        /// <summary>
        /// 创建LIN调试窗口
        ///
        /// </summary>
        /// <param name="session">
        /// LIN调试数据会话
        /// </param>
        public LinDebugForm(LinDebugSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));

            Initialize();

            ReplaceSession(session);

            StartRefreshTimer();

        }

        #endregion

        #region 初始化窗体

        /// <summary>
        /// 初始化窗体
        ///
        /// 完全代码创建
        ///
        /// 不使用Designer
        ///
        /// </summary>
        private void Initialize()
        {

            //
            // Form基础属性
            //
            Text = $"LIN Debug Tool {LinUIVersion.Version}";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1400, 900);
            MinimumSize = new Size(900, 700);
            BackColor = UITheme.BgBase;
            Font = UITheme.BodyFont;

            //
            // Tab 容器：调试 / 测试流程
            //
            _tabControl = UITabControl.Create(DockStyle.Fill);

            _debugTabPage = UITabControl.AddTab(_tabControl, "调试");
            _monitorPanel = new LinDebugMonitorPanel();
            _debugTabPage.Controls.Add(_monitorPanel);

            _testTabPage = UITabControl.AddTab(_tabControl, "测试流程");
            _testStepPanel = new LinTestStepPanel();
            _testTabPage.Controls.Add(_testStepPanel);

            // 统一登记，后续 Bind/Unbind/Clear 只需要遍历这个列表
            _sessionViews.Add(_monitorPanel);
            _sessionViews.Add(_testStepPanel);

            //
            // 状态栏
            //
            _statusBar = new LinStatusBar();

            //
            // 组装 Form：菜单在上，Tab 填满，状态栏在下
            //
            Controls.Add(_tabControl);
            Controls.Add(_statusBar);

            InitializeMenu();

            //=======================================



            //
            // 注意:
            //
            // SplitterDistance不能在初始化阶段设置
            //
            // 因为此时SplitContainer还没有实际尺寸。
            //
            // 放到Load事件中设置。
            //
            //this.Load +=
            //    (s, e) =>
            //    {

            //        if (topSplit.Width >
            //            topSplit.Panel1MinSize +
            //            topSplit.Panel2MinSize)
            //        {
            //            topSplit.SplitterDistance =
            //                280;
            //        }
            //    };

      

        }


        #endregion

        #region 菜单栏


        #region 菜单事件

        /// <summary>
        /// 新建工程
        /// </summary>
        private void Menu_NewProject(object sender,EventArgs e)
        {
            MessageBox.Show("新建工程功能后续实现。","LIN工程");
        }


        /// <summary>
        /// 打开工程。
        ///
        /// Form只提出请求，
        /// 实际Project生命周期由LinDebugHost处理。
        /// </summary>
        private void Menu_OpenProject(object sender,EventArgs e)
        {
            var handler =ProjectOpenRequested;

            if (handler != null)
            {
                handler(this,EventArgs.Empty);
            }
        }

        /// <summary>
        /// 关闭工程。
        ///
        private void Menu_CloseProject(object sender,EventArgs e)
        {
            var handler =ProjectCloseRequested;

            if (handler != null)
            {
                handler(this,EventArgs.Empty);
            }
        }

        /// <summary>
        /// 保存工程
        /// </summary>
        private void Menu_SaveProject(object sender,EventArgs e)
        {
            MessageBox.Show("保存工程功能后续实现。","LIN工程");
        }


        /// <summary>
        /// 工程另存为
        /// </summary>
        private void Menu_SaveProjectAs(object sender,EventArgs e)
        {
            MessageBox.Show("工程另存为功能后续实现。","LIN工程");
        }


        /// <summary>
        /// 退出
        /// </summary>
        private void Menu_Exit(object sender,EventArgs e)
        {
            Close();
        }


        /// <summary>
        /// 启动LIN
        /// </summary>
        private void Menu_Start(object sender,EventArgs e)
        {
            var handler =RuntimeStartRequested;

            if (handler != null)
            {
                handler(this,EventArgs.Empty);
            }
        }


        /// <summary>
        /// 停止LIN
        /// </summary>
        private void Menu_Stop(object sender,EventArgs e)
        {
            var handler =RuntimeStopRequested;

            if (handler != null)
            {
                handler(this,EventArgs.Empty);
            }
        }


        /// <summary>
        /// 关于
        /// </summary>
        private void Menu_About(object sender,EventArgs e)
        {
            MessageBox.Show($"LIN Debug Tool\r\n" +$"Version: {LinUIVersion.Version}","关于");
        }

        #endregion

        /// <summary>
        /// 初始化LIN调试菜单。
        ///
        /// 菜单只负责UI入口。
        ///
        /// 当前阶段:
        ///
        /// 工程
        /// 通信
        /// 视图
        /// 帮助
        ///
        /// 具体业务逻辑后续接入。
        /// </summary>
        private void InitializeMenu()
        {
            //
            // 创建统一菜单
            //
            _menuStrip =UIMenu.Create(DockStyle.Top);

            //
            // 工程
            //
            var projectMenu =UIMenu.AddMenu(_menuStrip,"工程");

            UIMenu.AddItem(projectMenu,"新建工程...",Menu_NewProject);

            _openProjectMenu =UIMenu.AddItem(projectMenu,"打开工程...",Menu_OpenProject);

            _closeProjectMenu =UIMenu.AddItem(projectMenu,"关闭工程",Menu_CloseProject);

            UIMenu.AddItem(projectMenu,"保存工程",Menu_SaveProject);

            UIMenu.AddItem(projectMenu,"另存为...",Menu_SaveProjectAs);

            UIMenu.AddSeparator(projectMenu);

            UIMenu.AddItem(projectMenu,"退出",Menu_Exit);

            //
            // 通信
            //
            var communicationMenu =UIMenu.AddMenu(_menuStrip,"通信");

            //
            // 创建“通信 → 启动”菜单项。
            //
            // 保存菜单项引用，
            // 后续根据Runtime状态控制Enabled。
            //
            _startMenuItem =UIMenu.AddItem(communicationMenu,"启动",Menu_Start);

            //
            // 创建“通信 → 停止”菜单项。
            //
            // 保存菜单项引用，
            // 后续根据Runtime状态控制Enabled。
            //
            _stopMenuItem =UIMenu.AddItem(communicationMenu,"停止",Menu_Stop);

            //
            // 视图
            //
            var viewMenu =UIMenu.AddMenu(_menuStrip,"视图");

            UIMenu.AddItem(viewMenu,"Frame");

            UIMenu.AddItem(viewMenu,"Signal");

            UIMenu.AddItem(viewMenu,"Trace");

            //
            // 帮助
            //
            var helpMenu =UIMenu.AddMenu(_menuStrip,"帮助");

            UIMenu.AddItem(helpMenu,"关于",Menu_About);

            //
            // 加入Form
            //
            Controls.Add(_menuStrip);

            _menuStrip.Dock = DockStyle.Top;
            MainMenuStrip = _menuStrip;

        }

        #endregion


        #region 数据绑定

        /// <summary>
        /// 将当前Debug窗口切换到新的Session。
        ///
        /// Project生命周期由LinDebugHost管理，
        /// Form只负责重新绑定UI数据。
        /// </summary>
        public void ReplaceSession(LinDebugSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            foreach (var view in _sessionViews)
            {
                view.UnbindSession(_session);
            }
            
            //
            // 切换Session。
            //
            _session =session;

            //
            // 重新绑定UI。
            //
            foreach (var view in _sessionViews)
            {
                view.BindSession(_session);
            }

            //
            // 新工程刚打开时一定是停止状态。
            //
            _runtimeRunning =false;

            UpdateRuntimeMenuState();

            //
            // 立即刷新状态栏。
            //
            RefreshData();
        }

        /// <summary>
        /// 清除当前Project Debug数据。
        ///
        /// 用于关闭工程。
        /// </summary>
        public void ClearSession()
        {
            foreach (var view in _sessionViews)
            {
                view.UnbindSession(_session);
                view.ClearSession();
            }

            _session =null;

            //
            // 更新运行状态。
            //
            SetRuntimeRunning(false);

            //
            // 刷新UI。
            //
            RefreshData();
        }

        #endregion




        #region 窗体刷新


        /// <summary>
        /// 创建UI刷新Timer
        /// 
        /// 测试:
        /// 
        /// Trace刷新
        /// Signal刷新
        /// </summary>
        private void StartRefreshTimer()
        {
            _refreshTimer =new Timer();

            //
            // 100ms刷新一次
            //
            _refreshTimer = new Timer { Interval = 100 };
            _refreshTimer.Tick += (s, e) => RefreshData();
            _refreshTimer.Start();
        }


        /// <summary>
        /// 刷新调试数据显示
        ///
        /// 调用来源:
        ///
        /// 1. Timer
        ///
        /// 2. 外部手动调用
        ///
        ///
        /// 不负责:
        ///
        /// LIN通信
        ///
        /// Frame计算
        ///
        /// </summary>
        public void RefreshData()
        {

            if (_session == null) return;
            //
            // 状态刷新
            //
            _statusBar.UpdateStatus(_session);
        }

        #endregion


        #region 生命周期

        /// <summary>
        /// 窗体关闭
        ///
        /// 释放Timer
        ///
        /// 防止资源泄漏
        ///
        /// </summary>
        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {

            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
            _refreshTimer = null;
            base.OnFormClosed(e);
        }


        #endregion

        public void SetRuntimeRunning(bool running)
        {
            _runtimeRunning = running;
            UpdateRuntimeMenuState();
        }

        /// <summary>
        /// 根据当前Project和Runtime状态更新通信菜单。
        ///
        /// 状态规则：
        ///
        /// 1. 没有Project：
        ///    启动 = 禁用
        ///    停止 = 禁用
        ///
        /// 2. Project已加载但Runtime未运行：
        ///    启动 = 可用
        ///    停止 = 禁用
        ///
        /// 3. Project已加载且Runtime正在运行：
        ///    启动 = 禁用
        ///    停止 = 可用
        /// </summary>
        private void UpdateRuntimeMenuState()
        {
            bool hasProject = _session != null;

            if (_startMenuItem != null) _startMenuItem.Enabled = hasProject && !_runtimeRunning;
            if (_stopMenuItem != null) _stopMenuItem.Enabled = hasProject && _runtimeRunning;
        }
    }

}
