using EditJson;
using GT_Common.Components;
using GT_Common.DriverForm.Alarm;
using GT_Common.DriverForm.Aynettek;
using GT_Common.DriverForm.Batch;
using GT_Common.DriverForm.CacheViewer;
using GT_Common.DriverForm.ConfigCenter;
using GT_Common.DriverForm.ConsumableEdit;
using GT_Common.DriverForm.Database;
using GT_Common.DriverForm.Keithley2790;
using GT_Common.DriverForm.Login;
using GT_Common.DriverForm.ModelSelectForm;
using GT_Common.DriverForm.Order;
using GT_Common.DriverForm.ProductCode;
using GT_Common.DriverForm.Recipe;
using GT_Common.DriverForm.SearchForm;
using GT_Common.DriverForm.SpecialForm;
using GT_Common.DriverForm.UserEdit;
using GT_Common.Helper;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.Helper.Database;
using GT_Common.Helper.Database.Core;
using GT_Common.Helper.Database.Repository;
using GT_Common.Helper.Database.Service;
using GT_Common.Helper.LanModelSync;
using GT_Common.Helper.Logging;
using GT_Common.Helper.Mssql;
using GT_Common.Helper.Oee;
using GT_Common.Helper.UIHelp;
using GT_Common.Model;
using GT_Common.MyEnum;
using RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IndicatorStatus = GT_Common.MyEnum.IndicatorStatus;
using LoginForm = GT_Common.DriverForm.Login.LoginForm;

namespace GT_Common
{
    public partial class BaseMainForm : Form
    {
        protected JsonService _jsonService = new JsonService();

        protected DeviceStatus currentStatus;
        protected DeviceStatusReporter reporter;
        protected ModelConfigReceiver receiver;
        protected MasterHeartbeatMonitor heartbeatMonitor;

        protected DataProvider _provider = new DataProvider();
        protected TitleUI titleUI;
        protected CurrentProductDataUI currentProductDataUI;
        protected DataUI dataUI;
        protected WorkStatusUI workStatusUI;
        protected OperationalStatusUI operationalStatusUI;
        protected ActionTipsUI actionTipsUI;
        protected ProductStatusUI productStatusUI;
        protected ProductionInformationUI productionInformationUI;

        //protected ControlEngine controlEngine;
        protected ControlEngineBase controlEngine;

        protected NotifyIcon trayIcon;
        protected ContextMenuStrip trayMenu;
        protected bool isRealExit = false;

        #region 窗口字段（声明为 protected，子类可访问）
        protected AynettekForm aynettekForm;
        protected SyncMonitorForm syncMonitorForm;
        protected SearchForm searchForm;
        protected SearchCheckForm searchCheckForm;
        protected EditJsonForm editJsonForm;
        protected RecipeForm recipeForm;
        protected SearchRecipeForm searchRecipeForm;
        protected AlarmForm AlarmForm;
        protected CacheViewerForm CacheViewerForm;
        protected ConsumableEditForm ConsumableEditForm;
        protected BatchConfigForm BatchConfigForm;
        protected BatchScanForm BatchScanForm;
        protected BatchRuntimeForm BatchRuntimeForm;
        protected SpecialForm SpecialForm;
        protected RecipeChangeLogForm RecipeChangeLogForm;
        protected ConfigCenterForm ConfigCenterForm;
        protected ProductCodeConfigForm ProductCodeConfigForm;
        protected BydMesForm OrderForm;
        protected UserManageForm userManageForm;
        #endregion

        public static event Action OnAppClosing;

        public BaseMainForm()
        {

            this.WindowState = FormWindowState.Maximized;
            this.MaximizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(1280, 840);

            if (IsDesignMode)
                return;

            Shared.productModel = _jsonService.LoadFromFile<ProductModel>(
             PathCenter.ConfigFile(Path.Combine("model", $"{Shared.productName}.json")));

            InitTrayIcon();
            this.FormClosing += MainForm_FormClosing;

            InitFormLayout();
            IniteMenu();
            InitStatus();
            //InitServices();


            this.Load += MainForm_Load;
            UiRefreshCenter.Start();
        }

