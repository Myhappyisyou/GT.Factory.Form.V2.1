using GT.Factory.Common.Enums;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;

namespace GT.Factory.Data.Core
{

    /// <summary>
    /// 多数据库配置
    /// 用于支持不同职责数据库
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MultiDatabaseOptions
    {
        public MultiDatabaseOptions()
        {
            Primary = new DatabaseNodeOptions();
            LocalCache = new DatabaseNodeOptions();
            Mes = new DatabaseNodeOptions();
            Log = new DatabaseNodeOptions();
        }

        /// <summary>
        /// 数据库表名前缀
        /// 例如：GT、SH
        /// </summary>
        [Category("数据库配置"), DisplayName("表名前缀")]
        [Description("数据库表名前缀，例如 GT 或 SH")]
        public TablePrefixType TablePrefix { get; set; } = TablePrefixType.GT;

        /// <summary>
        /// 主业务数据库
        /// </summary>
        [JsonProperty]
        [DisplayName("主库")]
        [Description("主业务数据库")]
        public DatabaseNodeOptions Primary { get; set; }

        /// <summary>
        /// 本地缓存数据库
        /// </summary>
        [JsonProperty]
        [DisplayName("本地数据库")]
        [Description("本地数据库")]
        public DatabaseNodeOptions LocalCache { get; set; }

        /// <summary>
        /// MES数据库
        /// </summary>
        [JsonProperty]
        [DisplayName("MES")]
        [Description("MES")]
        public DatabaseNodeOptions Mes { get; set; }

        /// <summary>
        /// 日志数据库
        /// </summary>
        [JsonProperty]
        [DisplayName("日志")]
        [Description("日志")]
        public DatabaseNodeOptions Log { get; set; }

        /// <summary>
        /// ✅ 控制 PropertyGrid 显示文本
        /// </summary>
        public override string ToString()
        {
            return $"数据库配置";
        }
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
