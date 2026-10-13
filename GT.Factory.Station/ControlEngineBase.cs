using GT.Factory.Common.Helpers;
using GT.Factory.Common.Models;
using GT.Factory.Logging.Logging;
using GT.Factory.PLC;
using GT.Factory.PLC.PlcComm;
using GT.Factory.Station.DriverForm.Batch;
using GT.Factory.Station.DriverForm.Recipe.RecipeParameter;
using GT.Factory.Station.Helper;
using GT.Factory.Station.Helper.BydMes;
using GT.Factory.Station.Helper.ClientTask;
using GT.Factory.Station.Helper.LanModelSync;
using GT.Factory.Station.Model;
using GT.Factory.Station.MyEnum;
using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommMethod = GT.Factory.Station.Helper.CommMethod;
using Consumables = GT.Factory.Common.Models.Consumables;
using DataItem = GT.Factory.Common.Models.DataItem;
using ResultCodeParse = GT.Factory.Common.Models.ResultCodeParse;

namespace GT.Factory.Station
{
    public abstract class ControlEngineBase : IDisposable
    {
        #region ===== 公共字段 =====

        //仪器配置
        protected InstrumentConfig instrumentConfig;

        protected PatchReader reader;

        //PLC交握配置
        protected PlcConfig plcConfig;

        //正常流程
        protected StandardProcess[] StandardProcesses;

        //点检流程
        protected CalibrationProcess[] CalibrationProcesses;

        //过站检测流程
        protected CheckResultProcess[] CheckResultProcesses;

        //返工流程
        protected ReworkProcess[] ReworkProcesses;

        //绑定流程
        protected BarBindProcess[] BarBindProcesses;

        //仪器流程
        protected InstrumentProcess[] InstrumentProcesses;

        // 新增：监听器生命周期管理
        private readonly List<ValueMonitorShort> _monitors = new List<ValueMonitorShort>();

        // 用于判断连接层参数是否变化
        private string _lastIp, _lastSectionName, _lastHeartbeatAddr, _lastPlcType;
        private ushort _lastLength;

        private readonly object _reloadLock = new object();


        //protected readonly UpdateSql updateSql = new UpdateSql();
        protected readonly UploadManager uploadManager = new UploadManager();

        protected readonly ComponentManager componentManager = new ComponentManager();

        protected DynamicExecutorFactory _executorFactory;
        protected readonly Dictionary<string, BaseProcessExecutor> _processExecutors = new Dictionary<string, BaseProcessExecutor>();

        protected LimitConfig RecipeConfig;
        protected List<ResultCodeParse> resultCodeParse = new List<ResultCodeParse>();

        //protected AccessMdbHelper db;

        protected TestDataProcessor testDataProcessor;

        protected IInstrumentDispatcher _instrumentDispatcher;

        private CancellationTokenSource _cts;
        private Task _backgroundTask;

        #endregion
       
        protected ProductionMonitor monitor = new ProductionMonitor();

        protected ProductionStats productionStats;

        private UploadAlarm uploadAlarm;

        private FacilityState facilityState;

        private List<Consumables> lsConsumables;

        private List<PlcConsumables> lsPlcConsumables;

        private bool _firstDataReceived = false;

        ClientTaskSender clientTask;

        private MesService mesServer = new MesService();

        //private DataRuntime dataRuntime;

        #region ===== 生命周期 =====

        public async Task Init()
        {
            try
            {
                BeforeInit();

                //ConnectNetworkShare();

                //await InitDatabases();

                InitLoadLocalCache();

                //  加载仪器配置
                InitInstrumentConfigFile();

                InitExternalDevices();          // ✅ 外设扩展点

                await LoadConfigs();

                InitPLC();

                // 记录当前连接参数基线，供后续 Reload 判断是否需要重启
                CaptureConnectionBaseline();

                if (NeedReadBatch())
                    ReadBatchesFromPLc();

                StartBackgroundLoop();

                // 订阅 PlcConfig 热重载事件
                PlcConfig.Reloaded += OnPlcConfigReloaded;

                AfterInit();                   // ✅ 收尾扩展点
            }
            catch (Exception ex)
            {
                DisplayLog.Error("初始化失败", ex, true);
            }
        }

        protected virtual void InitExternalDevices() 
        {
            _instrumentDispatcher = CreateInstrumentDispatcher();
            RegisterInstrumentHandlers(_instrumentDispatcher);
        }

