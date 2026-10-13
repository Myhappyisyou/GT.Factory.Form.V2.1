using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Product.Repository;
using System;
using System.Collections.Generic;

namespace GT.Factory.Product.Service
{
    /// <summary>
    /// 产品零部件业务服务。
    ///
    /// 负责产品零部件相关业务操作，
    /// 实际数据访问通过 IProductPartRepository 完成。
    ///
    /// 调用关系：
    ///
    /// ProductPartService
    ///        ↓
    /// IProductPartRepository
    ///        ↓
    /// GTProductPart
    /// </summary>
    public class ProductPartService :
    IProductPartRepository
    {
        /// <summary>
        /// 产品零部件数据访问对象。
        /// </summary>
        private readonly IProductPartRepository _repository;

    /// <summary>
    /// 创建产品零部件业务服务。
    /// </summary>
    public ProductPartService(
        IProductPartRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(
                    "repository");
            }

            _repository =
                repository;
        }

        /// <summary>
        /// 获取所有零部件。
        /// </summary>
        public List<GTProductPart> GetAll()
        {
            return _repository.GetAll();
        }

        /// <summary>
        /// 根据零部件 ID 获取零部件。
        /// </summary>
        public GTProductPart GetById(
            int partId)
        {
            if (partId <= 0)
            {
                return null;
            }

            return _repository.GetById(
                partId);
        }

        /// <summary>
        /// 根据产品型号 ID 获取零部件。
        /// </summary>
        public List<GTProductPart> GetByProductModelId(
            int productModelId)
        {
            if (productModelId <= 0)
            {
                return new List<GTProductPart>();
            }

            return _repository
                .GetByProductModelId(
                    productModelId);
        }

        /// <summary>
        /// 根据产品型号编码获取零部件。
        /// </summary>
        public List<GTProductPart> GetByProductModel(
            string productModel)
        {
            if (string.IsNullOrWhiteSpace(
                productModel))
            {
                return new List<GTProductPart>();
            }

            return _repository
                .GetByProductModel(
                    productModel.Trim());
        }

        /// <summary>
        /// 根据产品型号 ID 和部件编码查询部件。
        /// </summary>
        public GTProductPart GetByPartCode(
            int productModelId,
            string partCode)
        {
            if (productModelId <= 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(
                partCode))
            {
                return null;
            }

            return _repository.GetByPartCode(
                productModelId,
                partCode.Trim());
        }

        /// <summary>
        /// 根据产品型号 ID 和部件名称查询部件。
        /// </summary>
        public List<GTProductPart> GetByPartName(
            int productModelId,
            string partName)
        {
            if (productModelId <= 0)
            {
                return new List<GTProductPart>();
            }

            if (string.IsNullOrWhiteSpace(
                partName))
            {
                return new List<GTProductPart>();
            }

            return _repository.GetByPartName(
                productModelId,
                partName.Trim());
        }

        /// <summary>
        /// 新增零部件。
        /// </summary>
        public int Add(
            GTProductPart part)
        {
            ValidatePart(
                part);

            /*
             * 先确认所属产品型号存在。
             *
             * 这里通过 ProductModelRepository 无法直接检查，
             * 因为当前 Service 只负责 Part。
             *
             * 数据库 FK 会最终保证 ProductModelId 合法。
             */

            GTProductPart exists =
                _repository.GetByPartCode(
                    part.ProductModelId,
                    part.PartCode.Trim());

            if (exists != null)
            {
                throw new InvalidOperationException(
                    "该产品型号下已经存在相同的零部件编码："
                    + part.PartCode);
            }

            part.PartCode =
                part.PartCode.Trim();

            if (part.PartName == null)
            {
                part.PartName = "";
            }

            part.PartName =
                part.PartName.Trim();

            return _repository.Add(
                part);
        }

        /// <summary>
        /// 修改零部件。
        /// </summary>
        public void Update(
            GTProductPart part)
        {
            ValidatePart(
                part);

            GTProductPart exists =
                _repository.GetByPartCode(
                    part.ProductModelId,
                    part.PartCode.Trim());

            if (exists != null &&
                exists.PartId != part.PartId)
            {
                throw new InvalidOperationException(
                    "该产品型号下已经存在相同的零部件编码："
                    + part.PartCode);
            }

            part.PartCode =
                part.PartCode.Trim();

            if (part.PartName == null)
            {
                part.PartName = "";
            }

            part.PartName =
                part.PartName.Trim();

            _repository.Update(
                part);
        }

        /// <summary>
        /// 删除零部件。
        ///
        /// 如果已经存在 GTCodeRule，
        /// 数据库 FK 会阻止删除。
        /// </summary>
        public void Delete(
            int partId)
        {
            if (partId <= 0)
            {
                throw new ArgumentException(
                    "零部件 ID 无效。",
                    "partId");
            }

            GTProductPart part =
                _repository.GetById(
                    partId);

            if (part == null)
            {
                throw new InvalidOperationException(
                    "零部件不存在。");
            }

            _repository.Delete(
                partId);
        }

        /// <summary>
        /// 检查零部件基本数据。
        /// </summary>
        private void ValidatePart(
            GTProductPart part)
        {
            if (part == null)
            {
                throw new ArgumentNullException(
                    "part");
            }

            if (part.ProductModelId <= 0)
            {
                throw new ArgumentException(
                    "产品型号 ID 无效。",
                    "part");
            }

            if (string.IsNullOrWhiteSpace(
                part.PartCode))
            {
                throw new ArgumentException(
                    "零部件编码不能为空。",
                    "part");
            }

            if (part.PartCode.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "零部件编码长度不能超过 100 个字符。",
                    "part");
            }

            if (part.PartName != null &&
                part.PartName.Length > 200)
            {
                throw new ArgumentException(
                    "零部件名称长度不能超过 200 个字符。",
                    "part");
            }
        }
    }

}

