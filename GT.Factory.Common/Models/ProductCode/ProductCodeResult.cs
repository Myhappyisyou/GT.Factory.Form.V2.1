using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Models.ProductCode
{
    public class ProductCodeResult
    {
        public bool Success { get; set; }

        public string Code { get; set; }

        public string ErrorMessage { get; set; }
    }
}
