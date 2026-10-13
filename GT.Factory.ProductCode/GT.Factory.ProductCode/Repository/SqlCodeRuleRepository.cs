using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Data.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;

namespace GT.Factory.ProductCode.Repository
{
    /// <summary>
    /// ProductCode 对 Data 层规则仓储的适配器。
    /// </summary>
    public class SqlCodeRuleRepository : ICodeRuleRepository
    {
        private readonly ProductCodeRuleRepository _repository;

        public SqlCodeRuleRepository(ProductCodeRuleRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
        }

        public List<CodeRule> LoadRules()
        {
            return _repository.LoadRules();
        }

        public CodeRule GetRule(int ruleId)
        {
            return _repository.GetRule(ruleId);
        }

        public List<CodeRule> GetRuleByProductModel(string productModel)
        {
            return _repository.GetByProductModel(productModel);
        }

        public void AddRule(CodeRule rule)
        {
            _repository.AddRule(rule);
        }

        public void UpdateRule(CodeRule rule)
        {
            _repository.UpdateRule(rule);
        }

        public void DeleteRule(int ruleId)
        {
            _repository.DeleteRule(ruleId);
        }


        public List<CodeRule> GetByPartId(int partId)
        {
           return _repository.GetByPartId(partId);
        }

        public List<CodeRule> GetByProductModelId(int productModelId)
        {
            return _repository.GetByProductModelId(productModelId);
        }

        public List<CodeRule> GetByProductModel(string productModel)
        {
            return _repository.GetByProductModel(productModel);
        }


        public List<CodeRule> GetByProductModelAndPartCode(string productModel, string partCode)
        {
            return _repository.GetByProductModelAndPartCode(productModel, partCode);
        }

    }
}
