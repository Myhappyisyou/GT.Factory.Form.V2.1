using GT.Factory.Common.Models.ProductCode;
using System.Collections.Generic;

namespace GT.Factory.ProductCode.Repository
{
    /// <summary>
    /// 产品码规则仓储接口。
    /// </summary>
    public interface ICodeRuleRepository
    {
        List<CodeRule> LoadRules();
        CodeRule GetRule(int ruleId);

        /// <summary>
        /// 根据零部件 ID 获取该零部件下的所有规则。
        /// </summary>
        List<CodeRule> GetByPartId(int partId);

        /// <summary>
        /// 根据产品型号 ID 获取该型号下的所有规则。
        /// </summary>
        List<CodeRule> GetByProductModelId(int productModelId);

        /// <summary>
        /// 根据产品型号编码获取该型号下的所有规则。
        /// </summary>
        List<CodeRule> GetByProductModel(string productModel);

        /// <summary>
        /// 根据产品型号 ID 和零部件编码获取规则。
        /// </summary>
        //List<CodeRule> GetByPartCode(
        //    int productModelId,
        //    string partCode);

        /// <summary>
        /// 根据产品型号编码和零部件编码获取规则。
        /// </summary>
        List<CodeRule> GetByProductModelAndPartCode(
            string productModel,
            string partCode);

        void UpdateRule(CodeRule rule);
        void AddRule(CodeRule rule);
        void DeleteRule(int ruleId);
    }
}
