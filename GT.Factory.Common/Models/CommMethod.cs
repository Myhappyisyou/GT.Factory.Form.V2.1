using GT.Factory.Common.Enums;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Common.Helper
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
                    //DisplayLog.Error("解析路径时间失败", ex);
                    return "";
                }
            }
            else
            {
                return "";
            }
        }


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
            //var prefixEnum = Config.Instance.TablePrefix;

            var prefixEnum = TablePrefixType.GT;

            var tableName = table.GetDescription();

            // 如果不使用前缀
            if (prefixEnum == TablePrefixType.None)
                return tableName;

            return $"{prefixEnum}{tableName}";
        }
    }

    #endregion
}
