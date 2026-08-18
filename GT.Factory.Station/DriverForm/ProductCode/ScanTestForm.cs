using GT.Factory.UI.Component;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using GT.Factory.Station.DriverForm.ProductCode;
using GT.Factory.Station.Helper;
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
        this.StartPosition = FormStartPosition.CenterScreen;

        this.BackColor = UITheme.BgBase;

        InitUI();
    }

    private void InitUI()
    {
        #region 最近扫码显示

        lblLastScan = UILabel.FormLabel("最近扫码：");
        lblLastScan.Dock = DockStyle.Top;
        lblLastScan.TextAlign = ContentAlignment.MiddleLeft;
        lblLastScan.BackColor = UITheme.BgBase;

        #endregion

        #region 输入框

        txtInput = UIInput.Text();
        txtInput.Dock = DockStyle.Top;

        #endregion

        #region 结果显示（核心）

        lblResult = UILabel.FormLabel("等待扫码");
        lblResult.Font = UITheme.FontLarge;
        lblResult.Dock = DockStyle.Fill;
        lblResult.TextAlign = ContentAlignment.MiddleCenter;
        lblResult.BackColor = UITheme.BgBase;

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
            lblResult.ForeColor = UITheme.Success;
        }
        else
        {
            // ❌ NG 红色
            lblResult.Text = "NG\n未匹配规则";
            lblResult.ForeColor = UITheme.Danger;
        }

        // ✅ 只清输入框，不丢历史
        txtInput.Clear();
    }
}
