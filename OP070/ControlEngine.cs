using GT_Common;
using GT_Common.Helper;
using GT_Common.MyEnum;
using GT_Common.ProcessConfig;
using GT_Common.Model;
using GT_Common.Helper.PlcComm;
using GT_Common.Helper.ClientTask;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.LanModelSync;
using GT_Common.DriverForm.Recipe.RecipeParameter;
using GT_Common.DriverForm.Batch;
using GT_Common.Helper.Mssql;
using GT_Common.DriverForm.ProductCode;
using GT_Common.Helper.Logging;
using static GT_Common.UploadSql;
using HslCommunication.Profinet.Siemens;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace OP070
{
    public class ControlEngine : IDisposable
    {
        //PLC交握配置
        PlcConfig plcConfig;

        //正常流程
        StandardProcess[] StandardProcesses;

        //点检流程
        CalibrationProcess[] CalibrationProcesses;

        //过站检测流程
        CheckResultProcess[] CheckResultProcesses;

        //返工流程
        StandardProcess[] ReworkProcesses;

        //绑定流程
        BarBindProcess[] BarBindProcesses;

        //日志上传流程
        StandardProcess[] SaveLogProcesses;

        readonly UpdateSql updateSql = new UpdateSql();

        readonly UploadManager uploadManager = new UploadManager();

        readonly ComponentManager componentManager = new ComponentManager();
        
        public PatchReader reader;

        private DynamicExecutorFactory _executorFactory;

        private readonly Dictionary<string, BaseProcessExecutor> _processExecutors = new Dictionary<string, BaseProcessExecutor>();

        List<ResultCodeParse> resultCodeParse = new List<ResultCodeParse>();

        private AccessMdbHelper db;

        private TestDataProcessor testDataProcessor;

        UploadAlarm uploadAlarm;

        //  参数配置
        private LimitConfig RecipeConfig;

        private List<Consumables> lsConsumables;

        private List<PlcConsumables> lsPlcConsumables;

        private readonly static string _templateDbPath = Path.Combine(Config.Instance.DatadbPath, "Template.mdb");

        private bool _firstDataReceived = false;

        ClientTaskSender clientTask;

        private CancellationTokenSource _cts;

        private Task _backgroundTask;

        private MesService mesServer = new MesService();

        private ProductionMonitor monitor = new ProductionMonitor();
        private ProductionStats productionStats;

        public void Init()
        {
            try
            {
                ConnectNetworkShare();

                InitDatabases();

                InitLoadLocalCache();

                LoadConfigs();

                InitPLC();

                var plcConsumablesDict = lsPlcConsumables.ToDictionary(x => x.Name);

                ParameterServer.WriteParameterToPlc(RecipeConfig, reader, Shared.productModel);

                // 启动后台周期更新
                _cts = new CancellationTokenSource();
                _backgroundTask = Task.Run(() => BackgroundUpdateLoop(plcConsumablesDict, _cts.Token));

            }
            catch (Exception ex)
            {
                DisplayLog.Error("初始化失败", ex, true);
            }

            Shared.ProcessName = LocalConfig.Instance.ProcessName.Replace("&", "");

            Shared.orderNub = Shared.shopOrder != null ? Shared.shopOrder.OrderNum : "1000";

            Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "111111111111";

            Shared.monitor = monitor;
        }

        //  连接后台
        private void ConnectNetworkShare()
        {
            clientTask = new ClientTaskSender(Config.Instance.ServerDbtask);
        }

        //private void InitDatabases()
        //{
        //    db = new AccessMdbHelper(_templateDbPath);

        //    // 初始化时统一进入会话管理器
        //    DbContext.Init(db);
        //}

        private void InitDatabases()
        {
            string month = DateTime.Now.ToString("yyyy年MM月");

            var result = MonthlyAccessDbManager.GetCurrentMonthDbPath(month);

            db = new AccessMdbHelper(result.dbPath);

            DbContext.Set(db, month);

            DatabaseInitializer.InitMonthlyDatabase(db, result.isNew);
        }


        private void LoadConfigs()
        {
            //  加载PLC配置文件
            InitPlcConfigFile();
            //  加载参数配置文件
            InitParameterConfigFile();
            // 加载工序参数
            InitLoadProcessConfig();
            //  加载易损件信息
            InitLoadConsumables();
            //  加载易损件PLC配置信息
            InitLoadPlcConsumables();
            //  提示信息
            InitLoadActionTips();
            //  配置流程
            InitPlcConfig();
        }
        //  加载PLC配置文件
        private void InitPlcConfigFile()
        {
            PlcConfig.Initialize(PathCenter.ConfigFile("PlcConfig.json"));
            plcConfig = PlcConfig.Instance;
            plcConfig.Validate();
        }

        //  加载参数配置文件
        private void InitParameterConfigFile()
        {
            RecipeConfig = LimitConfigManager.Load();
        }

        //  加载工序易损件信息
        private void InitLoadConsumables()
        {
            lsConsumables = Ac_SelectConsumables(db, LocalConfig.Instance.ProcessName);
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
        private void InitLoadProcessConfig()
        {
            resultCodeParse = SelectResultCodeParse(plcConfig.ProcessNo);
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

        private void InitPLC()
        {
            SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1500, plcConfig.Ip);
         
            reader = new PatchReader(plc, plcConfig.SectionName, plcConfig.HeartbeatAddr, plcConfig.StartIndex, plcConfig.Length);

            testDataProcessor = new TestDataProcessor(componentManager, uploadManager, db, lsConsumables, clientTask, monitor);

            // 初始化工厂
            _executorFactory = new DynamicExecutorFactory(reader, componentManager, uploadManager, db, lsConsumables, clientTask, monitor, testDataProcessor);

            //// 注册全局处理器（适用于所有流程）
            //_executorFactory.RegisterGlobalHandler("AfterSnRead", async ctx =>
            //{
            //    // 全局SN读取后处理逻辑
            //    DisplayLog.ShowLogInfo($"全局处理 - 已读取SN: {ctx.Sn}");
            //});

            // 初始化流程执行器
            //_processExecutors.Add("Signal1", new StandardProcessExecutor(reader, updateSql, uploadManager));

            //_processExecutors.Add("Signal2", new StandardProcessExecutor(reader, updateSql, uploadManager));

            // 创建特定流程的执行器 

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
        public void IniPLCEvent()
        {
            //  标准流程
            InitializeStandardProcesses();

            //  返工
            InitializeReworkProcesses();

            //  过站检流程
            InitializeCheckResultProcesses();

            //  点检流程
            InitializeCalibrationProcesses();

            //  绑定
            InitializeBarBindProcesses();
        }

        //  初始化报警
        private void InitAlarm()
        {
            if (plcConfig.AlarmConfig.IsNeed)
            {
                uploadAlarm = new UploadAlarm(reader, plcConfig.AlarmConfig.AlarmStartAddress, plcConfig.AlarmConfig.AlarmLength, clientTask);
                uploadAlarm.Init();
                MainForm.OnAppClosing += () => uploadAlarm?.Stop();
            }
        }

        //  初始化设备状态
        private void InitEquipmentStata()
        {
            if (plcConfig.EquipmentStata.IsNeed)
            {
                FacilityState FacilityState = new FacilityState(reader, plcConfig.EquipmentStata.EquipmentStataAddress);
                FacilityState.Init(plcConfig.EquipmentStata.EquipmentId);
                MainForm.OnAppClosing += () => FacilityState.Stop(); // 订阅窗体关闭事件
            }
        }

        //  本地测试数据缓存
        private void InitLoadLocalCache()
        {
            #region 加载本地缓存

            //加载上传语句
            try
            {
                updateSql.Load();
            }
            catch (Exception)
            {
                // 加载分步上传缓存数据失败，可能会丢失部分数据
            }
            updateSql.SqlValueReady += UpdateSql_ValueReady;
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

        #region 缓存事件
        private void UpdateSql_ValueReady(object sender, SqlDataItem e)
        {
            UploadSql.InsetSql(updateSql, e.Sn, e.Process, e.SqlData);
        }

        private void UploadManager_ValueReady(object sender, DataItem e)
        {
            UploadSql.UploadBasicData1(updateSql, e.Sn, PlcConfig.Instance.ProcessNo, Shared.workOrder, e.Result, e.NgMsg, Shared.user.JobNub, e);
        }

        #endregion

        //正常流程
        private void InitializeStandardProcesses()
        {
            foreach (var process in StandardProcesses)
            {
                _processExecutors[process.Name] = CreateUploadExecutor(process);

                new ValueMonitorShort(reader, process.TriggerAddress)
               .ChangedHighEvent += async (s, e) =>
                  await _processExecutors[process.Name].ExecuteAsync(process, RecipeConfig, resultCodeParse);
            }
        }

        //返工
        private void InitializeReworkProcesses()
        {
            foreach (var process in ReworkProcesses)
            {
                _processExecutors[process.Name] = CreateReworkExecutor(process);

                new ValueMonitorShort(reader, process.TriggerAddress)
              .ChangedHighEvent += async (s, e) =>
                await _processExecutors[process.Name].ExecuteAsync(process, RecipeConfig, resultCodeParse);
            }
        }

        //过站检
        private void InitializeCheckResultProcesses()
        {
            foreach (var process in CheckResultProcesses)
            {
                _processExecutors[process.Name] = CreateCheckResultExecutor(process);

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await _processExecutors[process.Name].ExecuteCheckResultAsync(process);
            }
        }

        // 点检
        private void InitializeCalibrationProcesses()
        {
            foreach (var process in CalibrationProcesses)
            {
                _processExecutors[process.Name] = CreateCalibrationExecutor(process);

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await _processExecutors[process.Name].ExecuteCalibrationAsync(process, RecipeConfig, resultCodeParse);
            }
        }

        //  绑定
        private void InitializeBarBindProcesses()
        {
            foreach (var process in BarBindProcesses)
            {
                _processExecutors[process.Name] = CreateBarBindExecutor(process);

                new ValueMonitorShort(reader, process.TriggerAddress)
                    .ChangedHighEvent += async (s, e) =>
                        await _processExecutors[process.Name].ExecuteBarBindAsync(process);
            }
        }

        /// <summary>
        /// 数据上传交握流程
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutor(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                //[ProcessStageKeys.AfterDataRead] = ctx => {
                //    var shopOrder = mesServer.ValidateOrderInfor("", "", ctx.Sn, out bool result);
                //    if (result)
                //    {
                //        ctx.WritePlcResult = 2;
                //        DisplayLog.Warn($"流程 【{standardProcess.Name}】【{ctx.Sn}】获取工单失败", true);
                //    }

                //    return Task.CompletedTask; 
                //}
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// 绑定触发
        /// </summary>
        /// <param name="barBindProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateBarBindExecutor(BarBindProcess barBindProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                // 先在 BeforeBindSnRead 阶段赋值 ctx.PartInfos
                [ProcessStageKeys.BeforeBindSnRead] = ctx =>
                {
                    Console.WriteLine(barBindProcess.PartInfos.Count);

                    // 初始化 ctx.PartInfos，拷贝 Name 和 PlcChannel
                    ctx.PartInfos = barBindProcess.PartInfos
                        .Select(p => new PartInfo
                        {
                            Name = p.Name,
                            PlcChannel = p.PlcChannel
                        })
                        .ToList();
                    return Task.CompletedTask;
                },

                [ProcessStageKeys.AfterBindSnRead] = ctx => {

                    if (barBindProcess.PartInfos == null || ctx.PartInfos == null)
                    {
                        ctx.WritePlcResult = 2;

                        DisplayLog.Warn($"流程 【{barBindProcess.Name}】【{ctx.Sn}】 数据异常NULL：PartInfos={barBindProcess.PartInfos}, Data={ctx.PartInfos}", true);

                        throw new InvalidOperationException($"流程 【{barBindProcess.Name}】【{ctx.Sn}】 AfterDataRead 阶段数据未初始化");
                    }

                    if (barBindProcess.PartInfos.Count != ctx.PartInfos.Count)
                    {
                        ctx.WritePlcResult = 2;

                        DisplayLog.Warn($"流程 【{barBindProcess.Name}】【{ctx.Sn}】 绑定数量不匹配：PartInfos={barBindProcess.PartInfos.Count}, Data={ctx.PartInfos.Count}", true);

                        throw new InvalidOperationException(
                            $"流程 【{barBindProcess.Name}】【{ctx.Sn}】 绑定数量不匹配：PartInfos={barBindProcess.PartInfos.Count}, Data={ctx.PartInfos.Count}");
                    }

                    for (int i = 0; i < barBindProcess.PartInfos.Count; i++)
                    {
                        ctx.PartInfos[i].BarCode = ctx.PartInfos[i].PlcChannel.Value.ToString();

                        componentManager.AddOrUpdate(ctx.PartInfos[i].BarCode, ctx.Sn);
                        if (!Shared.blockMesBind)
                        {
                            BydMesCom.离散装配(ctx.Sn, ctx.PartInfos[i].BarCode, out bool ok, out string mes, out string xml);
                            ctx.WritePlcResult = ok ? 1 : 2;
                        }

                    }

                    return Task.CompletedTask;
                }
            };

            return _executorFactory.CreateBarBindProcessExecutor(barBindProcess, specificHandlers);
        }

        /// <summary>
        /// 返工流程
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateReworkExecutor(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = ctx => {

                    var shopOrder = mesServer.ValidateOrderInfor("", "", ctx.Sn, out bool result);
                    if (!result)
                    {
                        ctx.WritePlcResult = 2;  // 无效订单
                        DisplayLog.Warn($"流程 【{standardProcess.Name}】【{ctx.Sn}】获取工单失败", true);
                        return Task.CompletedTask;
                    }
                    ctx.MesOrder = shopOrder.Order;
                    Shared.shopOrder = shopOrder;
                    monitor.OnProductInput(ctx.Sn, shopOrder.Order, isRework: true);
                    if (!string.IsNullOrEmpty(ctx.Sn))
                    {
                        ctx.WritePlcResult = UploadSql.QueryDataUploadCount(ctx.Process.ProcessNo, ctx.Sn, Config.Instance.ReworkCountLimit, out int reworkFlag, out int ngCode);

                        List<Measurement<int>> lsMeasurements = new List<Measurement<int>>
                            {
                                new Measurement<int>
                                {
                                    Name = "返工数据",
                                    Value = ngCode,
                                    Unit = "",
                                    Status = ""
                                }
                            };

                        ctx.WritePlcResult = 1;
                    }
                    else
                    {
                        ctx.WritePlcResult = 2;
                    }

                    return Task.CompletedTask;
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// 过站检测流程
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateCheckResultExecutor(CheckResultProcess checkResultProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    if (!string.IsNullOrEmpty(ctx.Sn))
                    {
                        if (checkResultProcess.CheckResultInfo.CheckType == CheckType.local)
                        {
                            DataItem item = new DataItem { Sn = ctx.Sn, Step = checkResultProcess.CheckResultInfo.Step };
                            DataItem dataItem = uploadManager.CheckLoalDate(item);
                            if (dataItem != null)
                            {
                                ctx.WritePlcResult = dataItem.Result == "OK" ? 1 : 2;
                                DisplayLog.Info($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 查询{checkResultProcess.CheckResultInfo.Step}结果为{dataItem.Result}", true);
                            }
                            else
                            {
                                ctx.WritePlcResult = 1;

                                DisplayLog.Warn($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 查询{checkResultProcess.CheckResultInfo.Step}结果为无数据",true);
                            }
                        }
                        // Signal1特有的数据处理逻辑
                        else
                        {
                            var codeType = checkResultProcess.CheckResultInfo.Step == 50 ? CodeType.Shell : CodeType.Diffuser;

                            var rule = ProductCodeConfig.GetRule(Shared.productModel.BaseInfo.ProductName, codeType);

                            bool ok = ProductCodeConfig.Validate(ctx.Sn, rule, out string err);

                            if (!ok)
                            {
                                ctx.WritePlcResult = 2;
                                DisplayLog.Warn($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 产品码格式判断异常【{err}】", true);
                                return;
                            }
                            else
                            {
                                ctx.WritePlcResult = 1;

                                DisplayLog.Info($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 产品码格式判断合格【{err}】", true);
                            }

                            if (Shared.isOffline)
                            {
                                ctx.WritePlcResult = 1;
                                DisplayLog.Warn($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 查询{checkResultProcess.CheckResultInfo.Step}结果为无 【当前离线模式】", true);
                            }
                            else
                            {
                                if (checkResultProcess.CheckResultInfo.Step == 50)
                                {
                                    var shopOrder = mesServer.ValidateOrderInfor("", "", ctx.Sn, out bool result);
                                    if (!result)
                                    {
                                        ctx.WritePlcResult = 2;  // 无效订单
                                        DisplayLog.Warn($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】获取工单失败", true);
                                        return;
                                    }

                                    ctx.MesOrder = shopOrder.Order;
                                    Shared.shopOrder = shopOrder;
                                    monitor.OnProductInput(ctx.Sn, shopOrder.Order, false);
                                    // 4. MES 永远只认主码
                                    var (验证成功, MES反馈, XMLOUT) = await BydMesComAsync.条码验证Async(ctx.Sn);

                                    ctx.WritePlcResult = 验证成功 ? 1 : 2;

                                    DisplayLog.Info($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 查询   【{checkResultProcess.CheckResultInfo.Step}】结果为【{验证成功}】", true);
                                }
                                else
                                {
                                    ctx.WritePlcResult = UploadSql.SelectResultData("OP060", ctx.Sn);

                                    DisplayLog.Warn($"流程 【{checkResultProcess.Name}】【{ctx.Sn}】 查询【{checkResultProcess.CheckResultInfo.Step}】结果为【{ctx.WritePlcResult}】", true);
                                }

                            }
                        }
                    }
                    else
                    {
                        ctx.WritePlcResult = 2;
                    }
                }
            };

            return _executorFactory.CreateExecutor(checkResultProcess, specificHandlers);
        }

        /// <summary>
        /// 点检流程
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateCalibrationExecutor(CalibrationProcess process)
        {
            return _executorFactory.CreateCalibrationProcessExecutor(process);
        }

        //  上传PLC配方参数
        public void WriteParameterToPlc(ProductModel model)
        {
            InitParameterConfigFile();
            ParameterServer.WriteParameterToPlc(RecipeConfig, reader, model);
        }

        public void ReadBatchesFromPLc()
        {
            BatchRuntimeManager.reader = reader;

            var configs = BatchConfigLoader.Load();

            BatchRuntimeManager.Init(Shared.productName, configs);
        }

        private async Task BackgroundUpdateLoop(Dictionary<string, PlcConsumables> plcConsumablesDict, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    UpdatePLCStatus(plcConsumablesDict);
                    await UpdateDatabaseStatus();
                }
                catch (Exception ex)
                {
                    DisplayLog.Warn("Background update error: " + ex.Message);
                }

                await Task.Delay(1000, token);
            }
        }

        private void UpdatePLCStatus(Dictionary<string, PlcConsumables> plcConsumablesDict)
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

                Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "";

                Shared.totalNub = productionStats.Input.ToString();
                Shared.finishNub = productionStats.CompletedCount.ToString();
                Shared.okNub = productionStats.FinalPass.ToString();
                Shared.rate = productionStats.Yield.ToString();

                reader.Plc.Write(plcConfig.FixedInformation.OrderCount, Shared.orderNub.ToInt16());

                reader.Plc.Write(plcConfig.FixedInformation.TimeToPlc, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                foreach (var consumable in lsConsumables)
                {
                    if (plcConsumablesDict.TryGetValue(consumable.Name, out var plcConsumable))
                    {
                        try
                        {
                            reader.Plc.Write(plcConsumable.TheoreticalCountAddress, consumable.TheoreticalCount);
                            reader.Plc.Write(plcConsumable.UsedCountAddress, consumable.UsedCount);
                            reader.Plc.Write(plcConsumable.RemainderCountAddress, consumable.RemainderCount);
                        }
                        catch { /* 单个 consumable 写失败不影响其他 */ }
                    }
                }

                if (Shared.productModel != null)
                {
                    reader.Plc.Write(plcConfig.FixedInformation.ProductName, Shared.productModel.BaseInfo.ProductName);
                    //reader.Plc.Write(plcConfig.FixedInformation.ProductCode, Shared.productModel.BaseInfo.ProductCode);
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

        private async Task UpdateDatabaseStatus()
        {
            Shared.sqlSatus = await MSSqlHelper.CheckConnnected(MSSqlHelper.Conn1) ? IndicatorStatus.Normal : IndicatorStatus.Error;
            Shared.accessSatus = db.ConnectionState == ConnectionState.Open ? IndicatorStatus.Normal : IndicatorStatus.Error;
        }

        public void StopBackgroundUpdate()
        {
            _cts?.Cancel();
        }


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

        //  释放
        public void Dispose()
        {
            Shutdown();
        }
    }
}
