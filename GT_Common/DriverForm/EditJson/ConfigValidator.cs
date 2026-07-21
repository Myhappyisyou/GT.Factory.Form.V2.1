using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditJson
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

                // 验证所有流程
                ValidateProcesses(config.StandardProcess, "标准流程", result);
                ValidateProcesses(config.BarBindProcess, "绑定流程", result);
                ValidateProcesses(config.CalibrationProcesses, "点检流程", result);

                // 验证报警配置
                if (config.AlarmConfig?.IsNeed == true)
                {
                    ValidateAlarmConfig(config.AlarmConfig, result);
                }

                // 验证设备状态配置
                if (config.EquipmentStata?.IsNeed == true)
                {
                    ValidateEquipmentConfig(config.EquipmentStata, result);
                }
            }
            catch (Exception ex)
            {
                result.AddError("全局验证错误", ex.Message);
            }

            return result;
        }

        private void ValidateBasicConfig(PlcConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.Ip))
            {
                result.AddError("基本配置", "IP地址不能为空");
            }

            if (string.IsNullOrWhiteSpace(config.SectionName))
            {
                result.AddError("基本配置", "区段名称不能为空");
            }

            if (config.Length == 0)
            {
                result.AddWarning("基本配置", "读取地址长度为0，可能配置不正确");
            }
        }

        private void ValidateProcesses<T>(List<T> processes, string category, ValidationResult result) where T : ProcessBase
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

                    // 特殊验证逻辑
                    if (string.IsNullOrWhiteSpace(process.FinishFeedbackAddress))
                    {
                        result.AddWarning(process.Name, "完成反馈地址未设置");
                    }

                    if (process is StandardProcess stdProcess)
                    {
                        ValidateStandardProcess(stdProcess, result);
                    }
                    else if (process is BarBindProcess bindProcess)
                        ValidateBindProcess(bindProcess, result);
                    else if (process is CalibrationProcess calibProcess)
                        ValidateCalibrationProcess(calibProcess, result);
                }
                catch (Exception ex)
                {
                    result.AddError(process.Name, ex.Message);
                }
            }
        }

        private void ValidateStandardProcess(StandardProcess process, ValidationResult result)
        {
            if (process.PlcReadConfig?.IsEnabled == true)
            {
                var groups = process.PlcReadConfig.MeasureGroups;
                if (groups == null || !groups.Any())
                {
                    result.AddError(process.Name, "PLC读取配置已启用但未定义测点组");
                }
                else
                {
                    foreach (var g in groups)
                    {
                        if (g.Upper?.Address <= 0)
                            result.AddError(process.Name, $"测点 '{g.Name}' 上限地址无效");

                        if (g.Lower?.Address <= 0)
                            result.AddError(process.Name, $"测点 '{g.Name}' 下限地址无效");

                        if (g.Value?.Address <= 0)
                            result.AddError(process.Name, $"测点 '{g.Name}' 实际值地址无效");

                        if (g.Result?.Address <= 0)
                            result.AddError(process.Name, $"测点 '{g.Name}' 测试结果地址无效");
                    }
                }
            }

            if (process.FileConfig?.IsEnabled == true &&
                (process.FileConfig.FileItems == null || !process.FileConfig.FileItems.Any()))
            {
                result.AddWarning(process.Name, "文件配置已启用但未定义文件项");
            }
        }


        private void ValidateBindProcess(BarBindProcess process, ValidationResult result)
        {
            if (process.PartInfos == null || !process.PartInfos.Any())
            {
                result.AddWarning(process.Name, "未定义任何部件信息");
            }
        }

        private void ValidateCalibrationProcess(CalibrationProcess process, ValidationResult result)
        {
            if (process.TypeStationIds == null || !process.TypeStationIds.Any())
            {
                result.AddError(process.Name, "未定义任何点检类型");
            }
        }

        private void ValidateAlarmConfig(AlarmConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.AlarmStartAddress))
            {
                result.AddError("报警配置", "报警起始地址不能为空");
            }

            if (config.AlarmLength == 0)
            {
                result.AddWarning("报警配置", "报警地址长度为0");
            }
        }

        private void ValidateEquipmentConfig(EquipmentStata config, ValidationResult result)
        {
            if (config.EquipmentStataAddress == 0)
            {
                result.AddWarning("设备状态", "设备状态地址为0");
            }

            if (string.IsNullOrWhiteSpace(config.EquipmentId))
            {
                result.AddWarning("设备状态", "设备ID未设置");
            }
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
