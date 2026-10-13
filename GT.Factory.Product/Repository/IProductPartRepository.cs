using GT.Factory.Common.Models.ProductModel;
using System.Collections.Generic;

namespace GT.Factory.Product.Repository
{
    /// <summary>
    /// 产品零部件数据访问接口。
    ///
    /// 产品零部件属于产品型号：
    ///
    /// GTProductModel
    ///     ↓ ProductModelId
    /// GTProductPart
    /// </summary>
    public interface IProductPartRepository
    {
        /// <summary>
        /// 获取所有零部件。
        /// </summary>
        List<GTProductPart> GetAll();

        /// <summary>
        /// 根据零部件 ID 获取零部件。
        /// </summary>
        GTProductPart GetById(int partId);

        /// <summary>
        /// 根据产品型号 ID 获取该型号下的所有零部件。
        /// </summary>
        List<GTProductPart> GetByProductModelId(int productModelId);

        /// <summary>
        /// 根据产品型号编码获取该型号下的所有零部件。
        /// </summary>
        List<GTProductPart> GetByProductModel(string productModel);

        /// <summary>
        /// 根据产品型号 ID 和零部件编码获取零部件。
        /// </summary>
        GTProductPart GetByPartCode(int productModelId, string partCode);

        /// <summary>
        /// 根据产品型号 ID 和零部件名称获取零部件。
        /// </summary>
        List<GTProductPart> GetByPartName(int productModelId, string partName);

        /// <summary>
        /// 新增零部件。
        /// </summary>
        int Add(GTProductPart part);

        /// <summary>
        /// 修改零部件。
        /// </summary>
        void Update(GTProductPart part);

        /// <summary>
        /// 删除零部件。
        /// </summary>
        void Delete(int partId);
    }
}
