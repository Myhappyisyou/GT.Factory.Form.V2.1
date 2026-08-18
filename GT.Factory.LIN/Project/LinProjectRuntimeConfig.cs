using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Project
{
    /// <summary>
    /// LIN工程运行配置。
    /// </summary>
    public class LinProjectRuntimeConfig
    {
        //// <summary>
        /// 运行模式
        ///
        /// Simulation
        /// Hardware
        /// </summary>
        public LinRunMode Mode
        {
            get;
            set;
        }

        /// <summary>
        /// 自动启动LIN
        /// </summary>
        public bool AutoStart
        {
            get;
            set;
        }
    }
}
