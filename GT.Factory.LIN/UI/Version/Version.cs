using System;


namespace GT.Factory.LIN.UI.Version
{

    /// <summary>
    /// LIN UI版本信息
    ///
    /// 用于:
    ///
    /// 1. 标记当前UI状态
    /// 2. 防止代码版本混乱
    ///
    /// 后续升级:
    ///
    /// V0.1.0
    /// V0.2.0
    /// V1.0.0
    ///
    /// </summary>
    public static class LinUIVersion
    {

        /// <summary>
        /// 当前版本
        /// </summary>
        public const string Version =
            "V0.1.2";


        /// <summary>
        /// 编译日期
        /// </summary>
        public static readonly DateTime BuildTime =
            DateTime.Now;


    }

}
