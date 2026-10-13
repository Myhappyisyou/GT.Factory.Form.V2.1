using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Domain;
using System;

namespace GT.Factory.ProductCode.Generator
{
    /// <summary>
    /// 产品码 Segment 处理器统一接口。
    ///
    /// Generator 只依赖这个接口，不关心具体 Segment 的业务规则。
    /// </summary>
    public interface ICodeSegmentProcessor
    {
        CodeSegmentType SegmentType { get; }

        string Process(
            CodeRuleDetail detail,
            DateTime now);
    }
}
