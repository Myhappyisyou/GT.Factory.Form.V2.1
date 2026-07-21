using GT_Common.DriverForm.ProductCode;
using GT_Common.Helper;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class ScanTestForm : Form
{
    private TextBox txtInput;
    private Label lblResult;
    private Label lblLastScan;

    public ScanTestForm()
    {
        this.Text = "扫码规则测试";
        this.Width = 600;
        this.Height = 300;

        #region 最近扫码显示
        lblLastScan = new Label
        {
            Dock = DockStyle.Top,
            Height = 40,
            Font = new Font("Segoe UI", 12),
            BackColor = Color.FromArgb(245, 245, 245),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10),
            Text = "最近扫码："
        };
        #endregion

        #region 输入框
        txtInput = new TextBox
        {
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 16),
            Height = 40
        };
        #endregion

        #region 结果显示（核心）
        lblResult = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Gray,
            ForeColor = Color.White,
            Text = "等待扫码"
        };
        #endregion

        txtInput.KeyDown += TxtInput_KeyDown;

        this.Controls.Add(lblResult);
        this.Controls.Add(txtInput);
        this.Controls.Add(lblLastScan);

        this.Load += (s, e) => txtInput.Focus();
    }

    private void TxtInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;

        string code = txtInput.Text.Trim();

        if (string.IsNullOrEmpty(code))
            return;

        // ✅ 显示最近扫码（关键）
        lblLastScan.Text = $"最近扫码：{code}";

        var rules = ProductCodeConfig.Instance.Rules
            .Where(x => x.Enable);

        var match = rules.FirstOrDefault(r =>
            !string.IsNullOrEmpty(r.CodeMark) &&
            code.StartsWith(r.CodeMark) &&
            code.Length == r.Length
        );

        if (match != null)
        {
            // ✅ OK 绿色
            lblResult.Text = $"OK\n{EnumHelper.GetDescription(match.CodeType)}\n{match.Model}";
            lblResult.BackColor = Color.Green;
        }
        else
        {
            // ❌ NG 红色
            lblResult.Text = "NG\n未匹配规则";
            lblResult.BackColor = Color.Red;
        }

        // ✅ 只清输入框，不丢历史
        txtInput.Clear();
    }
}
