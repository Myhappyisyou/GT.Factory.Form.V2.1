using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EditJson;
using GT_Common.DriverForm.SearchForm;
using GT_Common.Helper;
using GT_Common.MyEnum;
using GT_Common.Components;
using GT_Common.DriverForm.Alarm;
using GT_Common.DriverForm.Aynettek;
using GT_Common.DriverForm.CacheViewer;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.Helper.LanModelSync;
using RecipeParameter;
using IndicatorStatus = GT_Common.MyEnum.IndicatorStatus;
using GT_Common.DriverForm.Batch;
using GT_Common.DriverForm.ConfigCenter;
using GT_Common.DriverForm.ConsumableEdit;
using GT_Common.DriverForm.Order;
using GT_Common.DriverForm.ProductCode;
using GT_Common.DriverForm.Recipe;
using GT_Common.DriverForm.SpecialForm;
using GT_Common.DriverForm.UserEdit;
using GT_Common.Helper.Mssql;
using GT_Common.Helper.UIHelp;
using System.Collections.Generic;
using System.IO;
using GT_Common.DriverForm.ModelSelectForm;
using GT_Common.Helper.Logging;

namespace GT_Common
{
    public partial class MainForm : Form
    {
        private JsonService _jsonService = new JsonService();

        private DeviceStatus currentStatus;
        private DeviceStatusReporter reporter;
        private ModelConfigReceiver receiver;
        private MasterHeartbeatMonitor heartbeatMonitor;

        DataProvider _provider = new DataProvider();
        private TitleUI titleUI;
        private CurrentProductDataUI currentProductDataUI;

        private DataUI dataUI;
        private WorkStatusUI workStatusUI;
        private OperationalStatusUI operationalStatusUI;
        private ActionTipsUI actionTipsUI;
        private ProductStatusUI productStatusUI;
        private ProductionInformationUI productionInformationUI;

        ControlEngine controlEngine;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private bool isRealExit = false;

        #region 窗口

        //  读卡器窗口
        AynettekForm aynettekForm;

        //  查询数据窗口
        SearchForm searchForm;
        SearchCheckForm searchCheckForm;

        //  PLC配置窗口
        EditJsonForm editJsonForm;

        //  参数配置查询窗口
        RecipeForm recipeForm;
        SearchRecipeForm searchRecipeForm;

        //  报警配置窗口
        AlarmForm AlarmForm;

        //  缓存查询窗口
        CacheViewerForm CacheViewerForm;

        ConsumableEditForm ConsumableEditForm;

        //  物料配置扫码窗口
        BatchConfigForm BatchConfigForm;
        BatchScanForm BatchScanForm;
        BatchRuntimeForm BatchRuntimeForm;

        //  特殊窗口
        SpecialForm SpecialForm;

        RecipeChangeLogForm RecipeChangeLogForm;

        //  系统配置
        ConfigCenterForm ConfigCenterForm;

        //  产品码配置
        ProductCodeConfigForm ProductCodeConfigForm;

        //  BydMes
        BydMesForm OrderForm;

        // 用户管理
        UserManageForm userManageForm;

        #endregion

        //  窗体关闭事件
        public static event Action OnAppClosing;

        public MainForm()
        {
            this.Text = "MES_OP010";
            this.WindowState = FormWindowState.Maximized;
            this.MaximizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.Size = new Size(1280, 840);
            // ===== 托盘菜单 =====
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("显示主界面", null, ShowMainForm);
            trayMenu.Items.Add("退出程序", null, ExitApp);

            // ===== 托盘图标 =====
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Text = "我的程序",
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => ShowMainForm(s, e);
            // ✅ 必须加这一行
            this.FormClosing += MainForm_FormClosing;
            //  初始化布局
            InitFormLayout();

            //  初始化菜单
            IniteMenu();

            //  初始化型号
            InitStatus();

            Config.Save();

            LocalConfig.Save();

            DbShiftManager.EnsureShiftDb();

            reporter = new DeviceStatusReporter(Config.Instance.ServerIP, 8881, () => currentStatus);
            reporter.Start();

            receiver = new ModelConfigReceiver(5555);
            receiver.OnModelReceived += ApplyModel;
            receiver.Start();

            heartbeatMonitor = new MasterHeartbeatMonitor(Config.Instance.ServerIP, 6666); // 控制端发心跳端口
            heartbeatMonitor.OnConnectionStateChanged += serverState =>
            {
                this.SafeInvoke(() =>
                {
                    Shared.serverSatus = serverState ? IndicatorStatus.Normal : IndicatorStatus.Error;
                    _provider.CurrentState = serverState;
                });
            };
            heartbeatMonitor.Start();

            this.Load += MainForm_Load;

            // ✅ 启动全局刷新
            UiRefreshCenter.Start();
        }

