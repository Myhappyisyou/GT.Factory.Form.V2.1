using GT.Factory.ProductCode.Domain.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DateConfig = GT.Factory.ProductCode.Domain.Config.DateConfig;

namespace GT.Factory.ProductCode.UI.Controls.ConfigEditors
{
    /// <summary>
    /// 日期 Segment 配置编辑器。
    ///
    /// 保留原有日期编辑控件位置，但配置模型改为 Parts。
    /// 支持：
    /// Year / Month / Day / DayOfYear
    /// 以及 Full / LastTwo / Number / TwoDigit / ThreeDigit / Mapping。
    /// </summary>
    public class DateConfigControl : UserControl, ICodeConfigEditor
    {
        private ComboBox _yearMode;
        private ComboBox _monthMode;
        private ComboBox _dayMode;
        private TextBox _yearMapping;
        private TextBox _monthMapping;
        private CheckBox _dayOfYear;

        public event EventHandler<object> ConfigChanged;

        public DateConfigControl()
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
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            table.Controls.Add(new Label { Text = "年份：", Dock = DockStyle.Fill }, 0, 0);
            _yearMode = CreateModeCombo("Full", "LastTwo", "Number", "Mapping");
            table.Controls.Add(_yearMode, 1, 0);

            _yearMapping = new TextBox();
            _yearMapping.Dock = DockStyle.Fill;
            table.Controls.Add(_yearMapping, 1, 1);
            table.Controls.Add(new Label { Text = "年份映射：", Dock = DockStyle.Fill }, 0, 1);

            table.Controls.Add(new Label { Text = "月份：", Dock = DockStyle.Fill }, 0, 2);
            _monthMode = CreateModeCombo("Number", "TwoDigit", "Mapping");
            table.Controls.Add(_monthMode, 1, 2);

            _monthMapping = new TextBox();
            _monthMapping.Dock = DockStyle.Fill;
            table.Controls.Add(new Label { Text = "月份映射：", Dock = DockStyle.Fill }, 0, 3);
            table.Controls.Add(_monthMapping, 1, 3);

            FlowLayoutPanel bottom = new FlowLayoutPanel();
            bottom.Dock = DockStyle.Fill;
            _dayMode = CreateModeCombo("Number", "TwoDigit", "Mapping");
            bottom.Controls.Add(new Label { Text = "日：" });
            bottom.Controls.Add(_dayMode);
            _dayOfYear = new CheckBox { Text = "使用一年中的第几天" };
            bottom.Controls.Add(_dayOfYear);
            table.Controls.Add(bottom, 1, 4);
            table.Controls.Add(new Label { Text = "日期：", Dock = DockStyle.Fill }, 0, 4);

            Controls.Add(table);

            _yearMode.SelectedIndexChanged += Changed;
            _monthMode.SelectedIndexChanged += Changed;
            _dayMode.SelectedIndexChanged += Changed;
            _yearMapping.TextChanged += Changed;
            _monthMapping.TextChanged += Changed;
            _dayOfYear.CheckedChanged += Changed;
        }

        private ComboBox CreateModeCombo(params string[] values)
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
            _yearMode.SelectedItem = "Full";
            _monthMode.SelectedItem = "TwoDigit";
            _dayMode.SelectedItem = "TwoDigit";
            _yearMapping.Text = "2026=A,2027=B";
            _monthMapping.Text = "1=1,2=2,3=3,4=4,5=5,6=6,7=7,8=8,9=9,10=A,11=B,12=C";
            _dayOfYear.Checked = false;
        }

        public void LoadConfig(string configJson)
        {
            if (string.IsNullOrWhiteSpace(configJson))
            {
                return;
            }

            DateConfig config = JsonConvert.DeserializeObject<DateConfig>(configJson);
            if (config == null || config.Parts == null)
            {
                return;
            }

            foreach (DatePartConfig part in config.Parts)
            {
                if (part == null)
                {
                    continue;
                }

                if (string.Equals(part.Part, "Year", StringComparison.OrdinalIgnoreCase))
                {
                    _yearMode.SelectedItem = part.Mode;
                    _yearMapping.Text = FormatMapping(part.Mapping);
                }
                else if (string.Equals(part.Part, "Month", StringComparison.OrdinalIgnoreCase))
                {
                    _monthMode.SelectedItem = part.Mode;
                    _monthMapping.Text = FormatMapping(part.Mapping);
                }
                else if (string.Equals(part.Part, "DayOfYear", StringComparison.OrdinalIgnoreCase))
                {
                    _dayOfYear.Checked = true;
                }
                else if (string.Equals(part.Part, "Day", StringComparison.OrdinalIgnoreCase))
                {
                    _dayMode.SelectedItem = part.Mode;
                }
            }
        }

        private string FormatMapping(MappingDefinition mapping)
        {
            if (mapping == null || mapping.Values == null)
            {
                return string.Empty;
            }

            List<string> values = new List<string>();
            foreach (KeyValuePair<string, string> item in mapping.Values)
            {
                values.Add(item.Key + "=" + item.Value);
            }
            return string.Join(",", values.ToArray());
        }

        private void Changed(object sender, EventArgs e)
        {
            if (ConfigChanged != null)
            {
                ConfigChanged(this, CreateConfig());
            }
        }

        private DateConfig CreateConfig()
        {
            DateConfig config = new DateConfig();

            config.Parts.Add(CreatePart("Year", Convert.ToString(_yearMode.SelectedItem), _yearMapping.Text));
            config.Parts.Add(CreatePart("Month", Convert.ToString(_monthMode.SelectedItem), _monthMapping.Text));

            if (_dayOfYear.Checked)
            {
                config.Parts.Add(new DatePartConfig { Part = "DayOfYear", Mode = "ThreeDigit" });
            }
            else
            {
                config.Parts.Add(CreatePart("Day", Convert.ToString(_dayMode.SelectedItem), string.Empty));
            }

            return config;
        }

        private DatePartConfig CreatePart(string part, string mode, string mappingText)
        {
            DatePartConfig config = new DatePartConfig();
            config.Part = part;
            config.Mode = string.IsNullOrWhiteSpace(mode) ? "Number" : mode;

            if (string.Equals(config.Mode, "Mapping", StringComparison.OrdinalIgnoreCase))
            {
                config.Mapping = ParseMapping(mappingText);
            }

            return config;
        }

        private MappingDefinition ParseMapping(string text)
        {
            MappingDefinition mapping = new MappingDefinition();
            if (string.IsNullOrWhiteSpace(text))
            {
                return mapping;
            }

            string[] pairs = text.Split(',');
            foreach (string pair in pairs)
            {
                string[] values = pair.Split(new[] { '=' }, 2);
                if (values.Length != 2)
                {
                    continue;
                }

                string key = values[0].Trim();
                string value = values[1].Trim();
                if (key.Length > 0)
                {
                    mapping.Values[key] = value;
                }
            }

            return mapping;
        }
    }
}
