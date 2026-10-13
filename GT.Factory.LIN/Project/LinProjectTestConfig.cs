using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Project
{
    /// <summary>
    /// LIN工程测试配置。
    ///
    /// 用于描述:
    ///
    /// *.linproj
    ///     ↓
    /// Test
    ///     ↓
    /// 测试工程文件
    ///
    /// 本对象只负责测试工程文件路径配置。
    ///
    /// 不负责:
    /// 测试流程解析
    /// 测试执行
    /// LIN通信
    /// UI
    /// </summary>
    public class LinProjectTestConfig
    {
        /// <summary>
        /// 测试工程文件路径。
        ///
        /// 路径相对于:
        ///
        /// 当前 .linproj 所在目录。
        ///
        /// 例如:
        ///
        /// Test\Motor_Test.json
        /// </summary>
        public string ProjectTestCasePath
        {
            get;
            set;
        }
    }
}