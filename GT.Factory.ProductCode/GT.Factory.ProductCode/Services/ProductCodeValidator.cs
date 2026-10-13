using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain.Config;
using System;
using System.Collections.Generic;

namespace GT.Factory.ProductCode.Services
{
    /// <summary>
    /// 产品码来料校验器。
    ///
    /// 用途：
    /// 判断一个外部来料产品码是否属于当前产品型号。
    ///
    /// 当前校验范围非常明确：
    /// 1. 校验产品码总长度。
    /// 2. 校验规则中所有 FixedText Segment。
    ///
    /// 以下内容不参与来料型号判断：
    /// 1. Date
    /// 2. Shift
    /// 3. SerialNumber
    ///
    /// 原因：
    /// 来料产品码可能是历史生产的产品，
    /// 因此不能要求日期是当前日期、班次是当前班次，
    /// 也不能要求流水号必须存在于当前数据库序列中。
    ///
    /// 这个类只回答一个问题：
    /// “这个产品码的结构是否属于这个型号？”
    /// </summary>
    public class ProductCodeValidator
    {
        /// <summary>
        /// 校验产品码是否符合指定规则。
        /// </summary>
        /// <param name="productCode">需要校验的产品码。</param>
        /// <param name="rule">产品码规则。</param>
        /// <returns>校验结果。</returns>
        public ProductCodeValidationResult Validate(
            string productCode,
            CodeRule rule)
        {
            ProductCodeValidationResult result =
                new ProductCodeValidationResult();

            result.ProductCode = productCode ?? string.Empty;

            // 规则不能为空。
            if (rule == null)
            {
                return CreateFailure(
                    productCode,
                    "产品码规则不能为空。");
            }

            // 产品码不能为空。
            if (string.IsNullOrEmpty(productCode))
            {
                return CreateFailure(
                    productCode,
                    "产品码不能为空。");
            }

            // 规则必须存在 Segment。
            if (rule.Details == null || rule.Details.Count == 0)
            {
                return CreateFailure(
                    productCode,
                    "产品码规则没有配置 Segment。");
            }

            // 按 SegmentOrder 排序。
            List<CodeRuleDetail> details =
                new List<CodeRuleDetail>(rule.Details);

            details.Sort(delegate (CodeRuleDetail x, CodeRuleDetail y)
            {
                return x.SegmentOrder.CompareTo(y.SegmentOrder);
            });

            // ---------------------------------------------------------
            // 第一项：校验产品码总长度
            // ---------------------------------------------------------
            int expectedLength = CalculateExpectedLength(details);

            if (expectedLength <= 0)
            {
                return CreateFailure(
                    productCode,
                    "无法根据产品码规则确定产品码长度。");
            }

            if (productCode.Length != expectedLength)
            {
                return CreateFailure(
                    productCode,
                    string.Format(
                        "产品码长度不正确。要求 {0} 位，实际 {1} 位。",
                        expectedLength,
                        productCode.Length));
            }

            // ---------------------------------------------------------
            // 第二项：校验所有 FixedText
            // ---------------------------------------------------------
            int currentPosition = 0;

            foreach (CodeRuleDetail detail in details)
            {
                if (detail == null || !detail.Enabled)
                {
                    continue;
                }

                // FixedText 才需要参与型号判断。
                if (detail.SegmentType != CodeSegmentType.FixedText)
                {
                    // 非固定字符 Segment：
                    // Date / Shift / SerialNumber 等全部跳过。
                    //
                    // 但是必须推进位置，
                    // 因为后面的 FixedText 可能位于产品码中间。
                    currentPosition += GetSegmentLength(detail);
                    continue;
                }

                string fixedText = GetFixedText(detail);

                if (string.IsNullOrEmpty(fixedText))
                {
                    return CreateFailure(
                        productCode,
                        string.Format(
                            "Segment {0} 的固定字符不能为空。",
                            detail.SegmentOrder));
                }

                if (currentPosition + fixedText.Length >
                    productCode.Length)
                {
                    return CreateFailure(
                        productCode,
                        string.Format(
                            "Segment {0} 的固定字符超出产品码范围。",
                            detail.SegmentOrder));
                }

                string actualText =
                    productCode.Substring(
                        currentPosition,
                        fixedText.Length);

                if (!string.Equals(
                    actualText,
                    fixedText,
                    StringComparison.Ordinal))
                {
                    return CreateFailure(
                        productCode,
                        string.Format(
                            "Segment {0} 固定字符不匹配。要求：{1}，实际：{2}。",
                            detail.SegmentOrder,
                            fixedText,
                            actualText));
                }

                currentPosition += fixedText.Length;
            }

            // 所有校验通过。
            result.IsValid = true;
            result.ErrorMessage = string.Empty;

            return result;
        }

