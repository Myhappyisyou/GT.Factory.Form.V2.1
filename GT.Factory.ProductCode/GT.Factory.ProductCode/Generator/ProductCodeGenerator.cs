using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace GT.Factory.ProductCode.Generator
{
    /// <summary>
    /// 产品码生成器。
    ///
    /// 这是新架构中产品码生成的唯一核心入口。
    /// SQL Server 不再负责解释 Segment 或拼接完整产品码。
    /// </summary>
    public class ProductCodeGenerator
    {
        private readonly ProcessorRegistry _registry;

        public ProductCodeGenerator(ProcessorRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException("registry");
            }

            _registry = registry;
        }

        /// <summary>
        /// 按 SegmentOrder 依次生成完整产品码。
        /// </summary>
        public string Generate(
            CodeRule rule, DateTime generationTime)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }


            if (rule.Details == null || rule.Details.Count == 0)
            {
                throw new InvalidOperationException("产品码规则没有配置 Segment。");
            }

            List<CodeRuleDetail> details =
                new List<CodeRuleDetail>(rule.Details);

            details.Sort(delegate(CodeRuleDetail x, CodeRuleDetail y)
            {
                return x.SegmentOrder.CompareTo(y.SegmentOrder);
            });

            StringBuilder builder = new StringBuilder();
            DateTime now = generationTime;
            foreach (CodeRuleDetail detail in details)
            {
                if (detail == null || !detail.Enabled)
                {
                    continue;
                }

                ICodeSegmentProcessor processor =
                    _registry.Get(detail.SegmentType);

                string value = processor.Process(detail, now);

                if (value == null)
                {
                    value = string.Empty;
                }

                builder.Append(value);
            }

            if (builder.Length == 0)
            {
                throw new InvalidOperationException("产品码规则生成结果为空。");
            }

            return builder.ToString();
        }
    }
}
