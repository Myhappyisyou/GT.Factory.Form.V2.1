using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Data.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Reflection;

namespace GT.Factory.Product.Repository
{
    /// <summary>
    /// ProductCode 对 Data 层规则仓储的适配器。
    /// </summary>
    public class SqlProductPartRepository : IProductPartRepository
    {
        private readonly ProductPartRepository _repository;

        public SqlProductPartRepository(ProductPartRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
        }

        public List<GTProductPart> GetAll()
        {
            return _repository.GetAll();
        }

        public GTProductPart GetById(int partId)
        {
            return _repository.GetById(partId);
        }

        public List<GTProductPart> GetByProductModelId(int productModelId)
        {
            return _repository.GetByProductModelId(productModelId);
        }

        public List<GTProductPart> GetByProductModel(string productModel)
        {
            return _repository.GetByProductModel(productModel);
        }

        public GTProductPart GetByPartCode(int productModelId, string partCode)
        {
            return _repository.GetByPartCode(productModelId, partCode);
        }

        public List<GTProductPart> GetByPartName(int productModelId, string partName)
        {
            return _repository.GetByPartName(productModelId, partName);
        }

        public int Add(GTProductPart part)
        {
            return _repository.Add(part);
        }

        public void Update(GTProductPart part)
        {
            _repository.Update(part);
        }

        public void Delete(int partId)
        {
            _repository.Delete(partId);
        }
    }
}
