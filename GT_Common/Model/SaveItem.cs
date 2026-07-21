using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class SaveItem
    {
        [DisplayName("工位名称")]
        public string Process_no { get; set; }
        [DisplayName("当前工单号")]
        public string Order_no { get; set; }
        [DisplayName("产品条码")]
        public string Bar_no { get; set; }
        [DisplayName("操作人员")]
        public string Operator_no { get; set; }
        [DisplayName("测试时间")]
        public string Do_time { get; set; }
        [DisplayName("测试结果")]
        public string Ok_flag { get; set; }
        [DisplayName("测试节拍")]
        public string Test_beat { get; set; }
        [DisplayName("测试项名称")]
        public string Test_item_name { get; set; }
        [DisplayName("测试项上限")]
        public string Test_item_up { get; set; }
        [DisplayName("测试项下限")]
        public string Test_item_down { get; set; }
        [DisplayName("测试项实际值")]
        public string Test_item_value { get; set; }

        [DisplayName("测试项单位")]
        public string Test_item_unit { get; set; }

    }
}
