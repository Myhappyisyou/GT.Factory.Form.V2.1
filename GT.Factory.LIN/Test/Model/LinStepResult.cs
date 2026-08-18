using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{
    public class LinStepResult
    {

        /// <summary>
        /// 步骤号
        /// </summary>
        public int StepNo
        {
            get;
            set;
        }


        /// <summary>
        /// 步骤名称
        ///
        /// </summary>
        public string StepName
        {
            get;
            set;
        }


        /// <summary>
        /// 是否成功
        ///
        /// </summary>
        public bool Success
        {
            get;
            set;
        }


        /// <summary>
        /// 实际值
        ///
        /// </summary>
        public object ActualValue
        {
            get;
            set;
        }


        /// <summary>
        /// 信息
        ///
        /// </summary>
        public string Message
        {
            get;
            set;
        }

    }
}
