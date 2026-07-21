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
using System.CodeDom.Compiler;

namespace OP110
{
    public class ControlEngine_new : ControlEngineBase
    {
        readonly ComponentManager componentManager = new ComponentManager();
        
        public PatchReader reader;

        public PatchReader Melsecreader;

        private DynamicExecutorFactory _executorFactory;

        Keithley2790Helper keithley2790Helper1;
        Keithley2790Helper keithley2790Helper2;

        //复位
        public static string CommandRST = "*RST\r\n FORM:ELEM READ\r\n";

        //外壳电阻 通路测试
        public static string CommandPERC = "CALC1:FORM PERC\r\n CALC1:STAT OFF\r\n SENS:FUNC \"RES\"\r\n SENS:RES:RANG 100\r\n SENS:RES:NPLC 0.1\r\n ROUT:MULT:CLOS (@108,115,118)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //弹片电阻 短路测试
        public static string CommandCLOSE = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121,124)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //桥式电阻 桥丝测试
        public static string CommandOPEN = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //绝缘电阻 绝缘测试
        public static string CommandVOLT = "SOUR:VOLT 500, (@128)\r\n CALC1:FORM S1V\r\n CALC1:STAT ON\r\n TRIG:DEL 2\r\n ROUT:MULT:CLOS (@101,103,108,116,118,121,122,123)\r\n READ?\r\n TRIG:DEL 0\r\n ROUT:OPEN:ALL\r\n";

        protected override void InitExternalDevices()
        {
            InitKeithley();
        }

        protected override Task BeforeCheckResultAsync(
            CheckResultProcess process,
            ProcessContext ctx)
        {
            string mainBar = UploadSql.QueryMainBarByPartBar("OP070", ctx.Sn);

            if (string.IsNullOrWhiteSpace(mainBar))
            {
                ctx.WritePlcResult = 2;
                return Task.CompletedTask;
            }

            ctx.Sn = mainBar;
            return Task.CompletedTask;
        }

