using GT_Common;
using GT_Common.DriverForm.Batch;
using GT_Common.Helper;
using GT_Common.Helper.ClientTask;
using GT_Common.Helper.Logging;
using GT_Common.Helper.PlcComm;
using GT_Common.Model;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static GT_Common.UploadSql;

namespace GT_Common.ProcessConfig
{
    public abstract class BaseProcessExecutor
    {
        protected readonly PatchReader _reader;
        protected readonly ComponentManager _componentManager;
        protected readonly UploadManager _uploadManager;
        protected readonly AccessMdbHelper _db;
        protected readonly List<Consumables> _lsConsumables;
        protected readonly List<PartRuntime> _partRuntimes;
        protected readonly ClientTaskSender _clientTaskSender;
        protected readonly ProductionMonitor _productionMonitor;
        private readonly ITestDataProcessor _dataProcessor;
        protected readonly IProcessStageHandler _stageHandler;
        private readonly Dictionary<string, Func<ProcessContext, Task>> _handlers;

        protected BaseProcessExecutor(
            PatchReader reader,
            ComponentManager componentManager,
            UploadManager uploadManager,
            AccessMdbHelper db,
            List<Consumables> lsConsumables,
            ClientTaskSender clientTaskSender,
            ProductionMonitor productionMonitor,
            ITestDataProcessor testDataProcessor,
            IProcessStageHandler stageHandler = null,
            Dictionary<string, Func<ProcessContext, Task>> specificHandlers = null)

        {
            _reader = reader;
            _componentManager = componentManager;
            _uploadManager = uploadManager;
            _db = db;
            _lsConsumables = lsConsumables;
            _clientTaskSender = clientTaskSender;
            _productionMonitor = productionMonitor;
            _dataProcessor = testDataProcessor ?? new TestDataProcessor(_componentManager, _uploadManager, _db, _lsConsumables, _clientTaskSender, _productionMonitor);
            _stageHandler = stageHandler;
            _handlers = specificHandlers ?? new Dictionary<string, Func<ProcessContext, Task>>();

        }

        public Task ExecuteAsync(StandardProcess process, LimitConfig limitConfig,List<ResultCodeParse> lsResultCodeParses) =>
        ExecuteProcessTemplate(process, context =>
        {
            context.LimitConfig = limitConfig;
            context.LsResultCodeParses = lsResultCodeParses;
            return ExecutePipelineAsync(context, GetStandardPipeline());
        });

        public Task ExecuteCheckResultAsync(CheckResultProcess process) =>
        ExecuteProcessTemplate(process, context =>
        {
            return ExecutePipelineAsync(context, GetCheckResultPipeline());
        });

        public Task ExecuteCalibrationAsync(CalibrationProcess process, LimitConfig limitConfig, List<ResultCodeParse> lsResultCodeParses) =>
        ExecuteProcessTemplate(process, context =>
        {
            context.LimitConfig = limitConfig;
            context.LsResultCodeParses = lsResultCodeParses;
            return ExecutePipelineAsync(context, GetCalibrationPipeline());
        });

        public Task ExecuteBarBindAsync(BarBindProcess process) =>
        ExecuteProcessTemplate(process, context =>
            ExecutePipelineAsync(context, GetBarBindPipeline()));

        private async Task ExecuteProcessTemplate<TProcess>(TProcess process, Func<ProcessContext, Task> executor) where TProcess : StandardProcess
        {
            var context = new ProcessContext { Process = process, Reader = _reader };
            var sw = Stopwatch.StartNew();
            DisplayLog.LogProcessStart(process.Name);

            try
            {
                await executor(context); // 执行传入的内部方法
            }
            catch (Exception ex)
            {
                ProcessMethods.WritePlcResult(process, _reader, "");
                DisplayLog.LogProcessError(process.Name, ex);
                await (_stageHandler?.OnError(context, ex) ?? Task.CompletedTask);
            }
            finally
            {
                ProcessMethods.WriteTriggerFinish(process, _reader);
                DisplayLog.LogProcessEnd(process.Name, sw.ElapsedMilliseconds.ToString());
            }
        }

