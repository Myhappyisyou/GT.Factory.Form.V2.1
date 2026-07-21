using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace GT_Common.DriverForm.Batch
{
    /// <summary>
    /// 批次扫码与数量管理界面（优化版）
    /// </summary>
    public partial class BatchScanForm : Form
    {
        private TextBox txtScan;
        private Label lblStatus,lblLastScan;
        private TableLayoutPanel panel;
        private readonly string _model; // 当前型号
        private List<ClientBatchConfig> configs;

        private static readonly Dictionary<string, string> FieldDisplay = new Dictionary<string, string>()
        {
            ["P"] = "工厂：",
            ["M"] = "物料编码：",
            ["B"] = "BYD批次：",
            ["Lot"] = "供应商批次：",
            ["S"] = "箱号：",
            ["PO"] = "采购订单：",
            ["Q"] = "数量：",
            ["D"] = "生产日期：",
            ["SN"] = "SN：",
            ["YX"] = "有效期：",
            ["DN"] = "DN码："
        };

        public BatchScanForm(string model)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            configs = BatchConfigLoader.Load();
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "批次扫描";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 600;
            this.Height = 700;
            this.BackColor = Color.FromArgb(245, 245, 240); // 柔和背景

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3
            };

            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // 扫码框
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // 状态条
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 160)); // 扫码框

            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 数据面板

            // ===== 扫码框 =====
            txtScan = new TextBox
            {
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 14, FontStyle.Regular),
                BackColor = Color.FromArgb(250, 250, 245),
                ForeColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Height = 40,       // 高一点，方便视觉
                ScrollBars = ScrollBars.Horizontal,
            };
            txtScan.KeyDown += TxtScan_KeyDown;

            lblLastScan = new Label
            {
                Padding = new Padding(10),

                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(245, 245, 240),
                ForeColor = Color.FromArgb(30, 30, 30),
                Font = new Font("Segoe UI", 12),
                AutoSize = true,                       // 高度随文字增长
                MaximumSize = new Size(580, 0),       // 控制宽度，超出自动换行行
            };

            // ===== 状态标签 =====
            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(240, 240, 235),
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            // ===== 数据面板 =====
            panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 2,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            main.Controls.Add(txtScan, 0, 0);
            main.Controls.Add(lblStatus, 0, 1);
            main.Controls.Add(lblLastScan, 0, 2);
            main.Controls.Add(panel, 0, 3);

            this.Controls.Add(main);
        }

        private void TxtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                HandleScan(txtScan.Text);
                lblLastScan.Text = txtScan.Text;

                txtScan.Clear();
            }
        }

        private void HandleScan(string text)
        {

            if (IsFullBatchCode(text))
            {
                HandleFullCode(text);
            }
            else if (IsShortCode(text))
            {
                HandleShortCode(text);
            }
            else
            {
                ShowError("无法识别的扫码格式");
            }

            
        }

        //  长码解析
        private void HandleFullCode(string text)
        {
            var data = BatchCore.Parse(text);

            if (!data.ContainsKey("Q"))
            {
                ShowError("缺少数量字段 Q");
                return;
            }

            var matchConfig = configs
                .Where(c => c.Model == _model)
                .FirstOrDefault(c => BatchCore.IsMatch(data, c));

            if (matchConfig == null)
            {
                ShowError($"型号 {_model} 扫码无匹配批次规则");
                return;
            }

            var q = BatchCore.ParseQuantity(data[matchConfig.QuantityField]);

            var runtime = BatchRuntimeManager.GetPart(matchConfig.PartName);
            if (runtime == null)
            {
                ShowError("✘ 未初始化零件");
                return;
            }

            runtime.TotalQty = q.Total;
            runtime.UsedQty = 0;
            runtime.BYDBatchNub = data[matchConfig.PartField];

            BatchLog.Scan(runtime.PartName, q.Total, q.BoxNUb, Environment.UserName);

            lblStatus.Text = $"✔ 当前批次对应零件: {runtime.PartName}";
            lblStatus.BackColor = Color.FromArgb(198, 239, 206);
            lblStatus.ForeColor = Color.FromArgb(34, 70, 34);
            RenderData(data, matchConfig);

            BatchRuntimeManager.ScanWriteToPlc(runtime);

            Thread.Sleep(500);

            BatchRuntimeManager.RefreshFromPlc();
        }

        //  短码解析
        private void HandleShortCode(string text)
        {

            string partType = text.Substring(0, 2);

            var matchConfig = configs
                .Where(c => c.Model == _model)
                .FirstOrDefault(c => c.MaterialCode== partType);

            if (matchConfig == null)
            {
                ShowError($"型号 {_model} 扫码无匹配批次规则");
                return;
            }

            var runtime = BatchRuntimeManager.GetPart(matchConfig.PartName);
            if (runtime == null)
            {
                ShowError("✘ 未初始化零件");
                return;
            }

            runtime.TotalQty = 0;
            runtime.UsedQty = 0;
            runtime.BYDBatchNub = text.TrimEnd();

            BatchLog.Scan(runtime.PartName, 0, 0, Environment.UserName);

            lblStatus.Text = $"✔ 当前批次对应零件: {runtime.PartName}";
            lblStatus.BackColor = Color.FromArgb(198, 239, 206);
            lblStatus.ForeColor = Color.FromArgb(34, 70, 34);

            BatchRuntimeManager.ScanWriteToPlc(runtime);

            Thread.Sleep(500);

            BatchRuntimeManager.RefreshFromPlc();
        }

        private void RenderData(Dictionary<string, string> data, ClientBatchConfig config)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();
            panel.RowStyles.Clear();
            panel.RowCount = 0;

            var orderedKeys = data.Keys
                .OrderBy(k => k == config.PartField ? 0 :
                              k == config.LotField ? 1 :
                              k == config.QuantityField ? 2 : 3);

            int row = 0;
            foreach (var key in orderedKeys)
            {
                panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var lbl = new Label
                {
                    Text = FieldDisplay.ContainsKey(key) ? FieldDisplay[key] : key,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };

                Control val;

                if (key == config.QuantityField)
                {
                    var q = BatchCore.ParseQuantity(data[key]);
                    var panelQ = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
                    var num = new NumericUpDown
                    {
                        Width = 100,
                        Minimum = 0,
                        Maximum = 1000000,
                        Value = q.Total,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold)
                    };
                    num.ValueChanged += (s, e) =>
                    {
                        short newTotal = (short)num.Value;
                        q.Total = newTotal;
                        data[key] = BuildQuantity(q.Total, q.BoxNUb, q.BoxCount);

                        var runtime = BatchRuntimeManager.GetPart(config.PartName);
                        if (runtime != null)
                            runtime.TotalQty = newTotal;

                        BatchLog.Write(config.PartName, "ModifyTotal", 0, newTotal, Environment.UserName);
                    };
                    panelQ.Controls.Add(new Label { Text = "数量:", AutoSize = true });
                    panelQ.Controls.Add(num);
                    panelQ.Controls.Add(new Label { Text = $" 箱号:{q.BoxNUb} / 总箱数:{q.BoxCount}", AutoSize = true });
                    val = panelQ;
                }
                else
                {
                    val = new Label
                    {
                        Text = string.IsNullOrWhiteSpace(data[key]) ? "-" : data[key],
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.FromArgb(50, 50, 50)
                    };
                }

                // 高亮关键字段
                if (key == config.QuantityField) val.BackColor = Color.FromArgb(255, 250, 250);
                if (key == config.LotField) val.BackColor = Color.FromArgb(250, 255, 250);
                if (key == config.PartField) val.BackColor = Color.FromArgb(250, 250, 255);

                panel.Controls.Add(lbl, 0, row);
                panel.Controls.Add(val, 1, row);
                row++;
            }

            panel.ResumeLayout();
        }

        private void ShowError(string msg)
        {
            lblStatus.Text = "✘ " + msg;
            lblStatus.BackColor = Color.FromArgb(255, 200, 200);
            lblStatus.ForeColor = Color.FromArgb(120, 20, 20);
            System.Media.SystemSounds.Beep.Play();
            panel.Controls.Clear();
        }

        public static string BuildQuantity(int total, int used, int remain) => $"{total}|{used}|{remain}";

        private bool IsFullBatchCode(string text)
        {
            return text.Contains(":") && text.Contains(";");
        }

        private bool IsShortCode(string text)
        {
            return text.Contains("/") && !text.Contains(";") && text.Length < 40;
        }

    }
}