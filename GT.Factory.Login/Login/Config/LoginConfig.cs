using GT.Factory.Common.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
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
    public class LoginConfig
    {
       
        #region 登陆缓存

        // ✅ 新增登录缓存控制参数
        [Category("登陆系统配置"), DisplayName("启用登录超时"), PropertyOrder(1)]
        public bool EnableLoginTimeout { get; set; } = true;

        [Category("登陆系统配置"), DisplayName("登录超时(分钟)"), PropertyOrder(2)]
        public int LoginTimeoutMinutes { get; set; } = 5;

        #endregion

        [JsonProperty]
        [Category("登陆系统配置"), DisplayName("身份识别方式"), PropertyOrder(3)]
        [Description("可多选：账号、刷卡、角色选择")]
        [Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(UITypeEditor))]
        public List<IdentityType> IdentityTypes { get; set; }

        [JsonProperty]
        [Category("登陆系统配置"), DisplayName("是否需要MES验证"), PropertyOrder(5)]
        [Description("启用后登录会进行MES校验")]
        public bool NeedMesValidation { get; set; }

        [JsonProperty]
        [Category("登陆系统配置"), DisplayName("角色列表"), PropertyOrder(4)]
        [Description("RoleSelect 模式使用")]
        [Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(UITypeEditor))]
        public List<UserLevel> Roles { get; set; }

        /// <summary>
        /// ✅ 控制 PropertyGrid 显示文本
        /// </summary>
        public override string ToString()
        {
            var names = IdentityTypes.Select(x => GetDescription(x));
            return $"登录方式: {string.Join(",", names)}";
        }

        private string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

            return attr?.Description ?? value.ToString();
        }

        /// <summary>
        /// 合法性校验
        /// </summary>
        public void Validate()
        {
            if (IdentityTypes == null || IdentityTypes.Count == 0)
                throw new InvalidOperationException("必须至少启用一种身份识别方式");

            if (IdentityTypes.Contains(IdentityType.RoleSelect) &&
                (Roles == null || Roles.Count == 0))
                throw new InvalidOperationException("启用角色选择必须配置角色列表");
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
