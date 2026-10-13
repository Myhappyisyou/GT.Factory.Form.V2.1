using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Data.Repository;
using GT.Factory.Product.Repository;
using GT.Factory.ProductCode.Domain;
using GT.Factory.ProductCode.Generator;
using GT.Factory.ProductCode.Processor;
using GT.Factory.ProductCode.Repository;
using System;

namespace GT.Factory.ProductCode.Service
{
    /// <summary>
    /// 产品码业务服务。
    ///
    /// 负责：
    /// 1. 根据 RuleId 读取规则。
    /// 2. 创建生成上下文。
    /// 3. 调用 ProductCodeGenerator。
    /// 4. 写入产品码历史。
    /// </summary>
    public class ProductCodeGeneratorService
    {
        private readonly ICodeRuleRepository _ruleRepository;
        private readonly IProductCodeRepository _productCodeRepository;
        private readonly IProductModelRepository _productModelRepository;
        private readonly IProductPartRepository _productPartRepository;
        public ProductCodeGeneratorService(
            ICodeRuleRepository ruleRepository,
            IProductCodeRepository productCodeRepository,
              IProductModelRepository productModelRepository,
            IProductPartRepository productPartRepository)
        {
            if (ruleRepository == null)
            {
                throw new ArgumentNullException("ruleRepository");
            }

            if (productCodeRepository == null)
            {
                throw new ArgumentNullException("productCodeRepository");
            }

            _ruleRepository = ruleRepository;
            _productCodeRepository = productCodeRepository;
            _productModelRepository = productModelRepository;
            _productPartRepository = productPartRepository;
        }

        /// <summary>
        /// 正式生产环境生成一个产品码。
        ///
        /// 一次生成过程中只获取一次 SQL Server 时间。
        /// Date、Shift、Serial、History 全部使用同一个 generationTime。
        /// </summary>
        public ProductCodeResult Generate(int ruleId)
        {
            try
            {
                // 1. 读取产品码规则。
                CodeRule rule =
                    _ruleRepository.GetRule(ruleId);

                if (rule == null || !rule.IsActive)
                {
                    return CreateFailure(
                        "产品码规则不存在或未启用。");
                }

                // 2. 从 SQL Server 获取本次生成的唯一时间。
                DateTime generationTime =
                    _productCodeRepository.GetDatabaseTime();

                // 3. 创建本次正式生成专用的 ProcessorRegistry。
                ProcessorRegistry registry =
                    new ProcessorRegistry(
                        new ICodeSegmentProcessor[]
                        {
                        new FixedTextProcessor(),
                        new DateProcessor(),
                        new ShiftProcessor(
                            _productCodeRepository),
                        new SerialProcessor(
                            _productCodeRepository,
                            rule.RuleId)
                        });

                // 4. 使用本次正式 Registry 创建 Generator。
                ProductCodeGenerator generator =
                    new ProductCodeGenerator(registry);

                // 5. 使用同一个 generationTime 生成完整产品码。
                string code =
                    generator.Generate(
                        rule,
                        generationTime);

                // 6. 使用同一个 generationTime 写入历史。
                _productCodeRepository.AddHistory(
                    code,
                    ruleId,
                    generationTime);

                return new ProductCodeResult
                {
                    Success = true,
                    Code = code,
                    ErrorMessage = string.Empty
                };
            }
            catch (Exception ex)
            {
                return CreateFailure(ex.Message);
            }
        }

        private ProductCodeResult CreateFailure(
            string message)
        {
            return new ProductCodeResult
            {
                Success = false,
                Code = string.Empty,
                ErrorMessage = message
            };
        }
    }
}
