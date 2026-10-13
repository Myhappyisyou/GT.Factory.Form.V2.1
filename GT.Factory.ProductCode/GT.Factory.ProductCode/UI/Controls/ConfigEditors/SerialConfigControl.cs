using GT.Factory.ProductCode.Domain.Config;
using Newtonsoft.Json;
using System;
using System.Windows.Forms;

namespace GT.Factory.ProductCode.UI.Controls.ConfigEditors
{
    /// <summary>
    /// 流水号配置编辑器。
    /// </summary>
    public class SerialConfigControl : UserControl, ICodeConfigEditor
    {
        private NumericUpDown _lengthBox;
        private TextBox _paddingBox;
        private ComboBox _resetCombo;
        private Label _previewLabel;

        public event EventHandler<object> ConfigChanged;

        public SerialConfigControl()
        {
            InitializeUI();
            LoadDefaults();
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;
            table.RowCount = 5;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            table.Controls.Add(new Label { Text = "长度：", Dock = DockStyle.Fill }, 0, 0);
            _lengthBox = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 6, Dock = DockStyle.Left };
            table.Controls.Add(_lengthBox, 1, 0);

            table.Controls.Add(new Label { Text = "补位字符：", Dock = DockStyle.Fill }, 0, 1);
            _paddingBox = new TextBox { Text = "0", MaxLength = 1, Dock = DockStyle.Left, Width = 60 };
            table.Controls.Add(_paddingBox, 1, 1);

            table.Controls.Add(new Label { Text = "重置：", Dock = DockStyle.Fill }, 0, 2);
            _resetCombo = CreateCombo("None", "Daily", "Monthly", "Yearly");
            table.Controls.Add(_resetCombo, 1, 2);

            

            table.Controls.Add(new Label { Text = "预览：", Dock = DockStyle.Fill }, 0, 3);
            _previewLabel = new Label { Text = "000001", Dock = DockStyle.Fill };
            table.Controls.Add(_previewLabel, 1, 3);

            Controls.Add(table);

            _lengthBox.ValueChanged += Changed;
            _paddingBox.TextChanged += Changed;
            _resetCombo.SelectedIndexChanged += Changed;
        }

        private ComboBox CreateCombo(params string[] values)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (string value in values)
            {
                combo.Items.Add(value);
            }
            combo.SelectedIndex = 0;
            return combo;
        }

        private void LoadDefaults()
        {
            _resetCombo.SelectedItem = "None";
            UpdatePreview();
        }

        public void LoadConfig(string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return;
            }

            SerialConfig config = JsonConvert.DeserializeObject<SerialConfig>(configJson);
            if (config == null)
            {
                return;
            }

            if (config.Length > 0 && config.Length <= 20)
            {
                _lengthBox.Value = config.Length;
            }

            _paddingBox.Text = string.IsNullOrEmpty(config.PaddingChar) ? "0" : config.PaddingChar.Substring(0, 1);
            _resetCombo.SelectedItem = string.IsNullOrWhiteSpace(config.ResetMode) ? "None" : config.ResetMode;
            UpdatePreview();
        }

        private void Changed(object sender, EventArgs e)
        {
            UpdatePreview();
            if (ConfigChanged != null)
            {
                ConfigChanged(this, CreateConfig());
            }
        }

        private SerialConfig CreateConfig()
        {
            return new SerialConfig
            {
                Length = (int)_lengthBox.Value,
                PaddingChar = string.IsNullOrEmpty(_paddingBox.Text) ? "0" : _paddingBox.Text.Substring(0, 1),
                ResetMode = Convert.ToString(_resetCombo.SelectedItem),
            };
        }

        private void UpdatePreview()
        {
            SerialConfig config = CreateConfig();
            string value = "1";
            char padding = string.IsNullOrEmpty(config.PaddingChar) ? '0' : config.PaddingChar[0];

            _previewLabel.Text = value.PadLeft(config.Length, padding);
        }
    }
}
