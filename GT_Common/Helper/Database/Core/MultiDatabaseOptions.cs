using GT_Common.MyEnum;
using GT_Common.ProcessConfig;
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
}
