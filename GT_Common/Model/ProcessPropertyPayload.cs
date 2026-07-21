using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class ProcessPropertyPayload
    {
        public string BarNo { get; set; }
        public string ProcessNo { get; set; }
        public string VouNo { get; set; }
        public string OkFlag { get; set; }
        public string NgMsg { get; set; }
        public string UserId { get; set; }
        public string[] Data { get; set; }
    }
}