        #region 流程执行
        protected virtual async Task ExecuteProcessInternal(ProcessContext context)
        {
            await RunStage("BeforeSnRead", async () =>
            {
                if (!await ReadSnCode(context))
                    throw new Exception("SN读取失败");
            }, "AfterSnRead", context);

            //await RunStage("BeforeSnRead",  () => ReadSnCode(context), "AfterSnRead", context);

            await RunStage("BeforeResultRead", () => ReadResult(context), "AfterResultRead", context);
            await RunStage("BeforeDataRead", () => ReadPlcData(context), "AfterDataRead", context);
            await RunStage("BeforeUpdateData", () => UpdateData(context), "AfterUpdateData", context);

            await RunStage("BeforeFileProcess", () => ProcessFiles(context), "AfterFileProcess", context);
            await RunStage("BeforePlcWrite", () => WritePlcResult(context), "AfterPlcWrite", context);
        }

        protected virtual async Task ExecuteCalibrationProcessInternal(ProcessContext context)
        {
            //await RunStage("BeforeLoadCalibrationOrPeriodInspection", () => LoadCalibrationOrPeriodInspection(context),
            //    "AfterLoadCalibrationOrPeriodInspection", context);

            //await RunStage("BeforeSnRead", async () =>
            //{
            //    if (!await OnlyReadSnCode(context))
            //        throw new OperationCanceledException("SN读取失败");
            //}, "AfterSnRead", context);

            await RunStage("BeforeSnRead", () => ReadSnCode(context), "AfterSnRead", context);

            await RunStage("BeforeResultRead", () => ReadResult(context), "AfterResultRead", context);
            await RunStage("BeforeReadCalibrationFlag", () => ReadCalibrationFlag(context), "AfterReadCalibrationFlag", context);
            await RunStage("BeforeDataRead", () => ReadPlcData(context), "AfterDataRead", context);

            await RunStage("BeforeUpdateData", () => UpdateCalibrationData(context), "AfterUpdateData", context);

            await RunStage("BeforeFileProcess", () => CombinedProcessFiles(context), "AfterFileProcess", context);
            //await RunStage("BeforeUpdateProcess", () => UpdateSHPeriodOrCalibrationValues(context), "AfterUpdateProcess", context);
            await RunStage("BeforePlcWrite", () => WritePlcResult(context), "AfterPlcWrite", context);
        }

        protected virtual async Task ExecuteBarBandProcessInternal(ProcessContext context)
        {
            //await RunStage("BeforeSnRead", async () =>
            //{
            //    if (!await ReadSnCode(context))
            //        throw new OperationCanceledException("SN读取失败");
            //}, "AfterSnRead", context);

            await RunStage("BeforeSnRead", () => ReadSnCode(context), "AfterSnRead", context);

            await RunStage("BeforeBindSnRead", () => BindSnRead(context), "AfterBindSnRead", context);

            await RunStage("BeforeUpdateProcess", () => BarBand(context), "AfterUpdateProcess", context);
            
            await RunStage("BeforePlcWrite", () => WritePlcResult(context), "AfterPlcWrite", context);
        }

        #endregion

        #region RunStage
        protected virtual async Task RunStage(string beforeStage, Func<Task> coreAction, string afterStage, ProcessContext context)
        {
            if (_stageHandler != null)
            {
                var beforeMethod = _stageHandler.GetType().GetMethod(beforeStage);
                if (beforeMethod != null)
                    await (Task)(beforeMethod.Invoke(_stageHandler, new object[] { context }) ?? Task.CompletedTask);
            }

            await coreAction();

            if (_stageHandler != null)
            {
                var afterMethod = _stageHandler.GetType().GetMethod(afterStage);
                if (afterMethod != null)
                    await (Task)(afterMethod.Invoke(_stageHandler, new object[] { context }) ?? Task.CompletedTask);
            }
        }
        #endregion

        #region Pipeline 配置
        private StageStep[] GetStandardPipeline() => new[]
        {
            Stage("SnRead", async (ctx) =>
            {
                if (!await ReadSnCode(ctx))
                    throw new OperationCanceledException("SN读取失败");
            }),
            //Stage("SnRead", ctx => ReadSnCode(ctx)),
            Stage("ResultRead", ctx => ReadResult(ctx)),
            Stage("DataRead", ctx => ReadPlcData(ctx)),
            Stage("UpdateData", ctx => UpdateData(ctx)),
            Stage("FileProcess", ctx => ProcessFiles(ctx)),
            Stage("PlcWrite", ctx => WritePlcResult(ctx))
        };

        private StageStep[] GetCheckResultPipeline() => new[]
        {
            Stage("SnRead", async (ctx) =>
            {
                if (!await ReadSnCode(ctx))
                    throw new OperationCanceledException("SN读取失败");
            }),
            //Stage("SnRead",  ctx => ReadSnCode(ctx)),
            //Stage("ResultRead", ctx => ReadResult(ctx)),
            Stage("PlcWrite", ctx => WritePlcResult(ctx))
        };

