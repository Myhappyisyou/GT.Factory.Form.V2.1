using GT.Factory.Common.Enums;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers;
using GT.Factory.Station.Model;
using GT.Factory.Station.MyEnum;
using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.Util;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Consumables = TaskContracts.Models.Consumables;
using LogicalTable = GT.Factory.Common.Enums.LogicalTable;
using TablePrefixType = GT.Factory.Common.Enums.TablePrefixType;
using GT.Factory.Logging.Logging;

namespace GT.Factory.Station.Helper
{
    public static class CommMethod
    {
        public static Regex imagePathmm = new Regex(@"\d{12}");
        public static Regex imagePathsss = new Regex(@"\d{15}");
        public static string AnalyzeImagePath(string bar_no, Regex regex)
        {
            // 原始字符串
            string originalString = bar_no;
            // 使用正则表达式匹配日期时间部分
            Match match = regex.Match(originalString);
            if (match.Success)
            {
                try
                {
                    // 提取匹配的日期时间字符串
                    string dateTimeString = match.Value;
                    // 将提取的字符串解析为 DateTime 对象
                    // 格式为 "yyyyMMddHHmm" 表示：年份 (yyyy), 月份 (MM), 日期 (dd), 小时 (HH), 分钟 (mm)
                    DateTime parsedDateTime = DateTime.ParseExact(dateTimeString, "yyyyMMddHHmm", null);

                    return parsedDateTime.ToString("yyyy-MM-dd");
                }
                catch (Exception ex)
                {
                    DisplayLog.Error("解析路径时间失败", ex);
                    return "";
                }
            }
            else
            {
                return "";
            }
        }


        #region 反馈文件路径

        public static string FtpFilePath(string Process_name, string sn, string fileSn, FileItemConfig fileConfig)
        {
            string CurrentDate = "";
            string fileName = "";
            string ftpPath = "";
            //文件类型是图片
            if (fileConfig.FileType == "img")
            {
                //判断反馈时间戳是否合格 截取日期

                // 判断相机类型
                if (fileConfig.CameraType == 2)
                {
                    if (fileSn != "" && fileSn.Length > 6)
                    {
                        CurrentDate = fileSn.Substring(0, 6);
                        fileName = $"{sn}_{fileSn.Insert(6, "_")}.{fileConfig.FileExtension}";
                    }
                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{CurrentDate}/{fileConfig.CameraName}/{fileName}";
                }
                // 判断相机类型 无SN
                else if (fileConfig.CameraType == 21)
                {
                    if (fileSn != "" && fileSn.Length > 6)
                    {
                        CurrentDate = fileSn.Substring(0, 6);
                        fileName = $"{fileSn.Insert(6, "_")}.{fileConfig.FileExtension}";
                    }
                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{CurrentDate}/{fileConfig.CameraName}/{fileName}";
                }
                else if (fileConfig.CameraType == 3)
                {
                    if (fileSn != "" && fileSn.Length > 6)
                    {
                        CurrentDate = fileSn.Substring(0, 6);
                        fileName = $"{sn}_{fileSn.Insert(6, "_")}_IMG_INTENSITY.{fileConfig.FileExtension}";
                    }

                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{CurrentDate}/{fileName}";
                }
                else if (fileConfig.CameraType == 4) //OP1400海康图片
                {
                    string CurrentMonth = "";
                    if (fileSn != "" && fileSn.Length > 6)
                    {
                        CurrentMonth = fileSn.Substring(0, 6);
                        CurrentDate = fileSn.Substring(0, 8);
                    }
                    fileName = $"1_{fileSn}.{fileConfig.FileExtension}";

                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{CurrentMonth}/{CurrentDate}/{fileName}";
                }
                else if (fileConfig.CameraType == 5) //OP1500焊锡图片
                {
                    if (fileSn != "" && fileSn.Length > 6)
                    {
                        CurrentDate = fileSn.Split(new char[] { '_' })[0].Substring(0, 8);
                    }
                    fileName = $"{fileSn}.{fileConfig.FileExtension}";

                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{CurrentDate}/{fileName}";
                }
                else if (fileConfig.CameraType == 6) //CCDType 6 cm压机img文件
                {
                    string loaclPath = HightFindFiles.CheckFileInCurrentAndPreviousDay(fileConfig.CameraName, sn).First();

                    ftpPath = FtpHelper.UploadFileToFtp(fileConfig.FolderName, loaclPath).Replace(CommonConfig.FtpPath, CommonConfig.SanhuaFtpPath);
                }
                else if (fileConfig.CameraType == 7) //CCDType 7 焊接img文件
                {
                    string loaclPath = HightFindFiles.GetFileInCurrentAndPreviousDay(fileConfig.CameraName);

                    ftpPath = FtpHelper.UploadFileToFtp(fileConfig.FolderName, Path.Combine(loaclPath, $"{fileSn}.{fileConfig.FileExtension}")).Replace(CommonConfig.FtpPath, CommonConfig.SanhuaFtpPath);
                }
            }
            else if (fileConfig.FileType == "csv")
            {
                // CCDType 6 cm压机img文件 

                if (fileConfig.CameraType == 6)
                {
                    ftpPath = HightFindFiles.CheckFileInCurrentAndPreviousDay(fileConfig.CameraName, sn).First();
                }
                // CCDType 7 奇石乐压机csv文件 无时间戳

                else if (fileConfig.CameraType == 7)
                {
                    fileName = $"{sn}.{fileConfig.FileExtension}";
                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{fileName}";
                }
                else
                {
                    ftpPath = $"{CommonConfig.SanhuaFtpPath}/{fileConfig.FolderName}/{fileName}";
                }
            }
            return ftpPath;
        }

