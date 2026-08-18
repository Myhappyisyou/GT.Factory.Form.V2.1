using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Models
{
    /// <summary>
    /// 抽检点检基类
    /// </summary>
    public class CalibrationOrPeriodInspection
    {
        public string CalibrationOrPeriodStationId { get; set; }

        public string[] Test_item { get; set; }

        public string[] Test_item_type { get; set; }

        public string[] Test_item_up { get; set; }

        public string[] Test_item_down { get; set; }
    }

}
