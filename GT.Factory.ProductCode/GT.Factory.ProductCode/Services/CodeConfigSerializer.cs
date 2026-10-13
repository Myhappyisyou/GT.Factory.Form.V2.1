using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.ProductCode.Domain.Config;
using Newtonsoft.Json;
using System;
using DateConfig = GT.Factory.ProductCode.Domain.Config.DateConfig;
using FixedTextConfig = GT.Factory.ProductCode.Domain.Config.FixedTextConfig;

namespace GT.Factory.ProductCode.Services
{
    /// <summary>
    /// Segment 配置 JSON 序列化器。
    ///
    /// 新架构中配置协议只保留当前版本，不再兼容旧 Format/Padding 字段。
    /// </summary>
    public class CodeConfigSerializer
    {
        public string Serialize(object config)
        {
            if (config == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(config);
        }

        public object Deserialize(CodeSegmentType segmentType, string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return null;
            }

            switch (segmentType)
            {
                case CodeSegmentType.FixedText:
                    return JsonConvert.DeserializeObject<FixedTextConfig>(configJson);
                case CodeSegmentType.Date:
                    return JsonConvert.DeserializeObject<DateConfig>(configJson);
                case CodeSegmentType.SerialNumber:
                    return JsonConvert.DeserializeObject<SerialConfig>(configJson);
                case CodeSegmentType.Shift:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException("segmentType", segmentType, "不支持的产品码段类型。");
            }
        }

        public T Deserialize<T>(string configJson) where T : class
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(configJson);
        }

        public object CreateDefaultConfig(CodeSegmentType segmentType)
        {
            switch (segmentType)
            {
                case CodeSegmentType.FixedText:
                    return new FixedTextConfig { Text = "GT" };

                case CodeSegmentType.Date:
                    DateConfig date = new DateConfig();
                    date.Parts.Add(new DatePartConfig { Part = "Year", Mode = "Full" });
                    date.Parts.Add(new DatePartConfig { Part = "Month", Mode = "TwoDigit" });
                    date.Parts.Add(new DatePartConfig { Part = "Day", Mode = "TwoDigit" });
                    return date;

                case CodeSegmentType.SerialNumber:
                    return new SerialConfig
                    {
                        Length = 6,
                        PaddingChar = "0",
                        ResetMode = "None"
                    };


                case CodeSegmentType.Shift:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException("segmentType", segmentType, "不支持的产品码段类型。");
            }
        }

        public bool IsConfigMatch(CodeSegmentType segmentType, object config)
        {
            if (config == null)
            {
                return segmentType == CodeSegmentType.Shift;
            }

            switch (segmentType)
            {
                case CodeSegmentType.FixedText:
                    return config is FixedTextConfig;
                case CodeSegmentType.Date:
                    return config is DateConfig;
                case CodeSegmentType.SerialNumber:
                    return config is SerialConfig;
                default:
                    return false;
            }
        }
    }
}
