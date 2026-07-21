using GT_Common.MyEnum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Core
{
    /// <summary>
    /// 单个数据库节点配置
    /// 表示一个数据库连接
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DatabaseNodeOptions
    {
        /// <summary>
        /// 是否启用该数据库节点
        /// </summary>
        [JsonProperty]
        [DisplayName("是否启用")]
        [Description("不启用时该数据库角色无效")]
        public bool Enabled { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        [JsonProperty]
        [DisplayName("数据库类型")]
        [Description("可选：SQL SERVER、MYSQL、SQLITE/ACCESS")]
        public DatabaseType DbType { get; set; }

        /// <summary>
        /// 原始连接字符串
        /// </summary>
        [JsonProperty]
        [DisplayName("连接配置")]
        [Description("SQLite填写数据库文件名，例如 XMOilPumpData.sqlite")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// 是否启用数据库路由
        /// （例如按年份分库）
        /// </summary>
        [JsonProperty]
        [DisplayName("是否启用数据库分库")]
        [Description("例如按年份分库")]
        public bool EnableRouting { get; set; }

        /// <summary>
        /// 路由策略名称（可扩展）
        /// 例如：YearRouting / ShiftRouting
        /// </summary>
        [JsonProperty]
        [DisplayName("数据库分库策略")]
        [Description("例如按年份分库YearRouting / ShiftRouting")]
        public string RoutingStrategy { get; set; }

        [Browsable(false)]
        public string RuntimeConnectionString
        {
            get
            {
                return DatabaseConnectionResolver.Resolve(this);
            }
        }

        /// <summary>
        /// ✅ 控制 PropertyGrid 显示文本
        /// </summary>
        public override string ToString()
        {
            return $"数据库类型: {GetDescription(DbType)}";
        }


        private string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

            return attr?.Description ?? value.ToString();
        }
    }
}