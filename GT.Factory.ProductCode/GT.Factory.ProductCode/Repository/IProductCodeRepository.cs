using GT.Factory.Common.Models.ProductCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.ProductCode.Repository
{
    public interface IProductCodeRepository
    {
        /// <summary>
        /// 获取单个规则。
        /// </summary>
        ProductCodeResult GetCode(int ruleId);

        /// <summary>
        /// 获取流水号
        /// </summary>
        long GetCode(int ruleId, string resetModel, DateTime generationTime);

        /// <summary>
        /// 获取班次
        /// </summary>
        string GetCurrentShift(DateTime generationTime);

        /// <summary>
        /// 保存历史产品码
        /// </summary>
        void AddHistory(string productCode,
            int ruleId, DateTime generationTime);

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        DateTime GetDatabaseTime();
    }

}
