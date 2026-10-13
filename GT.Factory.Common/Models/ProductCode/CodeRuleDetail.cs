using GT.Factory.Common.Enums.ProductCodeEnums;

namespace GT.Factory.Common.Models.ProductCode
{
    /// <summary>
    /// 产品码规则明细。
    ///
    /// 每一行代表产品码的一部分。
    ///
    /// 例如：
    ///
    /// 1 固定 GT
    /// 2 产品型号
    /// 3 日期
    /// 4 流水号
    ///
    /// </summary>
    public class CodeRuleDetail
    {

        /// <summary>
        /// 明细ID。
        /// </summary>
        public int DetailId
        {
            get;
            set;
        }


        /// <summary>
        /// 所属规则ID。
        /// </summary>
        public int RuleId
        {
            get;
            set;
        }


        /// <summary>
        /// 排序号。
        ///
        /// 决定生成顺序。
        /// </summary>
        public int SegmentOrder
        {
            get;
            set;
        }


        /// <summary>
        /// 段类型。
        ///
        /// 根据此类型决定：
        ///
        /// 1. UI显示
        /// 2. 配置模型
        /// 3. 生成逻辑
        ///
        /// </summary>
        public CodeSegmentType SegmentType
        {
            get;
            set;
        }


        /// <summary>
        /// 段配置JSON。
        ///
        /// 根据SegmentType不同，
        /// 保存不同配置模型。
        ///
        /// FixedText:
        /// {
        ///    "Text":"GT"
        /// }
        ///
        /// DateTime:
        /// {
        ///    "Format":1
        /// }
        ///
        /// Serial:
        /// {
        ///    "Length":6,
        ///    "Padding":"0"
        /// }
        ///
        /// </summary>
        public string ConfigJson
        {
            get;
            set;
        }


        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }
    }
}