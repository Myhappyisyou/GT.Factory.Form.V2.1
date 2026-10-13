using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.Consumable
{
    public class Consumables
    {
        [DisplayName("ID")]
        public int ID { get; set; }
        [DisplayName("易损件所在工位")]
        public string ProcessName { get; set; }
        [DisplayName("机台名称")]
        public string StationName { get; set; }
        [DisplayName("易损件所在位置")]
        public string Location { get; set; }
        [DisplayName("易损件名称")]
        public string Name { get; set; }
        [DisplayName("易损件理论使用次数")]
        public int TheoreticalCount { get; set; }
        [DisplayName("易损件已使用次数")]
        public int UsedCount { get; set; }
        [DisplayName("易损件剩余使用次数")]
        public int RemainderCount { get; set; }
    }
}
