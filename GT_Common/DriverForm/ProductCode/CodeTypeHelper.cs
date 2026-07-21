using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.ProductCode
{
    public static class CodeTypeHelper
    {
        
        public static string GetPartName(CodeType type)
        {
            var key = type.ToString();

            var map = ProductCodeSettingService.Instance.CodeTypeMap;

            if (map != null && map.ContainsKey(key))
                return map[key];

            return key; // fallback
        }
    }
}