        //  初始化布局
        public void InitFormLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
            };
            // ===== 左侧 数据区域 =====
            mainLayout.ColumnStyles.Clear(); // 清除
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));   // Left

            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
            };

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Header

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));   // Status

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));   // Data grid

            // ===== 顶部 Header =====

            titleUI = new TitleUI(Config.Instance.ProcessName, Config.Instance.Vison) { Dock = DockStyle.Fill };

            leftLayout.Controls.Add(titleUI, 0, 0);

            // ===== 中部 状态区域 =====

            currentProductDataUI = new CurrentProductDataUI { Dock = DockStyle.Fill };

            leftLayout.Controls.Add(currentProductDataUI, 0, 1);

            // ===== 底部 数据区域 =====

            dataUI = new DataUI(Config.Instance.ProcessName) { Dock = DockStyle.Fill };

            leftLayout.Controls.Add(dataUI, 0, 2);

            mainLayout.Controls.Add(leftLayout, 0, 0);

            // ===== 右部 数据区域 =====

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));   // Status

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));   // Header

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 27));   // Status

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Data grid

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));   // Data grid

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));   // Data grid

            workStatusUI = new WorkStatusUI { Dock = DockStyle.Fill };
            operationalStatusUI = new OperationalStatusUI { Dock = DockStyle.Fill };
            actionTipsUI = new ActionTipsUI { Dock = DockStyle.Fill };
            productStatusUI = new ProductStatusUI { Dock = DockStyle.Fill };
            productionInformationUI = new ProductionInformationUI { Dock = DockStyle.Fill };

            rightLayout.Controls.Add(workStatusUI, 0, 0);
            rightLayout.Controls.Add(operationalStatusUI, 0, 1);
            rightLayout.Controls.Add(actionTipsUI, 0, 2);
            rightLayout.Controls.Add(productStatusUI, 0, 3);
            rightLayout.Controls.Add(productionInformationUI, 0, 4);

            mainLayout.Controls.Add(rightLayout, 1, 0);

            this.Controls.Add(mainLayout);
        }

        //  初始化菜单
        public void IniteMenu()
        {
            #region 查询

            ToolStripMenuItem toolMenuItem = new ToolStripMenuItem();
            toolMenuItem.Text = "查询";

            ToolStripMenuItem searchMenuItem = new ToolStripMenuItem();
            searchMenuItem.Text = "生产数据查询";
            searchMenuItem.Click += tsm_SearchFormClicked;
            toolMenuItem.DropDownItems.Add(searchMenuItem);

            ToolStripMenuItem CacheViewerMenuItem = new ToolStripMenuItem();
            CacheViewerMenuItem.Text = "缓存查看";
            CacheViewerMenuItem.Click += tsm_CacheViewerClicked;
            toolMenuItem.DropDownItems.Add(CacheViewerMenuItem);

            #endregion

            #region 配置

            ToolStripMenuItem toolMenuItemConfig = new ToolStripMenuItem();
            toolMenuItemConfig.Text = "配置";

            ToolStripMenuItem JsonMenuItem = new ToolStripMenuItem();
            JsonMenuItem.Text = "PLC配置";
            JsonMenuItem.Click += tsm_JsonFormClicked;
            toolMenuItemConfig.DropDownItems.Add(JsonMenuItem);

            ToolStripMenuItem RecipeMenuItem = new ToolStripMenuItem();
            RecipeMenuItem.Text = "参数配置";
            RecipeMenuItem.Click += tsm_RecipeFormClicked;
            toolMenuItemConfig.DropDownItems.Add(RecipeMenuItem);

            ToolStripMenuItem AlarmMenuItem = new ToolStripMenuItem();
            AlarmMenuItem.Text = "报警配置";
            AlarmMenuItem.Click += tsm_AlarmFormClicked;
            toolMenuItemConfig.DropDownItems.Add(AlarmMenuItem);


            #endregion

            #region 仪表

            ToolStripMenuItem toolMenuDriver = new ToolStripMenuItem();
            toolMenuDriver.Text = "仪表";

            ToolStripMenuItem AynettekMenuItem = new ToolStripMenuItem();
            AynettekMenuItem.Text = "UID";
            AynettekMenuItem.Click += tsm_AynettekFormClicked;
            toolMenuDriver.DropDownItems.Add(AynettekMenuItem);

            #endregion

            #region 用户变更

            ToolStripMenuItem UserMenuItem = new ToolStripMenuItem();
            UserMenuItem.Text = "用户";

            ToolStripMenuItem ChangeUserMenuItem = new ToolStripMenuItem();
            ChangeUserMenuItem.Text = "切换用户";
            ChangeUserMenuItem.Click += tsm_UserFormClicked;
            UserMenuItem.DropDownItems.Add(ChangeUserMenuItem);
            //ToolStripMenuItem CancelUserMenuItem = new ToolStripMenuItem();
            //CancelUserMenuItem.Text = "注销用户";
            //CancelUserMenuItem.Click += tsm_CancelUserFormClicked;

            #endregion

            MenuStrip menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(225, 227, 228);

            menuStrip.Items.Add(toolMenuItem);
            menuStrip.Items.Add(toolMenuItemConfig);
            menuStrip.Items.Add(toolMenuDriver);
            menuStrip.Items.Add(UserMenuItem);
            //menuStrip.Items.Add(CancelUserMenuItem);
            menuStrip.Dock = DockStyle.Top;
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        //  初始化型号
        private void InitStatus()
        {
            string lastModel = LoadLastUsedModel();

            currentStatus = new DeviceStatus
            {
                Status = "Ready",
                ModelName = lastModel ?? Shared.productName
            };

            Shared.productName = currentStatus.ModelName;
        }


        //  初始化菜单
        public void IniteStatusStrip()
        {
            StatusStrip statusItem = new StatusStrip();

            ToolStripStatusLabel searchMenuItem = new ToolStripStatusLabel()
            {
                Dock = DockStyle.Fill,
            };
            searchMenuItem.Text = $"供方信息：{Config.Instance.SupplierInformation}  联系电话：{Config.Instance.AfterSaleCall}";
            statusItem.Items.Add(searchMenuItem);
            statusItem.BackColor = Color.FromArgb(225, 227, 228);
            statusItem.Items.Add(searchMenuItem);
            statusItem.Dock = DockStyle.Bottom;

            this.Controls.Add(statusItem);
        }

        #region 私有方法

        //  初始化主控ControlEngine
        private void InitControlEngine()
        {
            // 🔥 1. 先停旧的
            if (controlEngine != null)
            {
                controlEngine.Shutdown();
                controlEngine = null;
            }

            Shared.productModel = _jsonService.LoadFromFile<ProductModel>(PathCenter.ConfigFile(Path.Combine("model", $"{Shared.productName}.json")));

            controlEngine = new ControlEngine();

            controlEngine.Init();
        }

        //  接收型号
        private void ApplyModel(ProductModel model)
        {
            Invoke((MethodInvoker)(async () =>
            {
                currentStatus.Status = "Busy";
                currentStatus.ModelName = Shared.productName;
                Shared.productName = model.BaseInfo.ProductName;
                Shared.productCode = model.BaseInfo.ProductCode;
                Shared.productModel = model;
                _jsonService.SaveToFile<ProductModel>(PathCenter.ConfigFile(Path.Combine("model", $"{model.BaseInfo.ProductName}.json")), model);

                // 保存“最后使用型号”
                SaveLastUsedModel(model.BaseInfo.ProductName);
                DisplayLog.Info($"接收到型号：{model.BaseInfo.ProductName}", true);
                await Task.Delay(2000);
                currentStatus.Status = "Ready";
                currentStatus.ModelName = model.BaseInfo.ProductName;
                DisplayLog.Info("型号切换完成，状态恢复 Ready");
                controlEngine.WriteParameterToPlc(model);
                controlEngine.ReadBatchesFromPLc();

            }));
        }

        //  切换型号
        private void SwitchModel(ProductModel model)
        {
            Task.Run(() =>
            {
                try
                {

                    // 1. 停旧引擎
                    //controlEngine?.Shutdown();

                    // 2. 更新全局
                    Shared.productName = model.BaseInfo.ProductName;
                    Shared.productModel = model;
                    // 3. 新建引擎
                    //controlEngine = new ControlEngine();
                    controlEngine.WriteParameterToPlc(model);

                }
                catch (Exception ex)
                {
                    DisplayLog.Error("切换型号失败", ex, true);
                }
            });
        }

        //  保存最后使用型号
        private void SaveLastUsedModel(string productName)
        {
            string path = PathCenter.ConfigFile("last_model.json");
            var obj = new { LastModel = productName };
            _jsonService.SaveToFile(path, obj);
        }

        //  加载最后型号
        private string LoadLastUsedModel()
        {
            try
            {
                string path = PathCenter.ConfigFile("last_model.json");
                if (!File.Exists(path))
                    return null;

                var obj = _jsonService.LoadFromFile<Dictionary<string, string>>(path);
                if (obj != null && obj.TryGetValue("LastModel", out string model))
                {
                    return model;
                }
            }
            catch
            {
                // 日志一下，防止文件损坏
            }

            return null;
        }


        #endregion

        #region 菜单栏事件


        //  读卡器
        private void tsm_AynettekFormClicked(object sender, EventArgs e)
        {

            if (aynettekForm == null || aynettekForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                aynettekForm = new AynettekForm(LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort)
                {
                    Text = "安耐特读卡器"
                };
            }
            aynettekForm.Show();
        }

        //  查询历史数据
        private void tsm_SearchFormClicked(object sender, EventArgs e)
        {
            if (searchForm == null || searchForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                searchForm = new SearchForm(LocalConfig.Instance.ProcessName);
            }
            searchForm.Show();
        }

        //  点检数据查询
        private void tsm_SearchCheckFormClicked(object sender, EventArgs e)
        {
            if (searchCheckForm == null || searchCheckForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                searchCheckForm = new SearchCheckForm(LocalConfig.Instance.ProcessName);
            }
            searchCheckForm.Show();
        }

        //  查看参数配置展示
        private void tsm_SearchRecipeFormClicked(object sender, EventArgs e)
        {
            if (searchRecipeForm == null || searchRecipeForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                searchRecipeForm = new SearchRecipeForm();
            }
            searchRecipeForm.Show();
        }

        //  PLC配置
        private void tsm_JsonFormClicked(object sender, EventArgs e)
        {
            if (editJsonForm == null || editJsonForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                editJsonForm = new EditJsonForm();
            }
            editJsonForm.Show();
        }

        //  配方编辑
        private void tsm_RecipeFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.ADM, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (recipeForm == null || recipeForm.IsDisposed)
                    // 如果已释放，则重新创建实例
                    recipeForm = new RecipeForm();
                recipeForm.Show();
            });
        }

        //  报警配置
        private void tsm_AlarmFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (AlarmForm == null || AlarmForm.IsDisposed)
                    AlarmForm = new AlarmForm();

                AlarmForm.Show();
            });
        }

        //  缓存查看
        private void tsm_CacheViewerClicked(object sender, EventArgs e)
        {
            if (CacheViewerForm == null || CacheViewerForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                CacheViewerForm = new CacheViewerForm();
            }
            CacheViewerForm.Show();
        }

        //  配方参数修改日志
        private void tsm_RecipeChangeLogClicked(object sender, EventArgs e)
        {
            if (RecipeChangeLogForm == null || RecipeChangeLogForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                RecipeChangeLogForm = new RecipeChangeLogForm();
            }
            RecipeChangeLogForm.Show();
        }

        //  易损件配置
        private void tsm_ConsumableViewerClicked(object sender, EventArgs e)
        {
            if (ConsumableEditForm == null || ConsumableEditForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                ConsumableEditForm = new ConsumableEditForm(LocalConfig.Instance.ProcessName, Config.Instance.ServerApi, Config.Instance.ServerDbtask);
            }
            ConsumableEditForm.Show();
        }


        //  系统配置
        private void tsm_ConfigCenterFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (ConfigCenterForm == null || ConfigCenterForm.IsDisposed)
                    ConfigCenterForm = new ConfigCenterForm();

                ConfigCenterForm.Show();
            });
        }

        //  用户管理
        private void tsm_UserManageClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (userManageForm == null || userManageForm.IsDisposed)
                    userManageForm = new UserManageForm();

                userManageForm.Show();
            });
        }

        //  产品码配置
        private void tsm_ProductCodeConfigFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (ProductCodeConfigForm == null || ProductCodeConfigForm.IsDisposed)
                    ProductCodeConfigForm = new ProductCodeConfigForm();

                ProductCodeConfigForm.Show();
            });
        }


        //  工单
        private void tsm_OrderViewerClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (OrderForm == null || OrderForm.IsDisposed)
                    OrderForm = new BydMesForm();

                OrderForm.Show();
            });
        }


        //  屏蔽
        private void tsm_BlockViewerClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (SpecialForm == null || SpecialForm.IsDisposed)
                    SpecialForm = new SpecialForm();

                SpecialForm.Show();
            });
        }

        //  选型
        private void tsm_ModelSelectClicked(object sender, EventArgs e)
        {
            using (var form = new ModelSelectForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var model = form.SelectedModel;

                    if (model == null) return;

                    // 🔥 切换型号（关键）
                    SwitchModel(model);
                }
            }
        }

        //  物料扫码窗口
        private void tsm_BatchScanFormClicked(object sender, EventArgs e)
        {
            if (BatchScanForm == null || BatchScanForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                BatchScanForm = new BatchScanForm(Shared.productName);
            }
            BatchScanForm.Show();
        }

        //  物料配置窗口
        private void tsm_BatchConfigFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (BatchConfigForm == null || BatchConfigForm.IsDisposed)
                {
                    // 如果已释放，则重新创建实例
                    BatchConfigForm = new BatchConfigForm();
                }
                BatchConfigForm.Show();
            });
        }

        //  物料监控
        private void tsm_BatchRuntimeFormClicked(object sender, EventArgs e)
        {
            if (BatchRuntimeForm == null || BatchRuntimeForm.IsDisposed)
            {
                // 如果已释放，则重新创建实例
                BatchRuntimeForm = new BatchRuntimeForm();
            }
            BatchRuntimeForm.Show();
        }

        //  用户切换
        private void tsm_UserFormClicked(object sender, EventArgs e)
        {
            try
            {
                this.Hide();  // 👈 隐藏主窗体

                using (var loginForm = new LoginForm(new RfidService(), new UserApiService(Config.Instance.ServerApi, Config.Instance.ServerDbtask), new MesService(), new LocalUserService(), LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort))
                {
                    loginForm.StartPosition = FormStartPosition.CenterScreen;

                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // 登录成功后执行必要更新
                        Console.WriteLine($"用户已切换为: {Shared.user?.UserName ?? "未知"}");
                    }
                    else
                    {
                        // 登录失败或用户关闭窗口，可以选择关闭程序或保留旧用户
                        MessageBox.Show("用户未登录，继续使用当前账号。", "提示");
                    }
                }
            }
            finally
            {
                this.Show();          // 👈 恢复主窗体显示
                this.Activate();      // 👈 把焦点切回主窗体
            }
        }

        //  取消用户
        private void tsm_CancelUserFormClicked(object sender, EventArgs e)
        {
            AuthHelper.Logout();
        }

        #endregion

        #region 窗口事件

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //  初始化主控程序
                InitControlEngine();
            }
            catch (Exception EX)
            {
                MessageBox.Show(EX.Message);
                throw;
            }
        }

        private void ShowMainForm(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            isRealExit = true;   // 👈 标记真正退出
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 👇 如果不是“真正退出”，就隐藏
            if (!isRealExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1000, "提示", "程序已最小化到托盘", ToolTipIcon.Info);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            trayIcon.Visible = false; // 👈 防止托盘残留
            base.OnFormClosed(e);
            reporter?.Stop();
            heartbeatMonitor?.Stop();
            OnAppClosing?.Invoke();
            controlEngine.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            bool isMin = this.WindowState == FormWindowState.Minimized;

            // ✅ 统一控制刷新
            UiState.SetPaused(isMin);
            UiState.IsVisible = !isMin;

            if (!isMin)
            {
                // 还原时只刷新一次，不是疯狂刷新
                Task.Delay(100).ContinueWith(_ =>
                {
                    if (!IsDisposed)
                        UiRefreshCenter.RequestRefresh();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        // 窗体显示/隐藏时同步控制刷新
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UiState.IsVisible = this.Visible;
        }


        #endregion
    }
}
