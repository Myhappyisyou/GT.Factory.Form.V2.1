namespace GT.Factory.ProductCode.Domain.Config
{
    /// <summary>
    /// 日期中的一个组成部分。
    /// </summary>
    public class DatePartConfig
    {
        public string Part { get; set; }
        public string Mode { get; set; }
        public MappingDefinition Mapping { get; set; }
    }
}