        /// <summary>
        /// 根据规则计算产品码总长度。
        ///
        /// 注意：
        /// 这里不解析 Date / Shift / Serial 的具体内容，
        /// 只使用它们在规则中声明的长度。
        /// </summary>
        private int CalculateExpectedLength(
            List<CodeRuleDetail> details)
        {
            int length = 0;

            foreach (CodeRuleDetail detail in details)
            {
                if (detail == null || !detail.Enabled)
                {
                    continue;
                }

                length += GetSegmentLength(detail);
            }

            return length;
        }

        /// <summary>
        /// 获取 Segment 占用的产品码长度。
        /// </summary>
        private int GetSegmentLength(CodeRuleDetail detail)
        {
            if (detail == null)
            {
                return 0;
            }

            if (detail.SegmentType == CodeSegmentType.FixedText)
            {
                string fixedText = GetFixedText(detail);
                return fixedText == null ? 0 : fixedText.Length;
            }

            if (detail.SegmentType == CodeSegmentType.SerialNumber)
            {
                SerialConfig config =
                    Newtonsoft.Json.JsonConvert
                        .DeserializeObject<SerialConfig>(
                            detail.ConfigJson);

                if (config == null)
                {
                    return 0;
                }

                return config.Length;
            }

            if (detail.SegmentType == CodeSegmentType.Date)
            {
                return GetDateLength(detail);
            }

            if (detail.SegmentType == CodeSegmentType.Shift)
            {
                // Shift 当前按规则中实际生成的字符长度处理。
                // 当前系统班次为单字符，例如 D / N。
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// 获取 FixedText 的实际配置内容。
        /// </summary>
        private string GetFixedText(CodeRuleDetail detail)
        {
            FixedTextConfig config =
                Newtonsoft.Json.JsonConvert
                    .DeserializeObject<FixedTextConfig>(
                        detail.ConfigJson);

            if (config == null)
            {
                return string.Empty;
            }

            return config.Text ?? string.Empty;
        }

        /// <summary>
        /// 计算 Date Segment 的长度。
        ///
        /// 这里不验证日期值，只计算它在产品码中占用多少字符。
        /// </summary>
        private int GetDateLength(CodeRuleDetail detail)
        {
            DateConfig config =
                Newtonsoft.Json.JsonConvert
                    .DeserializeObject<DateConfig>(
                        detail.ConfigJson);

            if (config == null ||
                config.Parts == null)
            {
                return 0;
            }

            int length = 0;

            foreach (DatePartConfig part in config.Parts)
            {
                if (part == null ||
                    string.IsNullOrWhiteSpace(part.Part))
                {
                    continue;
                }

                string mode = part.Mode ?? "Number";

                if (string.Equals(
                    mode,
                    "Full",
                    StringComparison.OrdinalIgnoreCase))
                {
                    // Full 当前只支持 Year。
                    length += 4;
                }
                else if (string.Equals(
                    mode,
                    "LastTwo",
                    StringComparison.OrdinalIgnoreCase))
                {
                    length += 2;
                }
                else if (string.Equals(
                    mode,
                    "TwoDigit",
                    StringComparison.OrdinalIgnoreCase))
                {
                    length += 2;
                }
                else if (string.Equals(
                    mode,
                    "ThreeDigit",
                    StringComparison.OrdinalIgnoreCase))
                {
                    length += 3;
                }
                else if (string.Equals(
                    mode,
                    "Mapping",
                    StringComparison.OrdinalIgnoreCase))
                {
                    // Mapping 的长度理论上由映射值决定。
                    // 取当前配置中最长的映射值。
                    int maxLength = 0;

                    if (part.Mapping != null &&
                        part.Mapping.Values != null)
                    {
                        foreach (string value in
                            part.Mapping.Values.Values)
                        {
                            if (value != null &&
                                value.Length > maxLength)
                            {
                                maxLength = value.Length;
                            }
                        }
                    }

                    length += maxLength;
                }
                else
                {
                    // Number：
                    // 当前配置的日期 Number 通常为：
                    // Month / Day 等实际生成位数。
                    //
                    // 为了保持校验与生成规则一致，
                    // 这里按照具体 Part 判断。
                    if (string.Equals(
                        part.Part,
                        "Year",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        length += 4;
                    }
                    else if (string.Equals(
                        part.Part,
                        "Month",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        length += 1;
                    }
                    else if (string.Equals(
                        part.Part,
                        "Day",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        length += 1;
                    }
                    else if (string.Equals(
                        part.Part,
                        "DayOfYear",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        length += 1;
                    }
                }
            }

            return length;
        }

        /// <summary>
        /// 创建失败结果。
        /// </summary>
        private ProductCodeValidationResult CreateFailure(
            string productCode,
            string message)
        {
            return new ProductCodeValidationResult
            {
                IsValid = false,
                ProductCode = productCode ?? string.Empty,
                ErrorMessage = message
            };
        }
    }

    /// <summary>
    /// 产品码校验结果。
    /// </summary>
    public class ProductCodeValidationResult
    {
        /// <summary>
        /// 是否通过校验。
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// 被校验的产品码。
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// 校验失败原因。
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
