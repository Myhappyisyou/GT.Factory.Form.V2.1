using System.Collections.Generic;

namespace GT.Factory.ProductCode.Domain.Config
{
    /// <summary>
    /// 通用值映射定义。
    ///
    /// Key 和 Value 都使用字符串，避免把映射能力限制在日期领域。
    /// </summary>
    public class MappingDefinition
    {
        public MappingDefinition()
        {
            Values = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Values { get; set; }
    }
}