        private StageStep[] GetCalibrationPipeline() => new[]
        {
            Stage("SnRead", async (ctx) =>
            {
                if (!await OnlyReadSnCode(ctx))
                    throw new OperationCanceledException("SN读取失败");
            }),
            //Stage("LoadCalibrationOrPeriodInspection", ctx => LoadCalibrationOrPeriodInspection(ctx)),
            //Stage("SnRead", ctx => OnlyReadSnCode(ctx)),
            Stage("ResultRead", ctx => ReadResult(ctx)),
            Stage("ReadCalibrationFlag", ctx => ReadCalibrationFlag(ctx)),
            Stage("DataRead", ctx => ReadPlcData(ctx)),
            Stage("UpdateData", ctx => UpdateCalibrationData(ctx)),
            Stage("FileProcess", ctx => CombinedProcessFiles(ctx)),
            //Stage("UpdateProcess", ctx => UpdateSHPeriodOrCalibrationValues(ctx)),
            Stage("PlcWrite", ctx => WritePlcResult(ctx))
        };

        private StageStep[] GetBarBindPipeline() => new[]
        {
            Stage("SnRead", async (ctx) =>
            {
                if (!await ReadSnCode(ctx))
                    throw new OperationCanceledException("SN读取失败");
            }),
            //Stage("SnRead", ctx => ReadSnCode(ctx)),
            Stage("BindSnRead", ctx => BindSnRead(ctx)),
            Stage("UpdateProcess", async ctx =>await BarBand(ctx)),
            Stage("PlcWrite", ctx => WritePlcResult(ctx))
        };

        #endregion

        #region Pipeline 执行
        private async Task ExecutePipelineAsync(ProcessContext context, StageStep[] pipeline)
        {
            foreach (var step in pipeline)
            {
                await RunStageWithHandler($"Before{step.Name}", () => step.Action(context), $"After{step.Name}", context);
            }
        }

        private StageStep Stage(string name, Func<ProcessContext, Task> action) =>
            new StageStep(name, action);

        protected virtual async Task RunStageWithHandler(string beforeStage, Func<Task> coreAction, string afterStage, ProcessContext context)
        {
            // 优先从 handlers 中查找钩子
            if (_handlers.TryGetValue(beforeStage, out var beforeHandler))
                await beforeHandler(context);
            else
                await InvokeHandler(beforeStage, context);

            await coreAction();

            if (_handlers.TryGetValue(afterStage, out var afterHandler))
                await afterHandler(context);
            else
                await InvokeHandler(afterStage, context);
        }

        private async Task InvokeHandler(string methodName, ProcessContext context)
        {
            if (_stageHandler != null)
            {
                var method = _stageHandler.GetType().GetMethod(methodName);
                if (method != null)
                {
                    var result = method.Invoke(_stageHandler, new object[] { context });
                    if (result is Task task)
                        await task;
                }
            }
        }

        #endregion
       
        #region 核心步骤实现

        // 读取SN方法实现...
        protected virtual async Task<bool> ReadSnCode(ProcessContext context)
        {
            if (context.Process.SnConfig?.IsEnabled == true)
            {
                context.Sn = ProcessMethods.ReadSnCode(context.Process, _reader);

                if (string.IsNullOrEmpty(context.Sn))
                {
                    context.WritePlcResult = 2;
                    DisplayLog.LogProcessError(context.Process.Name, new Exception("SN 读取失败"));
                    return false; // 提前退出
                }
            }
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
            return true;
        }

        protected virtual async Task<bool> OnlyReadSnCode(ProcessContext context)
        {
            if (context.Process.SnConfig?.IsEnabled == true)
            {
                context.Sn = ProcessMethods.ReadSnCode(context.Process, _reader);
                if (string.IsNullOrEmpty(context.Sn))
                {
                    context.WritePlcResult = 2;
                    DisplayLog.LogProcessError(context.Process.Name, new Exception("SN 读取失败"), context.Sn);
                    return false; // 提前退出
                }
            }
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
            return true;
        }

        // 读取结果方法实现...
        protected virtual async Task ReadResult(ProcessContext context)
        {
            if (context.Process.ResultConfig?.IsEnabled == true)
            {
                context.Result = ProcessMethods.ReadResult(context.Process, _reader, out string ResultCode);
                context.ResultCode = ResultCode;
                context.ResultCode = context.LsResultCodeParses.FirstOrDefault(t => t.Ng_code == ResultCode)?.Ng_msg ?? ResultCode + "_";
            }
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
        }

