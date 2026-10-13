using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GT.Factory.ProductCode.Repository
{
    public class SqlProductCodeRepository : IProductCodeRepository
    {
        private readonly ProductCodeRepository _repository;

        public SqlProductCodeRepository(
            ProductCodeRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(
                    "repository");
            }

            _repository = repository;
        }

        public void AddHistory(string productCode, int ruleId, DateTime generationTime)
        {
            _repository
                         .AddHistory(productCode, ruleId, generationTime);
        }


        public ProductCodeResult GetCode(int ruleId)
        {
            return _repository
                           .GetCode(ruleId);
        }

        public long GetCode(int ruleId, string resetModel, DateTime generationTime)
        {
            return _repository
                           .GetCode(ruleId, resetModel, generationTime);
        }


        public string GetCurrentShift(DateTime generationTime)
        {
            return _repository
                          .GetCurrentShift(generationTime);
        }



        public DateTime GetDatabaseTime()
        {
            return _repository
                                      .GetDatabaseTime();
        }
    }
}

