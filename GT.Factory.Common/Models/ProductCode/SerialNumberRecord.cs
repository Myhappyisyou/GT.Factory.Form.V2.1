using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Common.Models.ProductCode
{
    public class SerialNumberRecord
    {
        public string ResetMode { get; set; }

        public long CurrentValue { get; set; }

        public DateTime LastResetDate { get; set; }
    }
}
