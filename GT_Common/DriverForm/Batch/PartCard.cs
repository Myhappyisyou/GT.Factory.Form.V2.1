using GT_Common.DriverForm.Batch;
using System;
using System.Drawing;
using System.Windows.Forms;

public class PartCard : Panel
{
    private PartRuntime _part;
    private TextBox txtTotal;
    private Label lblRemain;
    private Timer _debounceTimer;

    public PartCard(PartRuntime part)
    {
        _part = part;

        this.Width = 260;
        this.Height = 140;
        this.Margin = new Padding(10);
        this.Padding = new Padding(10);

        // 柔和卡片色
        this.BackColor = Color.FromArgb(245, 245, 240); // 米灰
        this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, this.Width, this.Height, 12, 12));

        this.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(235, 235, 230);
        this.MouseLeave += (s, e) => this.BackColor = Color.FromArgb(245, 245, 240);

        InitUI();
    }

    private void InitUI()
    {
        var lblTitle = new Label
        {
            Text = $"{_part.PartName}   M:{_part.MaterialCode}",
            ForeColor = Color.FromArgb(50, 50, 50), // 深灰色字体
            Dock = DockStyle.Top,
            Height = 25,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        var lblBatch = new Label
        {
            Text = $"B:{_part.BYDBatchNub}",
            ForeColor = Color.FromArgb(50, 50, 50), // 深灰色字体
            Dock = DockStyle.Top,
            Height = 25,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(1),

        };

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.Transparent
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

        txtTotal = new TextBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(255, 255, 250),
            ForeColor = Color.FromArgb(50, 50, 50),
            BorderStyle = BorderStyle.FixedSingle
        };

        lblRemain = new Label
        {
            Text = _part.RemainQty.ToString(),
            ForeColor = Color.FromArgb(34, 139, 34), // 柔和绿色
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        panel.Controls.Add(new Label { Text = "总数", ForeColor = Color.FromArgb(50, 50, 50) }, 0, 0);
        panel.Controls.Add(txtTotal, 1, 0);

        panel.Controls.Add(new Label { Text = "已用", ForeColor = Color.FromArgb(50, 50, 50) }, 0, 1);
        panel.Controls.Add(new Label { Text = _part.UsedQty.ToString(), ForeColor = Color.FromArgb(50, 50, 50) }, 1, 1);

        panel.Controls.Add(new Label { Text = "剩余", ForeColor = Color.FromArgb(50, 50, 50) }, 0, 2);
        panel.Controls.Add(lblRemain, 1, 2);

        this.Controls.Add(panel);
        this.Controls.Add(lblBatch);
        this.Controls.Add(lblTitle);

        txtTotal.TextChanged += TxtTotal_TextChanged;

        _debounceTimer = new Timer { Interval = 500 };
        _debounceTimer.Tick += (s, e) =>
        {
            _debounceTimer.Stop();
            BatchRuntimeManager.WriteToPlc(_part);
        };

        // 👇 放在后面
        txtTotal.Text = _part.TotalQty.ToString();
    }

    private void TxtTotal_TextChanged(object sender, EventArgs e)
    {
        if (!short.TryParse(txtTotal.Text, out short val)) return;
        if (val < _part.UsedQty) txtTotal.Text = _part.UsedQty.ToString();

        _part.TotalQty = val;

        UpdateRemain();

        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    private void UpdateRemain()
    {
        int remain = _part.RemainQty;
        lblRemain.Text = remain.ToString();

        if (remain <= 0)
            lblRemain.ForeColor = Color.FromArgb(205, 92, 92); // 柔和红色
        else if (remain < 10)
            lblRemain.ForeColor = Color.FromArgb(255, 165, 0); // 柔和橙色
        else
            lblRemain.ForeColor = Color.FromArgb(34, 139, 34); // 柔和绿色
    }

    #region 圆角支持
    [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    private static extern IntPtr CreateRoundRectRgn
    (
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect,
        int nWidthEllipse,
        int nHeightEllipse
    );
    #endregion
}