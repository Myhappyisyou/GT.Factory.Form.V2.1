using GT.Factory.Common.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public class AccountIdentityProvider : IIdentityProvider
    {
        public string Name => "账号登录";
        public event Action<User> LoginSuccess;

        private readonly LoginService _loginService;
        private readonly bool _needMes;

        private Panel _panel;
        private TextBox _txtUser;
        private TextBox _txtPass;
        private Label _lblStatus;

        public AccountIdentityProvider(LoginService loginService, bool needMesValidation)
        {
            _loginService = loginService;
            _needMes = needMesValidation;
            _panel = BuildUI();
        }

        private Panel BuildUI()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // 外层居中容器
            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };

            // 登录表单卡片
            var table = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(40),
                BackColor = IndustrialTheme.Card, // 使用主题卡片色，而不是白色
                Anchor = AnchorStyles.None
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            var lblUser = new Label { Text = "账号：", Anchor = AnchorStyles.Right, AutoSize = true, ForeColor = IndustrialTheme.Text, Font = IndustrialTheme.NormalFont };
            var lblPass = new Label { Text = "密码：", Anchor = AnchorStyles.Right, AutoSize = true, ForeColor = IndustrialTheme.Text, Font = IndustrialTheme.NormalFont };

            // TextBox 在 WinForms 不能直接设高度，通过 MultiLine=true 或者 Panel 包裹来解决。这里使用 MultiLine
            _txtUser = new TextBox { Width = 260, Height = 40, Multiline = true, Font = IndustrialTheme.NormalFont, BackColor = Color.FromArgb(45, 55, 72), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            _txtPass = new TextBox { Width = 260, Height = 40, Multiline = true, Font = IndustrialTheme.NormalFont, BackColor = Color.FromArgb(45, 55, 72), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, PasswordChar = '*' };

            // 给账号框增加回车跳转功能
            _txtUser.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true; // 阻止账号框换行

                    // 光标跳转到密码框，并全选里面的文字（如果之前有的话）
                    _txtPass.Focus();
                    _txtPass.SelectAll();
                }
            };

            _txtPass.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // ✅ 阻止“叮”的报警声和默认的按键行为
                    e.Handled = true;
                    // ✅ 彻底拦截按键，阻止其输入到文本框中（解决换行问题）
                    e.SuppressKeyPress = true;

                    BtnLogin_Click(s, e);
                }
            };

            var btnLogin = new Button
            {
                Text = "立即登录",
                Width = 260,
                Height = 50,
                BackColor = IndustrialTheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = IndustrialTheme.LargeFont,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            _lblStatus = new Label { ForeColor = IndustrialTheme.Danger, AutoSize = true, Font = new Font("微软雅黑", 10) };

            table.Controls.Add(lblUser, 0, 0); table.Controls.Add(_txtUser, 1, 0);
            table.Controls.Add(lblPass, 0, 1); table.Controls.Add(_txtPass, 1, 1);
            table.Controls.Add(btnLogin, 1, 2);
            table.Controls.Add(_lblStatus, 1, 3);

            outer.Controls.Add(table, 0, 0);
            panel.Controls.Add(outer);
            return panel;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = await _loginService.LoginByAccount(_txtUser.Text.Trim(), _txtPass.Text.Trim(), _needMes);
            if (user == null) { _lblStatus.Text = "登录失败，请检查账号密码"; return; }
            LoginSuccess?.Invoke(user);
        }

        public Control GetLoginControl() => _panel;
        public void Start() { }
        public void Stop() { }
    }
}