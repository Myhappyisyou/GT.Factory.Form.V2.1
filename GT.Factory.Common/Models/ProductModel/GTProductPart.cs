using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.Common.Models.ProductModel
{
    /// <summary>
    /// 产品部件。
    ///
    /// 一个产品型号可以包含多个部件，
    /// 一个部件可以拥有多个产品码规则。
    ///
    /// 数据关系：
    ///
    /// GTProductModel
    ///        │
    ///        │ ProductModelId
    ///        ▼
    /// GTProductPart
    ///        │
    ///        │ PartId
    ///        ▼
    /// GTCodeRule
    /// </summary>
    public class GTProductPart
    {
        /// <summary>
        /// 产品部件主键。
        /// </summary>
        public int PartId { get; set; }

        /// <summary>
        /// 所属产品型号主键。
        ///
        /// 对应 GTProductModel.ProductModelId。
        /// </summary>
        public int ProductModelId { get; set; }

        /// <summary>
        /// 部件编码。
        ///
        /// 例如：
        /// PUMP-BODY
        /// </summary>
        public string PartCode { get; set; }

        /// <summary>
        /// 部件名称。
        ///
        /// 例如：
        /// 泵体
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// 部件描述。
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
        /// 最后修改时间。
        /// </summary>
        public DateTime UpdateTime { get; set; }
    }
}
