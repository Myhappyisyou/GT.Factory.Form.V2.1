using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using System;
using System.Collections.Generic;

namespace GT.Factory.ProductCode.Generator
{
    /// <summary>
    /// Segment Processor 注册表。
    ///
    /// 新增 Segment 时只需要注册新的 Processor，
    /// ProductCodeGenerator 本身不需要增加 if/switch。
    /// </summary>
    public class ProcessorRegistry
    {
        private readonly Dictionary<CodeSegmentType, ICodeSegmentProcessor> _processors;

        public ProcessorRegistry(IEnumerable<ICodeSegmentProcessor> processors)
        {
            _processors = new Dictionary<CodeSegmentType, ICodeSegmentProcessor>();

            foreach (ICodeSegmentProcessor processor in processors)
            {
                if (processor == null)
                {
                    continue;
                }

                _processors[processor.SegmentType] = processor;
            }
        }

        public ICodeSegmentProcessor Get(CodeSegmentType segmentType)
        {
            ICodeSegmentProcessor processor;

            if (!_processors.TryGetValue(segmentType, out processor))
            {
                throw new InvalidOperationException(
                    "没有注册产品码 Segment Processor：" + segmentType);
            }

            return processor;
        }
    }
}
