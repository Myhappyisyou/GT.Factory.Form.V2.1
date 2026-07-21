using GT_Common.Model;
using GT_Common.MyEnum;
using GT_Common.ProcessConfig;
using GT_Common.Util;
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
using static GT_Common.Config;
using Consumables = TaskContracts.Models.Consumables;

namespace GT_Common.Helper
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
                    Logging.DisplayLog.Error("解析路径时间失败", ex);
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

        public static void SyncFieldConfig(StandardProcess[] processes, String process_no)
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
                        Save(processNo,
                             ref index,
                             $"{group.Name}上限");

                        Save(processNo,
                             ref index,
                             $"{group.Name}下限");

                        Save(processNo,
                             ref index,
                             $"{group.Name}实际值");

                        Save(processNo,
                             ref index,
                             $"{group.Name}测试结果");
                    }
                }
            }
        }

        private static void Save(string processNo,
                                 ref int index,
                                 string fieldNameCn)
        {
            UploadSql.SaveField(
                processNo,
                $"data{index++:D3}",
                fieldNameCn);
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
