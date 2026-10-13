using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.TreeEdit
{
    // 树节点配置类
    public class TreeNodeConfig
    {
        public string Name { get; set; }
        public bool Checked { get; set; }
        public string Tag { get; set; }
        public List<TreeNodeConfig> Children { get; set; }
    }
}
