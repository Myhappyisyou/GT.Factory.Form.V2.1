using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{

    /// <summary>
    /// LIN测试动作类型
    ///
    /// 描述一个测试Step执行什么操作
    ///
    /// </summary>
    public enum LinTestAction
    {
        /// <summary>
        /// 无动作
        /// </summary>
        None,

        /// <summary>
        /// 设置Signal值
        ///
        /// 例如:
        ///
        /// WRITE_SPEED=3000
        ///
        /// </summary>
        SetSignal,

        /// <summary>
        /// 等待
        ///
        /// </summary>
        Wait,

        /// <summary>
        /// 读取Signal
        ///
        /// </summary>
        ReadSignal,

        /// <summary>
        /// 判断Signal
        ///
        /// </summary>
        CheckSignal,

        /// <summary>
        /// 发送Frame
        ///
        /// 手动触发Frame
        ///
        /// </summary>
        SendFrame,

        /// <summary>
        /// 接收等待Frame
        ///
        /// </summary>
        WaitFrame

    }


    /// <summary>
    /// 信号比较方式
    ///
    /// </summary>
    public enum CompareType
    {


        /// <summary>
        /// 等于
        /// </summary>
        Equal,


        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual,


        /// <summary>
        /// 大于
        /// </summary>
        Greater,


        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterOrEqual,


        /// <summary>
        /// 小于
        /// </summary>
        Less,


        /// <summary>
        /// 小于等于
        /// </summary>
        LessOrEqual,


        /// <summary>
        /// 范围判断
        ///
        /// 例如:
        ///
        /// 2800~3200 rpm
        ///
        /// </summary>
        Range


    }


    public enum TestResultState
    {

        None,


        Running,


        Pass,


        Fail,


        Abort

    }


    public enum LinValueType
    {


        /// <summary>
        /// 原始值
        ///
        /// 例如:
        ///
        /// Raw=3000
        ///
        /// </summary>
        Raw,


        /// <summary>
        /// 工程值
        ///
        /// 例如:
        ///
        /// 3000 rpm
        ///
        /// </summary>
        Physical


    }


}
