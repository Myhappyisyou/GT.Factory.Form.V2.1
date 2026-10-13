using GT.Factory.Server.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Server.Component
{
    public partial class AddModelForm : UserControl
    {
        private readonly BindingList<PartItem> partList = new BindingList<PartItem>();
        private readonly List<ProductCodeRule> codeRuleList = new List<ProductCodeRule>();

        private readonly bool isEditMode;
        private readonly ModelData editingModel;

        public AddModelForm(ModelData model = null)
        {
            InitializeComponent();
            isEditMode = model != null;
            editingModel = model;

            InitUI();
            BindEvents();

            if (isEditMode)
                LoadModelData(model);
        }

        #region 初始化 UI

        private TextBox txtModelName;
        private TextBox txtPrefix, txtSuffix, txtAllowedChars, txtRegexPreview, txtSampleCode;
        private NumericUpDown numLength;
        private TextBox txtPartName;
        private CheckBox chkIsPrimary;
        private Button btnAddToTable;
        private DataGridView dgvParts;
        private Button btnAddPart, btnRemovePart, btnValidateSample, btnSave;

        private void InitUI()
        {
            this.Dock = DockStyle.Fill;

            var mainPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, AutoSize = true };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35));   // Data grid
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15));   // Data grid


            txtModelName = new TextBox { Dock = DockStyle.Fill, ForeColor = Color.White };
            SetPlaceholder(txtModelName, "型号名称");

            mainPanel.Controls.Add(Labeled("型号名称", txtModelName));// 0

            var rulePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
            txtPrefix = new TextBox {  Dock = DockStyle.Fill, Text = "SN-" };
            SetPlaceholder(txtPrefix, "前缀");

            numLength = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 8, Dock = DockStyle.Fill };
            txtAllowedChars = new TextBox { Text = "A-Z0-9", Dock = DockStyle.Fill };
            txtSuffix = new TextBox {  Dock = DockStyle.Fill };
            SetPlaceholder(txtSuffix, "后缀");

            txtRegexPreview = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, BackColor = SystemColors.Info };

            rulePanel.Controls.Add(Labeled("前缀", txtPrefix), 0, 0);
            rulePanel.Controls.Add(Labeled("长度", numLength), 1, 0);
            rulePanel.Controls.Add(Labeled("允许字符", txtAllowedChars), 2, 0);
            rulePanel.Controls.Add(Labeled("后缀", txtSuffix), 3, 0);
            rulePanel.Controls.Add(Labeled("正则表达式", txtRegexPreview), 0, 1);
            rulePanel.SetColumnSpan(txtRegexPreview, 4);
            mainPanel.Controls.Add(rulePanel);//    1

            var samplePanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            txtSampleCode = new TextBox { Width = 200 };
            SetPlaceholder(txtSampleCode, "示例：SN-1234ABCD");

            btnValidateSample = new Button { Text = "验证产品码示例", ForeColor = Color.White };
            samplePanel.Controls.Add(txtSampleCode);
            samplePanel.Controls.Add(btnValidateSample);
            mainPanel.Controls.Add(samplePanel);//  2

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoSize = true };
            var topPanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };

            txtPartName = new TextBox { Width = 150, ForeColor = Color.White };
            SetPlaceholder(txtPartName, "部件名称");

            chkIsPrimary = new CheckBox { Text = "是否主码", AutoSize = true, ForeColor = Color.White };
            btnAddToTable = new Button { Text = "添加部件", ForeColor = Color.White };
            topPanel.Controls.Add(txtPartName);
            topPanel.Controls.Add(chkIsPrimary);
            topPanel.Controls.Add(btnAddToTable);
            mainPanel.Controls.Add(topPanel);// 3

            dgvParts = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            dgvParts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "部件名称", Width = 150 });
            dgvParts.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "IsPrimary", HeaderText = "是否主码" });

         
            dgvParts.DataSource = partList;
            mainPanel.Controls.Add(dgvParts);//    4

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Right };
            btnAddPart = new Button { Text = "添加部件", ForeColor = Color.White };
            btnRemovePart = new Button { Text = "删除选中", ForeColor = Color.White };
            btnSave = new Button { Text = isEditMode ? "保存修改" : "添加", ForeColor = Color.White };
            btnPanel.Controls.Add(btnAddPart);
            btnPanel.Controls.Add(btnRemovePart);
            btnPanel.Controls.Add(btnSave);
            mainPanel.Controls.Add(btnPanel);// 5

            this.Controls.Add(mainPanel);

        }

        private Control Labeled(string text, Control ctl)
        {
            var panel = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, Height = 30 };
            panel.Controls.Add(new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = Color.White }, 0, 0);
            panel.Controls.Add(ctl, 1, 0);
            return panel;
        }

        private void BindEvents()
        {
            btnAddToTable.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtPartName.Text) || txtPartName.Text == (string)txtPartName.Tag)
                {
                    MessageBox.Show("请输入有效的部件名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (chkIsPrimary.Checked && partList.Any(p => p.IsPrimary))
                {
                    MessageBox.Show("只能有一个主码部件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                partList.Add(new PartItem
                {
                    Name = txtPartName.Text.Trim(),
                    IsPrimary = chkIsPrimary.Checked
                });
                AddPartItem();


                txtPartName.Text = (string)txtPartName.Tag;
                txtPartName.ForeColor = Color.Gray;
                chkIsPrimary.Checked = false;
            };
            txtPrefix.TextChanged += (sender, e) => UpdateRegexPreview();
            txtSuffix.TextChanged += (sender, e) => UpdateRegexPreview();
            txtAllowedChars.TextChanged += (sender, e) => UpdateRegexPreview();
            numLength.ValueChanged += (sender, e) => UpdateRegexPreview();
            btnValidateSample.Click += (sender, e) => ValidateSampleCode();
            btnAddPart.Click += (sender, e) => partList.Add(new PartItem());
            btnRemovePart.Click += (sender, e) =>
            {
                if (dgvParts.CurrentRow?.DataBoundItem is PartItem item)
                    partList.Remove(item);
            };
            btnSave.Click += (sender, e) => SaveModel();
        }


        #endregion

        private void UpdateRegexPreview()
        {
            var rule = GetCurrentRule();
            txtRegexPreview.Text = BuildRegex(rule);
        }

        private ProductCodeRule GetCurrentRule()
        {
            return new ProductCodeRule
            {
                PartItem = new PartItem()
                {
                    Name = txtModelName.Text.Trim(),
                    IsPrimary = chkIsPrimary.Checked,
                },
                Prefix = txtPrefix.Text,
                Suffix = txtSuffix.Text,
                BodyLength = (int)numLength.Value,
                AllowedChars = txtAllowedChars.Text,
            };
        }

        private void ValidateSampleCode()
        {
            var pattern = txtRegexPreview.Text;
            var sample = txtSampleCode.Text.Trim();

            if (Regex.IsMatch(sample, pattern))
                MessageBox.Show("产品码格式✅符合规则", "验证成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("❌产品码格式不符合规则", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void AddPartItem()
        {
            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                MessageBox.Show("型号名称不能为空");
                return ;
            }
            if (string.IsNullOrWhiteSpace(txtPartName.Text))
            {
                MessageBox.Show("部件名称不能为空");
                return ;
            }

            if (partList.GroupBy(p => p.Name).Any(g => g.Count() > 1))
            {
                MessageBox.Show("部件名称不能重复");
                return ;
            }

            var model = GetCurrentRule();
            codeRuleList.Add(model);
        
        }

        private void SaveModel()
        {
            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                MessageBox.Show("型号名称不能为空");
                return;
            }

            if (partList.GroupBy(p => p.Name).Any(g => g.Count() > 1))
            {
                MessageBox.Show("部件名称不能重复");
                return;
            }
            var partItem = new PartItem()
                {
                 Name=   txtModelName.Text.Trim(),

                };

            var model = new ModelData
            {
                Name = txtModelName.Text.Trim(),
                CodeRule = codeRuleList,
               
            };

            string json = System.Text.Json.JsonSerializer.Serialize(model, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText($"{model.Name}.json", json);

            MessageBox.Show("保存成功！");
        }

        private void LoadModelData(ModelData model)
        {
            txtModelName.Text = model.Name;
            txtPrefix.Text = model.CodeRule[0].Prefix;
            txtSuffix.Text = model.CodeRule[0].Suffix;
            numLength.Value = model.CodeRule[0].BodyLength;
            txtAllowedChars.Text = model.CodeRule[0].AllowedChars;

            foreach (var part in model.CodeRule)
                partList.Add(part.PartItem);
        }


        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.ForeColor = Color.Gray;
            textBox.Text = placeholder;
            textBox.Tag = placeholder;

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == (string)textBox.Tag)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = (string)textBox.Tag;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }

        #region 支持类定义

        public class PartItem
        {
            public string Name { get; set; }
            public bool IsPrimary { get; set; }
        }
       
        public class ProductCodeRule
        {
            public PartItem PartItem { get; set; }

            public string Prefix { get; set; }
            public int BodyLength { get; set; }
            public string AllowedChars { get; set; }
            public string Suffix { get; set; }
        }

        public class ModelData
        {
            public string Name { get; set; }
            public List<ProductCodeRule> CodeRule { get; set; }
        }

        public static string BuildRegex(ProductCodeRule rule)
        {
            string prefix = Regex.Escape(rule.Prefix ?? "");
            string suffix = Regex.Escape(rule.Suffix ?? "");
            string pattern = $"[{rule.AllowedChars}]{{{rule.BodyLength}}}";
            string full = $"{prefix}{pattern}{suffix}";
            return  $"^{full}$";
        }

        #endregion
    }

}
