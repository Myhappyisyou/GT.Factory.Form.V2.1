using System;
using System.Windows.Forms;
using GT.Factory.ProductCode.Domain.Config;
using Newtonsoft.Json;


namespace GT.Factory.Management.UI.ProductCode.Controls.ConfigEditors
{

    /// <summary>
    /// 固定文本配置编辑器。
    ///
    /// 对应：
    ///
    /// CodeSegmentType.FixedText
    ///
    /// Config:
    ///
    /// FixedTextConfig
    ///
    /// 示例：
    ///
    /// 输入：
    ///
    /// GT
    ///
    /// 保存：
    ///
    /// {
    ///     "Text":"GT"
    /// }
    ///
    /// </summary>
    public class FixedTextConfigControl :
        UserControl,
        ICodeConfigEditor
    {

        private TextBox _textBox;

        private Label _label;


        /// <summary>
        /// 配置变化事件。
        /// </summary>
        public event EventHandler<object> ConfigChanged;


        public FixedTextConfigControl()
        {

            InitializeUI();

        }


        /// <summary>
        /// 初始化界面。
        /// </summary>
        private void InitializeUI()
        {

            Dock =
                DockStyle.Fill;

            _label =
                new Label();

            _label.Text =
                "固定字符：";

            _label.Width =
                100;

            _label.Dock =
                DockStyle.Left;

            _textBox =
                new TextBox();

            _textBox.Dock =
                DockStyle.Fill;

            _textBox.TextChanged +=
                TextBox_TextChanged;

            Controls.Add(
                _textBox);

            Controls.Add(
                _label);

        }


        /// <summary>
        /// 加载已有配置。
        ///
        /// 用于编辑已有规则。
        /// </summary>
        public void LoadConfig(
            string configJson)
        {

            if (string.IsNullOrEmpty(configJson))
            {
                return;
            }

            FixedTextConfig config =
                JsonConvert.DeserializeObject<FixedTextConfig>(
                    configJson);

            if (config == null)
            {
                return;
            }

            _textBox.Text =
                config.Text;

        }


        /// <summary>
        /// 文本变化。
        ///
        /// 立即通知父控件。
        /// </summary>
        private void TextBox_TextChanged(
            object sender,
            EventArgs e)
        {

            FixedTextConfig config =
                new FixedTextConfig();

            config.Text =
                _textBox.Text;

            if (ConfigChanged != null)
            {
                ConfigChanged(
                    this,
                    config);
            }

        }

    }

}
