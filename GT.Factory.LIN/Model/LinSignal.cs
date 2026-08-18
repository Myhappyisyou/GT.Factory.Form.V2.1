using System.Collections.Generic;


namespace GT.Factory.LIN.Model
{


    /// <summary>
    /// LIN Signal模型
    ///
    /// Signal是Frame中的最小数据单元。
    ///
    /// 一个Signal描述：
    ///
    /// 1. 名称
    /// 2. 位长度
    /// 3. 发布节点
    /// 4. 接收节点
    /// 5. 数值转换规则
    /// 6. 枚举定义
    ///
    /// 示例：
    ///
    /// READ_ACTUAL_SPEED
    ///
    /// Length:
    ///     16bit
    ///
    /// Unit:
    ///     rpm
    ///
    /// </summary>
    public class LinSignal
    {


        /// <summary>
        /// Signal名称
        ///
        /// 示例：
        ///
        /// READ_ACTUAL_SPEED
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Signal长度
        ///
        /// 单位:
        /// bit
        ///
        /// 示例:
        ///
        /// 16
        ///
        /// </summary>
        public ushort Length
        {
            get;
            set;
        }

        /// <summary>
        /// 初始值
        ///
        /// 对应LDF:
        ///
        /// 65535
        ///
        /// </summary>
        public object InitialValue
        {
            get;
            set;
        }

        /// <summary>
        /// 发布节点
        ///
        /// 示例:
        ///
        /// Slave_1
        ///
        /// </summary>
        public LinNode Publisher
        {
            get;
            set;
        }

        /// <summary>
        /// 接收节点
        ///
        /// 示例:
        ///
        /// LIN_Master
        ///
        /// </summary>
        public LinNode Subscriber
        {
            get;
            set;
        }

        /// <summary>
        /// Signal编码定义
        ///
        /// 对应:
        ///
        /// Signal_encoding_types
        ///
        /// 
        /// 例如:
        ///
        /// 0 = Stop Mode
        /// 1 = Normal Operation
        ///
        /// </summary>
        public LinSignalEncoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// 物理单位
        ///
        /// 示例:
        ///
        /// rpm
        /// mA
        /// mV
        ///
        /// </summary>
        public string Unit
        {
            get;
            set;
        }

        /// <summary>
        /// 比例系数
        ///
        /// LDF:
        ///
        /// physical_value,
        /// 0,
        /// 65535,
        /// 1,
        /// 0
        ///
        /// 公式:
        ///
        /// Physical =
        /// Raw * Factor + Offset
        ///
        /// </summary>
        public double Factor
        {
            get;
            set;
        }
        = 1;

        /// <summary>
        /// 偏移量
        ///
        /// 公式:
        ///
        /// Physical =
        /// Raw * Factor + Offset
        ///
        /// </summary>
        public double Offset
        {
            get;
            set;
        }

        /// <summary>
        /// 最小物理值
        ///
        /// 例如:
        ///
        /// 温度:
        ///
        /// -50℃
        ///
        /// </summary>
        public double MinValue
        {
            get;
            set;
        }

        /// <summary>
        /// 最大物理值
        ///
        /// </summary>
        public double MaxValue
        {
            get;
            set;
        }


        /// <summary>
        /// 是否允许调试工具直接修改该Signal。
        ///
        /// true:
        ///     用户可以通过调试界面修改
        ///
        /// false:
        ///     Signal由Runtime/协议逻辑维护
        ///
        /// 注意:
        /// 该属性不是LDF标准字段，
        /// 属于调试运行策略。
        /// </summary>
        public bool IsUserEditable
        {
            get;
            set;
        }
        = true;



        #region Value Type

        /// <summary>
        /// Signal值类型。
        ///
        /// 用于告诉UI：
        ///
        /// 当前Signal应该以什么方式编辑。
        ///
        /// 注意：
        ///
        /// 这个值是根据Encoding自动判断的。
        ///
        /// 不建议LDF Parser手工设置。
        /// </summary>
        public LinSignalValueType ValueType
        {
            get
            {
                return GetValueType();
            }
        }


        /// <summary>
        /// 根据Encoding判断Signal值类型。
        ///
        /// 规则：
        ///
        /// PhysicalValues存在
        ///     ↓
        /// Physical
        ///
        /// 否则
        ///
        /// LogicalValues存在
        ///     ↓
        /// Logical
        ///
        /// 否则
        ///
        /// Raw
        ///
        /// 特别注意：
        ///
        /// Physical + Logical
        ///     ↓
        /// Physical
        ///
        /// 因为Physical定义优先。
        /// </summary>
        private LinSignalValueType GetValueType()
        {

            if (Encoding == null)
            {
                return LinSignalValueType.Raw;
            }


            //
            // Physical优先
            //
            // 即使同时存在LogicalValues，
            // 仍然按照Physical处理。
            //
            if (Encoding.PhysicalValues != null &&
                Encoding.PhysicalValues.Count > 0)
            {
                return LinSignalValueType.Physical;
            }


            //
            // 没有Physical，
            // 才判断Logical。
            //
            if (Encoding.LogicalValues != null &&
                Encoding.LogicalValues.Count > 0)
            {
                return LinSignalValueType.Logical;
            }


            //
            // 没有任何Encoding。
            //
            return LinSignalValueType.Raw;

        }

        #endregion
        public override string ToString()
        {
            return Name;
        }

    }



    /// <summary>
    /// Signal编码定义
    ///
    /// 对应LDF:
    ///
    /// Signal_encoding_types
    ///
    /// 支持:
    ///
    /// 1. 逻辑枚举
    /// 2. 物理转换
    ///
    /// </summary>
    public class LinSignalEncoding
    {


        /// <summary>
        /// 编码名称
        ///
        /// 示例:
        ///
        /// READ_STATUS_Encoding
        ///
        /// </summary>
        public string Name
        {
            get;
            set;
        }



        /// <summary>
        /// 枚举值定义
        ///
        /// Key:
        ///     原始值
        ///
        /// Value:
        ///     描述
        ///
        /// 示例:
        ///
        /// 0 Stop Mode
        /// 1 Normal Operation
        ///
        /// </summary>
        public Dictionary<int, string> LogicalValues
        {
            get;
            set;
        }
        =
        new Dictionary<int, string>();


        /// <summary>
        /// 物理值转换范围
        ///
        /// Raw:
        ///     总线值
        ///
        /// Physical:
        ///     实际工程值
        ///
        /// </summary>
        public List<LinPhysicalRange> PhysicalValues
        {
            get;
            set;
        }
        =
        new List<LinPhysicalRange>();

    }


    /// <summary>
    /// 物理转换定义
    ///
    /// LDF:
    ///
    /// physical_value,
    /// 0,
    /// 65534,
    /// 2,
    /// 0,
    /// "mA";
    ///
    /// </summary>
    public class LinPhysicalRange
    {


        /// <summary>
        /// Raw最小值
        /// </summary>
        public int RawMin
        {
            get;
            set;
        }



        /// <summary>
        /// Raw最大值
        /// </summary>
        public int RawMax
        {
            get;
            set;
        }



        /// <summary>
        /// 比例
        ///
        /// Physical =
        /// Raw * Factor + Offset
        ///
        /// </summary>
        public double Factor
        {
            get;
            set;
        }



        /// <summary>
        /// 偏移
        /// </summary>
        public double Offset
        {
            get;
            set;
        }



        /// <summary>
        /// 单位
        /// </summary>
        public string Unit
        {
            get;
            set;
        }

    }


}
