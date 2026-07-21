using GT_Common.Helper;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT_Common.DriverForm.DataDetail
{
    public partial class ProductionDetailForm : Form
    {
        private readonly ProductionMonitor _monitor;

        private TextBox txtWorkOrder;
        private Button btnQuery;

        private Label lblInput;
        private Label lblFirstPass;
        private Label lblFirstFail;
        private Label lblReworkPass;
        private Label lblReworkFail;
        private Label lblCompleted;

        private Label lblFPY;
        private Label lblYield;
        private Label lblCompletion;

        public ProductionDetailForm(
            ProductionMonitor monitor,
            string defaultWorkOrder)
        {
            _monitor = monitor;

            InitializeComponent();

            InitUI();

            txtWorkOrder.Text = defaultWorkOrder;

            Query();
        }

        private void InitUI()
        {
            Text = "生产统计详情";

            Width = 500;
            Height = 420;

            StartPosition = FormStartPosition.CenterParent;

            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            BackColor = Color.White;

            Font = new Font("Segoe UI", 9F);

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55
            };

            var lblWo = new Label
            {
                Text = "工单号",
                AutoSize = true,
                Left = 15,
                Top = 18
            };

            txtWorkOrder = new TextBox
            {
                Left = 70,
                Top = 14,
                Width = 300
            };

            btnQuery = new Button
            {
                Text = "查询",
                Width = 80,
                Height = 28,
                Left = 385,
                Top = 12
            };

            btnQuery.Click += (s, e) => Query();

            txtWorkOrder.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    Query();
                    e.SuppressKeyPress = true;
                }
            };

            topPanel.Controls.Add(lblWo);
            topPanel.Controls.Add(txtWorkOrder);
            topPanel.Controls.Add(btnQuery);

            var groupBox = new GroupBox
            {
                Text = "统计信息",
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 9
            };

            table.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            table.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            lblInput = CreateValueLabel();
            lblFirstPass = CreateValueLabel();
            lblFirstFail = CreateValueLabel();
            lblReworkPass = CreateValueLabel();
            lblReworkFail = CreateValueLabel();
            lblCompleted = CreateValueLabel();

            lblFPY = CreateValueLabel(true);
            lblYield = CreateValueLabel(true);
            lblCompletion = CreateValueLabel(true);

            AddRow(table, 0, "投入数量", lblInput);
            AddRow(table, 1, "一次OK", lblFirstPass);
            AddRow(table, 2, "一次NG", lblFirstFail);
            AddRow(table, 3, "返工OK", lblReworkPass);
            AddRow(table, 4, "返工NG", lblReworkFail);
            AddRow(table, 5, "完成数量", lblCompleted);

            AddRow(table, 6, "FPY", lblFPY);
            AddRow(table, 7, "最终良率", lblYield);
            AddRow(table, 8, "完成率", lblCompletion);

            groupBox.Controls.Add(table);

            Controls.Add(groupBox);
            Controls.Add(topPanel);
        }

        private Label CreateValueLabel(bool highlight = false)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = highlight
                    ? new Font("Segoe UI", 9F, FontStyle.Bold)
                    : new Font("Segoe UI", 9F)
            };
        }

        private void AddRow(
            TableLayoutPanel table,
            int row,
            string title,
            Label valueLabel)
        {
            table.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 32));

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            table.Controls.Add(lblTitle, 0, row);
            table.Controls.Add(valueLabel, 1, row);
        }

        private void Query()
        {
            var wo = txtWorkOrder.Text.Trim();

            if (string.IsNullOrWhiteSpace(wo))
            {
                MessageBox.Show(
                    "请输入工单号",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            var s = _monitor.GetStats(wo);

            lblInput.Text = s.Input.ToString();

            lblFirstPass.Text = s.FirstPass.ToString();

            lblFirstFail.Text = s.FirstFail.ToString();

            lblReworkPass.Text = s.ReworkPass.ToString();

            lblReworkFail.Text = s.ReworkFail.ToString();

            lblCompleted.Text = s.CompletedCount.ToString();

            lblFPY.Text = s.FPYText;

            lblYield.Text = s.YieldText;

            lblCompletion.Text = s.CompletionText;

            SetRateColor(lblFPY);
            SetRateColor(lblYield);
            SetRateColor(lblCompletion);
        }

        private void SetRateColor(Label label)
        {
            var text = label.Text.Replace("%", "");

            if (!double.TryParse(text, out double value))
                return;

            if (value >= 98)
            {
                label.ForeColor = Color.Green;
            }
            else if (value >= 95)
            {
                label.ForeColor = Color.DarkOrange;
            }
            else
            {
                label.ForeColor = Color.Red;
            }
        }
    }
}
