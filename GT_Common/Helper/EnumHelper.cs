using GT_Common.DriverForm.ProductCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum value)
        {
            if (value == null) return "";

            FieldInfo field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                             .FirstOrDefault() as DescriptionAttribute;

            return attr?.Description ?? value.ToString();
        }
    }

    public static class EnumBindHelper
    {
        public static List<EnumItem> GetCodeTypeItems()
        {
            return Enum.GetValues(typeof(CodeType))
                .Cast<CodeType>()
                .Select(x => new EnumItem
                {
                    Value = x,
                    Text = EnumHelper.GetDescription(x)
                })
                .ToList();
        }
    }

    public class EnumItem
    {
        public CodeType Value { get; set; }
        public string Text { get; set; }
    }
}
