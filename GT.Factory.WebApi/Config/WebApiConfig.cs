using GT.Factory.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GT.Factory.Login.Login
{
    /// <summary>
    /// 登录模块配置
    /// 只负责“登录策略”
    /// 不包含硬件通讯参数
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(SortedExpandableObjectConverter))]   // ✅ 关键
    public class WebApiConfig
    {

        [Category("服务配置"), DisplayName("DB任务地址"), PropertyOrder(1)]
        [Browsable(true)]
        public string ServerDbtask { get; set; } = @"http://192.168.1.232:4444/dbtask/"; // 目标电脑 IP 和共享名

        [Category("服务配置"), DisplayName("API地址"), PropertyOrder(2)]
        [Browsable(true)]
        public string ServerApi { get; set; } = @"http://192.168.1.232:4443/"; // 目标电脑 IP 和共享名

    }

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
