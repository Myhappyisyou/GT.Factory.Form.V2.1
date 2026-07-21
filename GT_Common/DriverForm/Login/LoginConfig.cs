using GT_Common.MyEnum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;

namespace GT_Common.DriverForm.Login
{
    /// <summary>
    /// 登录模块配置
    /// 只负责“登录策略”
    /// 不包含硬件通讯参数
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]   // ✅ 关键
    public class LoginConfig
    {
        [JsonProperty]
        [DisplayName("身份识别方式")]
        [Description("可多选：账号、刷卡、角色选择")]
        [Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(UITypeEditor))]
        public List<IdentityType> IdentityTypes { get; set; }

        [JsonProperty]
        [DisplayName("是否需要MES验证")]
        [Description("启用后登录会进行MES校验")]
        public bool NeedMesValidation { get; set; }

        [JsonProperty]
        [DisplayName("角色列表")]
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
}
