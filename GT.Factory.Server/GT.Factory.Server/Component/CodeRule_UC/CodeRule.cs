using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeRule
{
    public class CodeRule
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; }
        public string RuleDescription { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string CodeRegex { get; set; }
        public List<CodeRuleDetail> Details { get; set; } = new List<CodeRuleDetail>();
    }

    public class CodeRuleDetail
    {
        public int DetailId { get; set; }
        public int RuleId { get; set; }
        public string SegmentType { get; set; } // FixedText, Date, SerialNumber, Variable
        public string SegmentValue { get; set; }
        public string SegmentFormat { get; set; }
        public int? SegmentLength { get; set; }
        public int SegmentOrder { get; set; }
        public char PaddingChar { get; set; } = '0';
        public string PaddingSide { get; set; } = "Left"; // Left or Right
    }

    public class CodeSerialNumber
    {
        public int SerialId { get; set; }
        public int RuleId { get; set; }
        public string Prefix { get; set; }
        public int CurrentValue { get; set; }
        public string ResetMode { get; set; } // Never, Daily, Monthly, Yearly
        public DateTime? LastResetDate { get; set; }
    }
}
