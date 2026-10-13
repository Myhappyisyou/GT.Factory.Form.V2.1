using GT.Factory.PLC.PlcComm;
using GT.Factory.Station.DriverForm.EditJson;
using GT.Factory.Station.ProcessConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Station.EditJson.Components
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
            var txtAddress = Controls.Find("txtAddress", true)[0] as TextBox;
            var addr = txtAddress.Text?.Trim();

            if (!(PlcAddressHelper.ToReadAddress(addr) > 0))
            {
                MessageBox.Show("地址格式无效，请输入如 D100 或 100。");
                txtAddress.Focus();
                return;
            }

            SaveData();
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
            // 地址控件
            var txtAddress = new TextBox
            {
                Name = "txtAddress",
                Text = _config.Address ?? string.Empty,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(txtAddress, 1, 1);

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
            var txtAddress = Controls.Find("txtAddress", true)[0] as TextBox;
            var numLength = Controls.Find("numLength", true)[0] as NumericUpDown;

            chkEnabled.Checked = _config.IsEnabled;
            txtAddress.Text = _config.Address ?? string.Empty;
            numLength.Value = _config.Length;
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var txtAddress = Controls.Find("txtAddress", true)[0] as TextBox;
            var numLength = Controls.Find("numLength", true)[0] as NumericUpDown;

            _config.IsEnabled = chkEnabled.Checked;
            _config.Address = txtAddress.Text?.Trim();
            _config.Length = (int)numLength.Value;
        }


        
    }
}
