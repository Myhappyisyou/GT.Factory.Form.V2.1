using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.Alarms
{
    public class AlaemParse
    {
        [DisplayName("ID")]
        public int Id { get; private set; }
        [DisplayName("工序号")]
        public string ProcessNo { get; private set; }

        [DisplayName("机台名称")]
        public string ProcessName { get; private set; }

        [DisplayName("故障发生工位")]
        public string AlarmStation { get; private set; }
        [DisplayName("故障类型")]
        public string AlarmGrade { get; private set; }
        [DisplayName("PLC地址")]
        public string PlcAddr { get; private set; }

        [DisplayName("报警信息")]
        public string Description { get; private set; }
    }
}
