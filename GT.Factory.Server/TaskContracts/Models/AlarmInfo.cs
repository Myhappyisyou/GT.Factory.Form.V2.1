using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskContracts.Models
{
    public class AlarmInfo
    {
        public string ProcessName { get; set; }
        public string AlarmStation { get; set; }
        public string AlarmGrade { get; set; }
        public string Description { get; set; }
        public string CreateTime { get; set; }
        public string CloseTime { get; set; }
    }

}
