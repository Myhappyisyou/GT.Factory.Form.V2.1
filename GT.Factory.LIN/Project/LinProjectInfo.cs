using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Project
{
    /// <summary>
    /// LIN工程基本信息。
    /// </summary>
    public class LinProjectInfo
    {
        /// <summary>
        /// 工程名称
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 工程描述
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 工程文件版本
        /// </summary>
        public string Version
        {
            get;
            set;
        }


        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime
        {
            get;
            set;
        }
    }
}
