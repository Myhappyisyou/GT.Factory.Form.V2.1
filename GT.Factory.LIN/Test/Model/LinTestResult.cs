using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{
    public class LinTestResult
    {

        /// <summary>
        /// 测试名称
        /// </summary>
        public string TestName
        {
            get;
            set;
        }

        /// <summary>
        /// 总结果
        /// </summary>
        public TestResultState State
        {
            get;
            set;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// 每步结果
        /// </summary>
        public List<LinStepResult> Steps
        {
            get;
            set;
        }
        =
        new List<LinStepResult>();


    }
}
