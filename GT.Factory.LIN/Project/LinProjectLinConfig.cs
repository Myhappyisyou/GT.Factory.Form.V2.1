using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Project
{
    /// <summary>
    /// LIN工程相关文件配置。
    /// </summary>
    public class LinProjectLinConfig
    {
        /// <summary>
        /// LDF文件路径。
        ///
        /// 相对于Project文件目录。
        /// </summary>
        public string Ldf
        {
            get;
            set;
        }

        /// <summary>
        /// LIN运行配置文件。
        ///
        /// 相对于Project文件目录。
        /// </summary>
        public string Simulation
        {
            get;
            set;
        }
    }
}
