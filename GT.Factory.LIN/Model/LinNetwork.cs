using System.Collections.Generic;
using System.Linq;

namespace GT.Factory.LIN.Model
{
    /// <summary>
    /// LIN网络模型
    ///
    /// 一个LDF文件对应一个LIN网络。
    ///
    /// 例如：
    /// LIN_description_file
    ///
    /// 内包含：
    /// 1. LIN版本
    /// 2. 波特率
    /// 3. 节点
    /// 4. Frame
    /// 5. Signal
    /// 6. Schedule
    ///
    /// 解析LDF后，最终生成此对象。
    /// </summary>
    public class LinNetwork
    {

        /// <summary>
        /// LIN协议版本
        ///
        /// 示例：
        /// 2.1
        /// </summary>
        public string ProtocolVersion
        {
            get;
            set;
        }


        /// <summary>
        /// LIN语言版本
        ///
        /// 示例：
        /// 2.1
        /// </summary>
        public string LanguageVersion
        {
            get;
            set;
        }


        /// <summary>
        /// LIN通信速率
        ///
        /// 单位：
        /// bit/s
        ///
        /// 示例：
        /// 19200
        /// </summary>
        public int BaudRate
        {
            get;
            set;
        }



        /// <summary>
        /// 网络中的所有节点
        ///
        /// 包含：
        /// Master
        /// Slave
        /// </summary>
        public List<LinNode> Nodes
        {
            get;
            set;
        }
        =
        new List<LinNode>();


        


        /// <summary>
        /// 网络中的所有Frame
        ///
        /// 示例：
        /// ELOP_Rsp1
        /// ELOP_Cmd
        /// </summary>
        public List<LinFrame> Frames
        {
            get;
            set;
        }
        =
        new List<LinFrame>();



        /// <summary>
        /// 网络中的所有Signal
        ///
        /// Signal属于Frame的数据字段。
        /// </summary>
        public List<LinSignal> Signals
        {
            get;
            set;
        }
        =
        new List<LinSignal>();



        /// <summary>
        /// Schedule列表
        ///
        /// LIN Master根据Schedule发送Frame。
        /// </summary>
        public List<LinSchedule> Schedules
        {
            get;
            set;
        }
        =
        new List<LinSchedule>();

        /// <summary>
        /// Master节点
        /// LIN规定只能一个
        /// </summary>
        public LinNode Master
        {
            get
            {
                return Nodes.FirstOrDefault(
                    x => x.Type == LinNodeType.Master);
            }
        }

        /// <summary>
        /// 获取所有Slave节点
        ///
        /// 返回：
        ///
        /// Type = Slave
        ///
        /// 的节点集合。
        ///
        /// </summary>
        public IEnumerable<LinNode> Slaves
        {
            get
            {
                return Nodes
                    .Where(
                        x => x.Type == LinNodeType.Slave);
            }
        }


        public LinDiagnostic Diagnostic
        {
            get;
            set;
        }

    }
}
