using System.Collections.Generic;


namespace GT.Factory.LIN.Config
{

    /// <summary>
    /// LIN运行配置
    ///
    /// 保存调试工具启动参数。
    ///
    /// 不保存LDF协议内容。
    ///
    /// LDF负责:
    ///     LIN协议定义
    ///
    /// Config负责:
    ///     软件运行配置
    ///
    /// </summary>
    public class LinSimulationConfig
    {

        /// <summary>
        /// 默认Schedule
        ///
        /// 例如:
        ///
        /// New_ScheduleTable_1
        ///
        /// </summary>
        public string Schedule
        {
            get;
            set;
        }

        /// <summary>
        /// Slave行为配置
        ///
        /// 用于指定:
        ///
        /// Slave_1 -> Motor
        ///
        /// Slave_2 -> Door
        ///
        /// </summary>
        public List<LinSlaveSimulationConfig> Slaves
        {
            get;
            set;
        }

    }

    /// <summary>
    /// Slave配置
    ///
    /// </summary>
    public class LinSlaveSimulationConfig
    {

        /// <summary>
        /// Slave名称
        ///
        /// 对应:
        ///
        /// LDF Node Name
        ///
        /// </summary>
        public string NodeName
        {
            get;
            set;
        }

        /// <summary>
        /// 行为类型
        ///
        /// 例如:
        ///
        /// Motor
        ///
        /// </summary>
        public string Behavior
        {
            get;
            set;
        }

    }

}

