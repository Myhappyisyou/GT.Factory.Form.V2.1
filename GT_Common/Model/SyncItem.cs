using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class SyncItem
    {
        public int Id { get; set; }

        /// <summary>
        /// 数据类型（例如：ProcessProperty）
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// JSON 数据
        /// </summary>
        public string DataJson { get; set; }

        public DateTime CreateTime { get; set; }

        public int RetryCount { get; set; }

        /// <summary>
        /// 0=待同步 1=成功 2=永久失败
        /// </summary>
        public int Status { get; set; }
    }
}
