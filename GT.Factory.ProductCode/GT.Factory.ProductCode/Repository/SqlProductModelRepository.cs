using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Data.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace GT.Factory.ProductCode.Repository
{
    /// <summary>
    /// ProductCode 对 Data 层规则仓储的适配器。
    /// </summary>
    public class SqlProductModelRepository : IProductModelRepository
    {
        private readonly ProductModelRepository _repository;

        public SqlProductModelRepository(ProductModelRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            _repository = repository;
        }

     

        public List<GTProductModel> GetAll()
        {
            return _repository.GetAll();
        }

        public GTProductModel GetById(int productModelId)
        {
            return _repository.GetById(productModelId);
        }

        public GTProductModel GetByModel(string productModel)
        {
            return _repository.GetByModel(productModel);
        }

        public int Add(GTProductModel model)
        {
            return _repository.Add(model);
        }

        public void Update(GTProductModel model)
        {
             _repository.Update(model);
        }

        public void Delete(int productModelId)
        {
            _repository.Delete(productModelId);
        }
    }
}
