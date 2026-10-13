using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Product.Repository;
using System;
using System.Collections.Generic;

namespace GT.Factory.Product.Service
{
    /// <summary>
    /// 产品型号业务服务。
    ///
    /// 负责产品型号的业务操作，
    /// 实际数据访问通过 IProductModelRepository 完成。
    ///
    /// 调用关系：
    ///
    /// ProductModelManageForm
    ///        ↓
    /// ProductModelService
    ///        ↓
    /// IProductModelRepository
    ///        ↓
    /// GTProductModel
    /// </summary>
    public class ProductModelService :
    IProductModelRepository
    {
        /// <summary>
        /// 产品型号数据访问对象。
        /// </summary>
        private readonly IProductModelRepository _repository;

    /// <summary>
    /// 创建产品型号业务服务。
    /// </summary>
    public ProductModelService(
        IProductModelRepository repository)
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
        /// 获取所有产品型号。
        /// </summary>
        public List<GTProductModel> GetAll()
        {
            return _repository.GetAll();
        }

        /// <summary>
        /// 根据 ID 获取产品型号。
        /// </summary>
        public GTProductModel GetById(
            int productModelId)
        {
            if (productModelId <= 0)
            {
                return null;
            }

            return _repository.GetById(
                productModelId);
        }

        /// <summary>
        /// 根据型号编码获取产品型号。
        /// </summary>
        public GTProductModel GetByModel(
            string productModel)
        {
            if (string.IsNullOrWhiteSpace(
                productModel))
            {
                return null;
            }

            return _repository.GetByModel(
                productModel.Trim());
        }

        /// <summary>
        /// 新增产品型号。
        /// </summary>
        public int Add(
            GTProductModel model)
        {
            ValidateModel(
                model);

            /*
             * 型号编码是业务唯一标识，
             * 新增前先检查是否已经存在。
             */
            GTProductModel exists =
                _repository.GetByModel(
                    model.ProductModel.Trim());

            if (exists != null)
            {
                throw new InvalidOperationException(
                    "产品型号已经存在："
                    + model.ProductModel);
            }

            model.ProductModel =
                model.ProductModel.Trim();

            if (string.IsNullOrWhiteSpace(
                model.ProductName))
            {
                model.ProductName =
                    model.ProductModel;
            }

            return _repository.Add(
                model);
        }

        /// <summary>
        /// 修改产品型号。
        /// </summary>
        public void Update(
            GTProductModel model)
        {
            ValidateModel(
                model);

            /*
             * 修改时检查同名型号。
             *
             * 如果查到的是自己，则允许修改。
             */
            GTProductModel exists =
                _repository.GetByModel(
                    model.ProductModel.Trim());

            if (exists != null &&
                exists.ProductModelId
                != model.ProductModelId)
            {
                throw new InvalidOperationException(
                    "产品型号已经存在："
                    + model.ProductModel);
            }

            model.ProductModel =
                model.ProductModel.Trim();

            if (string.IsNullOrWhiteSpace(
                model.ProductName))
            {
                model.ProductName =
                    model.ProductModel;
            }

            _repository.Update(
                model);
        }

        /// <summary>
        /// 删除产品型号。
        ///
        /// 删除是否允许由数据库外键关系最终保证。
        /// 如果该型号存在零部件，通常不能直接删除。
        /// </summary>
        public void Delete(
            int productModelId)
        {
            if (productModelId <= 0)
            {
                throw new ArgumentException(
                    "产品型号 ID 无效。",
                    "productModelId");
            }

            GTProductModel model =
                _repository.GetById(
                    productModelId);

            if (model == null)
            {
                throw new InvalidOperationException(
                    "产品型号不存在。");
            }

            _repository.Delete(
                productModelId);
        }

        /// <summary>
        /// 检查产品型号基本数据。
        /// </summary>
        private void ValidateModel(
            GTProductModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(
                    "model");
            }

            if (string.IsNullOrWhiteSpace(
                model.ProductModel))
            {
                throw new ArgumentException(
                    "产品型号不能为空。",
                    "model");
            }

            if (model.ProductModel.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "产品型号长度不能超过 100 个字符。",
                    "model");
            }
        }

    }

}
