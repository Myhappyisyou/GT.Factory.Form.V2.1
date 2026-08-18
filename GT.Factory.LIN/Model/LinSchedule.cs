using System.Collections.Generic;


namespace GT.Factory.LIN.Model
{


    /// <summary>
    /// LIN Schedule模型
    ///
    /// LIN Master按照Schedule表运行。
    ///
    /// 一个Schedule包含多个发送任务。
    ///
    /// 示例：
    ///
    /// New_ScheduleTable_1
    ///
    ///     |
    ///     |
    ///     + ELOP_Cmd
    ///          delay 10ms
    ///
    ///     + ELOP_Rsp1
    ///          delay 10ms
    ///
    ///     + ELOP_Rsp2
    ///          delay 10ms
    ///
    /// </summary>
    public class LinSchedule
    {


        /// <summary>
        /// Schedule名称
        ///
        /// 对应LDF:
        ///
        /// Schedule_tables
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }



        /// <summary>
        /// Schedule执行列表
        ///
        /// 顺序非常重要。
        ///
        /// LIN Master按照列表顺序执行。
        ///
        /// </summary>
        public List<LinScheduleItem> Items
        {
            get;
            set;
        }
        =
        new List<LinScheduleItem>();


        public override string ToString()
        {
            return Name;
        }

    }





    /// <summary>
    /// Schedule中的一个任务项
    ///
    /// 描述：
    ///
    /// 某个时间点发送某个Frame。
    ///
    /// </summary>
    public class LinScheduleItem
    {


        /// <summary>
        /// Frame名称
        ///
        /// 对应:
        ///
        /// LinFrame.Name
        ///
        /// </summary>
        public string FrameName
        {
            get;
            set;
        }



        /// <summary>
        /// 绑定的Frame
        ///
        /// 
        /// Parser完成后可以关联。
        ///
        /// 好处:
        ///
        /// Scheduler运行时
        /// 不需要再次查找。
        ///
        /// </summary>
        public LinFrame Frame
        {
            get;
            set;
        }



        /// <summary>
        /// 调度间隔
        ///
        /// 单位:
        ///
        /// ms
        ///
        /// LDF:
        ///
        /// delay 10 ms
        ///
        /// </summary>
        public int DelayMs
        {
            get;
            set;
        }



        /// <summary>
        /// 执行顺序
        ///
        /// Schedule中：
        ///
        /// 第几个执行。
        ///
        /// </summary>
        public int Order
        {
            get;
            set;
        }


        public override string ToString()
        {
            return $"{FrameName} {DelayMs}ms";
        }

    }


}

