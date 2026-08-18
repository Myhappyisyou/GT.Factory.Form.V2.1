using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Models
{
    public class SearchDataPayload
    {
        //  工序
        public string ProcessNo { get; set; }

        //  起始时间
        public DateTime? StartTime { get; set; }

        //  结束时间
        public DateTime? EndTime { get; set; }

        //  结果
        public string OkFlag { get; set; }

        //  过滤列
        public string FilterColumn { get; set; }

        //  过滤值
        public string FilterValue { get; set; }

    }

    public class AccessTestDataDto
    {
        public int ID { get; set; }

        public string 工位名称 { get; set; }

        public string 当前工单号 { get; set; }

        public string 产品条码 { get; set; }

        public string 操作人员 { get; set; }

        public string 测试时间 { get; set; }

        public string 测试结果 { get; set; }

        public string 测试节拍 { get; set; }

        public string 测试项名称 { get; set; }

        public string 测试项上限 { get; set; }

        public string 测试项下限 { get; set; }

        public string 测试项实际值 { get; set; }

        public string 更新标识 { get; set; }

    }
}
