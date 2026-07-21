using GT_Common.MyEnum;
using GT_Common.DriverForm.EditJson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditJson
{
    public partial class CheckResultInfoEditorForm : UserControl, IApplyChanges
    {
        private readonly dynamic _config;

        public CheckResultInfoEditorForm(dynamic config)
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
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(new Label { Text = "类型", AutoSize = true }, 0, 0);


            var cmbProcessType = new ComboBox
            {
                Name = "cmbCheckType",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbProcessType.DataSource = Enum.GetValues(typeof(CheckType));
            cmbProcessType.SelectedItem = _config.CheckType;
            mainPanel.Controls.Add(cmbProcessType, 1, 0);

           

            // 步骤
            mainPanel.Controls.Add(new Label { Text = "步骤/工序:", AutoSize = true }, 0, 1);
            var numStep = new NumericUpDown
            {
                Name = "numStep",
                Minimum = 0,
                Maximum = 99999,
                Value = _config.Step,
                Dock = DockStyle.Fill
            };
            mainPanel.Controls.Add(numStep, 1, 1);

           

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
            var cmbCheckType = Controls.Find("cmbCheckType", true)[0] as ComboBox;
            cmbCheckType.SelectedItem = _config.CheckType;

            var numStep = Controls.Find("numStep", true)[0] as NumericUpDown;
            numStep.Value = _config.Step;

        }

        private void SaveData()
        {
            var cmbCheckType = Controls.Find("cmbCheckType", true)[0] as ComboBox;
            _config.CheckType = (CheckType)cmbCheckType.SelectedIndex;

            var numStep = Controls.Find("numStep", true)[0] as NumericUpDown;
            _config.Step = (int)numStep.Value;

        }
    }
}
