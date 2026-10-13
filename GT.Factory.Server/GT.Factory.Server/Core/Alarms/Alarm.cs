using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.Alarms
{
    public  class Alarm
    {
        [DisplayName("ID")]
        public int Id { get; private set; }

        [DisplayName("故障发生工位")]
        public int AlarmStation { get; private set; }
        [DisplayName("机台名称")]
        public string ProcessName { get; private set; }

        [DisplayName("故障类型")]
        public string AlarmGrade { get; private set; }

        [DisplayName("故障描述")]
        public string Description { get; private set; }

        [DisplayName("发生时间")]
        public string CreateTime { get; private set; }

        [DisplayName("结束时间")]
        public string CloseTime { get; private set; }

        [DisplayName("更新标识")]
        public string Flag { get; private set; } = "F";

    }
}
