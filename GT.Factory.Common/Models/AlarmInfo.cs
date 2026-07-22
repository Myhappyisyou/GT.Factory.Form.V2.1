using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Models
{
    public class AlarmInfo
    {
        //  ID
        public int Id { get; set; }
        //  工序号
        public string ProcessNo { get; set; }

        //  故障发生工位
        public string AlarmStation { get; set; }

        //  机台名称
        public string ProcessName { get; set; }

        //  故障类型
        public string AlarmGrade { get; set; }

        //  PLC地址
        public string PlcAddr { get; set; }

        //  故障描述
        public string Description { get; set; }

        //  发生时间
        public string CreateTime { get; set; }

        //  结束时间
        public string CloseTime { get; set; }

        //  更新标识
        public string Flag { get; set; } = "F";

    }

}
