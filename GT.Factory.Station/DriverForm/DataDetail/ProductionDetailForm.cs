using GT.Factory.Station.Helper;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.Station.DriverForm.DataDetail
{
    /// <summary>
    /// 生产统计详情窗口。
    ///
    /// 统计口径：
    ///
    /// 实际产品数量：
    /// FirstInput
    ///
    /// 返工次数：
    /// ReInput
    ///
    /// 一个 SN 返工不会增加实际产品数量。
    /// </summary>
    public partial class ProductionDetailForm : Form
    {
        #region 字段

        /// <summary>
        /// 生产统计监控器。
        /// </summary>
        private readonly ProductionMonitor _monitor;

        /// <summary>
        /// 自动刷新定时器。
        /// </summary>
        private readonly Timer _refreshTimer;

        /// <summary>
        /// 工单输入框。
        /// </summary>
        private TextBox txtWorkOrder;

        /// <summary>
        /// 查询按钮。
        /// </summary>
        private Button btnQuery;

        /// <summary>
        /// 清零按钮。
        /// </summary>
        private Button btnReset;

        /// <summary>
        /// 实际产品数量。
        /// </summary>
        private Label lblInput;

        /// <summary>
        /// 返工次数。
        /// </summary>
        private Label lblReInput;

        /// <summary>
        /// 一次 OK。
        /// </summary>
        private Label lblFirstPass;

        /// <summary>
        /// 一次 NG。
        /// </summary>
        private Label lblFirstFail;

        /// <summary>
        /// 返工 OK。
        /// </summary>
        private Label lblReworkPass;

        /// <summary>
        /// 返工 NG。
        /// </summary>
        private Label lblReworkFail;

        /// <summary>
        /// 最终 OK 产品数量。
        /// </summary>
        private Label lblFinalPass;

        /// <summary>
        /// 当前设备中产品数量。
        /// </summary>
        private Label lblPending;

        /// <summary>
        /// 当前 NG 待返工产品数量。
        /// </summary>
        private Label lblFailedPending;

        /// <summary>
        /// 一次通过率。
        /// </summary>
        private Label lblFPY;

        /// <summary>
        /// 最终通过率。
        /// </summary>
        private Label lblYield;

        /// <summary>
        /// 完成率。
        /// </summary>
        private Label lblCompletion;

        /// <summary>
        /// 状态栏。
        /// </summary>
        private Label lblStatus;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建生产统计详情窗口。
        /// </summary>
        public ProductionDetailForm(
            ProductionMonitor monitor,
            string defaultWorkOrder)
        {
            _monitor =
                monitor
                ?? throw new ArgumentNullException(
                    nameof(monitor));

            InitializeComponent();

            // 初始化界面。
            InitUI();

            // 设置默认工单。
            txtWorkOrder.Text =
                defaultWorkOrder ?? string.Empty;

            // 创建自动刷新定时器。
            _refreshTimer =
                new Timer
                {
                    Interval = 2000
                };

            _refreshTimer.Tick +=
                (s, e) => RefreshCurrentData();

            _refreshTimer.Start();

            // 首次查询。
            Query(false);
        }

        #endregion

        #region UI

        /// <summary>
        /// 初始化界面。
        /// </summary>
        private void InitUI()
        {
            Text =
                "生产统计详情";

            Width =
                620;

            Height =
                620;

            StartPosition =
                FormStartPosition.CenterParent;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox =
                false;

            MinimizeBox =
                false;

            BackColor =
                Color.White;

            Font =
                new Font(
                    "Segoe UI",
                    9F);

            Controls.Add(
                CreateStatisticsGroup());

            Controls.Add(
                CreateStatusPanel());

            Controls.Add(
                CreateTopPanel());
        }

        /// <summary>
        /// 创建顶部查询区域。
        /// </summary>
        private Panel CreateTopPanel()
        {
            var panel =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        60
                };

            var lblWorkOrder =
                new Label
                {
                    Text =
                        "工单号：",

                    AutoSize =
                        true,

                    Left =
                        15,

                    Top =
                        20
                };

            txtWorkOrder =
                new TextBox
                {
                    Left =
                        75,

                    Top =
                        16,

                    Width =
                        290
                };

            btnQuery =
                new Button
                {
                    Text =
                        "查询",

                    Width =
                        90,

                    Height =
                        30,

                    Left =
                        380,

                    Top =
                        14
                };

            btnQuery.Click +=
                (s, e) => Query(true);

            btnReset =
                new Button
                {
                    Text =
                        "清零",

                    Width =
                        90,

                    Height =
                        30,

                    Left =
                        485,

                    Top =
                        14,

                    BackColor =
                        Color.LightCoral,

                    ForeColor =
                        Color.White,

                    FlatStyle =
                        FlatStyle.Flat
                };

            btnReset.FlatAppearance.BorderSize =
                0;

            btnReset.Click +=
                (s, e) => ResetStats();

            txtWorkOrder.KeyDown +=
                TxtWorkOrder_KeyDown;

            panel.Controls.Add(
                lblWorkOrder);

            panel.Controls.Add(
                txtWorkOrder);

            panel.Controls.Add(
                btnQuery);

            panel.Controls.Add(
                btnReset);

            return panel;
        }

        /// <summary>
        /// 创建统计区域。
        /// </summary>
        private GroupBox CreateStatisticsGroup()
        {
            var groupBox =
                new GroupBox
                {
                    Text =
                        "设备产品统计",

                    Dock =
                        DockStyle.Fill,

                    Padding =
                        new Padding(15)
                };

            var table =
                new TableLayoutPanel
                {
                    Dock =
                        DockStyle.Fill,

                    ColumnCount =
                        2,

                    RowCount =
                        14
                };

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            // 初始化产品数量标签。
            lblInput =
                CreateValueLabel(true);

            lblReInput =
                CreateValueLabel();

            lblFirstPass =
                CreateValueLabel();

            lblFirstFail =
                CreateValueLabel();

            lblReworkPass =
                CreateValueLabel();

            lblReworkFail =
                CreateValueLabel();

            lblFinalPass =
                CreateValueLabel(true);

            // 初始化当前状态标签。
            lblPending =
                CreateValueLabel();

            lblFailedPending =
                CreateValueLabel(true);

            // 初始化比率标签。
            lblFPY =
                CreateValueLabel(true);

            lblYield =
                CreateValueLabel(true);

            lblCompletion =
                CreateValueLabel(true);

            // 第一部分：实际产品数量。
            AddRow(
                table,
                0,
                "实际产品数量",
                lblInput);

            AddRow(
                table,
                1,
                "返工次数",
                lblReInput);

            AddSeparatorRow(
                table,
                2);

            // 第二部分：测试结果。
            AddRow(
                table,
                3,
                "一次 OK",
                lblFirstPass);

            AddRow(
                table,
                4,
                "一次 NG",
                lblFirstFail);

            AddRow(
                table,
                5,
                "返工 OK",
                lblReworkPass);

            AddRow(
                table,
                6,
                "返工 NG",
                lblReworkFail);

            AddRow(
                table,
                7,
                "最终 OK 产品",
                lblFinalPass);

            AddSeparatorRow(
                table,
                8);

            // 第三部分：当前状态。
            AddRow(
                table,
                9,
                "当前设备中",
                lblPending);

            AddRow(
                table,
                10,
                "当前 NG 待返工",
                lblFailedPending);

            AddSeparatorRow(
                table,
                11);

            // 第四部分：生产指标。
            AddRow(
                table,
                12,
                "一次通过率 FPY",
                lblFPY);

            AddRow(
                table,
                13,
                "最终通过率",
                lblYield);

            groupBox.Controls.Add(
                table);

            return groupBox;
        }

        /// <summary>
        /// 创建底部状态栏。
        /// </summary>
        private Panel CreateStatusPanel()
        {
            var panel =
                new Panel
                {
                    Dock =
                        DockStyle.Bottom,

                    Height =
                        35,

                    Padding =
                        new Padding(
                            10,
                            5,
                            10,
                            5),

                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            lblStatus =
                new Label
                {
                    Dock =
                        DockStyle.Fill,

                    Text =
                        "准备就绪",

                    ForeColor =
                        Color.Gray,

                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            panel.Controls.Add(
                lblStatus);

            return panel;
        }

        /// <summary>
        /// 创建数值显示标签。
        /// </summary>
        private Label CreateValueLabel(
            bool highlight = false)
        {
            return new Label
            {
                Dock =
                    DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Font =
                    highlight
                        ? new Font(
                            "Segoe UI",
                            9F,
                            FontStyle.Bold)
                        : new Font(
                            "Segoe UI",
                            9F)
            };
        }

        /// <summary>
        /// 添加统计行。
        /// </summary>
        private void AddRow(
            TableLayoutPanel table,
            int row,
            string title,
            Label valueLabel)
        {
            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    32));

            var titleLabel =
                new Label
                {
                    Text =
                        title,

                    Dock =
                        DockStyle.Fill,

                    TextAlign =
                        ContentAlignment.MiddleLeft
                };

            table.Controls.Add(
                titleLabel,
                0,
                row);

            table.Controls.Add(
                valueLabel,
                1,
                row);
        }

        /// <summary>
        /// 添加分隔行。
        /// </summary>
        private void AddSeparatorRow(
            TableLayoutPanel table,
            int row)
        {
            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    10));

            var separator =
                new Panel
                {
                    Dock =
                        DockStyle.Fill,

                    Height =
                        1,

                    Margin =
                        new Padding(
                            0,
                            4,
                            0,
                            4),

                    BackColor =
                        Color.LightGray
                };

            table.Controls.Add(
                separator,
                0,
                row);

            table.SetColumnSpan(
                separator,
                2);
        }

        #endregion

        #region 查询

        /// <summary>
        /// Enter 键查询。
        /// </summary>
        private void TxtWorkOrder_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            Query(true);

            e.SuppressKeyPress =
                true;
        }

        /// <summary>
        /// 查询当前工单。
        /// </summary>
        private void Query(
            bool showMessage)
        {
            var workOrderNo =
                txtWorkOrder.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                ClearDisplay();

                if (showMessage)
                {
                    MessageBox.Show(
                        "请输入工单号。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return;
            }

            var stats =
                _monitor.GetStats(
                    workOrderNo);

            UpdateDisplay(
                stats,
                workOrderNo);

            lblStatus.Text =
                $"工单 [{workOrderNo}] "
                + $"查询时间："
                + $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// 自动刷新当前工单数据。
        /// </summary>
        private void RefreshCurrentData()
        {
            if (IsDisposed ||
                Disposing)
            {
                return;
            }

            var workOrderNo =
                txtWorkOrder.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                return;
            }

            var stats =
                _monitor.GetStats(
                    workOrderNo);

            UpdateDisplay(
                stats,
                workOrderNo);

            lblStatus.Text =
                $"自动刷新："
                + $"{DateTime.Now:HH:mm:ss}";
        }

        /// <summary>
        /// 更新统计数据显示。
        /// </summary>
        private void UpdateDisplay(
            ProductionStats stats,
            string workOrderNo)
        {
            if (stats == null)
            {
                stats =
                    new ProductionStats();
            }

            // 实际产品数量。
            //
            // 注意：
            // 不包含返工次数。
            lblInput.Text =
                stats.Input.ToString();

            // 返工次数。
            lblReInput.Text =
                stats.ReInput.ToString();

            // 首次测试结果。
            lblFirstPass.Text =
                stats.FirstPass.ToString();

            lblFirstFail.Text =
                stats.FirstFail.ToString();

            // 返工测试结果。
            lblReworkPass.Text =
                stats.ReworkPass.ToString();

            lblReworkFail.Text =
                stats.ReworkFail.ToString();

            // 最终 OK 产品数量。
            lblFinalPass.Text =
                stats.FinalPass.ToString();

            // 当前设备中的产品。
            //
            // 使用工单过滤，
            // 避免显示其他工单的数据。
            lblPending.Text =
                _monitor
                    .GetPendingCount(
                        workOrderNo)
                    .ToString();

            // 当前 NG 待返工产品。
            //
            // 一个 SN 无论返工多少次，
            // 这里只算一个实际产品。
            var failedCount =
                _monitor.GetFailedProductCount(
                    workOrderNo);

            lblFailedPending.Text =
                failedCount.ToString();

            // NG 待返工产品使用颜色提示。
            lblFailedPending.ForeColor =
                failedCount > 0
                    ? Color.Red
                    : Color.Green;

            // 更新生产指标。
            lblFPY.Text =
                stats.FPYText;

            lblYield.Text =
                stats.YieldText;

            lblCompletion.Text =
                stats.CompletionText;

            // 设置指标颜色。
            SetRateColor(
                lblFPY);

            SetRateColor(
                lblYield);

            SetRateColor(
                lblCompletion);
        }

        /// <summary>
        /// 清空统计显示。
        /// </summary>
        private void ClearDisplay()
        {
            lblInput.Text =
                "0";

            lblReInput.Text =
                "0";

            lblFirstPass.Text =
                "0";

            lblFirstFail.Text =
                "0";

            lblReworkPass.Text =
                "0";

            lblReworkFail.Text =
                "0";

            lblFinalPass.Text =
                "0";

            lblPending.Text =
                "0";

            lblFailedPending.Text =
                "0";

            lblFPY.Text =
                "0.0%";

            lblYield.Text =
                "0.0%";

            lblCompletion.Text =
                "0.0%";

            SetRateColor(
                lblFPY);

            SetRateColor(
                lblYield);

            SetRateColor(
                lblCompletion);
        }

        #endregion

        #region 清零

        /// <summary>
        /// 清零当前工单统计。
        /// </summary>
        private void ResetStats()
        {
            var workOrderNo =
                txtWorkOrder.Text.Trim();

            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                MessageBox.Show(
                    "请输入要清零的工单号。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            var result =
                MessageBox.Show(
                    $"确定要清零工单 [{workOrderNo}] 的统计数据吗？\n\n"
                    + "注意：\n"
                    + "当前设备中的产品不会删除。\n"
                    + "NG 待返工产品不会删除。",
                    "确认清零",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            var success =
                _monitor.ResetWorkOrder(
                    workOrderNo);

            if (!success)
            {
                MessageBox.Show(
                    $"工单 [{workOrderNo}] 不存在。",
                    "操作失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            MessageBox.Show(
                $"工单 [{workOrderNo}] "
                + "统计数据已清零。",
                "操作成功",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            Query(false);
        }

        #endregion

        #region 颜色

        /// <summary>
        /// 根据百分比设置颜色。
        /// </summary>
        private void SetRateColor(
            Label label)
        {
            var text =
                label.Text
                    .Replace(
                        "%",
                        string.Empty)
                    .Trim();

            if (!double.TryParse(
                    text,
                    out var value))
            {
                label.ForeColor =
                    Color.Black;

                return;
            }

            if (value >= 98)
            {
                label.ForeColor =
                    Color.Green;
            }
            else if (value >= 95)
            {
                label.ForeColor =
                    Color.DarkOrange;
            }
            else
            {
                label.ForeColor =
                    Color.Red;
            }
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 窗口关闭时停止自动刷新。
        /// </summary>
        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            _refreshTimer.Stop();

            _refreshTimer.Dispose();

            base.OnFormClosed(e);
        }

        #endregion
    }
}
