using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain.Config;
using GT.Factory.ProductCode.Generator;
using GT.Factory.ProductCode.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.ProductCode.Processor
{
    public class SerialProcessor : ICodeSegmentProcessor
    {
        private readonly IProductCodeRepository _repository;
        private readonly int _ruleId;

        public SerialProcessor(
            IProductCodeRepository repository,
            int ruleId)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
            _ruleId = ruleId;
        }

        public CodeSegmentType SegmentType
        {
            get
            {
                return CodeSegmentType.SerialNumber;
            }
        }

        public string Process(
            CodeRuleDetail detail,
            DateTime generationTime)
        {
            SerialConfig config =
                JsonConvert.DeserializeObject<SerialConfig>(
                    detail.ConfigJson);

            if (config == null)
            {
                throw new InvalidOperationException(
                    "Serial 配置不能为空。");
            }

            if (config.Length <= 0)
            {
                throw new InvalidOperationException(
                    "Serial Length 必须大于 0。");
            }

            string resetMode = config.ResetMode;

            long value =
                _repository.GetCode(
                    _ruleId,
                    resetMode,
                    generationTime);

            string text =
                value.ToString(
                    CultureInfo.InvariantCulture);

            if (text.Length > config.Length)
            {
                throw new InvalidOperationException(
                    "产品码流水号超过配置长度。");
            }

            char padding =
                string.IsNullOrEmpty(config.PaddingChar)
                    ? '0'
                    : config.PaddingChar[0];

            return text.PadLeft(
                config.Length,
                padding);
        }
    }
}
