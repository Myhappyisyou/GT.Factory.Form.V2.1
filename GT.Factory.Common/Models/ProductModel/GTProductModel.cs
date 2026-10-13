using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.Common.Models.ProductModel
{
    /// <summary>
    /// 产品型号。
    ///
    /// 一个产品型号可以包含多个产品部件，
    /// 例如：
    ///
    /// XMOIL-PUMP-A
    ///     ├── PUMP-BODY
    ///     ├── PUMP-COVER
    ///     └── PUMP-ROTOR
    ///
    /// 产品码规则不直接关联产品型号，
    /// 而是通过 GTProductPart 间接关联。
    /// </summary>
    public class GTProductModel
    {
        /// <summary>
        /// 产品型号主键。
        /// </summary>
        public int ProductModelId { get; set; }

        /// <summary>
        /// 产品型号编码。
        ///
        /// 例如：
        /// XMOIL-PUMP-A
        /// </summary>
        public string ProductModel { get; set; }

        /// <summary>
        /// 产品名称。
        ///
        /// 例如：
        /// X系列机油泵
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品型号描述。
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