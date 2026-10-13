using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using GT.Factory.Common.Enums.ProductCodeEnums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT.Factory.Data.Repository
{

    /// <summary>
    /// 产品码规则数据库仓储。
    ///
    /// 负责：
    ///
    /// GTCodeRule
    /// GTCodeRuleDetail
    ///
    /// 数据访问。
    ///
    /// 不负责：
    ///
    /// 1. 产品码生成
    /// 2. 流水号处理
    /// 3. MES交互
    ///
    /// </summary>
    public class ProductCodeRuleRepository
    {


        private readonly IDatabase _db;

        /// <summary>
        /// 产品码规则基础查询语句。
        ///
        /// GTCodeRule 不直接保存产品型号。
        ///
        /// 查询关系：
        ///
        /// GTCodeRule
        ///     ↓ PartId
        /// GTProductPart
        ///     ↓ ProductModelId
        /// GTProductModel
        /// </summary>
        private const string RuleSelectSql = @"
                    SELECT
                        R.RuleId,
                        R.PartId,

                        P.ProductModelId,
                        M.ProductModel,

                        P.PartCode,
                        P.PartName,

                        R.RuleName,
                        R.Description,
                        R.IsActive,
                        R.CreateTime,
                        R.UpdateTime

                    FROM GTCodeRule R

                    INNER JOIN GTProductPart P
                        ON R.PartId = P.PartId

                    INNER JOIN GTProductModel M
                        ON P.ProductModelId = M.ProductModelId
                        ";

        /// <summary>
        /// 构造函数。
        /// </summary>
        public ProductCodeRuleRepository(
            DatabaseManager manager)
        {

            _db =
                manager.Primary
                ?? throw new Exception(
                    "Primary数据库未配置");

        }




        #region 查询



        /// <summary>
        /// 查询全部产品码规则。
        ///
        /// 同时加载规则明细。
        /// </summary>
        public List<CodeRule> LoadRules()
        {

            string sql = RuleSelectSql + @"
                ORDER BY
                    M.ProductModel,
                    P.PartCode,
                    R.RuleId
            ";



            List<CodeRule> rules =
                 _db.Query<CodeRule>(
                    sql);



            foreach (CodeRule rule in rules)
            {

                rule.Details =
                     LoadDetails(
                        rule.RuleId);

            }


            return rules;

        }



        /// <summary>
        /// 根据规则ID查询。
        /// </summary>
        public CodeRule LoadRule(
            int ruleId)
        {

            string sql = RuleSelectSql + @"
                WHERE R.RuleId = @RuleId
            ";




            CodeRule rule =
                 _db.QuerySingle<CodeRule>(
                    sql,
                    new
                    {
                        ruleId
                    });



            if (rule == null)
            {
                return null;
            }



            rule.Details =
                 LoadDetails(
                    ruleId);



            return rule;

        }

        /// <summary>
        /// 根据零部件 ID 查询该零部件下的全部产品码规则。
        /// </summary>
        public List<CodeRule> LoadRulesByPartId(
            int partId)
        {
            string sql = RuleSelectSql + @"
                WHERE R.PartId = @PartId
                ORDER BY R.RuleId
            ";

            List<CodeRule> rules =
                _db.Query<CodeRule>(
                    sql,
                    new
                    {
                        PartId = partId
                    });

            foreach (CodeRule rule in rules)
            {
                rule.Details =
                    LoadDetails(rule.RuleId);
            }

            return rules;
        }

        /// <summary>
        /// 根据产品型号 ID 获取该型号下的全部规则。
        /// </summary>
        public List<CodeRule> LoadRulesByProductModelId(
            int productModelId)
        {
            string sql = RuleSelectSql + @"
                WHERE P.ProductModelId = @ProductModelId

                ORDER BY
                    P.PartCode,
                    R.RuleId
            ";

            List<CodeRule> rules =
                _db.Query<CodeRule>(
                    sql,
                    new
                    {
                        ProductModelId = productModelId
                    });

            foreach (CodeRule rule in rules)
            {
                rule.Details =
                    LoadDetails(rule.RuleId);
            }

            return rules;
        }


        /// <summary>
        /// 根据产品型号编码获取该型号下的全部规则。
        /// </summary>
        public List<CodeRule> LoadRulesByProductModel(
            string productModel)
        {
            string sql = RuleSelectSql + @"
                WHERE M.ProductModel = @ProductModel

                ORDER BY
                    P.PartCode,
                    R.RuleId
            ";

            List<CodeRule> rules =
                _db.Query<CodeRule>(
                    sql,
                    new
                    {
                        ProductModel = productModel
                    });

            foreach (CodeRule rule in rules)
            {
                rule.Details =
                    LoadDetails(rule.RuleId);
            }

            return rules;
        }



        /// <summary>
        /// 根据产品型号编码和零部件编码获取规则。
        /// </summary>
        public List<CodeRule> LoadRules(
            string productModel,
            string partCode)
        {
            string sql = RuleSelectSql + @"
                WHERE M.ProductModel = @ProductModel
                  AND P.PartCode = @PartCode

                ORDER BY R.RuleId
            ";

            List<CodeRule> rules =
                _db.Query<CodeRule>(
                    sql,
                    new
                    {
                        ProductModel = productModel,
                        PartCode = partCode
                    });

            foreach (CodeRule rule in rules)
            {
                rule.Details =
                    LoadDetails(rule.RuleId);
            }

            return rules;
        }


        /// <summary>
        /// 查询规则段。
        /// </summary>
        private List<CodeRuleDetail> LoadDetails(
            int ruleId)
        {

            string sql = @"

                SELECT

                    DetailId,
                    RuleId,
                    SegmentOrder,
                    SegmentType,
                    ConfigJson,
                    Enabled

                FROM GTCodeRuleDetail

                WHERE RuleId=@RuleId

                ORDER BY SegmentOrder

            ";



            return _db.Query<CodeRuleDetail>(
                sql,
                new
                {
                    ruleId
                });

        }


        #endregion


        #region 新增


        /// <summary>
        /// 新增产品码规则。
        ///
        /// 顺序：
        ///
        /// 1. 新增GTCodeRule
        /// 2. 获取RuleId
        /// 3. 新增Detail
        ///
        /// </summary>
        public void InsertRule(
            CodeRule rule)
        {

            string sql = @"

                INSERT INTO GTCodeRule
                (
                    RuleName,
                    PartId,
                    Description,
                    IsActive,
                    CreateTime
                )

                VALUES
                (
                    @RuleName,
                    @PartId,
                    @Description,
                    @IsActive,
                    GETDATE()
                );


                SELECT CAST(SCOPE_IDENTITY() AS INT);

            ";

            int ruleId =
                 _db.QuerySingle<int>(
                    sql,
                    new
                    {
                        rule.RuleName,
                        rule.PartId,
                        rule.Description,
                        rule.IsActive
                    });

            rule.RuleId =
                ruleId;

            if (rule.Details == null)
            {
                return;
            }


            foreach (CodeRuleDetail detail
                in rule.Details)
            {

                InsertDetail(
                   ruleId,
                   detail);

            }

        }


        /// <summary>
        /// 新增规则明细。
        /// </summary>
        private void InsertDetail(
            int ruleId,
            CodeRuleDetail detail)
        {

            string sql = @"

                INSERT INTO GTCodeRuleDetail
                (
                    RuleId,
                    SegmentOrder,
                    SegmentType,
                    ConfigJson,
                    Enabled
                )

                VALUES
                (
                    @RuleId,
                    @SegmentOrder,
                    @SegmentType,
                    @ConfigJson,
                    @Enabled
                )

            ";


            _db.Execute(
               sql,
               new
               {
                   RuleId = ruleId,

                   detail.SegmentOrder,

                   SegmentType =
                       (int)detail.SegmentType,

                   detail.ConfigJson,

                   detail.Enabled

               });

        }


        #endregion


        #region 更新


        /// <summary>
        /// 更新产品码规则。
        ///
        /// 更新策略：
        ///
        /// 1. 更新主表
        /// 2. 删除旧Detail
        /// 3. 重新插入Detail
        ///
        /// </summary>
        public void UpdateRule(
            CodeRule rule)
        {


            string sql = @"

                UPDATE GTCodeRule

                SET

                    RuleName=@RuleName,

                    PartId=@PartId,

                    Description=@Description,

                    IsActive=@IsActive,

                    UpdateTime=GETDATE()


                WHERE RuleId=@RuleId

            ";

            _db.Execute(
               sql,
               new
               {
                   rule.RuleId,

                   rule.PartId,

                   rule.RuleName,

                   rule.Description,

                   rule.IsActive
               });


            DeleteDetails(
               rule.RuleId);


            if (rule.Details == null)
            {
                return;
            }

            foreach (CodeRuleDetail detail
                in rule.Details)
            {

                InsertDetail(
                   rule.RuleId,
                   detail);

            }

        }



        #endregion


        #region 删除



        /// <summary>
        /// 删除规则。
        ///
        /// 先删除Detail。
        /// 再删除Rule。
        /// </summary>
        public void DeleteRule(
            int ruleId)
        {


            DeleteDetails(
               ruleId);



            string sql = @"

                DELETE FROM GTCodeRule

                WHERE RuleId=@RuleId

            ";



            _db.Execute(
               sql,
               new
               {
                   ruleId
               });

        }





        /// <summary>
        /// 删除规则明细。
        /// </summary>
        private void DeleteDetails(
            int ruleId)
        {

            string sql = @"

                DELETE FROM GTCodeRuleDetail

                WHERE RuleId=@RuleId

            ";



            _db.Execute(
               sql,
               new
               {
                   ruleId
               });

        }



        #endregion


    }

}
