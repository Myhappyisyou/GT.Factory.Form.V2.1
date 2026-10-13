using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Data;
using GT.Factory.Data.Abstractions;
using GT.Factory.Data.Core;
using System;
using System.Collections.Generic;

namespace GT.Factory.Product.Repository
{
    /// <summary>
    /// 产品零部件数据库仓储。
    ///
    /// 负责：
    ///
    /// GTProductPart
    ///
    /// 数据访问。
    ///
    /// 产品零部件属于产品型号：
    ///
    /// GTProductModel
    ///     ↓ ProductModelId
    /// GTProductPart
    ///
    /// 本仓储只负责零部件数据访问。
    ///
    /// 不负责：
    ///
    /// 1. 产品码规则
    /// 2. 产品码生成
    /// 3. 流水号处理
    /// 4. MES交互
    /// </summary>
    public class ProductPartRepository: IProductPartRepository
    {
        private readonly IDatabase _db;

        /// <summary>
        /// 构造函数。
        /// </summary>
        public ProductPartRepository(DatabaseManager manager)
        {
            _db = manager.Primary;

            if (_db == null)
            {
                throw new Exception("Primary数据库未配置");
            }
        }

        #region 查询

        /// <summary>
        /// 获取所有产品零部件。
        ///
        /// 同时关联产品型号，
        /// 返回的 GTProductPart 中会包含 ProductModelId。
        /// </summary>
        public List<GTProductPart> GetAll()
        {
            string sql = @"
                SELECT
                    PartId,
                    ProductModelId,
                    PartCode,
                    PartName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductPart
                ORDER BY
                    ProductModelId,
                    PartId
            ";

            return _db.Query<GTProductPart>(sql);
        }

        /// <summary>
        /// 根据零部件 ID 获取零部件。
        /// </summary>
        public GTProductPart GetById(int partId)
        {
            string sql = @"
                SELECT
                    PartId,
                    ProductModelId,
                    PartCode,
                    PartName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductPart 
                WHERE PartId = @PartId
            ";

            return _db.QuerySingle<GTProductPart>(
                sql,
                new
                {
                    PartId = partId
                });
        }

        /// <summary>
        /// 根据产品型号 ID 获取该型号下的所有零部件。
        /// </summary>
        public List<GTProductPart> GetByProductModelId(
            int productModelId)
        {
            string sql = @"
                SELECT
                    PartId,
                    ProductModelId,
                    PartCode,
                    PartName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductPart P
                WHERE ProductModelId = @ProductModelId
                ORDER BY PartId
            ";

            return _db.Query<GTProductPart>(
                sql,
                new
                {
                    ProductModelId = productModelId
                });
        }

        /// <summary>
        /// 根据产品型号编码获取该型号下的所有零部件。
        ///
        /// 查询关系：
        ///
        /// GTProductModel.ProductModelId
        ///     =
        /// GTProductPart.ProductModelId
        /// </summary>
        public List<GTProductPart> GetByProductModel(
            string productModel)
        {
            string sql = @"
                SELECT
                    P.PartId,
                    P.ProductModelId,
                    P.PartCode,
                    P.PartName,
                    P.Description,
                    P.IsActive,
                    P.CreateTime,
                    P.UpdateTime
                FROM GTProductPart P
                INNER JOIN GTProductModel M
                    ON P.ProductModelId = M.ProductModelId
                WHERE M.ProductModel = @ProductModel
                ORDER BY P.PartId
            ";

            return _db.Query<GTProductPart>(
                sql,
                new
                {
                    ProductModel = productModel
                });
        }

        /// <summary>
        /// 根据产品型号 ID 和零部件编码获取零部件。
        ///
        /// ProductModelId + PartCode
        /// 是零部件的业务定位条件。
        /// </summary>
        public GTProductPart GetByPartCode(
            int productModelId,
            string partCode)
        {
            string sql = @"
                SELECT
                    PartId,
                    ProductModelId,
                    PartCode,
                    PartName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductPart
                WHERE ProductModelId = @ProductModelId
                  AND PartCode = @PartCode
            ";

            return _db.QuerySingle<GTProductPart>(
                sql,
                new
                {
                    ProductModelId = productModelId,
                    PartCode = partCode
                });
        }

        /// <summary>
        /// 根据产品型号 ID 和零部件名称查询零部件。
        ///
        /// 一个型号下允许存在多个名称相同的零部件，
        /// 因此这里返回 List。
        /// </summary>
        public List<GTProductPart> GetByPartName(
            int productModelId,
            string partName)
        {
            string sql = @"
                SELECT
                    PartId,
                    ProductModelId,
                    PartCode,
                    PartName,
                    Description,
                    IsActive,
                    CreateTime,
                    UpdateTime
                FROM GTProductPart
                WHERE ProductModelId = @ProductModelId
                  AND PartName = @PartName
                ORDER BY PartId
            ";

            return _db.Query<GTProductPart>(
                sql,
                new
                {
                    ProductModelId = productModelId,
                    PartName = partName
                });
        }

        #endregion

        #region 新增

        /// <summary>
        /// 新增产品零部件。
        ///
        /// 返回数据库生成的 PartId。
        /// </summary>
        public int Add(GTProductPart part)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }

            string sql = @"
                INSERT INTO GTProductPart
                (
                    ProductModelId,
                    PartCode,
                    PartName,
                    Description,
                    IsActive,
                    CreateTime
                )
                VALUES
                (
                    @ProductModelId,
                    @PartCode,
                    @PartName,
                    @Description,
                    @IsActive,
                    GETDATE()
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            int partId = _db.QuerySingle<int>(
                sql,
                new
                {
                    part.ProductModelId,
                    part.PartCode,
                    part.PartName,
                    part.Description,
                    part.IsActive
                });

            part.PartId = partId;

            return partId;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 修改产品零部件。
        /// </summary>
        public void Update(GTProductPart part)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }

            string sql = @"
                UPDATE GTProductPart
                SET
                    ProductModelId = @ProductModelId,
                    PartCode = @PartCode,
                    PartName = @PartName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdateTime = GETDATE()
                WHERE PartId = @PartId
            ";

            _db.Execute(
                sql,
                new
                {
                    part.PartId,
                    part.ProductModelId,
                    part.PartCode,
                    part.PartName,
                    part.Description,
                    part.IsActive
                });
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除产品零部件。
        ///
        /// 注意：
        ///
        /// GTCodeRule.PartId 引用了 GTProductPart.PartId。
        ///
        /// 如果该零部件已经存在产品码规则，
        /// 数据库可能因为外键约束而拒绝删除。
        ///
        /// 因此业务层更推荐使用 IsActive=false
        /// 进行停用，而不是直接物理删除。
        /// </summary>
        public void Delete(int partId)
        {
            string sql = @"
                DELETE FROM GTProductPart
                WHERE PartId = @PartId
            ";

            _db.Execute(
                sql,
                new
                {
                    PartId = partId
                });
        }

        #endregion
    }
}