        #endregion

        #region 自定义DGV背景色

        public static void ApplyRowHighlightRules(MergedDataGridView grid, int ColumnIndex, string ContainsText, Color? HighlightColor = null, Color? foreColor = null)
        {
            grid.RowBackColorProvider = rowIndex =>
            {
                if (rowIndex >= 0 && rowIndex < grid.RowCount)
                {
                    var value = grid[ColumnIndex, rowIndex]?.Value?.ToString();
                    if (!string.IsNullOrEmpty(value) && value.Contains(ContainsText))
                    {
                        return HighlightColor;
                    }
                }
                return null;
            };
        }



        #endregion

        #region 自定义异步

        public static void SafeInvoke(this Control control, Action action)
        {
            if (control == null || control.IsDisposed) return;

            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
            //if (control.IsHandleCreated && !control.IsDisposed)
            //{
            //    control.Invoke(action);
            //}
        }

        public static void SafeInvokeDebug(this Control control, Action action)
        {
            if (control.IsHandleCreated && !control.IsDisposed)
            {
                try
                {
                    control.Invoke(action);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"控件类型: {control.GetType().FullName}");
                    Console.WriteLine($"控件 Name: {control.Name}");
                    Console.WriteLine($"控件 Handle: {control.Handle}");
                    Console.WriteLine(ex);
                    throw;
                }
            }
            else
            {
                Console.WriteLine($"控件未创建或已释放: {control.GetType().FullName}, Name: {control.Name}");
            }
        }


        /// <summary>
        /// 支持 async/await 的线程安全 UI 调用方法，兼容 .NET Framework 4.5+
        /// </summary>
        public static Task SafeInvokeAsync(this Control control, Func<Task> asyncAction)
        {
            if (control.InvokeRequired)
            {
                var tcs = new TaskCompletionSource<object>();

                control.BeginInvoke(new Action(() =>
                {
                    // 显式执行异步方法
                    var task = asyncAction();
                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            tcs.SetException(t.Exception.InnerException ?? t.Exception);
                        else if (t.IsCanceled)
                            tcs.SetCanceled();
                        else
                            tcs.SetResult(null);
                    }, TaskScheduler.Default);
                }));

