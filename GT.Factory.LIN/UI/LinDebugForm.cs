using GT.Factory.LIN.Model;
using GT.Factory.LIN.ProjectDebug;
using GT.Factory.LIN.UI.Controls;
using GT.Factory.LIN.UI.Version;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
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

        #region 字段

        /// <summary>
        /// LIN调试会话
        ///
        /// UI和Core之间唯一桥梁
        ///
        /// </summary>
        private readonly LinDebugSession _session;

        /// <summary>
        /// UI刷新定时器
        /// 
        /// 测试阶段使用
        /// 后续替换为DebugRuntime
        /// </summary>
        private Timer _refreshTimer;

        /// <summary>
        /// LIN调试菜单
        /// </summary>
        private MenuStrip _menuStrip;

        /// <summary>
        /// Frame树
        ///
        /// 左侧显示
        ///
        /// </summary>
        private LinFrameTreeView _frameTree;

        /// <summary>
        /// Trace列表
        ///
        /// 中间显示TX/RX
        ///
        /// </summary>
        private LinTraceGrid _traceGrid;


        /// <summary>
        /// Frame Data显示
        /// </summary>
        private LinFrameDataPanel _frameDataPanel;


        /// <summary>
        /// Signal面板
        ///
        /// 下方显示Signal
        ///
        /// </summary>
        private LinSignalPanel _signalPanel;


        /// <summary>
        /// LIN状态栏
        ///
        /// 底部显示
        ///
        /// </summary>
        private LinStatusBar _statusBar;

        #endregion

        #region 构造

        /// <summary>
        /// 创建LIN调试窗口
        ///
        /// </summary>
        /// <param name="session">
        /// LIN调试数据会话
        /// </param>
        public LinDebugForm(
            LinDebugSession session)
        {

            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session));
            }

            _session =
                session;

            Initialize();

            BindData();

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
            this.Text =
                $"LIN Debug Tool {LinUIVersion.Version}";

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.Size =
                new Size(
                    1400,
                    900);

            this.MinimumSize =
                new Size(
                    900,
                    700);

            this.BackColor =
                UITheme.BgBase;

            this.Font =
                UITheme.BodyFont;

         

            //
            // 主布局
            //
            var main =
                new TableLayoutPanel();

            main.Dock =
                DockStyle.Fill;

            main.RowCount =
                3;

            main.ColumnCount =
                1;

            //
            // 上方区域
            //
            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    40));

            //
            // 下方Signal
            //
            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    30));

            //
            // 下方Frame
            //
            main.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    30));

            //
            // 创建上部Split
            //
            var topSplit =
                new SplitContainer();



            topSplit.Dock =
                DockStyle.Fill;

            //
            // 注意:
            //
            // SplitterDistance不能在初始化阶段设置
            //
            // 因为此时SplitContainer还没有实际尺寸。
            //
            // 放到Load事件中设置。
            //
            this.Load +=
                (s, e) =>
                {

                    if (topSplit.Width >
                        topSplit.Panel1MinSize +
                        topSplit.Panel2MinSize)
                    {

                        topSplit.SplitterDistance =
                            280;

                    }

                };


            //
            // Frame Tree
            //
            _frameTree =
                new LinFrameTreeView();



            topSplit.Panel1.Controls.Add(
                _frameTree);


            //
            // Trace
            //
            _traceGrid =
                new LinTraceGrid();



            topSplit.Panel2.Controls.Add(
                _traceGrid);

            
            //
            // 加入顶部
            //
            main.Controls.Add(
                topSplit,
                0,
                0);


            //
            // Signal区域
            //
            _signalPanel =
                new LinSignalPanel();



            main.Controls.Add(
                _signalPanel,
                0,
                1);

            //
            // FrameData
            //
            _frameDataPanel =
                new LinFrameDataPanel();


            main.Controls.Add(
                _frameDataPanel,
                0,
                2);


            //
            // 创建状态栏
            //
            _statusBar =
                new LinStatusBar();


            //
            // 添加主界面
            //
            this.Controls.Add(
                main);



            //
            // 添加底部状态栏
            //
            this.Controls.Add(
                _statusBar);

            //
            // 创建菜单
            //
            InitializeMenu();
        }


        #endregion

        #region 菜单栏


        #region 菜单事件

        /// <summary>
        /// 新建工程
        /// </summary>
        private void Menu_NewProject(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "新建工程功能后续实现。",
                "LIN工程");
        }


        /// <summary>
        /// 打开工程
        /// </summary>
        private void Menu_OpenProject(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "打开工程功能后续实现。",
                "LIN工程");
        }


        /// <summary>
        /// 保存工程
        /// </summary>
        private void Menu_SaveProject(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "保存工程功能后续实现。",
                "LIN工程");
        }


        /// <summary>
        /// 工程另存为
        /// </summary>
        private void Menu_SaveProjectAs(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "工程另存为功能后续实现。",
                "LIN工程");
        }


        /// <summary>
        /// 退出
        /// </summary>
        private void Menu_Exit(
            object sender,
            EventArgs e)
        {
            Close();
        }


        /// <summary>
        /// 启动LIN
        /// </summary>
        private void Menu_Start(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "LIN启动功能后续接入。",
                "LIN通信");
        }


        /// <summary>
        /// 停止LIN
        /// </summary>
        private void Menu_Stop(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "LIN停止功能后续接入。",
                "LIN通信");
        }


        /// <summary>
        /// 关于
        /// </summary>
        private void Menu_About(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                $"LIN Debug Tool\r\n" +
                $"Version: {LinUIVersion.Version}",
                "关于");
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
            _menuStrip =
                UIMenu.Create(DockStyle.Top);


            //
            // 工程
            //
            var projectMenu =
                UIMenu.AddMenu(
                    _menuStrip,
                    "工程");


            UIMenu.AddItem(
                projectMenu,
                "新建工程...",
                Menu_NewProject);


            UIMenu.AddItem(
                projectMenu,
                "打开工程...",
                Menu_OpenProject);


            UIMenu.AddItem(
                projectMenu,
                "保存工程",
                Menu_SaveProject);


            UIMenu.AddItem(
                projectMenu,
                "另存为...",
                Menu_SaveProjectAs);


            UIMenu.AddSeparator(
                projectMenu);


            UIMenu.AddItem(
                projectMenu,
                "退出",
                Menu_Exit);


            //
            // 通信
            //
            var communicationMenu =
                UIMenu.AddMenu(
                    _menuStrip,
                    "通信");


            UIMenu.AddItem(
                communicationMenu,
                "启动",
                Menu_Start);


            UIMenu.AddItem(
                communicationMenu,
                "停止",
                Menu_Stop);


            //
            // 视图
            //
            var viewMenu =
                UIMenu.AddMenu(
                    _menuStrip,
                    "视图");


            UIMenu.AddItem(
                viewMenu,
                "Frame");


            UIMenu.AddItem(
                viewMenu,
                "Signal");


            UIMenu.AddItem(
                viewMenu,
                "Trace");


            //
            // 帮助
            //
            var helpMenu =
                UIMenu.AddMenu(
                    _menuStrip,
                    "帮助");


            UIMenu.AddItem(
                helpMenu,
                "关于",
                Menu_About);


            //
            // 加入Form
            //
            Controls.Add(
                _menuStrip);

            _menuStrip.Dock = DockStyle.Top;
            MainMenuStrip =
                _menuStrip;

        }

        #endregion


        #region 数据绑定

        /// <summary>
        /// 绑定Session数据
        ///
        /// </summary>
        private void BindData()
        {

            //
            // Frame
            //
            _frameTree.LoadFrames(
                _session.Frames);


            //
            // Signal
            //
            _signalPanel.LoadSignals(
                _session.Signals);


            //
            // FrameData
            //
            _frameDataPanel.LoadFrames(
                _session.Frames);


            //
            // Frame选择事件
            //
            _frameTree.FrameSelected +=
                FrameTree_FrameSelected;


            //
            // Trace
            //
            _traceGrid.UpdateTrace(
                _session.Trace.Items);


            //
            // Trace实时事件
            //
            // LIN收到一帧
            //
            // 立即刷新UI
            //
            _session.TraceAdded +=
                Session_TraceAdded;


            //
            // Signal修改事件
            //
            _signalPanel.SignalValueChanged +=
                SignalPanel_SignalValueChanged;


            //
            // Frame数据变化
            //
            _session.FrameChanged +=
                Session_FrameChanged;
        }


        #endregion


        #region Frame选择


        /// <summary>
        /// Frame选择
        ///
        /// Tree选择Frame后
        ///
        /// 更新Signal显示
        ///
        /// </summary>
        private void FrameTree_FrameSelected(
            LinFrameInfo frame)
        {

            if (frame == null)
                return;

            //
            // 显示当前Frame Signal
            //
            _signalPanel.ShowFrame(frame);

        }


        #endregion


        #region Signal修改


        /// <summary>
        /// UI修改Signal
        ///
        /// 转交Session
        ///
        /// </summary>
        private void SignalPanel_SignalValueChanged(
            string name,
            object value)
        {

            _session.SetSignalValue(
                name,
                value);

        }


        #endregion


        #region Trace新增


        /// <summary>
        /// Trace新增
        ///
        /// 来源:
        ///
        /// LinDebugSession
        ///
        /// </summary>
        private void Session_TraceAdded(
            LinTraceItem item)
        {

            _traceGrid.AddTrace(
                item);

        }


        private void Session_FrameChanged(
            LinFrameInfo frame)
        {

            _frameDataPanel.UpdateFrame(
                frame);

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

            _refreshTimer =
                new Timer();


            //
            // 100ms刷新一次
            //
            _refreshTimer.Interval =
                100;


            _refreshTimer.Tick +=
                (s, e) =>
                {

                    RefreshData();

                };


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

            //
            // Trace刷新
            //
            // 数据来源:
            //
            // LinTraceLogger.Items
            //
            //_traceGrid.UpdateTrace(
            //    _session.Trace.Items);

            //
            // Signal刷新
            //
            // 请求Core更新Signal
            //
            //_session.RefreshSignals();

            //_session.RefreshFrames();

            //
            // 更新Signal显示
            //
            //_signalPanel.RefreshValues();

            //
            // 更新FrameData
            //
            //_frameDataPanel.RefreshData();


            //
            // 状态刷新
            //
            _statusBar.UpdateStatus(
                _session);
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

            //
            // 停止Timer
            //
            if (_refreshTimer != null)
            {

                _refreshTimer.Stop();


                _refreshTimer.Dispose();


                _refreshTimer =
                    null;

            }

            base.OnFormClosed(
                e);

        }


        #endregion

    }

}
