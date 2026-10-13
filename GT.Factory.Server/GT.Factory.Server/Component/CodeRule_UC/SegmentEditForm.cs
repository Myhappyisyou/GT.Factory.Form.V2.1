using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeRule
{
    public partial class SegmentEditForm : Form
    {
        public int SegmentOrder { get; set; }
        public string SegmentType { get; set; }
        public string SegmentValue { get; set; }
        public string SegmentFormat { get; set; }
        public int? SegmentLength { get; set; }
        public char PaddingChar { get; set; } = '0';
        public string PaddingSide { get; set; } = "Left";

        public SegmentEditForm()
        {
            InitializeComponent();

            // 初始化段类型下拉框
            typeComboBox.Items.AddRange(new object[] {
                "FixedText",
                "Date",
                "SerialNumber",
                "Variable"
            });
            typeComboBox.SelectedIndex = 0;

            // 初始化填充方向下拉框
            paddingSideComboBox.Items.AddRange(new object[] {
                "Left",
                "Right"
            });
            paddingSideComboBox.SelectedIndex = 0;
        }

        private void SegmentEditForm_Load(object sender, EventArgs e)
        {
            orderNumericUpDown.Value = SegmentOrder;
            typeComboBox.SelectedItem = SegmentType ?? "FixedText";
            valueTextBox.Text = SegmentValue;
            formatTextBox.Text = SegmentFormat;
            lengthNumericUpDown.Value = SegmentLength ?? 0;
            paddingCharTextBox.Text = PaddingChar.ToString();
            paddingSideComboBox.SelectedItem = PaddingSide ?? "Left";

            UpdateControlsVisibility();
        }

        private void typeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateControlsVisibility();
        }

        private void UpdateControlsVisibility()
        {
            string selectedType = typeComboBox.SelectedItem?.ToString();

            valueLabel.Visible = selectedType == "FixedText" || selectedType == "Variable";
            valueTextBox.Visible = selectedType == "FixedText" || selectedType == "Variable";

            formatLabel.Visible = selectedType == "Date";
            formatTextBox.Visible = selectedType == "Date";

            lengthLabel.Visible = selectedType != "FixedText";
            lengthNumericUpDown.Visible = selectedType != "FixedText";

            paddingCharLabel.Visible = selectedType != "FixedText";
            paddingCharTextBox.Visible = selectedType != "FixedText";

            paddingSideLabel.Visible = selectedType != "FixedText";
            paddingSideComboBox.Visible = selectedType != "FixedText";

            // 设置默认值
            if (selectedType == "Date" && string.IsNullOrEmpty(formatTextBox.Text))
            {
                formatTextBox.Text = "yyyyMMdd";
            }
            else if (selectedType == "SerialNumber" && lengthNumericUpDown.Value == 0)
            {
                lengthNumericUpDown.Value = 5;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(typeComboBox.Text))
            {
                MessageBox.Show("请选择段类型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((typeComboBox.Text == "FixedText" || typeComboBox.Text == "Variable") && string.IsNullOrWhiteSpace(valueTextBox.Text))
            {
                MessageBox.Show("请输入段值", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SegmentOrder = (int)orderNumericUpDown.Value;
            SegmentType = typeComboBox.Text;
            SegmentValue = valueTextBox.Text;
            SegmentFormat = formatTextBox.Text;
            SegmentLength = lengthNumericUpDown.Value > 0 ? (int?)lengthNumericUpDown.Value : null;

            if (!string.IsNullOrWhiteSpace(paddingCharTextBox.Text))
            {
                PaddingChar = paddingCharTextBox.Text[0];
            }

            PaddingSide = paddingSideComboBox.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
