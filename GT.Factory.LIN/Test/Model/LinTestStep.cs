using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{

    //Step1:

    //SetSignal

    //WRITE_SPEED = 3000

    //Step2:

    //Wait

    //Step3:

    //CheckSignal

    //READ_ACTUAL_SPEED

    /// <summary>
    /// LIN测试步骤
    ///
    /// 一个Step完成一次测试动作
    ///最小测试动作
    /// </summary>
    public class LinTestStep
    {

        /// <summary>
        /// 步骤号
        ///
        /// </summary>
        public int StepNo
        {
            get;
            set;
        }

        /// <summary>
        /// 步骤名称
        ///
        /// 示例:
        ///
        /// 设置目标速度
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }
        = true;

        /// <summary>
        /// 动作类型
        ///
        /// Send
        /// Receive
        /// Wait
        /// Check
        ///
        /// </summary>
        public LinTestAction Action
        {
            get;
            set;
        }

        /// <summary>
        /// 关联Frame
        ///
        /// 用于:
        ///
        /// SendFrame
        /// WaitFrame
        ///
        /// </summary>
        public string FrameName
        {
            get;
            set;
        }

        /// <summary>
        /// Signal设置
        ///
        /// 用于:
        ///
        /// SetSignal
        /// 
        /// </summary>
        public List<LinSignalValue> Values
        {
            get;
            set;
        }
        =
        new List<LinSignalValue>();

        /// <summary>
        /// Signal检查
        ///
        /// </summary>
        public List<LinSignalCheck> Checks
        {
            get;
            set;
        }
        =
        new List<LinSignalCheck>();

        /// <summary>
        /// 超时时间
        ///
        /// ms
        ///
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }

    }

}
