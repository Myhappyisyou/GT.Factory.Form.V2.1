using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Generator;
using GT.Factory.ProductCode.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.ProductCode.Processor
{
    public class ShiftProcessor : ICodeSegmentProcessor
    {
        private readonly IProductCodeRepository _repository;

        public ShiftProcessor(
            IProductCodeRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
        }

        public CodeSegmentType SegmentType
        {
            get
            {
                return CodeSegmentType.Shift;
            }
        }

        public string Process(
            CodeRuleDetail detail,
            DateTime generationTime)
        {
            string shift =
                _repository.GetCurrentShift(
                    generationTime);

            if (string.IsNullOrEmpty(shift))
            {
                throw new InvalidOperationException(
                    "根据生成时间无法确定当前班次：" +
                    generationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return shift;
        }
    }
}