        /// <summary>
        /// ✅ 新增：工厂方法，创建仪器分派器实例
        /// 子类可覆盖此方法使用自定义的分派器实现
        /// </summary>
        protected virtual IInstrumentDispatcher CreateInstrumentDispatcher()
        {
            return new InstrumentDispatcher();
        }

        /// <summary>
        /// ✅ 修改：让子类按产线注册不同仪器
        /// 在 InitExternalDevices 中调用
        /// dispatcher 参数已可用
        /// </summary>

        protected virtual void RegisterInstrumentHandlers(IInstrumentDispatcher dispatcher)
        {

            InstrumentFactory factory = new InstrumentFactory();

            foreach (
                InstrumentDeviceConfig config
                in instrumentConfig.Instruments)
            {

                InstrumentCreateResult result =
                    factory.Create(config);


                dispatcher.Register(
                    config.Name,
                    result.Handler,
                    result.Instrument);
            }
        }

        protected virtual bool NeedReadBatch() => false;

        protected virtual void BeforeInit() { }
        protected virtual void AfterInit() { }

        #endregion
        


        private async Task LoadConfigs()
        {
           
            //  加载PLC配置文件
            InitPlcConfigFile();
            //  加载参数配置文件
            InitParameterConfigFile();
            // 加载工序参数
            InitLoadProcessConfig();
            //  加载易损件信息
            //await InitLoadConsumables();
            //  加载易损件PLC配置信息
            //InitLoadPlcConsumables();
            //  提示信息
            InitLoadActionTips();
            //  配置流程
            InitPlcConfig();
            //  更新数据库解析字段
            await InitFieldParse();
        }

        //  加载仪器配置文件
        private void InitInstrumentConfigFile()
        {
            InstrumentConfig.Initialize(PathCenter.ConfigFile("InstrumentConfig.json"));
            instrumentConfig = InstrumentConfig.Instance;
            instrumentConfig.Validate();

        }

        //  加载PLC配置文件
        private void InitPlcConfigFile()
        {
            PlcConfig.Initialize(PathCenter.ConfigFile("PlcConfig.json"));
            plcConfig = PlcConfig.Instance;
            plcConfig.Validate();

            Shared.productNo = plcConfig.PlcConnection.ProcessNo;
        }

        //  加载参数配置文件
        private void InitParameterConfigFile()
        {
            RecipeConfig = LimitConfigManager.Load();
        }

        //  加载批次参数配置文件
        private void InitBatchConfigFile()
        {
            var configs = BatchConfigLoader.Load();
        }

        //  加载工序易损件信息
        private async Task InitLoadConsumables()
        {
            //lsConsumables = Ac_SelectConsumables(db, "");
            lsConsumables = await AppRuntime.Current.DataRuntime.Repositories.Access.GetByStationAsync("");

        }

        //  加载工序易损件PLC地址配置信息
        private void InitLoadPlcConsumables()
        {
            lsPlcConsumables = ConsumablesConfigManager.Load();
        }

        //  加载工序提示信息
        private void InitLoadActionTips()
        {
            Shared.lsActionTips = ActionTipModelConfigManager.Load();
        }

        //  加载工序参数
        private async void InitLoadProcessConfig()
        {
            resultCodeParse = await AppRuntime.Current.DataRuntime.Repositories.Processes.SelectResultCodeParse(plcConfig.PlcConnection.ProcessNo);
            //resultCodeParse =UploadSql.SelectResultCodeParse(plcConfig.ProcessNo);
        }

        //  配置流程
        private void InitPlcConfig()
        {
            //加载交握配置
            StandardProcesses = plcConfig.StandardProcess.Where(t => t.ProcessType == ProcessType.StandardProcess).Select(t => t).ToArray();

            //加载点检交握配置
            CalibrationProcesses = plcConfig.CalibrationProcesses.Where(t => t.ProcessType == ProcessType.CalibrationProcesses).Select(t => t).ToArray();

            //加载返工交握配置
            ReworkProcesses = plcConfig.ReworkProcess.Where(t => t.ProcessType == ProcessType.ReworkProcess).Select(t => t).ToArray();

            //过站检测交握配置
            CheckResultProcesses = plcConfig.CheckResultProcess.Where(t => t.ProcessType == ProcessType.CheckResultProcesses).Select(t => t).ToArray();

            //加载绑定交握配置
            BarBindProcesses = plcConfig.BarBindProcess.Where(t => t.ProcessType == ProcessType.BarBindProcess).Select(t => t).ToArray();

            //加载仪器交握配置
            InstrumentProcesses = plcConfig.InstrumentProcesses.Where(t => t.ProcessType == ProcessType.InstrumentProcess).Select(t => t).ToArray();
        }