        //  初始化电测仪器
        private async void InitKeithley()
        {
            keithley2790Helper1 = new Keithley2790Helper();
            await keithley2790Helper1.ResetDeviceAsync(LocalConfig.Instance.ST40_1_IP, LocalConfig.Instance.ST40_1_Port);

            keithley2790Helper2 = new Keithley2790Helper();
            await keithley2790Helper2.ResetDeviceAsync(LocalConfig.Instance.ST40_2_IP, LocalConfig.Instance.ST40_2_Port);
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

                case "ST10获取高度":
                    return CreateUploadExecutorLength(process);
                case "ST40_1通路测试":
                    return CreateUploadExecutorST40_1_PERC(process);
                case "ST40_1绝缘电阻测试":
                    return CreateUploadExecutorST40_1_VOLT(process);
                case "ST40_1桥丝电阻测试":
                    return CreateUploadExecutorST40_1_OPEN(process);
                case "ST40_1短路电阻测试":
                    return CreateUploadExecutorST40_1_CLOSE(process);
                case "ST40_2通路测试":
                    return CreateUploadExecutorST40_2_PERC(process);
                case "ST40_2绝缘电阻测试":
                    return CreateUploadExecutorST40_2_VOLT(process);
                case "ST40_2桥丝电阻测试":
                    return CreateUploadExecutorST40_2_OPEN(process);
                case "ST40_2短路电阻测试":
                    return CreateUploadExecutorST40_2_CLOSE(process);
                default:
                    return base.CreateStandardExecutor(process);
            }

        }

        /// <summary>
        /// 查询上游高度
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorLength(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {

                [ProcessStageKeys.AfterSnRead] = ctx => {

                    string mainBar = componentManager.GetAssemblyCode(ctx.Sn);
                    DisplayLog.Info(
                           $"流程 【{standardProcess.Name}】【{ctx.Sn}】 查询到主码【{mainBar}】",
                           true);
                    if (string.IsNullOrWhiteSpace(mainBar))
                    {
                        ctx.WritePlcResult = 2;

                        DisplayLog.Warn(
                            $"流程 【{standardProcess.Name}】【{ctx.Sn}】 未查询到主码绑定关系，直接判NG",
                            true);

                        return Task.CompletedTask;
                    }

                    float value = UploadSql.SelectValueData(LocalConfig.Instance.LengthProcessNo, mainBar, LocalConfig.Instance.LengthFieldName);

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "高度",
                        Value = value,
                        Unit = "mm",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 高度" + value.ToString(), true);
                    return Task.CompletedTask;
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_1 通路测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_1_PERC(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {

                    string PercValue = await keithley2790Helper1.GetMeasurementsAsync(LocalConfig.Instance.ST40_1_IP, LocalConfig.Instance.ST40_1_Port, 3000, CommandPERC);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 通路电阻 {PercValue}", true);

                    CommMethod.TryExtractScientificFloat(PercValue, out float value);
                    ctx.WritePlcResult = 1;
                    //float value = 0.11f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "通路测试",
                        Value = value,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 通路电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_1 绝缘测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_1_VOLT(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string VoltValue = await keithley2790Helper1.GetMeasurementsAsync(LocalConfig.Instance.ST40_1_IP, LocalConfig.Instance.ST40_1_Port, 3000, CommandVOLT);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 绝缘电阻 {VoltValue}", true);

                    CommMethod.TryExtractScientificFloat(VoltValue, out float value);
                    ctx.WritePlcResult = 1;

                    //float value = 0.12f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "绝缘测试",
                        Value = value / 1000000,
                        Unit = "MΩ",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 绝缘电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_1 桥丝测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_1_OPEN(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string OpenValue = await keithley2790Helper1.GetMeasurementsAsync(LocalConfig.Instance.ST40_1_IP, LocalConfig.Instance.ST40_1_Port, 3000, CommandOPEN);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 桥丝电阻 {OpenValue}", true);

                    CommMethod.TryExtractScientificFloat(OpenValue, out float value);
                    ctx.WritePlcResult = 1;

                    //float value = 0.13f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "桥丝测试",
                        Value = value,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 桥丝电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_1 短路测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_1_CLOSE(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string closeValue = await keithley2790Helper1.GetMeasurementsAsync(LocalConfig.Instance.ST40_1_IP, LocalConfig.Instance.ST40_1_Port, 3000, CommandCLOSE);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 短路电阻 {closeValue}", true);

                    CommMethod.TryExtractScientificFloat(closeValue, out float value);

                    ctx.WritePlcResult = 1;

                    //float value = 0.14f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "短路测试",
                        Value = value,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 短路电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_2 通路测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_2_PERC(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string PercValue = await keithley2790Helper2.GetMeasurementsAsync(LocalConfig.Instance.ST40_2_IP, LocalConfig.Instance.ST40_2_Port, 3000, CommandPERC);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 通路电阻 {PercValue}", true);

                    CommMethod.TryExtractScientificFloat(PercValue, out float value);

                    ctx.WritePlcResult = 1;

                    //float value = 0.21f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "通路测试",
                        Value = value,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 通路电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_2 绝缘测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_2_VOLT(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string VoltValue = await keithley2790Helper2.GetMeasurementsAsync(LocalConfig.Instance.ST40_2_IP, LocalConfig.Instance.ST40_2_Port, 3000, CommandVOLT);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 绝缘电阻 {VoltValue}", true);

                    CommMethod.TryExtractScientificFloat(VoltValue, out float value);

                    ctx.WritePlcResult = 1;

                    //float value = 0.22f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "绝缘测试",
                        Value = value / 1000000,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 绝缘电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_2 桥丝测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_2_OPEN(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string OpenValue = await keithley2790Helper2.GetMeasurementsAsync(LocalConfig.Instance.ST40_2_IP, LocalConfig.Instance.ST40_2_Port, 3000, CommandOPEN);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 桥丝电阻 {OpenValue}", true);

                    CommMethod.TryExtractScientificFloat(OpenValue, out float value);

                    ctx.WritePlcResult = 1;

                    //float value = 0.23f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "桥丝测试",
                        Value = value,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 桥丝电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }

        /// <summary>
        /// ST40_2 短路测试
        /// </summary>
        /// <param name="standardProcess"></param>
        /// <returns></returns>
        private BaseProcessExecutor CreateUploadExecutorST40_2_CLOSE(StandardProcess standardProcess)
        {
            var specificHandlers = new Dictionary<string, Func<ProcessContext, Task>>
            {
                [ProcessStageKeys.AfterSnRead] = async ctx =>
                {
                    string closeValue = await keithley2790Helper2.GetMeasurementsAsync(LocalConfig.Instance.ST40_2_IP, LocalConfig.Instance.ST40_2_Port, 3000, CommandCLOSE);
                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 短路电阻 {closeValue}", true);

                    CommMethod.TryExtractScientificFloat(closeValue, out float value);

                    ctx.WritePlcResult = 1;

                    //float value = 0.24f;

                    List<IMeasurement> lsMeasurements = new List<IMeasurement>();
                    lsMeasurements.Add(new Measurement<float>
                    {
                        Name = "短路测试",
                        Value = value,
                        Unit = "Ω",
                        Status = "OK"
                    });
                    ProcessMethods.WritePlcSwitch(standardProcess.PlcWriteConfig.WriteItems, lsMeasurements, reader);

                    DisplayLog.Info($"流程 【{standardProcess.Name}】【{ctx.Sn}】 短路电阻" + value.ToString(), true);
                }
            };

            return _executorFactory.CreateExecutor(standardProcess, specificHandlers);
        }


    }
}
