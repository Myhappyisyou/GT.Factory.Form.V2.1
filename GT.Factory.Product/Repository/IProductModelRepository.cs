using System.Collections.Generic;
using GT.Factory.Common.Models.ProductModel;

namespace GT.Factory.Product.Repository
{
    /// <summary>
    /// 产品型号数据访问接口。
    ///
    /// 负责：
    /// 1. 获取所有产品型号。
    /// 2. 根据型号 ID 获取产品型号。
    /// 3. 根据型号编码获取产品型号。
    /// 4. 新增、修改、删除产品型号。
    /// </summary>
    public interface IProductModelRepository
    {
        /// <summary>
        /// 获取所有产品型号。
        /// </summary>
        List<GTProductModel> GetAll();

        /// <summary>
        /// 根据产品型号 ID 获取产品型号。
        /// </summary>
        GTProductModel GetById(int productModelId);

        /// <summary>
        /// 根据产品型号编码获取产品型号。
        /// </summary>
        GTProductModel GetByModel(string productModel);

        /// <summary>
        /// 新增产品型号。
        /// </summary>
        int Add(GTProductModel model);

        /// <summary>
        /// 修改产品型号。
        /// </summary>
        void Update(GTProductModel model);

        /// <summary>
        /// 删除产品型号。
        /// </summary>
        void Delete(int productModelId);
    }
}