        //  更新数据库解析字段
        private async Task InitFieldParse()
        {
            await CommMethod.SyncFieldConfigAsync(StandardProcesses, plcConfig.PlcConnection.ProcessNo);
        }

        private void InitPLC()
        {

            var plc = PlcFactory.Create(plcConfig.PlcConnection.PlcType, plcConfig.PlcConnection.Ip);

            reader = new PatchReader(
               plc,
               plcConfig.PlcConnection.SectionName,
               plcConfig.PlcConnection.HeartbeatAddr,
               plcConfig.PlcConnection.StartIndex,
               plcConfig.PlcConnection.Length);

            testDataProcessor = new TestDataProcessor(componentManager, uploadManager, lsConsumables, clientTask, monitor);

            // 初始化工厂
            _executorFactory = new DynamicExecutorFactory(reader, componentManager, uploadManager, lsConsumables, clientTask, monitor, testDataProcessor, _instrumentDispatcher);

            // 监听数据更新
            reader.ValueUpdateEvent += (s, data) =>
            {
                if (!_firstDataReceived)
                {
                    _firstDataReceived = true;
                    InitAlarm();
                    InitEquipmentStata();
                }
            };

            IniPLCEvent();

            reader.Start();
        }

        private void CaptureConnectionBaseline()
        {
            _lastIp = plcConfig.PlcConnection.Ip;
            _lastSectionName = plcConfig.PlcConnection.SectionName;
            _lastHeartbeatAddr = plcConfig.PlcConnection.HeartbeatAddr;
            _lastLength = plcConfig.PlcConnection.Length;
            _lastPlcType = plcConfig.PlcConnection.PlcType.ToString();
        }

        /// <summary>
        /// PlcConfig.json 被保存并 Reload 后触发。
        /// plcConfig 字段与 PlcConfig.Instance 是同一个对象引用，内容已经是最新的了，
        /// 这里只需要重建"结构性"依赖（监听器、执行器、报警、设备状态）。
        /// </summary>
        private void OnPlcConfigReloaded(PlcConfig cfg)
        {
            lock (_reloadLock)
            {
                try
                {
                    // 1. 连接层参数变化 —— 不支持热加载，只提示，不做处理
                    bool connChanged =
                        _lastIp != cfg.PlcConnection.Ip ||
                        _lastSectionName != cfg.PlcConnection.SectionName ||
                        _lastHeartbeatAddr != cfg.PlcConnection.HeartbeatAddr ||
                        _lastLength != cfg.PlcConnection.Length ||
                        _lastPlcType != cfg.PlcConnection.PlcType.ToString();

                    if (connChanged)
                    {
                        DisplayLog.Warn("检测到PLC连接参数(Ip/SectionName/HeartbeatAddr/Length/PlcType)已修改，" +
                                        "该部分改动需要重启软件才能生效，本次热加载仅刷新流程相关配置。", true);
                    }

                    // 2. 停掉所有旧的流程触发监听，防止重复触发
                    foreach (var m in _monitors) m.Dispose();
                    _monitors.Clear();
                    _processExecutors.Clear();

                    // 3. plcConfig 字段与 cfg 是同一个引用，直接按最新内容重新切分数组
                    InitPlcConfig();

                    // 4. 重新建立所有流程的触发监听（地址、开关都已是最新）
                    IniPLCEvent();

                    // 5. 报警 / 设备状态：这两个模块持有独立的地址快照，需要显式重启
                    RestartAlarm();
                    RestartEquipmentState();

                    DisplayLog.Info("PLC配置热加载完成", true);
                }
                catch (Exception ex)
                {
                    DisplayLog.Error("PLC配置热加载失败", ex, true);
                }
            }
        }

        private void RestartAlarm()
        {
            uploadAlarm?.Stop();
            uploadAlarm = null;

            if (plcConfig.AlarmConfig.IsNeed)
            {
                uploadAlarm = new UploadAlarm(reader, plcConfig.AlarmConfig.AlarmStartAddress,
                                               plcConfig.AlarmConfig.AlarmLength, clientTask);
                uploadAlarm.Init();
            }
        }

