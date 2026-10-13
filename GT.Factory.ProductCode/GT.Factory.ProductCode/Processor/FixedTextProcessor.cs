using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain;
using GT.Factory.ProductCode.Domain.Config;
using Newtonsoft.Json;
using System;

namespace GT.Factory.ProductCode.Processor
{
    /// <summary>
    /// 固定文本 Segment。
    /// </summary>
    public class FixedTextProcessor : Generator.ICodeSegmentProcessor
    {
        public CodeSegmentType SegmentType
        {
            get { return CodeSegmentType.FixedText; }
        }

        public string Process(CodeRuleDetail detail, DateTime date)
        {
            FixedTextConfig config =
                JsonConvert.DeserializeObject<FixedTextConfig>(detail.ConfigJson);

            if (config == null || config.Text == null)
            {
                throw new InvalidOperationException("FixedText 配置不能为空。");
            }

            return config.Text;
        }
    }
}
