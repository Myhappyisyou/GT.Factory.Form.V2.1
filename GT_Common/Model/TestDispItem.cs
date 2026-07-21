using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class TestDispItem
    {
        public string OrderNub { get; set; }

        public string BarNo { get; set; }

        public string MainBar { get; set; }

        public string PartBar { get; set; }

        public string UserName { get; set; }

        public string TaktTime { get; set; }

        public string DoTime { get; set; } = "";

        public string Ok_flag { get; set; } = "";

        public string MesResult { get; set; }

        public List<PlcMeasureGroup> PlcMeasureGroups { get; set; }
    }
}
