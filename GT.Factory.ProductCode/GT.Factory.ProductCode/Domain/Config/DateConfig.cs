using System.Collections.Generic;

namespace GT.Factory.ProductCode.Domain.Config
{
    /// <summary>
    /// 日期 Segment 的完整配置。
    ///
    /// Date 不再使用固定 Format 数字。
    /// 一个日期 Segment 由多个 Part 按顺序组成。
    /// </summary>
    public class DateConfig
    {
        public DateConfig()
        {
            Parts = new List<DatePartConfig>();
        }

        public List<DatePartConfig> Parts { get; set; }
    }
}