        private void RestartEquipmentState()
        {
            facilityState?.Stop();
            facilityState = null;

            if (plcConfig.EquipmentStata.IsNeed)
            {
                facilityState = new FacilityState(reader, plcConfig.EquipmentStata.EquipmentStataAddress);
                facilityState.Init(plcConfig.EquipmentStata.EquipmentId);
            }
        }

        // PLC事件
        private void IniPLCEvent()
        {
            //  标准流程
            InitializeStandardProcesses();

            //  返工
            InitializeReworkProcesses();

            //  过站检
            InitializeCheckResultProcesses();

            //  点检
            InitializeCalibrationProcesses();

            //  绑定
            InitializeBarBindProcesses();

            //  仪器
            InitializeInstrumentProcesses();
        }

        //  初始化报警
        private void InitAlarm()
        {
            if (plcConfig.AlarmConfig.IsNeed)
            {
                RestartAlarm();
                // 注意：不要再写 BaseMainForm.OnAppClosing += () => uploadAlarm?.Stop();
                // 因为 Shutdown() 已经会调用 uploadAlarm?.Stop()，重复订阅会导致每次热加载都多注册一次静态事件

                ////Thread.Sleep(500);
                //uploadAlarm = new UploadAlarm(reader, plcConfig.AlarmConfig.AlarmStartAddress, plcConfig.AlarmConfig.AlarmLength, clientTask);
                //uploadAlarm.Init();
                //BaseMainForm.OnAppClosing += () => uploadAlarm?.Stop();
            }
        }

        //  初始化设备状态
        private void InitEquipmentStata()
        {
            if (plcConfig.EquipmentStata.IsNeed)
            {
                RestartEquipmentState();
                // 同理，不再单独订阅 OnAppClosing，统一在 Shutdown() 里处理
                //Thread.Sleep(1000);
                //facilityState = new FacilityState(reader, plcConfig.EquipmentStata.EquipmentStataAddress);
                //facilityState.Init(plcConfig.EquipmentStata.EquipmentId);
                //BaseMainForm.OnAppClosing += () => facilityState.Stop(); // 订阅窗体关闭事件
            }
        }

        private void InitLoadLocalCache()
        {
            #region 加载本地缓存

            //加载缓存
            try
            {
                uploadManager.Load();
            }
            catch (Exception)
            {
                // 加载分步上传缓存数据失败，可能会丢失部分数据
            }
            uploadManager.ValueReady += UploadManager_ValueReady;

            try
            {
                componentManager.Load();
            }
            catch (Exception)
            {

            }

            #endregion
        }

        #region 初始化PLC流程

        //  正常流程
        private void InitializeStandardProcesses()
        {
            foreach (var process in StandardProcesses)
            {
                var executor = CreateStandardExecutor(process);
                _processExecutors[process.Name] = executor;

                var monitor = new ValueMonitorShort(reader, process.TriggerAddress);
                monitor.ChangedHighEvent += async (s, e) =>
                    await executor.ExecuteAsync(process, RecipeConfig, resultCodeParse);

                _monitors.Add(monitor);
            }
        }

        //  返工
        private void InitializeReworkProcesses()
        {
            foreach (var process in ReworkProcesses)
            {
                var executor = CreateReworkExecutor(process);
                _processExecutors[process.Name] = executor;

                var monitor = new ValueMonitorShort(reader, process.TriggerAddress);
                    monitor.ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteReworkAsync(process, RecipeConfig, resultCodeParse);

                _monitors.Add(monitor);

            }
        }

        //  过站检
        private void InitializeCheckResultProcesses()
        {
            foreach (var process in CheckResultProcesses)
            {
                var executor = CreateCheckResultExecutor(process);
                _processExecutors[process.Name] = executor;

                var monitor = new ValueMonitorShort(reader, process.TriggerAddress);
                    monitor.ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteCheckResultAsync(process);

                _monitors.Add(monitor);

            }
        }

        //  点检
        private void InitializeCalibrationProcesses()
        {
            foreach (var process in CalibrationProcesses)
            {
                var executor = CreateCalibrationExecutor(process);
                _processExecutors[process.Name] = executor;

                var monitor = new ValueMonitorShort(reader, process.TriggerAddress);
                    monitor.ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteCalibrationAsync(process, RecipeConfig, resultCodeParse);

                _monitors.Add(monitor);

            }
        }

