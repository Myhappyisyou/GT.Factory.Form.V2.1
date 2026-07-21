using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Batch
{
    public class PartRuntime
    {
        public string PartName { get; set; }
        //物料编码
        public string MaterialCode { get; set; }
        //byd批次
        public string BYDBatchNub { get; set; }

        public short TotalQty { get; set; }

        public short UsedQty { get; set; }

        public short RemainQty => (short)(TotalQty - UsedQty);

        public ClientBatchConfig Config { get; set; }
    }
}
