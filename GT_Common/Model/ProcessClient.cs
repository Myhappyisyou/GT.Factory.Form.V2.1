using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class ProcessClient
    {
        [DisplayName("ID")]
        public int ID { get; set; }

        [DisplayName("工序名")]
        public string ClientName { get; set; }

        [DisplayName("工序IP")]
        public string ClientIp { get; set; }
    }
}
