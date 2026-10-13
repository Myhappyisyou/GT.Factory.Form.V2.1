using System.Windows.Forms;
using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.ProductCode.UI.Controls.ConfigEditors;


namespace GT.Factory.ProductCode.UI.Factories
{

    /// <summary>
    /// 产品码段编辑器工厂。
    ///
    /// 根据SegmentType创建对应UI编辑器。
    ///
    /// 注意：
    ///
    /// 当前只负责UI。
    ///
    /// 不涉及：
    ///
    /// 数据库
    /// 服务
    /// 保存
    /// </summary>
    public static class SegmentConfigFactory
    {

        /// <summary>
        /// 创建配置编辑控件。
        /// </summary>
        public static Control Create(
            CodeSegmentType type)
        {

            switch (type)
            {

                case CodeSegmentType.FixedText:

                    return new FixedTextConfigControl();


                case CodeSegmentType.Date:

                    return new DateConfigControl();


                case CodeSegmentType.SerialNumber:

                    return new SerialConfigControl();


                case CodeSegmentType.Shift:

                    return CreateInfo(
                        "班次由系统自动计算");


                default:

                    return CreateInfo(
                        "未知类型");

            }

        }


        private static Control CreateInfo(
            string text)
        {

            Label label =
                new Label();

            label.Text =
                text;

            label.Dock =
                DockStyle.Fill;

            label.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            return label;

        }

    }

}
