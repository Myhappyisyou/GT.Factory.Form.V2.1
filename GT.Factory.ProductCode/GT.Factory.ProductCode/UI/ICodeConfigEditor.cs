using System;


namespace GT.Factory.ProductCode.UI
{

    /// <summary>
    /// 产品码配置编辑器接口。
    ///
    /// 每一种Segment配置控件
    /// 都必须实现这个接口。
    ///
    /// 例如：
    ///
    /// DateConfigControl
    ///
    /// SerialConfigControl
    ///
    /// </summary>
    public interface ICodeConfigEditor
    {

        /// <summary>
        /// 配置变化事件。
        /// </summary>
        event EventHandler<object> ConfigChanged;


        /// <summary>
        /// 加载已有配置。
        ///
        /// 数据来源：
        ///
        /// CodeRuleDetail.ConfigJson
        /// </summary>
        void LoadConfig(
            string configJson);

    }

}
