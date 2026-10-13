using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CodeRule
{
    public partial class CodeRuleForm : UserControl
    {
        private CodeRuleService _codeRuleService;
        private List<CodeRule> _rules = new List<CodeRule>();
        private CodeRule _currentRule;

        public CodeRuleForm()
        {
            InitializeComponent();
            _codeRuleService = new CodeRuleService("Server=.;database=BYD;uid=sa;pwd=root");
            LoadRules();
        }
     
        private void LoadRules()
        {
            try
            {
                _rules = _codeRuleService.GetAllRules();
                ruleListBox.DataSource = null;
                ruleListBox.DataSource = _rules;
                ruleListBox.DisplayMember = "RuleName";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载规则失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ruleListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ruleListBox.SelectedItem is CodeRule selectedRule)
            {
                _currentRule = selectedRule;
                DisplayRuleDetails(selectedRule);
            }
        }

        private void DisplayRuleDetails(CodeRule rule)
        {
            ruleNameTextBox.Text = rule.RuleName;
            descriptionTextBox.Text = rule.RuleDescription;
            isActiveCheckBox.Checked = rule.IsActive;
            codeRegexTextBox.Text = rule.CodeRegex;
            segmentsDataGridView.Rows.Clear();
            foreach (var detail in rule.Details)
            {
                segmentsDataGridView.Rows.Add(
                    detail.SegmentOrder,
                    detail.SegmentType,
                    detail.SegmentValue,
                    detail.SegmentFormat,
                    detail.SegmentLength,
                    detail.PaddingChar,
                    detail.PaddingSide
                );
            }
        }

        private void newRuleButton_Click(object sender, EventArgs e)
        {
            _currentRule = new CodeRule
            {
                RuleName = "新规则_" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                IsActive = true,
                Details = new List<CodeRuleDetail>()
            };

            ruleNameTextBox.Text = _currentRule.RuleName;
            descriptionTextBox.Text = string.Empty;
            isActiveCheckBox.Checked = true;
            segmentsDataGridView.Rows.Clear();
        }

        private void saveRuleButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ruleNameTextBox.Text))
            {
                MessageBox.Show("规则名称不能为空", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _currentRule.RuleName = ruleNameTextBox.Text.Trim();
                _currentRule.RuleDescription = descriptionTextBox.Text.Trim();
                _currentRule.IsActive = isActiveCheckBox.Checked;
                _currentRule.ModifiedBy = Environment.UserName;
                _currentRule.CodeRegex=codeRegexTextBox.Text.Trim();
                // 从DataGridView更新段详情
                _currentRule.Details.Clear();
                foreach (DataGridViewRow row in segmentsDataGridView.Rows)
                {
                    if (row.IsNewRow) continue;

                    var detail = new CodeRuleDetail
                    {
                        SegmentOrder = Convert.ToInt32(row.Cells["colOrder"].Value),
                        SegmentType = row.Cells["colType"].Value.ToString(),
                        SegmentValue = row.Cells["colValue"].Value?.ToString(),
                        SegmentFormat = row.Cells["colFormat"].Value?.ToString(),
                        SegmentLength = row.Cells["colLength"].Value != null ? (int?)Convert.ToInt32(row.Cells["colLength"].Value) : null,
                        PaddingChar = row.Cells["colPaddingChar"].Value != null ? Convert.ToChar(row.Cells["colPaddingChar"].Value) : '0',
                        PaddingSide = row.Cells["colPaddingSide"].Value?.ToString() ?? "Left"
                    };

                    _currentRule.Details.Add(detail);
                }

                // 保存到数据库
                int ruleId = _codeRuleService.SaveCodeRule(_currentRule);
                if (_currentRule.RuleId == 0)
                {
                    _currentRule.RuleId = ruleId;
                    _rules.Add(_currentRule);
                }

                // 刷新列表
                ruleListBox.DataSource = null;
                ruleListBox.DataSource = _rules;
                ruleListBox.DisplayMember = "RuleName";
                ruleListBox.SelectedItem = _currentRule;

                MessageBox.Show("规则保存成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存规则失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void addSegmentButton_Click(object sender, EventArgs e)
        {
            using (var form = new SegmentEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    segmentsDataGridView.Rows.Add(
                        form.SegmentOrder,
                        form.SegmentType,
                        form.SegmentValue,
                        form.SegmentFormat,
                        form.SegmentLength,
                        form.PaddingChar,
                        form.PaddingSide
                    );
                }
            }
            codeRegexTextBox.Text = GenerateRegexPattern(segmentsDataGridView);
        }

        private void editSegmentButton_Click(object sender, EventArgs e)
        {
            if (segmentsDataGridView.SelectedRows.Count == 0) return;

            var row = segmentsDataGridView.SelectedRows[0];
            using (var form = new SegmentEditForm())
            {
                form.SegmentOrder = Convert.ToInt32(row.Cells["colOrder"].Value);
                form.SegmentType = row.Cells["colType"].Value.ToString();
                form.SegmentValue = row.Cells["colValue"].Value?.ToString();
                form.SegmentFormat = row.Cells["colFormat"].Value?.ToString();
                form.SegmentLength = row.Cells["colLength"].Value != null ? (int?)Convert.ToInt32(row.Cells["colLength"].Value) : null;
                form.PaddingChar = row.Cells["colPaddingChar"].Value != null ? Convert.ToChar(row.Cells["colPaddingChar"].Value) : '0';
                form.PaddingSide = row.Cells["colPaddingSide"].Value?.ToString() ?? "Left";

                if (form.ShowDialog() == DialogResult.OK)
                {
                    row.Cells["colOrder"].Value = form.SegmentOrder;
                    row.Cells["colType"].Value = form.SegmentType;
                    row.Cells["colValue"].Value = form.SegmentValue;
                    row.Cells["colFormat"].Value = form.SegmentFormat;
                    row.Cells["colLength"].Value = form.SegmentLength;
                    row.Cells["colPaddingChar"].Value = form.PaddingChar;
                    row.Cells["colPaddingSide"].Value = form.PaddingSide;
                }
            }
            codeRegexTextBox.Text = GenerateRegexPattern(segmentsDataGridView);

        }

        private void deleteSegmentButton_Click(object sender, EventArgs e)
        {
            if (segmentsDataGridView.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("确定要删除选中的段吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in segmentsDataGridView.SelectedRows)
                    {
                        if (!row.IsNewRow)
                        {
                            segmentsDataGridView.Rows.Remove(row);
                        }
                    }
                }
            }
            codeRegexTextBox.Text = GenerateRegexPattern(segmentsDataGridView);
          

        }

        private void generateCodeButton_Click(object sender, EventArgs e)
        {
            if (_currentRule == null || _currentRule.RuleId == 0)
            {
                MessageBox.Show("请先保存规则", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string code = _codeRuleService.GetNextCode(_currentRule.RuleName);
                generatedCodeTextBox.Text = code;
                MessageBox.Show(Regex.IsMatch(code, codeRegexTextBox.Text) ? "匹配成功" : "匹配失败");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成编码失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        string GenerateRegexPattern(DataGridView dataGridView)
        {
            var codeRegex = new List<string>();

            var parts = new List<CodeRuleDetail>();

            foreach (DataGridViewRow row in segmentsDataGridView.Rows)
            {
                if (row.IsNewRow) continue;

                var detail = new CodeRuleDetail
                {
                    SegmentOrder = Convert.ToInt32(row.Cells["colOrder"].Value),
                    SegmentType = row.Cells["colType"].Value.ToString(),
                    SegmentValue = row.Cells["colValue"].Value?.ToString(),
                    SegmentFormat = row.Cells["colFormat"].Value?.ToString(),
                    SegmentLength = row.Cells["colLength"].Value != null ? (int?)Convert.ToInt32(row.Cells["colLength"].Value) : null,
                    PaddingChar = row.Cells["colPaddingChar"].Value != null ? Convert.ToChar(row.Cells["colPaddingChar"].Value) : '0',
                    PaddingSide = row.Cells["colPaddingSide"].Value?.ToString() ?? "Left"
                };

                parts.Add(detail);
            }
            foreach (var rule in parts.OrderBy(r => r.SegmentOrder))
            {
                switch (rule.SegmentType)
                {
                    case "FixedText":
                        codeRegex.Add(Regex.Escape(rule.SegmentValue ?? ""));
                        break;

                    case "Date":
                        codeRegex.Add(@"\d{6}"); // yyMMdd（如需更精准可以构造日期范围）
                        break;

                    case "SerialNumber":
                        codeRegex.Add(@"\d{" + rule.SegmentLength + "}"); // 或更具体：@"\d{5}"
                        break;
                    case "Variable":
                        codeRegex.Add($"[A-Z0-9]{{{rule.SegmentLength}}}"); // fallback
                        break;
                    default:
                        codeRegex.Add(@".+"); // fallback
                        break;
                }
            }

            return "^" + string.Join("", codeRegex) + "$"; // 完全匹配
        }

    }
}