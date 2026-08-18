using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{

    //Motor_Function_Test

    //步骤:

    //1 设置速度
    //2 等待
    //3 检查反馈速度

    /// <summary>
    /// LIN测试案例
    ///
    /// 一个TestCase代表一次完整测试流程。
    ///一个完整测试流程
    /// 示例:
    ///
    /// 电机功能测试
    ///
    /// 包含:
    ///
    /// 设置速度
    /// 等待
    /// 检查反馈
    ///
    /// </summary>
    public class LinTestCase
    {

        /// <summary>
        /// 测试名称
        ///
        /// 示例:
        ///
        /// Motor_Function_Test
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 描述
        ///
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 是否自动执行
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }
        = true;

        /// <summary>
        /// 测试步骤
        ///
        /// 按顺序执行
        ///
        /// </summary>
        public List<LinTestStep> Steps
        {
            get;
            set;
        }
        =
        new List<LinTestStep>();

    }

}
