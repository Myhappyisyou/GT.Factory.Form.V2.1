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
    public partial class DataItemEditorForm : UserControl, IApplyChanges
    {
        private readonly dynamic _config;

        public DataItemEditorForm(dynamic config)
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
            this.SuspendLayout();
            Dock = DockStyle.Fill;


            // 主面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10)
            };

            // 启用复选框
            var chkEnabled = new CheckBox
            {
                Text = "启用配置",
                Name = "chkEnabled",
                Checked = _config.IsEnabled,
                AutoSize = true
            };
            mainPanel.Controls.Add(chkEnabled, 0, 0);
            mainPanel.SetColumnSpan(chkEnabled, 2);

            // 名称 (如果存在)
            if (_config.GetType().GetProperty("Name") != null)
            {
                mainPanel.Controls.Add(new Label { Text = "名称:", AutoSize = true }, 0, 1);
                var txtName = new TextBox
                {
                    Name = "txtName",
                    Text = _config.Name,
                    Dock = DockStyle.Fill
                };
                mainPanel.Controls.Add(txtName, 1, 1);
            }

            // 地址
            mainPanel.Controls.Add(new Label { Text = "地址:", AutoSize = true }, 0, 2);
            var numAddress = new NumericUpDown
            {
                Name = "numAddress",
                Minimum = 0,
                Maximum = 99999,
                Value = _config.Address,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(numAddress, 1, 2);

            // 长度
            mainPanel.Controls.Add(new Label { Text = "长度:", AutoSize = true }, 0, 3);
            var numLength = new NumericUpDown
            {
                Name = "numLength",
                Minimum = 0,
                Maximum = 100,
                Value = _config.Length,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(numLength, 1, 3);

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            var btnSave = new Button
            {
                Text = "保存",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                Width = 80
            };
            btnSave.Click += (s, e) => SaveData();

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Right,
                Left = 90,
                Width = 80
            };

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);

            Controls.Add(mainPanel);
            Controls.Add(buttonPanel);

            //AcceptButton = btnSave;
            //CancelButton = btnCancel;
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            chkEnabled.Checked = _config.IsEnabled;

            if (_config.GetType().GetProperty("Name") != null)
            {
                var txtName = Controls.Find("txtName", true)[0] as TextBox;
                txtName.Text = _config.Name;
            }

            var numAddress = Controls.Find("numAddress", true)[0] as NumericUpDown;
            numAddress.Value = _config.Address;

            var numLength = Controls.Find("numLength", true)[0] as NumericUpDown;
            numLength.Value = _config.Length;
        }

        private void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            _config.IsEnabled = chkEnabled.Checked;

            if (_config.GetType().GetProperty("Name") != null)
            {
                var txtName = Controls.Find("txtName", true)[0] as TextBox;
                _config.Name = txtName.Text;
            }

            var numAddress = Controls.Find("numAddress", true)[0] as NumericUpDown;
            _config.Address = (int)numAddress.Value;

            var numLength = Controls.Find("numLength", true)[0] as NumericUpDown;
            _config.Length = (int)numLength.Value;
        }
    }
}