                return tcs.Task;
            }
            else
            {
                return asyncAction();
            }
        }

        #endregion

        #region PLC 数据解析（稳定、单一职责）

        public static List<PlcTestGroup> ParsePlcData(List<PlcMeasureGroup> groups)
        {
            var result = new List<PlcTestGroup>();

            foreach (var g in groups)
            {
                result.Add(new PlcTestGroup
                {
                    Name = g.Name,
                    Upper = g.Upper.Value.ToString(),
                    Lower = g.Lower.Value.ToString(),
                    Value = g.Value.Value.ToString(),
                    Result = ((int)g.Result.Value) == 1 ? "OK" : "NG"
                });
            }

            return result;
        }


        #endregion


        /// <summary>
        /// 将仪器测试数据转换成系统标准的 PlcMeasureGroup 数据格式。
        /// 仪器数据不要求必须包含上下限，未提供的上下限统一保持为空。
        /// 仪器最终结果会应用到每一个测试项，保证后续 BuildSaveItems 可以继续使用。
        /// </summary>
        public static List<PlcMeasureGroup> ConvertInstrumentData(
            IEnumerable<InstrumentMeasure> measures,
            bool isOk)
        {
            // 创建标准数据集合。
            List<PlcMeasureGroup> result =
                new List<PlcMeasureGroup>();

            // 将仪器最终结果转换成现有系统使用的 1/0。
            int resultValue = isOk ? 1 : 0;

            // 遍历仪器返回的所有测试项。
            foreach (InstrumentMeasure item in measures)
            {
                // 跳过无效的测试项。
                if (item == null)
                    continue;

                // 创建标准测点。
                PlcMeasureGroup measure =
                    new PlcMeasureGroup();

                // 设置业务主键。
                measure.FieldName = item.FieldName;

                // 设置显示名称。
                measure.Name = item.Name;

                // 设置单位。
                measure.Unit = item.Unit ?? string.Empty;

                // 仪器没有上下限时，Value 保持为空。
                measure.Upper.Value = item.Upper;
                measure.Lower.Value = item.Lower;

                // 设置仪器实际测量值。
                measure.Value.Value = item.Value;

                // 仪器最终结果统一转换成 1/0。
                measure.Result.Value = resultValue;

                // 加入标准数据集合。
                result.Add(measure);
            }

            return result;
        }

        /// <summary>
        /// 将新仪器架构返回的 IMeasurement 数据转换成系统统一的
        /// PlcMeasureGroup 数据。
        ///
        /// 该方法用于 InstrumentProcess 的 MES / 数据库数据流。
        /// PLC 数据流仍然直接使用原始 IMeasurement。
        ///
        /// 结果处理规则：
        /// 1. 如果 Handler 能获取每个测试项的独立结果，
        ///    IMeasurement.Status 保存对应测试项结果。
        /// 2. 如果仪器只有一个最终结果，
        ///    Handler 将该最终结果写入每个 IMeasurement.Status。
        /// 3. 本方法不再重新计算或覆盖单项结果。
        /// 4. 上限和下限当前允许为空。
        /// </summary>
        public static List<PlcMeasureGroup> ConvertInstrumentData(
            IEnumerable<IMeasurement> measurements)
        {
            // 创建统一测试数据集合。
            List<PlcMeasureGroup> result =
                new List<PlcMeasureGroup>();

            if (measurements == null)
                return result;

            foreach (IMeasurement item in measurements)
            {
                // 跳过无效测试项。
                if (item == null)
                    continue;

                // 创建系统统一测量项。
                PlcMeasureGroup measure =
                    new PlcMeasureGroup();

                // IMeasurement 当前没有独立 FieldName，
                // 因此使用 Name 作为系统字段名称。
                measure.FieldName = item.Name;

                // 设置测试项名称。
                measure.Name = item.Name;

                // 设置测试单位。
                measure.Unit = item.Unit ?? string.Empty;

                // 当前 IMeasurement 不包含上下限，
                // 因此上下限允许为空。
                measure.Upper.Value = null;
                measure.Lower.Value = null;

                // 设置实际测试值。
                measure.Value.Value = item.Value;

                // 使用 Handler 已经确定的测试结果。
                // 如果是单项结果，则这里就是单项结果；
                // 如果仪器只有总结果，则这里就是继承后的总结果。
                measure.Result.Value = item.Status == "OK" ? 1 : 0;

                result.Add(measure);
            }

            return result;
        }

        /// <summary>
        /// 将仪器返回的最终结果统一转换成系统使用的 1/0。
        /// </summary>
        public static int ConvertInstrumentResult(object result)
        {
            if (result == null)
                return 0;

            string text = result.ToString().Trim();

            if (string.Equals(text, "OK", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (string.Equals(text, "PASS", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (string.Equals(text, "NG", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (string.Equals(text, "FAIL", StringComparison.OrdinalIgnoreCase))
                return 0;

            int number;

            if (int.TryParse(text, out number))
                return number == 1 ? 1 : 0;

            return 0;
        }

        #region 参数组名解析

        private static string BuildParameterGroupName(ProcessContext context)
        {
            if (context?.Process == null)
                throw new Exception("ProcessContext.Process 为空");

            string baseName = context.Process.ParameterGroupName;

            var mode = (CalibrationMode)context.CalibrationFlag;

            switch (mode)
            {
                case CalibrationMode.Normal:
                    // 0：正常生产，不加后缀
                    return baseName;

                case CalibrationMode.Standard:
                    return $"{baseName}_标准";

                case CalibrationMode.UpperNg:
                    return $"{baseName}_上限NG";

                case CalibrationMode.LowerNg:
                    return $"{baseName}_下限NG";

                default:
                    throw new Exception($"未知的 CalibrationMode：{context.CalibrationFlag}");
            }
        }

        #endregion

        #region MyRegion

        public static void GetConfigManager(
            ProcessContext context,
            out List<string> testItems,
            out List<string> testItemUnits)
        {
            testItems = new List<string>();
            testItemUnits = new List<string>();

            if (context?.LimitConfig?.Groups == null)
                throw new Exception("LimitConfig 或 Groups 未初始化");

            string parametersName = BuildParameterGroupName(context);

            var group = context.LimitConfig.Groups
                .FirstOrDefault(g => g.ParametersName == parametersName);

            if (group == null)
                throw new Exception($"未找到参数组：{parametersName}");

            foreach (var param in group.Parameters)
            {
                testItems.Add(param.Name);

                switch (param)
                {
                    case LimitParameter p:
                        testItemUnits.Add(p.Unit?.ToString());
                        break;

                    case BooleanParameter p:
                        testItemUnits.Add(p.Unit?.ToString());
                        break;

                    case EnumParameter p:
                        testItemUnits.Add(p.Unit?.ToString());
                        break;

                    case TextParameter p:
                        testItemUnits.Add(p.Unit?.ToString());
                        break;

                    case IntParameter p:
                        testItemUnits.Add(p.Unit?.ToString());
                        break;

                    default:
                        testItemUnits.Add(string.Empty);
                        break;
                }
            }
        }

        private static async Task SaveAsync(string processNo,
                                            int index,
                                            string fieldNameCn)
        {
            await AppRuntime.Current.DataRuntime.Repositories.Processes.SaveFieldAsync(processNo,
                $"data{index:D3}",
                fieldNameCn);
            //UploadSql.SaveField(
            //    processNo,
            //    $"data{index++:D3}",
            //    fieldNameCn);
        }

        public static async Task SyncFieldConfigAsync(StandardProcess[] processes, String process_no)
        {
            if (processes == null || processes.Length == 0)
                return;

            foreach (var processGroup in processes.GroupBy(p => p.ProcessNo))
            {
                string processNo = process_no;

                int index = 1;

                foreach (var process in processGroup)
                {
                    if (process.PlcReadConfig?.IsEnabled != true)
                        continue;

                    if (process.PlcReadConfig.MeasureGroups == null)
                        continue;

                    foreach (var group in process.PlcReadConfig.MeasureGroups)
                    {
                        await SaveAsync(processNo, index++, $"{group.Name}上限");
                        await SaveAsync(processNo, index++, $"{group.Name}下限");
                        await SaveAsync(processNo, index++, $"{group.Name}实际值");
                        await SaveAsync(processNo, index++, $"{group.Name}测试结果");
                    }
                }
            }
        }
        #endregion


        // 如果查询结果可能包含其他格式，可以使用更安全的方法
        public static int CountMatchingRecordsSafe(List<string> queryResults, string[] targetNumbers)
        {

            // 检查是否包含不在目标数组中的数字
            bool hasNonTargetNumber = queryResults
                .Select(ngMsg => ngMsg.Split('_').FirstOrDefault())
                .Any(numberPart => !string.IsNullOrEmpty(numberPart) && !targetNumbers.Contains(numberPart));


            // 否则返回匹配目标数组的记录数量
            return queryResults
                .Count(ngMsg =>
                {
                    string numberPart = ngMsg.Split('_').FirstOrDefault();
                    return !string.IsNullOrEmpty(numberPart) && targetNumbers.Contains(numberPart);
                });

        }

        /// <summary>
        /// 统计 queryResults 中包含目标数的次数（更安全的版本）
        /// </summary>
        public static int CountMatchingRecordsSafe(List<string> queryResults)
        {
            if (queryResults == null)
                return 0;

            int count = 0;

            foreach (var msg in queryResults)
            {
                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                string numberPart = msg.Split('_').FirstOrDefault();
                if (string.IsNullOrEmpty(numberPart))
                    continue;

                if (int.TryParse(numberPart, out int num))
                {
                    // 判断是否在 11400 到 11499 之间
                    if (num >= 11400 && num <= 11499)
                        count++;
                }
            }

            return count;
        }


        public static List<IMeasurement> ParseMeasurements(string rawLog)
        {
            List<IMeasurement> list = new List<IMeasurement>();

            if (string.IsNullOrWhiteSpace(rawLog))
                return list;

            // 找到 CNT
            int cntIndex = rawLog.IndexOf("CNT:");
            if (cntIndex < 0) return list;

            string paramPart = rawLog.Substring(cntIndex);

            // 去掉 CNT 本身
            int firstComma = paramPart.IndexOf(',');
            if (firstComma >= 0)
                paramPart = paramPart.Substring(firstComma + 1);

            var parts = paramPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim().TrimEnd('.'); // ✅ 去掉结尾 .

                if (!part.Contains(":")) continue;

                var kv = part.Split(':');
                if (kv.Length < 2) continue;

                string name = kv[0].Trim();
                string rawValue = kv[1].Trim();

                // 状态
                string status = "OK";
                if (i + 1 < parts.Length && !parts[i + 1].Contains(":"))
                {
                    status = parts[i + 1].Trim().TrimEnd('.');
                    i++;
                }

                // ✅ 更强正则（支持符号单位）
                var match = Regex.Match(rawValue, @"([-+]?\d*\.?\d+)\s*([^\d\s]*)");

                float value = 0;
                string unit = "";

                if (match.Success)
                {
                    float.TryParse(match.Groups[1].Value,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out value);

                    unit = match.Groups[2].Value;
                }

                list.Add(new Measurement<float>
                {
                    Name = name,
                    Value = value,
                    Unit = unit,
                    Status = status
                });
            }

            return list;
        }

        #region 易损件百分比
        public static void CheckAndAlertConsumables(List<Consumables> consumables)
        {
            var seriousItems = consumables.Where(c => c.GetRemainderPercentage() <= 5).ToList();
            var warningItems = consumables.Where(c => c.GetRemainderPercentage() > 5 && c.GetRemainderPercentage() <= 10).ToList();
            var noticeItems = consumables.Where(c => c.GetRemainderPercentage() > 10 && c.GetRemainderPercentage() <= 20).ToList();

            if (seriousItems.Any() || warningItems.Any() || noticeItems.Any())
            {
                StringBuilder alertMessage = new StringBuilder();
                alertMessage.AppendLine("📢 易损件剩余使用次数提醒");
                alertMessage.AppendLine();

                // 严重警告 (≤5%)
                if (seriousItems.Any())
                {
                    alertMessage.AppendLine("🔴 严重警告 - 请立即更换以下易损件 (剩余 ≤ 5%)：");
                    foreach (var item in seriousItems)
                    {
                        alertMessage.AppendLine($"   • {item.Name} (位置: {item.Location})");
                        alertMessage.AppendLine($"     剩余: {item.RemainderCount} 次 ({item.GetRemainderPercentage():F1}%)");
                        alertMessage.AppendLine();
                    }
                }

                // 警告 (5% ~ 10%)
                if (warningItems.Any())
                {
                    alertMessage.AppendLine("🟠 警告 - 以下易损件需要准备更换 (剩余 5% ~ 10%)：");
                    foreach (var item in warningItems)
                    {
                        alertMessage.AppendLine($"   • {item.Name} (位置: {item.Location})");
                        alertMessage.AppendLine($"     剩余: {item.RemainderCount} 次 ({item.GetRemainderPercentage():F1}%)");
                        alertMessage.AppendLine();
                    }
                }

                // 注意 (10% ~ 20%)
                if (noticeItems.Any())
                {
                    alertMessage.AppendLine("🟡 注意 - 以下易损件剩余次数不多 (剩余 10% ~ 20%)：");
                    foreach (var item in noticeItems)
                    {
                        alertMessage.AppendLine($"   • {item.Name} (位置: {item.Location})");
                        alertMessage.AppendLine($"     剩余: {item.RemainderCount} 次 ({item.GetRemainderPercentage():F1}%)");
                        alertMessage.AppendLine();
                    }
                }

                // 显示统计信息
                alertMessage.AppendLine($"📊 统计: 严重({seriousItems.Count}) 警告({warningItems.Count}) 注意({noticeItems.Count})");

                // 显示提示框
                MessageBox.Show(alertMessage.ToString(), "易损件库存提醒",
                    MessageBoxButtons.OK,
                    seriousItems.Any() ? MessageBoxIcon.Error :
                    warningItems.Any() ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
        }
        #endregion


        #region 科学计数字符串转浮点数

        public static bool TryExtractScientificFloat(string input, out float result)
        {
            result = 0f;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // 使用正则表达式提取科学计数法部分
            string pattern = @"[+-]?\d+\.\d+E[+-]\d+";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                string scientificValue = match.Value;
                Console.WriteLine($"提取到的科学计数法: '{scientificValue}'");

                if (float.TryParse(scientificValue,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out result))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 数组NormalizeResultToNgIfAnyNg

        public static void NormalizeResultToNgIfAnyNg(string[] results)
        {
            if (results.Any(r => r == "NG"))
            {
                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = "NG";
                }
            }
        }


        #endregion

    }


    public class PowerData
    {
        public DateTime Timestamp { get; set; }
        public string DeviceId { get; set; }
        public string DataType { get; set; }
        public int Count { get; set; }
        public Dictionary<int, decimal> Measurements { get; set; } = new Dictionary<int, decimal>();

        public override string ToString()
        {
            return $"{Timestamp:MM-dd HH:mm:ss} {DeviceId}:{DataType},CNT:{Count},测量点:{Measurements.Count}个";
        }
    }


    public static class ListExtensions
    {
        public static void RemoveRanges<T>(this List<T> list, List<(int Start, int Length)> ranges)
        {
            // 倒序删除，防止索引错位
            foreach (var (start, length) in ranges.OrderByDescending(r => r.Start))
            {
                if (start >= 0 && start + length <= list.Count)
                {
                    list.RemoveRange(start, length);
                }
            }
        }
    }

    #region 枚举相关方法

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null
                ? value.ToString()
                : attribute.Description;
        }
    }

    #endregion

    #region 数据库表相关方法

    /// <summary>
    /// 统一表名解析器
    /// 负责拼接：前缀 + 实际表名
    /// </summary>
    public static class TableNameResolver
    {
        public static string Get(LogicalTable table)
        {
            var prefixEnum = Config.Instance.TablePrefix;

            var tableName = table.GetDescription();

            // 如果不使用前缀
            if (prefixEnum == TablePrefixType.None)
                return tableName;

            return $"{prefixEnum}{tableName}";
        }
    }

    #endregion
}
