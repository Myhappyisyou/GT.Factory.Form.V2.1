using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskContracts.Models
{
    public class ProcessData
    {
        public DataItem DataItem { get; set; }
        public string Process_no { get; set; }
        public string Order_no { get; set; }
        public string Operator_no { get; set; }
        public string Do_time { get; set; }
        public string[] Ok_flag { get; set; }
        public string Test_beat { get; set; }
    }

    public class DataItem
    {
        /// <summary>
        /// 产品码
        /// </summary>
        public string Sn;
        /// <summary>
        /// 时间
        /// </summary>
        public string Do_time;
        /// <summary>
        /// 数据
        /// </summary>
        public string[] Data;
        /// <summary>
        /// 上限
        /// </summary>
        public string[] Data_up;
        /// <summary>
        /// 下限
        /// </summary>
        public string[] Data_down;
        /// <summary>
        /// 结果
        /// </summary>
        public string[] Data_result;
        /// <summary>
        /// 测试项
        /// </summary>
        public string[] Data_name;
        /// <summary>
        /// NG信息
        /// </summary>
        public string NgMsg = "";
        /// <summary>
        /// 结果
        /// </summary>
        public string Result;
        /// <summary>
        /// 子工序号
        /// </summary>
        public int Step = 1;
        /// <summary>
        /// 最后子工序标志
        /// </summary>
        public bool IsLastStep;

    }

}
