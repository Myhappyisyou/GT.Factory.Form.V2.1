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
    public partial class ResultWriteConfigEditor : UserControl, IApplyChanges
    {
        private readonly ResultWriteConfig _config;

        public ResultWriteConfigEditor(ResultWriteConfig config)
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
            var txtAddress = new TextBox
            {
                Name = "numAddress",
             
                Text = _config.Address,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(txtAddress, 1, 2);

            Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            chkEnabled.Checked = _config.IsEnabled;

            var numAddress = Controls.Find("numAddress", true)[0] as TextBox;
            numAddress.Text = _config.Address;
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            _config.IsEnabled = chkEnabled.Checked;

            var numAddress = Controls.Find("numAddress", true)[0] as TextBox;
            _config.Address = numAddress.Text;
        }
    }
}
