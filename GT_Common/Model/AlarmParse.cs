using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class AlarmParse
    {
        [DisplayName("ID")]
        public int Id { get;  set; }
        [DisplayName("工序号")]
        public string ProcessNo { get;  set; }

        [DisplayName("机台名称")]
        public string ProcessName { get;  set; }

        [DisplayName("故障发生工位")]
        public string AlarmStation { get;  set; }
        [DisplayName("故障类型")]
        public string AlarmGrade { get;  set; }
        [DisplayName("PLC地址")]
        public string PlcAddr { get;  set; }

        [DisplayName("报警信息")]
        public string Description { get;  set; }
    }
}
