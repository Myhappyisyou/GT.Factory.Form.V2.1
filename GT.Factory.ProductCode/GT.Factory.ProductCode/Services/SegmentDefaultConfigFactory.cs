using GT.Factory.Common.Enums.ProductCodeEnums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.ProductCode.Services
{
    /// <summary>
    /// Segment 默认配置工厂。
    ///
    /// 负责给新增加的 Segment 创建一份“合法的初始配置”。
    ///
    /// 注意：
    /// 这里的配置不是最终用户配置，
    /// 而是保证 Segment 创建出来以后，
    /// 即使用户还没有修改，也能够被 UI 正常显示和编辑。
    /// </summary>
    public static class SegmentDefaultConfigFactory
    {
        /// <summary>
        /// 创建指定 Segment 类型的默认配置。
        /// </summary>
        public static string Create(
            CodeSegmentType segmentType)
        {
            JObject config;

            switch (segmentType)
            {
                case CodeSegmentType.FixedText:
                    config = CreateFixedText();
                    break;

                case CodeSegmentType.Date:
                    config = CreateDate();
                    break;

                case CodeSegmentType.Shift:
                    config = CreateShift();
                    break;


                case CodeSegmentType.SerialNumber:
                    config = CreateSerialNumber();
                    break;


                default:
                    config = new JObject();
                    break;
            }

            return config.ToString(
                Newtonsoft.Json.Formatting.None);
        }

        /// <summary>
        /// 固定文本默认配置。
        ///
        /// 默认：
        /// GT
        /// </summary>
        private static JObject CreateFixedText()
        {
            JObject config = new JObject();

            config["Text"] = "GT";

            return config;
        }

        /// <summary>
        /// 日期默认配置。
        ///
        /// 默认：
        /// 年后两位 + 月两位 + 日两位。
        ///
        /// 例如：
        /// 2026-09-21
        /// →
        /// 260921
        /// </summary>
        private static JObject CreateDate()
        {
            JObject config = new JObject();

            JArray parts = new JArray();

            JObject year = new JObject();
            year["Part"] = "Year";
            year["Mode"] = "LastTwo";

            JObject month = new JObject();
            month["Part"] = "Month";
            month["Mode"] = "TwoDigit";

            JObject day = new JObject();
            day["Part"] = "Day";
            day["Mode"] = "TwoDigit";

            parts.Add(year);
            parts.Add(month);
            parts.Add(day);

            config["Parts"] = parts;

            return config;
        }

        /// <summary>
        /// 班次默认配置。
        ///
        /// 默认不指定额外参数，
        /// 由 ShiftProcessor 根据当前班次配置计算。
        /// </summary>
        private static JObject CreateShift()
        {
            return new JObject();
        }

        /// <summary>
        /// 产品型号默认配置。
        ///
        /// 默认从 ProductCodeContext.Model 获取。
        /// </summary>
        private static JObject CreateProductModel()
        {
            return new JObject();
        }

        /// <summary>
        /// 流水号默认配置。
        ///
        /// 默认：
        /// 4位
        /// 左侧补0
        /// 不重置
        /// 正向递增
        /// </summary>
        private static JObject CreateSerialNumber()
        {
            JObject config = new JObject();

            config["Length"] = 4;
            config["PaddingChar"] = "0";
            config["ResetMode"] = 0;
            config["Direction"] = 1;

            return config;
        }

        /// <summary>
        /// 变量默认配置。
        ///
        /// 暂时不指定具体变量，
        /// 等用户进入配置界面后选择。
        /// </summary>
        private static JObject CreateVariable()
        {
            return new JObject();
        }
    }
}