        //  绑定
        private void InitializeBarBindProcesses()
        {
            foreach (var process in BarBindProcesses)
            {
                var executor = CreateBarBindExecutor(process);
                _processExecutors[process.Name] = executor;

                var monitor = new ValueMonitorShort(reader, process.TriggerAddress);
                    monitor.ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteBarBindAsync(process);

                _monitors.Add(monitor);

            }
        }

        //  仪器
        private void InitializeInstrumentProcesses()
        {
            foreach (var process in InstrumentProcesses)
            {
                var executor = CreateInstrumentExecutor(process);
                _processExecutors[process.Name] = executor;

                var monitor = new ValueMonitorShort(reader, process.TriggerAddress);
                    monitor.ChangedHighEvent += async (s, e) =>
                    {
                        try
                        {
                            await executor.ExecuteInstrumentAsync(process);
                        }
                        catch (Exception ex)
                        {
                            DisplayLog.LogProcessError(process.Name, ex);
                        }
                    };

                _monitors.Add(monitor);

            }
        }

        #endregion

        #region ===== 可覆盖执行器创建 =====

        
        #region 正常交互流程

        // 正常交互流程
        protected virtual BaseProcessExecutor CreateStandardExecutor(StandardProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetStandardHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetStandardHandlers(StandardProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>();
        }

        #endregion

        #region 过站检交互流程

        protected virtual BaseProcessExecutor CreateCheckResultExecutor(CheckResultProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetCheckResultHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetCheckResultHandlers(CheckResultProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    // 1️⃣ 基础保护
                    if (string.IsNullOrWhiteSpace(ctx.Sn))
                    {
                        ctx.WritePlcResult = 2;
                        return;
                    }

                    // 2️⃣ 前置扩展
                    await BeforeCheckResultAsync(process, ctx);

                    // 如果前置已判NG直接结束
                    if (ctx.WritePlcResult == 2)
                        return;

                    // 3️⃣ 核心逻辑
                    await CoreCheckResultAsync(process, ctx);

                    // 4️⃣ 后置扩展
                    await AfterCheckResultAsync(process, ctx);
                }
            };
        }

        protected virtual async Task CoreCheckResultAsync(CheckResultProcess process, ProcessContext ctx)
        {
            if (process.CheckResultInfo.CheckType == CheckType.local)
            {
                await HandleLocalCheckAsync(process, ctx);
            }
            else
            {
                await HandleMesCheckAsync(process, ctx);
            }
        }

        protected virtual Task HandleLocalCheckAsync(CheckResultProcess process, ProcessContext ctx)
        {
            var item = new DataItem
            {
                Sn = ctx.Sn,
                Step = process.CheckResultInfo.Step
            };

            var dataItem = uploadManager.CheckLoalDate(item);

            ctx.WritePlcResult = dataItem?.Result == "OK" ? 1 : 2;

            return Task.CompletedTask;
        }

        protected virtual async Task HandleMesCheckAsync(CheckResultProcess process, ProcessContext ctx)
        {
            var shopOrder = mesServer.ValidateOrderInfor(
                "",
                Config.Instance.Resource,
                "",
                out bool result);

            //if (!result)
            //{
            //    ctx.WritePlcResult = 2;
            //    return;
            //}
            //shopOrder.Order = "111111111111";
            //ctx.MesOrder = shopOrder.Order;
            //Shared.shopOrder = shopOrder;

            ctx.MesOrder = "111111111111";
            monitor.OnProductInput(ctx.Sn, "111111111111");

            var (ok, _, _) = await BydMesComAsync.条码验证Async(ctx.Sn,Shared.user.JobNum);

            ctx.WritePlcResult = ok ? 1 : 2;
        }

