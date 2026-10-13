using System;
using System.Drawing;
using System.Windows.Forms;
using GT.Factory.Station.ProcessConfig;

namespace GT.Factory.Station.EditJson.Components
{
    /// <summary>
    /// PLC业务测点编辑器。
    ///
    /// 一个 PlcMeasureGroup：
    ///
    /// FieldName
    /// Name
    /// Unit
    ///
    /// Upper
    /// Lower
    /// Value
    /// Result
    /// </summary>
    public class PlcMeasureGroupEditor : Form
    {
        private readonly PlcMeasureGroup _group;
        private readonly bool _isNew;

        private TextBox _txtFieldName;
        private TextBox _txtName;
        private TextBox _txtUnit;

        private PlcChannelEditor _upperEditor;
        private PlcChannelEditor _lowerEditor;
        private PlcChannelEditor _valueEditor;
        private PlcChannelEditor _resultEditor;

        private Button _btnOk;
        private Button _btnCancel;

        public PlcMeasureGroupEditor(
            PlcMeasureGroup group,
            bool isNew)
        {
            _group =
                group ?? throw
                new ArgumentNullException(nameof(group));

            _isNew = isNew;

            InitializeUI();

            LoadData();
        }

        private void InitializeUI()
        {
            Text = _isNew
                ? "新增PLC测点"
                : "编辑PLC测点";

            StartPosition =
                FormStartPosition.CenterParent;

            MinimumSize =
                new Size(720, 650);

            Size =
                new Size(780, 720);

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = false;

            MinimizeBox = false;

            BuildUI();
        }

        private void BuildUI()
        {
            var mainPanel =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 3,
                    Padding = new Padding(10)
                };

            mainPanel.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    105));

            mainPanel.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100));

            mainPanel.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    45));

            // =========================================================
            // 基础信息
            // =========================================================

            var basicGroup =
                BuildBasicInfoPanel();

            mainPanel.Controls.Add(
                basicGroup,
                0,
                0);

            // =========================================================
            // 通道配置
            // =========================================================

            var channelPanel =
                BuildChannelPanel();

            mainPanel.Controls.Add(
                channelPanel,
                0,
                1);

            // =========================================================
            // Button
            // =========================================================

            var buttonPanel =
                new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection =
                        FlowDirection.RightToLeft,
                    WrapContents = false
                };

            _btnCancel =
                new Button
                {
                    Text = "取消",
                    Width = 90,
                    Height = 30,
                    DialogResult =
                        DialogResult.Cancel
                };

            _btnOk =
                new Button
                {
                    Text = "确定",
                    Width = 90,
                    Height = 30
                };

            _btnOk.Click +=
                BtnOk_Click;

            buttonPanel.Controls.Add(
                _btnCancel);

            buttonPanel.Controls.Add(
                _btnOk);

            mainPanel.Controls.Add(
                buttonPanel,
                0,
                2);

            Controls.Add(mainPanel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private GroupBox BuildBasicInfoPanel()
        {
            var group =
                new GroupBox
                {
                    Text = "测点基本信息",
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8)
                };

            var panel =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = 2
                };

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    80));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    80));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            panel.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50));

            panel.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    50));

            // FieldName

            panel.Controls.Add(
                CreateLabel("业务主键"),
                0,
                0);

            _txtFieldName =
                new TextBox
                {
                    Dock = DockStyle.Fill
                };

            panel.Controls.Add(
                _txtFieldName,
                1,
                0);

            // Name

            panel.Controls.Add(
                CreateLabel("显示名称"),
                2,
                0);

            _txtName =
                new TextBox
                {
                    Dock = DockStyle.Fill
                };

            panel.Controls.Add(
                _txtName,
                3,
                0);

            // Unit

            panel.Controls.Add(
                CreateLabel("单位"),
                0,
                1);

            _txtUnit =
                new TextBox
                {
                    Dock = DockStyle.Fill
                };

            panel.Controls.Add(
                _txtUnit,
                1,
                1);

            group.Controls.Add(panel);

            return group;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private Panel BuildChannelPanel()
        {
            var panel =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true
                };

            var layout =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(0)
                };

            // Upper

            _upperEditor =
                new PlcChannelEditor(
                    "上限",
                    true);

            layout.Controls.Add(
                _upperEditor,
                0,
                0);

            // Lower

            _lowerEditor =
                new PlcChannelEditor(
                    "下限",
                    true);

            layout.Controls.Add(
                _lowerEditor,
                0,
                1);

            // Value

            _valueEditor =
                new PlcChannelEditor(
                    "实际值",
                    true);

            layout.Controls.Add(
                _valueEditor,
                0,
                2);

            // Result

            _resultEditor =
                new PlcChannelEditor(
                    "结果",
                    true);

            layout.Controls.Add(
                _resultEditor,
                0,
                3);

            panel.Controls.Add(layout);

            return panel;
        }

        private void LoadData()
        {
            _txtFieldName.Text =
                _group.FieldName ?? "";

            _txtName.Text =
                _group.Name ?? "";

            _txtUnit.Text =
                _group.Unit ?? "";

            _upperEditor.LoadChannel(
                _group.Upper);

            _lowerEditor.LoadChannel(
                _group.Lower);

            _valueEditor.LoadChannel(
                _group.Value);

            _resultEditor.LoadChannel(
                _group.Result);
        }

        private void BtnOk_Click(
            object sender,
            EventArgs e)
        {
            if (!ValidateInput())
                return;

            SaveData();

            DialogResult =
                DialogResult.OK;

            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(
                _txtFieldName.Text))
            {
                MessageBox.Show(
                    "业务主键不能为空。",
                    "配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtFieldName.Focus();

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                _txtName.Text))
            {
                MessageBox.Show(
                    "显示名称不能为空。",
                    "配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                _txtName.Focus();

                return false;
            }

            // 实际值是必须配置的

            if (!_valueEditor.IsConfigured())
            {
                MessageBox.Show(
                    "实际值通道必须配置。",
                    "配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!_upperEditor.ValidateChannel(
                out string upperError))
            {
                MessageBox.Show(
                    upperError,
                    "上限配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!_lowerEditor.ValidateChannel(
                out string lowerError))
            {
                MessageBox.Show(
                    lowerError,
                    "下限配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!_valueEditor.ValidateChannel(
                out string valueError))
            {
                MessageBox.Show(
                    valueError,
                    "实际值配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!_resultEditor.ValidateChannel(
                out string resultError))
            {
                MessageBox.Show(
                    resultError,
                    "结果配置错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            return true;
        }

        private void SaveData()
        {
            _group.FieldName =
                _txtFieldName.Text.Trim();

            _group.Name =
                _txtName.Text.Trim();

            _group.Unit =
                _txtUnit.Text.Trim();

            _group.Upper =
                _upperEditor.GetChannel();

            _group.Lower =
                _lowerEditor.GetChannel();

            _group.Value =
                _valueEditor.GetChannel();

            _group.Result =
                _resultEditor.GetChannel();
        }
    }
}