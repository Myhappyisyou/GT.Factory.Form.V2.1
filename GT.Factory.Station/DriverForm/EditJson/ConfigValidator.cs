using GT.Factory.PLC.PlcComm;
using GT.Factory.Station.ProcessConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.EditJson
{
    public class ConfigValidator
    {
        public ValidationResult Validate(PlcConfig config)
        {
            var result = new ValidationResult();

            try
            {
                // 验证基本配置
                ValidateBasicConfig(config, result);

                // ========== 修改：补充所有流程验证 ==========
                ValidateProcesses(config.StandardProcess, "标准流程", result);
                ValidateProcesses(config.CheckResultProcess, "过站检流程", result);
                ValidateProcesses(config.BarBindProcess, "绑定流程", result);
                ValidateProcesses(config.CalibrationProcesses, "点检流程", result);
                ValidateProcesses(config.PeriodProcesses, "抽检流程", result);
                ValidateProcesses(config.ReworkProcess, "返工流程", result);
                ValidateProcesses(config.InstrumentProcesses, "仪器通讯流程", result);

                // 验证报警配置
                if (config.AlarmConfig?.IsNeed == true)
                    ValidateAlarmConfig(config.AlarmConfig, result);

                // 验证设备状态配置
                if (config.EquipmentStata?.IsNeed == true)
                    ValidateEquipmentConfig(config.EquipmentStata, result);
            }
            catch (Exception ex)
            {
                result.AddError("全局验证错误", ex.Message);
            }

            return result;
        }

        private void ValidateBasicConfig(PlcConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.PlcConnection.Ip))
                result.AddError("基本配置", "IP地址不能为空");

            //if (string.IsNullOrWhiteSpace(config.SectionName))
            //    result.AddError("基本配置", "区段名称不能为空");

            if (config.PlcConnection.Length == 0)
                result.AddWarning("基本配置", "读取地址长度为0，可能配置不正确");
        }

        private void ValidateProcesses<T>(
            List<T> processes,
            string category,
            ValidationResult result) where T : ProcessBase
        {
            if (processes == null || processes.Count == 0)
            {
                result.AddWarning(category, $"没有配置任何{category}");
                return;
            }

            foreach (var process in processes)
            {
                try
                {
                    process.Validate();

                    // 完成反馈地址验证
                    if (string.IsNullOrWhiteSpace(process.FinishFeedbackAddress))
                        result.AddWarning(process.Name, "完成反馈地址未设置");

                    // SnConfig 验证（ProcessBase 公共字段）
                    if (process.SnConfig?.IsEnabled == true)
                        ValidateSnConfig(process.SnConfig, process.Name, result);

                    // ResultWriteConfig 验证（ProcessBase 公共字段）
                    if (process.ResultWriteConfig?.IsEnabled == true)
                        ValidateResultWriteConfig(process.ResultWriteConfig, process.Name, result);

                    // 各流程特殊验证
                    if (process is StandardProcess stdProcess)
                        ValidateStandardProcess(stdProcess, result);
                    else if (process is CheckResultProcess checkProcess)
                        ValidateCheckResultProcess(checkProcess, result);
                    else if (process is BarBindProcess bindProcess)
                        ValidateBindProcess(bindProcess, result);
                    else if (process is CalibrationProcess calibProcess)
                        ValidateCalibrationProcess(calibProcess, result);
                    else if (process is PeriodProcess periodProcess)
                        ValidatePeriodProcess(periodProcess, result);
                    else if (process is ReworkProcess reworkProcess)
                        ValidateReworkProcess(reworkProcess, result);
                }
                catch (Exception ex)
                {
                    result.AddError(process.Name, ex.Message);
                }
            }
        }

        // ========== SnConfig 验证 ==========
        private void ValidateSnConfig(SnConfig config, string processName, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.Address))
                result.AddError(processName, "SN码地址不能为空");

            if (!PlcAddressHelper.TryToReadAddress(config.Address, out _))
                result.AddError(processName, $"SN码地址格式不正确: {config.Address}");

            if (config.Length <= 0)
                result.AddError(processName, "SN码读取长度必须大于0");
        }

        // ========== ResultWriteConfig 验证 ==========
        private void ValidateResultWriteConfig(
            ResultWriteConfig config,
            string processName,
            ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.Address))
                result.AddError(processName, "结果回写地址不能为空");
        }

        // ========== StandardProcess 验证 ==========
        private void ValidateStandardProcess(StandardProcess process, ValidationResult result)
        {
            // ResultConfig 验证
            if (process.ResultConfig?.IsEnabled == true)
                ValidateResultConfig(process.ResultConfig, process.Name, result);

            // PlcReadConfig 验证
            if (process.PlcReadConfig?.IsEnabled == true)
                ValidatePlcReadConfig(process.PlcReadConfig, process.Name, result);

            // FileConfig 验证
            if (process.FileConfig?.IsEnabled == true)
                ValidateFileConfig(process.FileConfig, process.Name, result);
        }

        // ========== CheckResultProcess 验证 ==========
        private void ValidateCheckResultProcess(CheckResultProcess process, ValidationResult result)
        {
            if (process.CheckResultInfo == null)
                result.AddWarning(process.Name, "过站检信息未配置");
        }

        // ========== BarBindProcess 验证 ==========
        private void ValidateBindProcess(BarBindProcess process, ValidationResult result)
        {
            if (process.PartInfos == null || !process.PartInfos.Any())
                result.AddWarning(process.Name, "未定义任何部件信息");
            else
            {
                foreach (var part in process.PartInfos)
                {
                    if (string.IsNullOrWhiteSpace(part.Name))
                        result.AddError(process.Name, "部件码名称不能为空");

                    if (part.PlcChannel == null || !part.PlcChannel.IsConfigured())
                        result.AddError(process.Name, $"部件 '{part.Name}' 的PLC通道未配置");
                }
            }
        }

        // ========== CalibrationProcess 验证 ==========
        private void ValidateCalibrationProcess(CalibrationProcess process, ValidationResult result)
        {
            if (process.TypeStationIds == null || !process.TypeStationIds.Any())
                result.AddError(process.Name, "未定义任何点检类型");

            if (process.ResultConfig?.IsEnabled == true)
                ValidateResultConfig(process.ResultConfig, process.Name, result);

            if (process.PlcReadConfig?.IsEnabled == true)
                ValidatePlcReadConfig(process.PlcReadConfig, process.Name, result);

            if (process.FileConfig?.IsEnabled == true)
                ValidateFileConfig(process.FileConfig, process.Name, result);
        }

        // ========== PeriodProcess 验证 ==========
        private void ValidatePeriodProcess(PeriodProcess process, ValidationResult result)
        {
            if (process.TypeStationIds == null || !process.TypeStationIds.Any())
                result.AddError(process.Name, "未定义任何抽检类型");

            if (process.ResultConfig?.IsEnabled == true)
                ValidateResultConfig(process.ResultConfig, process.Name, result);

            if (process.PlcReadConfig?.IsEnabled == true)
                ValidatePlcReadConfig(process.PlcReadConfig, process.Name, result);

            if (process.FileConfig?.IsEnabled == true)
                ValidateFileConfig(process.FileConfig, process.Name, result);
        }

        // ========== ReworkProcess 验证 ==========
        private void ValidateReworkProcess(ReworkProcess process, ValidationResult result)
        {
            if (process.PlcWriteConfig?.IsEnabled == true &&
                (process.PlcWriteConfig.WriteItems == null ||
                 !process.PlcWriteConfig.WriteItems.Any()))
            {
                result.AddWarning(process.Name, "PLC写入配置已启用但未定义写入项");
            }
        }

        // ========== ResultConfig 验证 ==========
        private void ValidateResultConfig(ResultConfig config, string processName, ValidationResult result)
        {
            // ========== 修改：Address 是 string ==========
            if (string.IsNullOrWhiteSpace(config.Address))
                result.AddError(processName, "结果读取地址不能为空");

            else if (!PlcAddressHelper.TryToReadAddress(config.Address, out _))
                result.AddError(processName, $"结果读取地址格式不正确: {config.Address}");
        }

        // ========== PlcReadConfig 验证 ==========
        private void ValidatePlcReadConfig(PlcReadConfig config, string processName, ValidationResult result)
        {
            var groups = config.MeasureGroups;

            if (groups == null || !groups.Any())
            {
                result.AddError(processName, "PLC读取配置已启用但未定义测点组");
                return;
            }

            foreach (var g in groups)
            {
                // ========== 修改：Address 是 string，用 IsConfigured() 判断 ==========
                if (g.Upper != null && g.Upper.IsConfigured())
                {
                    if (!PlcAddressHelper.TryToReadAddress(g.Upper.Address, out _))
                        result.AddError(processName, $"测点 '{g.Name}' 上限地址格式不正确: {g.Upper.Address}");
                }

                if (g.Lower != null && g.Lower.IsConfigured())
                {
                    if (!PlcAddressHelper.TryToReadAddress(g.Lower.Address, out _))
                        result.AddError(processName, $"测点 '{g.Name}' 下限地址格式不正确: {g.Lower.Address}");
                }

                if (g.Value == null || !g.Value.IsConfigured())
                {
                    result.AddError(processName, $"测点 '{g.Name}' 实际值通道未配置");
                }
                else if (!PlcAddressHelper.TryToReadAddress(g.Value.Address, out _))
                {
                    result.AddError(processName, $"测点 '{g.Name}' 实际值地址格式不正确: {g.Value.Address}");
                }

                if (g.Result != null && g.Result.IsConfigured())
                {
                    if (!PlcAddressHelper.TryToReadAddress(g.Result.Address, out _))
                        result.AddError(processName, $"测点 '{g.Name}' 测试结果地址格式不正确: {g.Result.Address}");
                }
            }
        }

        // ========== FileConfig 验证 ==========
        private void ValidateFileConfig(FileConfig config, string processName, ValidationResult result)
        {
            if (config.FileItems == null || !config.FileItems.Any())
            {
                result.AddWarning(processName, "文件配置已启用但未定义文件项");
                return;
            }

            foreach (var item in config.FileItems)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                    result.AddError(processName, "文件项名称不能为空");

                if (string.IsNullOrWhiteSpace(item.Address))
                    result.AddError(processName, $"文件项 '{item.Name}' 码地址不能为空");

                if (item.Length <= 0)
                    result.AddError(processName, $"文件项 '{item.Name}' 长度必须大于0");
            }
        }

        private void ValidateAlarmConfig(AlarmConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.AlarmStartAddress))
                result.AddError("报警配置", "报警起始地址不能为空");

            if (config.AlarmLength == 0)
                result.AddWarning("报警配置", "报警地址长度为0");
        }

        private void ValidateEquipmentConfig(EquipmentStata config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.EquipmentStataAddress))
                result.AddWarning("设备状态", "设备状态地址为0");

            if (string.IsNullOrWhiteSpace(config.EquipmentId))
                result.AddWarning("设备状态", "设备ID未设置");
        }
    }

    public class ValidationResult
    {
        public List<ValidationMessage> Errors { get; } = new List<ValidationMessage>();
        public List<ValidationMessage> Warnings { get; } = new List<ValidationMessage>();

        public bool HasErrors => Errors.Any();
        public bool HasWarnings => Warnings.Any();
        public bool IsValid => !HasErrors;

        public void AddError(string category, string message)
        {
            Errors.Add(new ValidationMessage(category, message));
        }

        public void AddWarning(string category, string message)
        {
            Warnings.Add(new ValidationMessage(category, message));
        }

        public IEnumerable<string> GetAllMessages()
        {
            foreach (var error in Errors)
            {
                yield return $"[错误] {error.Category}: {error.Message}";
            }

            foreach (var warning in Warnings)
            {
                yield return $"[警告] {warning.Category}: {warning.Message}";
            }
        }
    }

    public class ValidationMessage
    {
        public string Category { get; }
        public string Message { get; }

        public ValidationMessage(string category, string message)
        {
            Category = category;
            Message = message;
        }
    }
}
