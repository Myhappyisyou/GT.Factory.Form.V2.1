using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    public interface IMeasurement
    {
        string Name { get; }
        object Value { get; }
        string Unit { get; }
        string Status { get; }
    }

    public class Measurement<T>: IMeasurement
    {
        /// <summary>
        /// 参数名称，例如 IRMS1、VPEAK2
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数值
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 单位（例如 KA、V、MS 等）
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 状态（OK / NG），可用于 bool 类型或其他逻辑
        /// </summary>
        public string Status { get; set; }

        // 显式实现接口
        object IMeasurement.Value => Value;

        /// <summary>
        /// 获取显示值
        /// </summary>
        public string DisplayValue
        {
            get
            {
                if (typeof(T) == typeof(bool))
                {
                    bool b = Convert.ToBoolean(Value);
                    return b ? "OK" : "NG";
                }
                else
                {
                    return $"{Value}{Unit}";
                }
            }
        }
    }


}
