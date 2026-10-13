using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain;
using GT.Factory.ProductCode.Domain.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GT.Factory.ProductCode.Processor
{
    /// <summary>
    /// 日期 Segment 处理器。
    ///
    /// Date 采用 Parts + Mode + Mapping 的组合方式。
    /// 新增日期表达方式只需要扩展这里的 Part/Mode，
    /// 不需要修改 ProductCodeGenerator 或 SQL。
    /// </summary>
    public class DateProcessor : Generator.ICodeSegmentProcessor
    {
        public CodeSegmentType SegmentType
        {
            get { return CodeSegmentType.Date; }
        }

        public string Process(CodeRuleDetail detail, DateTime now)
        {
            DateConfig config =
                JsonConvert.DeserializeObject<DateConfig>(detail.ConfigJson);

            if (config == null || config.Parts == null || config.Parts.Count == 0)
            {
                throw new InvalidOperationException("Date 必须至少配置一个 Part。");
            }

            StringBuilder builder = new StringBuilder();

            foreach (DatePartConfig part in config.Parts)
            {
                if (part == null || string.IsNullOrWhiteSpace(part.Part))
                {
                    continue;
                }

                builder.Append(ProcessPart(part, now));
            }

            return builder.ToString();
        }

        private string ProcessPart(DatePartConfig part, DateTime now)
        {
            string raw = GetRawValue(part.Part, now);
            string mode = part.Mode ?? "Number";

            if (string.Equals(mode, "Mapping", StringComparison.OrdinalIgnoreCase))
            {
                if (part.Mapping == null || part.Mapping.Values == null)
                {
                    throw new InvalidOperationException(
                        "日期 Part 使用 Mapping 时必须提供 Mapping.Values。");
                }

                string mapped;
                if (!part.Mapping.Values.TryGetValue(raw, out mapped))
                {
                    throw new InvalidOperationException(
                        "日期映射不存在：" + part.Part + "=" + raw);
                }

                return mapped ?? string.Empty;
            }

            if (string.Equals(mode, "Full", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(part.Part, "Year", StringComparison.OrdinalIgnoreCase))
                {
                    return now.Year.ToString("0000", CultureInfo.InvariantCulture);
                }

                throw new InvalidOperationException(
                    "Mode=Full 目前只支持 Year。");
            }

            if (string.Equals(mode, "LastTwo", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(part.Part, "Year", StringComparison.OrdinalIgnoreCase))
                {
                    return (now.Year % 100).ToString("00", CultureInfo.InvariantCulture);
                }

                throw new InvalidOperationException(
                    "Mode=LastTwo 目前只支持 Year。");
            }

            if (string.Equals(mode, "TwoDigit", StringComparison.OrdinalIgnoreCase))
            {
                int value = int.Parse(raw, CultureInfo.InvariantCulture);
                return value.ToString("00", CultureInfo.InvariantCulture);
            }

            if (string.Equals(mode, "ThreeDigit", StringComparison.OrdinalIgnoreCase))
            {
                int value = int.Parse(raw, CultureInfo.InvariantCulture);
                return value.ToString("000", CultureInfo.InvariantCulture);
            }

            if (string.Equals(mode, "Number", StringComparison.OrdinalIgnoreCase))
            {
                return int.Parse(raw, CultureInfo.InvariantCulture)
                    .ToString(CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException(
                "不支持的日期 Mode：" + mode);
        }

        private string GetRawValue(string part, DateTime now)
        {
            if (string.Equals(part, "Year", StringComparison.OrdinalIgnoreCase))
            {
                return now.Year.ToString(CultureInfo.InvariantCulture);
            }

            if (string.Equals(part, "Month", StringComparison.OrdinalIgnoreCase))
            {
                return now.Month.ToString(CultureInfo.InvariantCulture);
            }

            if (string.Equals(part, "Day", StringComparison.OrdinalIgnoreCase))
            {
                return now.Day.ToString(CultureInfo.InvariantCulture);
            }

            if (string.Equals(part, "DayOfYear", StringComparison.OrdinalIgnoreCase))
            {
                return now.DayOfYear.ToString(CultureInfo.InvariantCulture);
            }

            throw new InvalidOperationException("不支持的日期 Part：" + part);
        }
    }
}
