using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.Model
{
    public class currentProductDataModel
    {
        public string Sn { get; set; }
        public BindingList<TestItemMD>  Data { get; set; }
        
    }
}
