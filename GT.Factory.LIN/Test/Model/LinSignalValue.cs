using GT.Factory.LIN.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{
    //作用：

    //设置Signal值

    //例如：

    //WRITE_SPEED = 3000
    public class LinSignalValue
    {

        /// <summary>
        /// Signal名称
        ///
        /// 对应:
        ///
        /// LinSignal.Name
        ///
        /// </summary>
        public string Signal
        {
            get;
            set;
        }

        /// <summary>
        /// 值类型
        ///
        /// 默认Physical
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
        /// 值
        ///
        /// object支持:
        ///
        /// byte
        /// ushort
        /// int
        /// double
        /// string
        ///
        /// </summary>
        public object Value
        {
            get;
            set;
        }


    }
}
