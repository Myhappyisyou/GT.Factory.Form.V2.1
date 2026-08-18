using GT.Factory.Common.Models;
using GT.Factory.Data.Core;
using GT_Common;
using GT_Common.DriverForm.Batch;
using GT_Common.DriverForm.ProductCode;
using GT_Common.DriverForm.Recipe.RecipeParameter;
using GT_Common.Helper;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.ClientTask;
using GT_Common.Helper.LanModelSync;
using GT_Common.Helper.Logging;
using GT_Common.Helper.Mssql;
using GT_Common.Helper.PlcComm;
using GT_Common.Model;
using GT_Common.MyEnum;
using GT_Common.ProcessConfig;
using HslCommunication.Profinet.Siemens;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TaskContracts.Models;
using Consumables = GT.Factory.Common.Models.Consumables;
using DataItem = GT.Factory.Common.Models.DataItem;
using ResultCodeParse = GT.Factory.Common.Models.ResultCodeParse;

namespace GT_Common
{
    public abstract class ControlEngineBase : IDisposable
    {
        #region ===== 公共字段 =====

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
        protected StandardProcess[] ReworkProcesses;

        //绑定流程
        protected BarBindProcess[] BarBindProcesses;

        protected readonly UpdateSql updateSql = new UpdateSql();
        protected readonly UploadManager uploadManager = new UploadManager();
        protected readonly ComponentManager componentManager = new ComponentManager();

        protected DynamicExecutorFactory _executorFactory;
        protected readonly Dictionary<string, BaseProcessExecutor> _processExecutors = new Dictionary<string, BaseProcessExecutor>();

        protected LimitConfig RecipeConfig;
        protected List<ResultCodeParse> resultCodeParse = new List<ResultCodeParse>();

        protected AccessMdbHelper db;
        protected TestDataProcessor testDataProcessor;

        private CancellationTokenSource _cts;
        private Task _backgroundTask;

        #endregion
       
        protected ProductionMonitor monitor = new ProductionMonitor();
        protected ProductionStats productionStats;

        UploadAlarm uploadAlarm;

        private List<Consumables> lsConsumables;

        private List<PlcConsumables> lsPlcConsumables;

        private bool _firstDataReceived = false;

        ClientTaskSender clientTask;

        private MesService mesServer = new MesService();

        private DataRuntime dataRuntime;

        #region ===== 生命周期 =====

        public async Task Init()
        {
            try
            {
                ConnectNetworkShare();
                await InitDatabases();
                InitLoadLocalCache();

                InitExternalDevices();          // ✅ 外设扩展点

                await LoadConfigs();
                InitPLC();

                if (NeedReadBatch())
                    ReadBatchesFromPLc();

                StartBackgroundLoop();

                AfterInit();                   // ✅ 收尾扩展点
            }
            catch (Exception ex)
            {
                DisplayLog.Error("初始化失败", ex, true);
            }
        }

        protected virtual void InitExternalDevices() { }

        protected virtual bool NeedReadBatch() => false;

        protected virtual void AfterInit() { }

        #endregion
        
        private void ConnectNetworkShare()
        {
            clientTask = new ClientTaskSender(Config.Instance.ServerDbtask);
            clientTask.LogCallback = msg =>
            {
                DisplayLog.Info(msg);
            };
        }

        private async Task InitDatabases()
        {
            string month = DateTime.Now.ToString("yyyy年MM月");

            var result = MonthlyAccessDbManager.GetCurrentMonthDbPath(month);

            db = new AccessMdbHelper(result.dbPath);

            DbContext.Set(db, month);

            await DatabaseInitializer.InitMonthlyDatabase(db, result.isNew);

            dataRuntime = new DataRuntime(Config.Instance.DatabaseOptions);

            //UploadSql.Initialize(dataRuntime.ProcessService);
        }

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

