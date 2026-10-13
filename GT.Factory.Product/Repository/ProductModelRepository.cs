using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using System;
using System.Collections.Generic;

namespace GT.Factory.Product.Repository
{
    /// <summary>
    /// 产品型号数据库仓储。
    ///
    /// 负责：
    ///
    /// GTProductModel
    ///
    /// 数据访问。
    ///
    /// 产品型号是产品码系统的第一层业务对象：
    ///
    /// GTProductModel
    ///     ↓ ProductModelId
    /// GTProductPart
    ///     ↓ PartId
    /// GTCodeRule
    ///
    /// 本仓储只负责产品型号数据访问。
    ///
    /// 不负责：
    ///
    /// 1. 产品零部件
    /// 2. 产品码规则
    /// 3. 产品码生成
    /// 4. 流水号处理
    /// 5. MES交互
    /// </summary>
    public class ProductModelRepository : IProductModelRepository
    {
        private readonly IDatabase _db;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public ProductModelRepository(DatabaseManager manager)
        {
            _db = manager.Primary;

            if (_db == null)
            {
                throw new Exception("Primary数据库未配置");
            }
        }

        #region 查询

        /// <summary>
        /// 获取所有产品型号。
        ///
        /// 默认按照 ProductModelId 排序。
        /// </summary>
        public List<GTProductModel> GetAll()
        {
            string sql = @"
                SELECT
                    ProductModelId,
                    ProductModel,
                    ProductName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductModel
                ORDER BY ProductModelId
            ";

            return _db.Query<GTProductModel>(sql);
        }

        /// <summary>
        /// 根据产品型号 ID 获取产品型号。
        /// </summary>
        public GTProductModel GetById(
            int productModelId)
        {
            string sql = @"
                SELECT
                    ProductModelId,
                    ProductModel,
                    ProductName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductModel
                WHERE ProductModelId = @ProductModelId
            ";

            return _db.QuerySingle<GTProductModel>(
                sql,
                new
                {
                    ProductModelId = productModelId
                });
        }

        /// <summary>
        /// 根据产品型号编码获取产品型号。
        ///
        /// ProductModel 是产品型号的业务编码，
        /// 数据库中应该保持唯一。
        /// </summary>
        public GTProductModel GetByModel(
            string productModel)
        {
            string sql = @"
                SELECT
                    ProductModelId,
                    ProductModel,
                    ProductName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductModel
                WHERE ProductModel = @ProductModel
            ";

            return _db.QuerySingle<GTProductModel>(
                sql,
                new
                {
                    ProductModel = productModel
                });
        }

        #endregion

        #region 新增

        /// <summary>
        /// 新增产品型号。
        ///
        /// 返回数据库生成的 ProductModelId。
        /// </summary>
        public int Add(
            GTProductModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            string sql = @"
                INSERT INTO GTProductModel
                (
                    ProductModel,
                    ProductName,
                    Description,
                    IsActive,
                    CreateTime
                )
                VALUES
                (
                    @ProductModel,
                    @ProductName,
                    @Description,
                    @IsActive,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            int productModelId =
                _db.QuerySingle<int>(
                    sql,
                    new
                    {
                        model.ProductModel,
                        model.ProductName,
                        model.Description,
                        model.IsActive
                    });

            model.ProductModelId =
                productModelId;

            return productModelId;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 修改产品型号。
        /// </summary>
        public void Update(
            GTProductModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            string sql = @"
                UPDATE GTProductModel
                SET
                    ProductModel = @ProductModel,
                    ProductName = @ProductName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdateTime = GETDATE()
                WHERE ProductModelId = @ProductModelId
            ";

            _db.Execute(
                sql,
                new
                {
                    model.ProductModelId,
                    model.ProductModel,
                    model.ProductName,
                    model.Description,
                    model.IsActive
                });
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除产品型号。
        ///
        /// 注意：
        ///
        /// GTProductPart.ProductModelId
        /// 引用了 GTProductModel.ProductModelId。
        ///
        /// 如果该型号下面已经存在零部件，
        /// 数据库外键可能拒绝删除。
        ///
        /// 实际业务中更推荐将 IsActive 设置为 false，
        /// 而不是直接物理删除。
        /// </summary>
        public void Delete(
            int productModelId)
        {
            string sql = @"
                DELETE FROM GTProductModel
                WHERE ProductModelId = @ProductModelId
            ";

            _db.Execute(
                sql,
                new
                {
                    ProductModelId = productModelId
                });
        }

        #endregion
    }
}