        // 读取数据方法实现...
        protected virtual async Task ReadPlcData(ProcessContext context)
        {
            if (context.Process.PlcReadConfig?.IsEnabled != true)
                return;

            context.Data = await ProcessMethods.ReadPlcData(context.Process, _reader, context.Sn);
            context.TaktTime = ProcessMethods.ReadTaktTime(context.Process, _reader);
        }

        protected virtual async Task UpdateData(ProcessContext context)
        {
            if (context.Process.PlcReadConfig?.IsEnabled != true)
                return;
            await _dataProcessor.HandleData(context);
        }

        // 读取文件方法实现...
        protected virtual async Task ProcessFiles(ProcessContext context)
        {
            if (context.Process.FileConfig?.IsEnabled == true)
            {
                ProcessMethods.HandleFileUploads(context.Process, context.Sn, _reader);
            }
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
        }

        // 点检抽检读取数据方法实现...
        protected virtual async Task OnlyReadPlcData(ProcessContext context)
        {
            if (context.Process.PlcReadConfig?.IsEnabled != true)
                return;

            context.Data = await ProcessMethods.ReadPlcData(context.Process, _reader, context.Sn);
            context.TaktTime = ProcessMethods.ReadTaktTime(context.Process, _reader);

        }

        // 点检抽检读取标志方法实现...
        protected virtual async Task ReadCalibrationFlag(ProcessContext context)
        {
           
            context.CalibrationFlag =  ProcessMethods.ReadCalibrationFlag(context.Process, _reader);
        }

        // 点检抽检读取数据上传方法实现...
        protected virtual async Task UpdateCalibrationData(ProcessContext context)
        {
            await _dataProcessor.CalibrationHandleData(context);
        }

        // 点检抽检读取文件实现...
        protected virtual async Task CombinedProcessFiles(ProcessContext context)
        {
            if (context.Process.FileConfig?.IsEnabled == true)
            {
                //context.Data.AddRange(ProcessMethods.HandleFileUploads(context.Process, context.Sn, _reader));
            }
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
        }

        // 写入PLC数据方法实现...
        protected virtual async Task WritePlcResult(ProcessContext context)
        {
            if (context.Process.ResultWriteConfig?.IsEnabled == true)
            {
                ProcessMethods.WritePlcResult(context.Process, _reader, context.Sn, context.WritePlcResult);
            }
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
        }

        // 绑定方法实现...

        protected virtual async Task BindSnRead(ProcessContext context)
        {
            ProcessMethods.ReadBinSnCode(context.Process, _reader, context.PartInfos);
            await Task.CompletedTask; // 如果需要保持async兼容性就加上这行
        }

        protected virtual async Task<bool> BarBand(ProcessContext context)
        {
            try
            {
                if (!(context.Process is BarBindProcess barBindProcess))
                {
                    DisplayLog.Warn("BarBand：当前流程不是 BarBindProcess");
                    return false;
                }

                if (barBindProcess.PartInfos == null)
                {
                    DisplayLog.Warn("BarBand：PartInfos 或 Data 为空");
                    return false;
                }

                //if (barBindProcess.PartInfos.Count != context.Data.Count)
                //{
                //    DisplayLog.Warn($"BarBand：绑定数量不匹配 PartInfos={barBindProcess.PartInfos.Count}, Data={context.Data.Count}");
                //    return false;
                //}

                // 1. 写入部件码
                for (int i = 0; i < barBindProcess.PartInfos.Count; i++)
                {
                    barBindProcess.PartInfos[i].BarCode = context.PartInfos[i].PlcChannel.Value.ToString();
                }

                context.PartInfos = barBindProcess.PartInfos;

                // 2. 上传数据库 / MES
                await ProcessMethods.UploadPartNumbersAsync(
                    context.Sn,
                    context.Process.ProcessNo,
                    context.PartInfos.ToArray());
                context.WritePlcResult = 1;
                DisplayLog.LogProcessInfo(context.Process.Name,"绑定成功", context.Sn);
                return true;
            }
            catch (Exception ex)
            {
                context.WritePlcResult = 2;
                DisplayLog.LogProcessError(context.Process.Name, ex, context.Sn);

                return false;
            }
        }

        // 加载抽检点检项实现...
        protected virtual async Task LoadCalibrationOrPeriodInspection(ProcessContext context)
        {
            context.CalibrationOrPeriodInspection = ProcessMethods.LoadCalibrationOrPeriodInspection(context.Process, context.Reader);
            await Task.CompletedTask;
        }

        #endregion
    }
}
