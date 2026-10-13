using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Model
{

    /// <summary>
    /// LIN测试工程
    ///
    /// 对应一个产品型号
    ///
    /// 例如:
    ///
    /// BYD_300W_HEV_ELOP
    ///
    /// 一个Project包含多个测试案例。
    ///
    /// 例如:
    ///
    /// 初始化测试
    /// 功能测试
    /// 出厂测试
    ///
    /// </summary>
    public class LinTestProject
    {

        /// <summary>
        /// 产品型号
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 项目描述
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        
        /// <summary>
        /// 测试案例集合
        /// </summary>
        public List<LinTestCase> Cases
        {
            get;
            set;
        }
        =
        new List<LinTestCase>();

    }
}
