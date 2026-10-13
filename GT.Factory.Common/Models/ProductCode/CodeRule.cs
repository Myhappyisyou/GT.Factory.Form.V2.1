using System;
using System.Collections.Generic;

namespace GT.Factory.Common.Models.ProductCode
{
    /// <summary>
    /// 产品码规则。
    ///
    /// 数据库 GTCodeRule 真正的归属关系是：
    ///
    /// GTCodeRule.PartId
    ///     ↓
    /// GTProductPart.PartId
    ///     ↓
    /// GTProductPart.ProductModelId
    ///     ↓
    /// GTProductModel.ProductModelId
    /// </summary>
    public class CodeRule
    {
        /// <summary>
        /// 规则 ID。
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// 所属零部件 ID。
        /// </summary>
        public int PartId { get; set; }

        /// <summary>
        /// 产品型号 ID。
        ///
        /// 查询时通过 GTProductPart 关联得到。
        /// </summary>
        public int ProductModelId { get; set; }

        /// <summary>
        /// 产品型号。
        ///
        /// 查询显示字段，不直接保存到 GTCodeRule。
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 零部件编码。
        ///
        /// 查询显示字段，不直接保存到 GTCodeRule。
        /// </summary>
        public string PartCode { get; set; }

        /// <summary>
        /// 零部件名称。
        ///
        /// 查询显示字段，不直接保存到 GTCodeRule。
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// 规则名称。
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 规则说明。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间。
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 规则明细。
        /// </summary>
        public List<CodeRuleDetail> Details { get; set; }
    }
}

