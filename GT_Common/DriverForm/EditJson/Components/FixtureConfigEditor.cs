using GT_Common.ProcessConfig;
using GT_Common.DriverForm.EditJson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditJson
{
    public partial class FixtureConfigEditor : UserControl, IApplyChanges
    {
        private readonly FixtureConfig _config;

        public FixtureConfigEditor(FixtureConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            InitializeUI();
            LoadData();
        }

        public void ApplyChanges()
        {
            SaveData(); // 调用内部逻辑，把界面写回到 _config
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            // 启用复选框
            var chkEnabled = new CheckBox
            {
                Text = "启用治具配置",
                Name = "chkEnabled",
                Checked = _config.IsEnabled,
                AutoSize = true
            };
            mainPanel.Controls.Add(chkEnabled, 0, 0);
            mainPanel.SetColumnSpan(chkEnabled, 2);

            // 地址
            mainPanel.Controls.Add(new Label { Text = "地址:", AutoSize = true }, 0, 1);
            var numAddress = new NumericUpDown
            {
                Name = "numAddress",
                Minimum = 0,
                Maximum = 99999,
                Value = _config.Address,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(numAddress, 1, 1);

            // 长度
            mainPanel.Controls.Add(new Label { Text = "长度:", AutoSize = true }, 0, 2);
            var numLength = new NumericUpDown
            {
                Name = "numLength",
                Minimum = 0,
                Maximum = 100,
                Value = _config.Length,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(numLength, 1, 2);

            Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var numAddress = Controls.Find("numAddress", true)[0] as NumericUpDown;
            var numLength = Controls.Find("numLength", true)[0] as NumericUpDown;

            chkEnabled.Checked = _config.IsEnabled;
            numAddress.Value = _config.Address;
            numLength.Value = _config.Length;
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var numAddress = Controls.Find("numAddress", true)[0] as NumericUpDown;
            var numLength = Controls.Find("numLength", true)[0] as NumericUpDown;

            _config.IsEnabled = chkEnabled.Checked;
            _config.Address = (int)numAddress.Value;
            _config.Length = (int)numLength.Value;
        }
    }
}
