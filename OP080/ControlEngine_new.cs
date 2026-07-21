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
using System.Text.RegularExpressions;
using System.Data;
using System.IO;
using GT_Common.Helper.QueryClient;
using GT_Common.Util.LableStatus;
using System.CodeDom.Compiler;

namespace OP080
{
    public class ControlEngine_new : ControlEngineBase
    {
        protected override void AfterInit()
        {

            Shared.ProcessName = LocalConfig.Instance.ProcessName.Replace("&", "");

            Shared.orderNub = Shared.shopOrder != null ? Shared.shopOrder.OrderNum : "1000";

            Shared.workOrder = Shared.shopOrder != null ? Shared.shopOrder.Order : "111111111111";

            Shared.monitor = monitor;
        }

        /// <summary>
        /// 数据上传流程
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        protected override BaseProcessExecutor CreateStandardExecutor(StandardProcess process)
        {
            switch (process.Name)
            {

                case "ST70监控流程":
                    return CreateUploadExecutorST70(process);
                case "ST80监控流程":
                    return CreateUploadExecutorST80(process);
                case "ST90监控流程":
                    return CreateUploadExecutorST90(process);
                default:
                    return base.CreateStandardExecutor(process);
            }

        }
        /// <summary>
        /// ST70电阻焊
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST70(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = ctx => {

                    DisplayLog.Info(
                      $"流程 【{standardProcess.Name}】【{ctx.Sn}】获取数据",
                      true);

                    //var lsMeasurements = TwhHelper.GetAndBuildMeasurements(
                    //                                        LocalConfig.Instance.Thw_ST70_IP,
                    //                                        LocalConfig.Instance.Thw_ST70_Port);

                    //DisplayLog.Info(
                    //    $"流程 【{standardProcess.Name}】【{ctx.Sn}】获取数据{string.Join(",", lsMeasurements.Select(t => t.Value))}",
                    //    true);

                    //ProcessMethods.WritePlcSwitch(
                    //    standardProcess.PlcWriteConfig.WriteItems,
                    //    lsMeasurements,
                    //    reader);

                    //var sb = TwhHelper.GetKaCsv(
                    //    LocalConfig.Instance.Thw_ST70_IP,
                    //    LocalConfig.Instance.Thw_ST70_Port);

                    //CsvSaver.BasicSave(ctx.Sn, "ST70", sb);
                    ctx.WritePlcResult = 1;
                    return Task.CompletedTask;
                }
            };
            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST80电阻焊
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST80(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = ctx => {
                    var lsMeasurements = TwhHelper.GetAndBuildMeasurements(
                                                            LocalConfig.Instance.Thw_ST80_IP,
                                                            LocalConfig.Instance.Thw_ST80_Port);

                    DisplayLog.Info(
                        $"流程 【{standardProcess.Name}】【{ctx.Sn}】获取数据{string.Join(",", lsMeasurements.Select(t => t.Value))}",
                        true);

                    ProcessMethods.WritePlcSwitch(
                        standardProcess.PlcWriteConfig.WriteItems,
                        lsMeasurements,
                        reader);

                    var sb = TwhHelper.GetKaCsv(
                        LocalConfig.Instance.Thw_ST80_IP,
                        LocalConfig.Instance.Thw_ST80_Port);

                    CsvSaver.BasicSave(ctx.Sn, "ST80", sb);
                    ctx.WritePlcResult = 1;

                    return Task.CompletedTask;
                }
            };
            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST90电阻焊
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST90(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = ctx => {
                    var lsMeasurements = TwhHelper.GetAndBuildMeasurements(
                                                            LocalConfig.Instance.Thw_ST90_IP,
                                                            LocalConfig.Instance.Thw_ST90_Port);

                    DisplayLog.Info(
                        $"流程 【{standardProcess.Name}】【{ctx.Sn}】获取数据{string.Join(",", lsMeasurements.Select(t => t.Value))}",
                        true);

                    ProcessMethods.WritePlcSwitch(
                        standardProcess.PlcWriteConfig.WriteItems,
                        lsMeasurements,
                        reader);

                    var sb = TwhHelper.GetKaCsv(
                        LocalConfig.Instance.Thw_ST90_IP,
                        LocalConfig.Instance.Thw_ST90_Port);

                    CsvSaver.BasicSave(ctx.Sn, "ST90", sb);
                    ctx.WritePlcResult = 1;

                    return Task.CompletedTask;
                }
            };
            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

    }
}