        //  加载PLC配置文件
        private void InitPlcConfigFile()
        {
            PlcConfig.Initialize(PathCenter.ConfigFile("PlcConfig.json"));
            plcConfig = PlcConfig.Instance;
            plcConfig.Validate();

            Shared.productNo = plcConfig.ProcessNo;
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
            resultCodeParse = await AppRuntime.Current.DataRuntime.Repositories.Processes.SelectResultCodeParse(plcConfig.ProcessNo);
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
            ReworkProcesses = plcConfig.StandardProcess.Where(t => t.ProcessType == ProcessType.ReworkProcess).Select(t => t).ToArray();

            //过站检测交握配置
            CheckResultProcesses = plcConfig.CheckResultProcess.Where(t => t.ProcessType == ProcessType.CheckResultProcesses).Select(t => t).ToArray();

            //加载绑定交握配置
            BarBindProcesses = plcConfig.BarBindProcess.Where(t => t.ProcessType == ProcessType.BarBindProcess).Select(t => t).ToArray();
        }

        //  更新数据库解析字段
        private async Task InitFieldParse()
        {
            await CommMethod.SyncFieldConfigAsync(StandardProcesses, plcConfig.ProcessNo);
        }

        private void InitPLC()
        {
            //SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1500, plcConfig.Ip);
         
            //reader = new PatchReader(plc, plcConfig.SectionName, plcConfig.HeartbeatAddr, plcConfig.StartIndex, plcConfig.Length);

            var plc = PlcFactory.Create(plcConfig);

            reader = new PatchReader(
                plc,
                plcConfig.SectionName,
                plcConfig.HeartbeatAddr,
                plcConfig.StartIndex,
                plcConfig.Length);

            testDataProcessor = new TestDataProcessor(componentManager, uploadManager, db, lsConsumables, clientTask, monitor);

            // 初始化工厂
            _executorFactory = new DynamicExecutorFactory(reader, componentManager, uploadManager, db, lsConsumables, clientTask, monitor, testDataProcessor);

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

        // PLC事件
        private void IniPLCEvent()
        {
            // 标准流程
            InitializeStandardProcesses();

            //  返工
            InitializeReworkProcesses();

            //  过站检
            InitializeCheckResultProcesses();

            // 点检
            InitializeCalibrationProcesses();

            //  绑定
            InitializeBarBindProcesses();
        }

        //  初始化报警
        private void InitAlarm()
        {
            if (plcConfig.AlarmConfig.IsNeed)
            {
                //Thread.Sleep(500);
                uploadAlarm = new UploadAlarm(reader, plcConfig.AlarmConfig.AlarmStartAddress, plcConfig.AlarmConfig.AlarmLength, clientTask);
                uploadAlarm.Init();
                BaseMainForm.OnAppClosing += () => uploadAlarm?.Stop();
            }
        }

        //  初始化设备状态
        private void InitEquipmentStata()
        {
            if (plcConfig.EquipmentStata.IsNeed)
            {
                //Thread.Sleep(1000);
                FacilityState FacilityState = new FacilityState(reader, plcConfig.EquipmentStata.EquipmentStataAddress);
                FacilityState.Init(plcConfig.EquipmentStata.EquipmentId);
                BaseMainForm.OnAppClosing += () => FacilityState.Stop(); // 订阅窗体关闭事件
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

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteAsync(process, RecipeConfig, resultCodeParse);
            }
        }

        //  返工
        private void InitializeReworkProcesses()
        {
            foreach (var process in ReworkProcesses)
            {
                var executor = CreateReworkExecutor(process);
                _processExecutors[process.Name] = executor;

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteAsync(process, RecipeConfig, resultCodeParse);
            }
        }

        //  过站检
        private void InitializeCheckResultProcesses()
        {
            foreach (var process in CheckResultProcesses)
            {
                var executor = CreateCheckResultExecutor(process);
                _processExecutors[process.Name] = executor;

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteCheckResultAsync(process);
            }
        }

        //  点检
        private void InitializeCalibrationProcesses()
        {
            foreach (var process in CalibrationProcesses)
            {
                var executor = CreateCalibrationExecutor(process);
                _processExecutors[process.Name] = executor;

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteCalibrationAsync(process, RecipeConfig, resultCodeParse);
            }
        }

        //  绑定
        private void InitializeBarBindProcesses()
        {
            foreach (var process in BarBindProcesses)
            {
                var executor = CreateBarBindExecutor(process);
                _processExecutors[process.Name] = executor;

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await executor.ExecuteBarBindAsync(process);
            }
        }

        #endregion

        #region ===== 可覆盖执行器创建 =====

        // 正常交互流程
        protected virtual BaseProcessExecutor CreateStandardExecutor(StandardProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetStandardHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetStandardHandlers(StandardProcess process)
        {
            return new Dictionary<string, Func<ProcessContext, Task>>();
        }

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

            if (!result)
            {
                ctx.WritePlcResult = 2;
                return;
            }

            ctx.MesOrder = shopOrder.Order;
            Shared.shopOrder = shopOrder;
            monitor.OnProductInput(ctx.Sn, shopOrder.Order, false);

            var (ok, _, _) = await BydMesComAsync.条码验证Async(ctx.Sn);

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

        protected virtual BaseProcessExecutor CreateReworkExecutor(StandardProcess process)
        {
            return _executorFactory.CreateExecutor(process, GetReworkHandlers(process));
        }

        protected virtual Dictionary<string, Func<ProcessContext, Task>> GetReworkHandlers(StandardProcess process)
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

        protected virtual async Task CoreReworkAsync(StandardProcess process, ProcessContext ctx)
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
            monitor.OnProductInput(ctx.Sn, shopOrder.Order, isRework: true);

            var reworkQueryResult = await dataRuntime.Repositories.Processes.QueryDataUploadCountAsync(ctx.Sn, plcConfig.ProcessNo, Config.Instance.ReworkCountLimit);

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

        protected virtual Task BeforeReworkAsync(StandardProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        protected virtual Task AfterReworkAsync(StandardProcess process, ProcessContext ctx)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region 绑定交互流程

        private BaseProcessExecutor CreateBarBindExecutor(BarBindProcess process)
        {

            return _executorFactory.CreateBarBindProcessExecutor(process, GetBarBindHandlers(process));
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
            return _executorFactory.CreateCalibrationProcessExecutor(process, GetCalibrationHandlers(process));
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

        #endregion

        #region 私有方法

        //  数据上传事件触发
        private async void UploadManager_ValueReady(object sender, DataItem e)
        {
            var payload =
                new ProcessPropertyFourPayload
                {
                    BarNo = e.Sn,

                    ProcessNo = PlcConfig.Instance.ProcessNo,

                    VouNo = Shared.workOrder,

                    OkFlag = e.Result,

                    NgMsg = e.NgMsg,

                    UserId = Shared.user.JobNub,

                    Data = e
                };

            await dataRuntime.ProcessService.UploadFourAsync(payload);

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
                Shared.rate = productionStats.Yield.ToString();

                reader.Plc.Write(plcConfig.FixedInformation.OrderCount, Shared.orderNub.ToInt16());

                reader.Plc.Write(plcConfig.FixedInformation.TimeToPlc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (Shared.productModel != null)
                {
                    reader.Plc.Write(plcConfig.FixedInformation.ProductName, Shared.productModel.BaseInfo.ProductName);
                }

                if (Shared.user != null)
                {
                    reader.Plc.Write(plcConfig.FixedInformation.JobNub, Convert.ToInt32(Shared.user.JobNub));
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
            Shared.sqlSatus = await MSSqlHelper.CheckConnnected(MSSqlHelper.Conn1) ? IndicatorStatus.Normal : IndicatorStatus.Error;
            Shared.accessSatus = db.ConnectionState == ConnectionState.Open ? IndicatorStatus.Normal : IndicatorStatus.Error;
        }

        #endregion

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

                // 5️⃣ 清空执行器（防止引用残留）
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
