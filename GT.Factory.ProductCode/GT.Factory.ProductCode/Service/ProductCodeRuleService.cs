using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Data.Repository;
using GT.Factory.Product.Repository;
using GT.Factory.ProductCode.Repository;
using System;
using System.Collections.Generic;


namespace GT.Factory.ProductCode.Service
{
    /// <summary>
    /// 产品码规则服务。
    ///
    /// 负责向 UI 提供统一的规则管理入口。
    ///
    /// UI 不需要知道当前规则来自：
    ///
    /// JSON
    /// SQL Server
    ///
    /// 具体数据源由 ICodeRuleRepository 决定。
    /// </summary>
    public class ProductCodeRuleService
    {
        private readonly ICodeRuleRepository _repository;
        private readonly IProductCodeRepository _productCodeRepository;

        /// <summary>
        /// 创建产品码规则服务。
        /// </summary>
        public ProductCodeRuleService(
            ICodeRuleRepository repository, 
            IProductCodeRepository productCodeRepository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(
                    "repository");
            }

            _repository = repository;
            _productCodeRepository = productCodeRepository;
        }

        /// <summary>
        /// 获取全部产品码规则。
        /// </summary>
        public List<CodeRule> LoadRules()
        {
            return _repository.LoadRules();
        }

        /// <summary>
        /// 获取指定规则。
        /// </summary>
        public CodeRule GetRule(int ruleId)
        {
            return _repository.GetRule(ruleId);
        }

        /// <summary>
        /// 获取指定规则。
        /// </summary>
        public List<CodeRule> LoadRulesByPartId(int partId)
        {
            return _repository.GetByPartId(partId);
        }

        /// <summary>
        /// 新增规则。
        ///
        /// RuleId <= 0：
        ///     认为是新规则。
        /// </summary>
        public void AddRule(CodeRule rule)
        {
            ValidateRule(rule);

            _repository.AddRule(rule);
        }

        /// <summary>
        /// 更新规则。
        /// </summary>
        public void UpdateRule(CodeRule rule)
        {
            ValidateRule(rule);

            if (rule.RuleId <= 0)
            {
                throw new ArgumentException(
                    "更新规则时 RuleId 必须大于 0。",
                    "rule");
            }

            _repository.UpdateRule(rule);
        }

        /// <summary>
        /// 删除规则。
        /// </summary>
        public void DeleteRule(CodeRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(
                    "rule");
            }

            if (rule.RuleId <= 0)
            {
                throw new ArgumentException(
                    "删除规则时 RuleId 必须大于 0。",
                    "rule");
            }

            _repository.DeleteRule(rule.RuleId);
        }

        /// <summary>
        /// 废弃的 存储过程方法
        /// </summary>
        public ProductCodeResult GetProductCode(int ruleId)
        {
            return _productCodeRepository.GetCode(ruleId);
        }

        /// <summary>
        /// 生成流水号
        /// </summary>
        public long GetSerialNumber(int ruleId, string resetModel, DateTime generationTime)
        {
            return _productCodeRepository.GetCode(ruleId, resetModel, generationTime);
        }

        /// <summary>
        /// 生成班次
        /// </summary>
        public string GetCurrentShift(DateTime generationTime)
        {
            return _productCodeRepository.GetCurrentShift(generationTime);
        }

        /// <summary>
        /// 保存历史产品吗
        /// </summary>
        public void AddHistory(string productCode, int ruleId, DateTime generationTime)
        {
            _productCodeRepository.AddHistory(productCode, ruleId, generationTime);
        }

        /// <summary>
        /// 检查规则基本信息。
        ///
        /// 详细 Segment 配置是否合法，
        /// 由后续产品码规则验证逻辑负责。
        /// </summary>
        private void ValidateRule(CodeRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(
                    "rule");
            }

            if (string.IsNullOrWhiteSpace(
                rule.RuleName))
            {
                throw new ArgumentException(
                    "产品码规则名称不能为空。",
                    "rule");
            }
        }
    }
}
