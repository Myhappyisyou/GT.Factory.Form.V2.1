using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.ChangeRecordHelp
{
    // 变更记录实体类
    public class ChangeRecord
    {
        [DisplayName("时间戳")]
        public DateTime Timestamp { get; set; }

        [DisplayName("控件名称")]
        public string ControlName { get; set; }

        [DisplayName("控件类型")]
        public string ControlType { get; set; }

        [DisplayName("旧值")]
        public string OldValue { get; set; }

        [DisplayName("新值")]
        public string NewValue { get; set; }

        /// <summary>
        /// 获取变更摘要
        /// </summary>
        public string ChangeSummary => $"{ControlName}: {OldValue} → {NewValue}";
    }
}