        protected virtual void InitTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("显示主界面", null, ShowMainForm);
            trayMenu.Items.Add("退出程序", null, ExitApp);

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Text = "我的程序",
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => ShowMainForm(s, e);
        }

        protected virtual void InitServices()
        {
            reporter = new DeviceStatusReporter(Config.Instance.ServerIP, 8881, () => currentStatus);
            reporter.Start();

            receiver = new ModelConfigReceiver(5555);
            receiver.OnModelReceived += ApplyModel;
            receiver.Start();

            heartbeatMonitor = new MasterHeartbeatMonitor(Config.Instance.ServerIP, 6666);
            heartbeatMonitor.OnConnectionStateChanged += serverState =>
            {
                this.SafeInvoke(() =>
                {
                    Shared.serverSatus = serverState ? IndicatorStatus.Normal : IndicatorStatus.Error;
                    _provider.CurrentState = serverState;
                });
            };
            heartbeatMonitor.Start();
        }

        // 虚方法，子类可重写布局
        protected virtual void InitFormLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
            };

            mainLayout.ColumnStyles.Clear();
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            var leftLayout = CreateLeftPanel();
            mainLayout.Controls.Add(leftLayout, 0, 0);

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            var rightLayout = CreateRightPanel();
            mainLayout.Controls.Add(rightLayout, 1, 0);

            this.Controls.Add(mainLayout);
        }

        protected virtual TableLayoutPanel CreateLeftPanel()
        {
            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
            };

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            titleUI = new TitleUI(Config.Instance.ProcessName, Config.Instance.Vison) { Dock = DockStyle.Fill };
            leftLayout.Controls.Add(titleUI, 0, 0);

            currentProductDataUI = new CurrentProductDataUI { Dock = DockStyle.Fill };
            leftLayout.Controls.Add(currentProductDataUI, 0, 1);

            dataUI = new DataUI(Config.Instance.ProcessName) { Dock = DockStyle.Fill };
            ConfigureDataUIColumns(); // 子类可重写
            leftLayout.Controls.Add(dataUI, 0, 2);

            return leftLayout;
        }

        protected virtual TableLayoutPanel CreateRightPanel()
        {
            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 27));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 24));

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

            return rightLayout;
        }

        // 子类可重写配置数据表格列
        protected virtual void ConfigureDataUIColumns()
        {
            dataUI.SetColumns(new List<(string, int, Func<TestDispItem, string>)>
            {
                ("序号", 60, item => null),
                ("管壳码", 200, item => item.MainBar),
                ("测试结果", 100, item => item.Ok_flag),
            });
        }

        // 虚方法，子类可添加额外菜单
        protected virtual void IniteMenu()
        {
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(225, 227, 228);

            menuStrip.Items.Add(CreateQueryMenu());
            menuStrip.Items.Add(CreateConfigMenu());
            menuStrip.Items.Add(CreateDriverMenu());
            menuStrip.Items.Add(CreateUserMenu());
            menuStrip.Items.Add(CreateModelSelectMenu());
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        // 查询
        protected virtual ToolStripMenuItem CreateQueryMenu()
        {
            var menu = new ToolStripMenuItem("查询");
            AddMenuItem(menu, "生产数据查询", tsm_SearchFormClicked);
            AddMenuItem(menu, "点检数据查询", tsm_SearchCheckFormClicked);
            AddMenuItem(menu, "缓存查看", tsm_CacheViewerClicked);
            AddMenuItem(menu, "查看参数配置展示", tsm_SearchRecipeFormClicked);
            AddMenuItem(menu, "配方参数修改日志", tsm_RecipeChangeLogClicked);

            return menu;
        }

        // 配置
        protected virtual ToolStripMenuItem CreateConfigMenu()
        {
            var menu = new ToolStripMenuItem("配置");
            AddMenuItem(menu, "PLC配置", tsm_JsonFormClicked);
            AddMenuItem(menu, "参数配置", tsm_RecipeFormClicked);
            AddMenuItem(menu, "报警配置", tsm_AlarmFormClicked);
            AddMenuItem(menu, "系统配置", tsm_ConfigCenterFormClicked);
            AddMenuItem(menu, "用户管理", tsm_UserManageClicked);
            AddMenuItem(menu, "产品码配置", tsm_ProductCodeConfigFormClicked);
            AddMenuItem(menu, "BYD_MES测试", tsm_BydMesViewerClicked);
            AddMenuItem(menu, "屏蔽", tsm_BlockViewerClicked);

            return menu;
        }

        // 仪器仪表
        protected virtual ToolStripMenuItem CreateDriverMenu()
        {
            var menu = new ToolStripMenuItem("仪表");
            AddMenuItem(menu, "UID", tsm_AynettekFormClicked);
            AddMenuItem(menu, "数据库", tsm_SyncMonitorFormClicked);
            return menu;
        }

        // 用户切换
        protected virtual ToolStripMenuItem CreateUserMenu()
        {
            var menu = new ToolStripMenuItem("用户");
            AddMenuItem(menu, "切换用户", tsm_UserFormClicked);
            return menu;
        }

        // 新增型号选择菜单
        protected virtual ToolStripMenuItem CreateModelSelectMenu()
        {
            var menu = new ToolStripMenuItem("型号选择");
            menu.Click += tsm_ModelSelectClicked;
            return menu;
        }

        protected void AddMenuItem(ToolStripMenuItem parent, string text, EventHandler clickHandler)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += clickHandler;
            parent.DropDownItems.Add(item);
        }

        #region 公共方法
        protected virtual void InitStatus()
        {
            string lastModel = LoadLastUsedModel();
            currentStatus = new DeviceStatus
            {
                Status = "Ready",
                ModelName = lastModel ?? Shared.productName
            };
            Shared.productName = currentStatus.ModelName;
        }

        //protected virtual void InitControlEngine()
        //{
        //    if (controlEngine != null)
        //    {
        //        controlEngine.Shutdown();
        //        controlEngine = null;
        //    }

        //    Shared.productModel = _jsonService.LoadFromFile<ProductModel>(
        //        PathCenter.ConfigFile(Path.Combine("model", $"{Shared.productName}.json")));

        //    controlEngine = new ControlEngine();
        //    controlEngine.Init();
        //}

        protected virtual ControlEngineBase CreateControlEngine()
        {
            // 给个默认实现，这样才不会报错
            return null;
        }
        protected virtual void InitControlEngine()
        {
            if (controlEngine != null)
            {
                controlEngine.Shutdown();
                controlEngine = null;
            }

            Shared.productModel = _jsonService.LoadFromFile<ProductModel>(
               PathCenter.ConfigFile(Path.Combine("model", $"{Shared.productName}.json")));

            controlEngine = CreateControlEngine();
            controlEngine.Init();
        }

        protected virtual void ApplyModel(ProductModel model)
        {
            Invoke((MethodInvoker)(async () =>
            {
                currentStatus.Status = "Busy";
                Shared.productName = model.BaseInfo.ProductName;
                Shared.productCode = model.BaseInfo.ProductCode;
                Shared.productModel = model;
                _jsonService.SaveToFile<ProductModel>(
                    PathCenter.ConfigFile(Path.Combine("model", $"{model.BaseInfo.ProductName}.json")), model);

                SaveLastUsedModel(model.BaseInfo.ProductName);
                await Task.Delay(2000);
                currentStatus.Status = "Ready";
                currentStatus.ModelName = model.BaseInfo.ProductName;
                controlEngine.WriteParameterToPlc(model);
                controlEngine.ReadBatchesFromPLc();
            }));
        }

        protected virtual void SwitchModel(ProductModel model)
        {
            Task.Run(() =>
            {
                try
                {
                    Shared.productName = model.BaseInfo.ProductName;
                    Shared.productModel = model;
                    controlEngine.WriteParameterToPlc(model);
                }
                catch (Exception ex)
                {
                    DisplayLog.Error("切换型号失败", ex, true);
                }
            });
        }

        protected void SaveLastUsedModel(string productName)
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

        #region 菜单事件（声明为虚方法）
        /// <summary>
        /// 读卡器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_AynettekFormClicked(object sender, EventArgs e)
        {
            if (aynettekForm?.IsDisposed != false)
                aynettekForm = new AynettekForm(LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort);
            aynettekForm.Show();
        }
        /// <summary>
        /// 数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_SyncMonitorFormClicked(object sender, EventArgs e)
        {
            // 创建数据库管理器
            var dbManager = new DatabaseManager(Config.Instance.DatabaseOptions);

            // 创建缓存仓储
            var syncRepo = new SyncQueueRepository(dbManager);

            // 创建业务服务
            var processService = new ProcessService(dbManager, syncRepo);

            // 创建回传服务
            var syncService = new SyncService(dbManager, syncRepo);

            // 创建健康监测
            var healthMonitor = new DatabaseHealthMonitor(dbManager.Primary);

            var payload = new ProcessPropertyPayload
            {
                BarNo = "SN002",
                ProcessNo = "OP010",
                VouNo = "V001",
                OkFlag = "OK",
                NgMsg = "",
                UserId = "OP01",
                Data = new[] { "12.3", "45.6", "78.9" }
            };

             processService.UploadAsync(payload);

            if (syncMonitorForm?.IsDisposed != false)
                syncMonitorForm = new SyncMonitorForm(healthMonitor,syncRepo,syncService, dbManager.Primary);
            syncMonitorForm.Show();
        }
        /// <summary>
        /// 查询生产数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_SearchFormClicked(object sender, EventArgs e)
        {
            if (searchForm?.IsDisposed != false)
                searchForm = new SearchForm(LocalConfig.Instance.ProcessName);
            searchForm.Show();
        }

        /// <summary>
        /// 点检数据查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_SearchCheckFormClicked(object sender, EventArgs e)
        {
            if (searchCheckForm?.IsDisposed != false)
                searchCheckForm = new SearchCheckForm(LocalConfig.Instance.ProcessName);
            searchCheckForm.Show();
        }

        /// <summary>
        /// 缓存查看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_CacheViewerClicked(object sender, EventArgs e)
        {
            if (CacheViewerForm?.IsDisposed != false)
                CacheViewerForm = new CacheViewerForm();
            CacheViewerForm.Show();
        }

        /// <summary>
        /// 查看参数配置展示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_SearchRecipeFormClicked(object sender, EventArgs e)
        {
            if (searchRecipeForm?.IsDisposed!=false)
                searchRecipeForm = new SearchRecipeForm();
            searchRecipeForm.Show();
        }

        /// <summary>
        /// 配方参数修改日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_RecipeChangeLogClicked(object sender, EventArgs e)
        {
            if (RecipeChangeLogForm?.IsDisposed!=false)
                RecipeChangeLogForm = new RecipeChangeLogForm();
            RecipeChangeLogForm.Show();
        }

        /// <summary>
        /// PLC配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_JsonFormClicked(object sender, EventArgs e)
        {
            if (editJsonForm?.IsDisposed != false)
                editJsonForm = new EditJsonForm();
            editJsonForm.Show();
        }

        /// <summary>
        /// 参数配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_RecipeFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.ADM, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (recipeForm?.IsDisposed != false)
                    recipeForm = new RecipeForm();
                recipeForm.Show();
            });
        }

        /// <summary>
        /// 报警配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_AlarmFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (AlarmForm?.IsDisposed != false)
                    AlarmForm = new AlarmForm();
                AlarmForm.Show();
            });
        }

        /// <summary>
        /// 系统配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_ConfigCenterFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (ConfigCenterForm?.IsDisposed != false)
                    ConfigCenterForm = new ConfigCenterForm();
                ConfigCenterForm.Show();
            });
        }

        /// <summary>
        /// 用户管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_UserManageClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (userManageForm?.IsDisposed != false)
                    userManageForm = new UserManageForm();
                userManageForm.Show();
            });
        }

        /// <summary>
        /// 产品码配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_ProductCodeConfigFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (ProductCodeConfigForm?.IsDisposed != false)
                    ProductCodeConfigForm = new ProductCodeConfigForm();
                ProductCodeConfigForm.Show();
            });
        }

        /// <summary>
        /// BYD_MES测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_BydMesViewerClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (OrderForm?.IsDisposed != false)
                    OrderForm = new BydMesForm();
                OrderForm.Show();
            });
        }

        /// <summary>
        /// 屏蔽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_BlockViewerClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (SpecialForm?.IsDisposed != false)
                    SpecialForm = new SpecialForm();

                SpecialForm.Show();
            });
        }

        /// <summary>
        /// 用户切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void tsm_UserFormClicked(object sender, EventArgs e)
        {
            try
            {
                this.Hide();

                var localConfig = Config.Instance;

                // 校验登录配置
                localConfig.Login.Validate();

                // 初始化 RFID（如果启用）
                if (localConfig.EnableCardReader)
                {
                    //rfidService.Init(localConfig.UIDIP, localConfig.UIDPort);
                }

                var loginService = new LoginService(
                    new LocalUserService(),
                    new UserApiService(Config.Instance.ServerApi, Config.Instance.ServerDbtask),
                    new MesService());

                var providers = IdentityProviderFactory.CreateProviders(
                    localConfig.Login,
                    loginService,
                    localConfig.EnableCardReader ? new RfidService() : null);

                var loginForm = new GT_Common.DriverForm.Login.LoginForm(providers);

                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var user = loginForm.CurrentUser;
                }

                //this.Hide();
                //using (var loginForm = new LoginForm(new RfidService(), new UserApiService(Config.Instance.ServerApi, Config.Instance.ServerDbtask), new MesService(), new LocalUserService(), LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort))
                //{
                //    loginForm.ShowDialog();
                //}
            }
            finally
            {
                this.Show();
                this.Activate();
            }
        }

        /// <summary>
        /// 型号选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_ModelSelectClicked(object sender, EventArgs e)
        {
            using (var form = new ModelSelectForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.SelectedModel != null)
                    SwitchModel(form.SelectedModel);
            }
        }
        #endregion

        #region 窗口事件
        private void MainForm_Load(object sender, EventArgs e)
        {
            //if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            //    return;

            Config.Save();
            LocalConfig.Save();
            DbShiftManager.EnsureShiftDb();

            InitServices();


            try { InitControlEngine(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); throw; }
        }

        private void ShowMainForm(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Maximized;
            this.BringToFront();
        }

        private void ExitApp(object sender, EventArgs e)
        {
            isRealExit = true;
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isRealExit && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(1000, "提示", "程序已最小化到托盘", ToolTipIcon.Info);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            trayIcon.Visible = false;
            base.OnFormClosed(e);
            reporter?.Stop();
            heartbeatMonitor?.Stop();
            OnAppClosing?.Invoke();
            controlEngine?.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            bool isMin = this.WindowState == FormWindowState.Minimized;
            UiState.SetPaused(isMin);
            UiState.IsVisible = !isMin;

            if (!isMin)
            {
                Task.Delay(100).ContinueWith(_ =>
                {
                    if (!IsDisposed)
                        UiRefreshCenter.RequestRefresh();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            UiState.IsVisible = this.Visible;
        }

        #endregion

        protected bool IsDesignMode
        {
            get
            {
                return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                    || Process.GetCurrentProcess().ProcessName == "devenv"
                    || this.DesignMode;
            }
        }
    }
}