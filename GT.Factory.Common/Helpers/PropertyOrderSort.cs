using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Helpers
{
    /// <summary>
    /// 属性排序辅助类，用于在属性网格中按指定顺序显示属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyOrderAttribute : Attribute
    {
        public int Order { get; }
        public PropertyOrderAttribute(int order) => Order = order;
    }

    public class SortedExpandableObjectConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(
            ITypeDescriptorContext context,
            object value,
            Attribute[] attributes)
        {
            // 获取原始属性列表
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(value, attributes);

            // 按 PropertyOrderAttribute 排序
            var sorted = props.Cast<PropertyDescriptor>()
                .OrderBy(p =>
                {
                    var attr = p.Attributes
                        .OfType<PropertyOrderAttribute>()
                        .FirstOrDefault();
                    return attr?.Order ?? int.MaxValue; // 没有标记的排最后
                })
                .ToArray();

            return new PropertyDescriptorCollection(sorted);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            => true; // ✅ 必须返回 true，否则 GetProperties 不会被调用
    }

}
