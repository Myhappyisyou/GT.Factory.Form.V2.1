using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class ObjectExtensions
    {
        public static short ToInt16(this object value, short defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            return short.TryParse(value.ToString(), out short result) ? result : defaultValue;
        }

        public static int ToInt32(this object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            return int.TryParse(value.ToString(), out int result) ? result : defaultValue;
        }

        public static string ToStringSafe(this object value, string defaultValue = "")
        {
            return value?.ToString() ?? defaultValue;
        }
    }
}
