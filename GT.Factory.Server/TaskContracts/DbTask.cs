using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskContracts
{
    public class DbTask
    {
        public string TaskType { get; set; }     // 任务类型
        public string PayloadJson { get; set; }  // JSON 数据
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

}