        protected virtual Task BeforeCheckResultAsync(CheckResultProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterCheckResultAsync(CheckResultProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region 返工交互流程

        protected virtual BaseProcessExecutor CreateReworkExecutor(ReworkProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetReworkHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetReworkHandlers(ReworkProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    // 1️⃣ 基础保护
                    if (string.IsNullOrWhiteSpace(ctx.Sn))
                    {
                        ctx.WritePlcResult = 2;
                        return;
                    }

                    // 2️⃣ 前置扩展
                    await BeforeReworkAsync(process, ctx);

                    // 如果前置已判NG直接结束
                    if (ctx.WritePlcResult == 2)
                        return;

                    // 3️⃣ 核心逻辑
                    await CoreReworkAsync(process, ctx);

                    // 4️⃣ 后置扩展
                    await AfterReworkAsync(process, ctx);
                }
            };

        }

        protected virtual async Task CoreReworkAsync(ReworkProcess process, ProcessContext ctx)
        {
            var shopOrder = mesServer.ValidateOrderInfor("", "", ctx.Sn, out bool result);
            if (!result)
            {
                ctx.WritePlcResult = 2;  // 无效订单
                DisplayLog.Warn($"流程 【{process.Name}】【{ctx.Sn}】获取工单失败", true);
                return;
            }
            ctx.MesOrder = shopOrder.Order;
            Shared.shopOrder = shopOrder;
            monitor.OnProductInput(ctx.Sn, shopOrder.Order);

            var reworkQueryResult = await AppRuntime.Current.DataRuntime.Repositories.Processes.QueryDataUploadCountAsync(ctx.Sn, plcConfig.PlcConnection.ProcessNo, Config.Instance.ReworkCountLimit);

            ctx.WritePlcResult = reworkQueryResult.Status;

            List<IMeasurement> lsMeasurements = new List<IMeasurement>
            {
                new Measurement<int>
                {
                    Name = "返工数据",
                    Value = reworkQueryResult.ReworkFlag,
                    Unit = "",
                    Status = ""
                }
            };

            if (process.PlcWriteConfig.IsEnabled)
            {
                ProcessMethods.WritePlcSwitch(
                           process.PlcWriteConfig.WriteItems,
                           lsMeasurements,
                           reader);
            }
            DisplayLog.Info($"流程 【{process.Name}】【{ctx.Sn}】返工查询，返工标志【{reworkQueryResult.ReworkFlag}】,返工代码【{reworkQueryResult.NgCode}】", true);

            return;
        }

        protected virtual Task BeforeReworkAsync(ReworkProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterReworkAsync(ReworkProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region 绑定交互流程

        private BaseProcessExecutor CreateBarBindExecutor(BarBindProcess process)
        {

            return _executorFactory.CreateExecutor(process, GetBarBindHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetBarBindHandlers(BarBindProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterDataRead] = async ctx => 
                {

                    // 1️⃣ 基础保护
                    if (string.IsNullOrWhiteSpace(ctx.Sn))
                    {
                        ctx.WritePlcResult = 2;
                        return;
                    }

                    // 2️⃣ 前置扩展
                    await BeforeBarBindkAsync(process, ctx);

                    // 如果前置已判NG直接结束
                    if (ctx.WritePlcResult == 2)
                        return;

                    // 3️⃣ 核心逻辑
                    await CoreBarBindAsync(process, ctx);

                    // 4️⃣ 后置扩展
                    await AfterBarBindAsync(process, ctx);
                }
            };
        }

        protected virtual Task CoreBarBindAsync(BarBindProcess process, ProcessContext ctx)
        {
            if (ctx.PartInfos == null || ctx.Data == null)
            {
                ctx.WritePlcResult = 2;

                DisplayLog.Warn($"流程 【{process.Name}】【{ctx.Sn}】 数据异常NULL：PartInfos={ctx.PartInfos}, Data={ctx.Data}", true);

                throw new InvalidOperationException("流程 【{barBindProcess.Name}】【{ctx.Sn}】 AfterDataRead 阶段数据未初始化");
            }

            if (ctx.PartInfos.Count != ctx.Data.Count)
            {
                ctx.WritePlcResult = 2;

                DisplayLog.Warn($"流程 【{process.Name}】【{ctx.Sn}】 绑定数量不匹配：PartInfos={ctx.PartInfos.Count}, Data={ctx.Data.Count}", true);

                throw new InvalidOperationException(
                    $"流程 【{process.Name}】【{ctx.Sn}】 绑定数量不匹配：PartInfos={ctx.PartInfos.Count}, Data={ctx.Data.Count}");
            }

            for (int i = 0; i < ctx.PartInfos.Count; i++)
            {
                ctx.PartInfos[i].BarCode = ctx.PartInfos[i].PlcChannel.Value.ToString();
            }

            return Task.CompletedTask;
        }

        protected virtual Task BeforeBarBindkAsync(BarBindProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterBarBindAsync(BarBindProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region 点检交互流程

        private BaseProcessExecutor CreateCalibrationExecutor(CalibrationProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetCalibrationHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetCalibrationHandlers(CalibrationProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterDataRead] = async ctx =>
                {

                    // 1️⃣ 基础保护
                    if (string.IsNullOrWhiteSpace(ctx.Sn))
                    {
                        ctx.WritePlcResult = 2;
                        return;
                    }

                    // 2️⃣ 前置扩展
                    await BeforeCalibrationkAsync(process, ctx);

                    // 如果前置已判NG直接结束
                    if (ctx.WritePlcResult == 2)
                        return;

                    // 3️⃣ 核心逻辑
                    await CoreCalibrationAsync(process, ctx);

                    // 4️⃣ 后置扩展
                    await AfterCalibrationAsync(process, ctx);
                }
            };
        }

        protected virtual Task CoreCalibrationAsync(CalibrationProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task BeforeCalibrationkAsync(CalibrationProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterCalibrationAsync(CalibrationProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region 仪器交互流程

        protected virtual BaseProcessExecutor CreateInstrumentExecutor(InstrumentProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetInstrumentHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetInstrumentHandlers(InstrumentProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    // 1️⃣ 基础保护
                    if (string.IsNullOrWhiteSpace(ctx.Sn))
                    {
                        ctx.WritePlcResult = 2;
                        return;
                    }

                    DisplayLog.Warn($"流程 【{process.Name}】【{ctx.Sn}】前置扩展", true);

                    // 2️⃣ 前置扩展
                    await BeforeInstrumentAsync(process, ctx);

                    // 如果前置已判NG直接结束
                    if (ctx.WritePlcResult == 2)
                        return;

                    // 3️⃣ 核心逻辑
                    await CoreInstrumentAsync(process, ctx);

                    // 4️⃣ 后置扩展
                    await AfterInstrumentAsync(process, ctx);
                },

                
            };

        }

        protected virtual Task CoreInstrumentAsync(InstrumentProcess process, ProcessContext ctx)
        {
            //InstrumentResult measurementsResult = new InstrumentResult
            //{
            //    Measures = new List<InstrumentMeasure>
            //        {
            //            new InstrumentMeasure
            //            {
            //                FieldName = "LeakRate",
            //                Name = "泄漏率",
            //                Unit = "Pa",
            //                Value = 0.015
            //            },
            //            new InstrumentMeasure
            //            {
            //                FieldName = "Pressure",
            //                Name = "测试压力",
            //                Unit = "kPa",
            //                Value = 1.20
            //            },
            //            new InstrumentMeasure
            //            {
            //                FieldName = "Result",
            //                Name = "测试结果",
            //                Unit = "",
            //                Value = "OK"
            //            }
            //        },
            //    Result = true
            //};

            //ctx.Result="OK";
            //ctx.ResultCode = "6001_";
            //// 将仪器数据转换成系统标准的 PlcMeasureGroup 集合。
            //List<PlcMeasureGroup> data =
            //    CommMethod.ConvertInstrumentData(
            //        measurementsResult.Measures,
            //        (bool)measurementsResult.Result);
            //ctx.Data.Clear();
            //ctx.Data.AddRange(data);
            return Task.CompletedTask;

        }


        protected virtual Task BeforeInstrumentAsync(InstrumentProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterInstrumentAsync(InstrumentProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        #endregion

        #endregion

        #region 私有方法

        //  数据上传事件触发
        private async void UploadManager_ValueReady(object sender, DataItem e)
        {
            var payload =
                new ProcessPropertyFourPayload
                {
                    BarNo = e.Sn,

                    ProcessNo = PlcConfig.Instance.PlcConnection.ProcessNo,

                    VouNo = Shared.workOrder,

                    OkFlag = e.Result,

                    NgMsg = e.NgMsg,

                    UserId = Shared.user.JobNum,

                    Data = e
                };

            await AppRuntime.Current.DataRuntime.ProcessService.UploadFourAsync(payload);

        }

        #endregion

        #region 公有方法

        //  上传PLC配方参数
        public void WriteParameterToPlc(ProductModel model)
        {
            InitParameterConfigFile();
            ParameterServer.WriteParameterToPlc(RecipeConfig, reader, model);
        }

        //  从PLC读取物料信息
        public void ReadBatchesFromPLc()
        {
            BatchRuntimeManager.reader = reader;

            var configs = BatchConfigLoader.Load();

            BatchRuntimeManager.Init(Shared.productName, configs);
        }

        #endregion

        #region ===== 后台线程 =====

        private void StartBackgroundLoop()
        {
            _cts = new CancellationTokenSource();
            _backgroundTask = Task.Run(() => BackgroundUpdateLoop(_cts.Token));
        }

        private async Task BackgroundUpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    UpdatePLCStatus();
                    await UpdateDatabaseStatus();
                }
                catch (Exception ex)
                {
                    DisplayLog.Warn("后台更新异常：" + ex.Message);
                }

                await Task.Delay(1000, token);
            }
        }

        protected virtual void UpdatePLCStatus() 
        {
            if (!reader.IsConnected)
            {
                Shared.plcSatus = IndicatorStatus.Error;
                return;
            }

            Shared.plcSatus = IndicatorStatus.Normal;

            try
            {
                Shared.orderNub = Shared.shopOrder != null ? Shared.shopOrder.OrderNum : "1000";

                Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "111111111111";

                productionStats = monitor.GetStats(Shared.workOrder);

                Shared.totalNub = productionStats.Input.ToString();
                Shared.finishNub = productionStats.CompletedCount.ToString();
                Shared.okNub = productionStats.FinalPass.ToString();
                Shared.rate = productionStats.YieldText;

                reader.Plc.Write(plcConfig.FixedInformation.OrderCount, Convert.ToInt16(Shared.orderNub));

                reader.Plc.Write(plcConfig.FixedInformation.TimeToPlc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (Shared.productModel != null)
                {
                    reader.Plc.Write(plcConfig.FixedInformation.ProductName, Shared.productModel.BaseInfo.ProductName);
                }

                if (Shared.user != null)
                {
                    reader.Plc.Write(plcConfig.FixedInformation.JobNub, Convert.ToInt32(Shared.user.JobNum));
                    reader.Plc.Write(plcConfig.FixedInformation.LevelEnum, (short)Shared.user.LevelEnum);
                    reader.Plc.Write(plcConfig.FixedInformation.UserName, Shared.user.UserName, Encoding.Default);
                }
            }
            catch
            {
                // PLC 读取/写入异常
                Shared.plcSatus = IndicatorStatus.Error;
            }
        }

        //  数据库状态获取
        protected virtual async Task UpdateDatabaseStatus()
        {
            //Shared.sqlSatus = await MSSqlHelper.CheckConnnected(MSSqlHelper.Conn1) ? IndicatorStatus.Normal : IndicatorStatus.Error;
            Shared.sqlSatus = await AppRuntime.Current.DataRuntime.Database.Primary.PingAsync() ? IndicatorStatus.Normal : IndicatorStatus.Error;
            //Shared.accessSatus = db.ConnectionState == ConnectionState.Open ? IndicatorStatus.Normal : IndicatorStatus.Error;
        }

        #endregion

        /// <summary>
        /// 关闭外部仪器设备。
        ///
        /// 子站如果使用了电源、气密仪、扭力工具等外部设备，
        /// 可以通过重写该方法释放对应的仪器资源。
        /// </summary>
        protected virtual void ShutdownExternalDevices()
        {
            // 基类默认没有需要关闭的外部仪器。
           
        }

        /// <summary>
        /// 关闭控制引擎。
        ///
        /// 除了停止 PLC、监控器和后台任务外，
        /// 还负责通知子站释放外部仪器。
        /// </summary>
        public void Shutdown()
        {
            try
            {
                // 1️⃣ 停后台线程
                _cts?.Cancel();
                _backgroundTask?.Wait(2000);

                // 2️⃣ 停PLC读取
                reader?.Stop();

                // 3️⃣ 停报警
                uploadAlarm?.Stop();
                facilityState?.Stop();

                foreach (var m in _monitors) m.Dispose(); // 新增
                _monitors.Clear();

                PlcConfig.Reloaded -= OnPlcConfigReloaded; // 新增：防止内存泄漏

                // 5. 释放外部仪器。
                //
                // 子站通过重写 ShutdownExternalDevices()
                // 释放自己创建的仪器对象。
                ShutdownExternalDevices();

                // 6. 清理流程执行器。
                _processExecutors.Clear();
            }
            catch (Exception ex)
            {
                DisplayLog.Warn("ControlEngine Shutdown error: " + ex.Message);
            }
        }

        public void Dispose()
        {
            Shutdown();
        }
    }
}
