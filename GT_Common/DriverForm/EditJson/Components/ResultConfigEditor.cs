using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.EditJson.Components
{
    public partial class ResultConfigEditor : UserControl, IApplyChanges
    {
        private readonly ResultConfig _config;

        public ResultConfigEditor(ResultConfig config)
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

            // 主面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            // 启用复选框
            var chkEnabled = new CheckBox
            {
                Text = "启用写入结果配置",
                Name = "chkEnabled",
                Checked = _config.IsEnabled,
                AutoSize = true,
                Dock = DockStyle.Top
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
            var length = _config.SuccessFlag;
            if (length < 1)
                length = 2; // 或你定义的默认值
            mainPanel.Controls.Add(new Label { Text = "工艺代码:", AutoSize = true }, 0, 2);
            var numLength = new NumericUpDown
            {
                Name = "numFlag",
                Minimum = 0,
                Maximum = 99999,
                Value = length,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(numLength, 1, 2);

            Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            chkEnabled.Checked = _config.IsEnabled;

            var numAddress = Controls.Find("numAddress", true)[0] as NumericUpDown;
            numAddress.Value = _config.Address;

            var numFlag = Controls.Find("numFlag", true)[0] as NumericUpDown;
            numFlag.Value = _config.SuccessFlag;
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            _config.IsEnabled = chkEnabled.Checked;

            var numAddress = Controls.Find("numAddress", true)[0] as NumericUpDown;
            _config.Address = (int)numAddress.Value;

            var numFlag = Controls.Find("numFlag", true)[0] as NumericUpDown;
            _config.SuccessFlag = (int)numFlag.Value;
        }
    }
}
