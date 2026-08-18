using GT.Factory.LIN.Model;
using GT.Factory.LIN.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{
    //判断Signal结果
    public class LinSignalCheck
    {

        /// <summary>
        /// Signal名称
        /// </summary>
        public string Signal
        {
            get;
            set;
        }

        /// <summary>
        /// 比较方式
        ///
        /// Equal
        /// Greater
        /// Less
        ///
        /// </summary>
        public CompareType Compare
        {
            get;
            set;
        }

        /// <summary>
        /// 比较值
        /// </summary>
        public object Value
        {
            get;
            set;
        }

        /// <summary>
        /// 数据类型
        ///
        /// 默认:
        /// Physical
        ///
        /// </summary>
        public LinValueType ValueType
        {
            get;
            set;
        }
        =
        LinValueType.Physical;

        /// <summary>
        /// 容差
        ///
        /// 用于模拟量
        ///
        /// 例如:
        ///
        /// 3000±50
        ///
        /// </summary>
        public double Tolerance
        {
            get;
            set;
        }

    }

}
