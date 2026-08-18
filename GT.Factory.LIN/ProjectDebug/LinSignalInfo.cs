using GT.Factory.LIN.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.ProjectDebug
{


    /// <summary>
    /// LIN Signal 调试信息
    ///
    /// 一个Signal对应LDF中的一个信号定义。
    ///
    /// 例如:
    ///
    /// WRITE_SPEED
    ///
    /// READ_ACTUAL_SPEED
    ///
    ///
    /// 该对象主要用于:
    ///
    /// 1. 调试软件显示
    ///
    /// 2. Signal值修改
    ///
    /// 3. Signal值监控
    ///
    /// </summary>
    public class LinSignalInfo
    {


        /// <summary>
        /// Signal名称
        ///
        /// 来自LDF
        ///
        /// 例如:
        ///
        /// WRITE_SPEED
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }





        /// <summary>
        /// 所属Frame名称
        ///
        /// 一个Signal一定属于某个Frame
        ///
        /// 例如:
        ///
        /// WRITE_SPEED
        /// 
        /// 属于:
        ///
        /// ELOP_Cmd
        ///
        /// </summary>
        public string FrameName
        {
            get;
            set;
        }





        /// <summary>
        /// Frame发布节点
        ///
        /// 来自LDF
        ///
        /// 例如:
        ///
        /// LIN_Master
        ///
        /// Slave_1
        ///
        /// </summary>
        public string Publisher
        {
            get;
            set;
        }


        /// <summary>
        /// 原始LDF Signal
        /// </summary>
        public LinSignal Source
        {
            get;
            set;
        }


        // <summary>
        /// 当前值
        ///
        /// 实际来源:
        /// LinRuntime
        ///
        /// </summary>
        public object Value
        {
            get;
            internal set;
        }





        /// <summary>
        /// 是否Master发送
        ///
        /// true:
        ///     Master发送Frame
        ///
        /// false:
        ///     Slave响应
        ///
        /// </summary>
        /// <summary>
        /// Signal所属Frame
        /// </summary>
        public LinFrameInfo Frame
        {
            get;
            internal set;
        }



    }


}
