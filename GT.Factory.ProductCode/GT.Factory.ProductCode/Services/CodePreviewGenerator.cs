using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain;
using GT.Factory.ProductCode.Generator;
using GT.Factory.ProductCode.Repository;
using GT.Factory.ProductCode.Processor;
using System;
using System.Collections.Generic;

namespace GT.Factory.ProductCode.Services
{
    /// <summary>
    /// 产品码预览生成器。
    ///
    /// 预览与正式生成共用 Processor，避免 UI 自己维护一套日期/流水规则。
    /// Serial 在预览时使用模拟流水，不访问数据库。
    /// </summary>
    public class CodePreviewGenerator
    {
        public string GeneratePreview(
            CodeRule rule)
        {

           
            ProcessorRegistry registry = new ProcessorRegistry(
                new ICodeSegmentProcessor[]
                {
                    new PreviewFixedTextProcessor(),
                    new PreviewDateProcessor(),
                    new PreviewShiftProcessor(),
                    new PreviewSerialProcessor()
                });

            ProductCodeGenerator generator = new ProductCodeGenerator(registry);

            return generator.Generate(rule,DateTime.Now);
        }

        private class PreviewFixedTextProcessor : FixedTextProcessor { }

        private class PreviewDateProcessor : DateProcessor { }


        private class PreviewShiftProcessor : PreviewShiftProcessorBase { }


        private class PreviewSerialProcessor : ICodeSegmentProcessor
        {
            private readonly string _value;

            public PreviewSerialProcessor()
            {
                _value = "1";
            }

            public GT.Factory.Common.Enums.ProductCodeEnums.CodeSegmentType SegmentType
            {
                get { return GT.Factory.Common.Enums.ProductCodeEnums.CodeSegmentType.SerialNumber; }
            }

            public string Process(CodeRuleDetail detail, DateTime date)
            {
                GT.Factory.ProductCode.Domain.Config.SerialConfig config =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<GT.Factory.ProductCode.Domain.Config.SerialConfig>(detail.ConfigJson);

                string value = string.IsNullOrEmpty(_value) ? "1" : _value;
                char padding = string.IsNullOrEmpty(config.PaddingChar) ? '0' : config.PaddingChar[0];

                if (config.Length <= 0)
                {
                    throw new InvalidOperationException("Serial Length 必须大于 0。");
                }

                if (_value.Length > config.Length)
                {
                    throw new InvalidOperationException("预览流水号超过配置长度。");
                }

                return value.PadLeft(config.Length, padding);
            }
        }

        private class PreviewShiftProcessorBase : ICodeSegmentProcessor
        {
            public GT.Factory.Common.Enums.ProductCodeEnums.CodeSegmentType SegmentType
            {
                get { return GT.Factory.Common.Enums.ProductCodeEnums.CodeSegmentType.Shift; }
            }

            public string Process(CodeRuleDetail detail, DateTime date)
            {
               
                return "D";
            }
        }
    }
}
