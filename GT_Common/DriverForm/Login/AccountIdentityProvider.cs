using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // ✅ 外层容器（负责居中）
            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1
            };

            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ✅ 内层表单容器
            var table = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = Color.White
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // 账号
            var lblUser = new Label
            {
                Text = "账号：",
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };

            _txtUser = new TextBox
            {
                Width = 200
            };

            // 密码
            var lblPass = new Label
            {
                Text = "密码：",
                Anchor = AnchorStyles.Right,
                AutoSize = true
            };

            _txtPass = new TextBox
            {
                Width = 200,
                UseSystemPasswordChar = true
            };

            var btnLogin = new Button
            {
                Text = "登录",
                Width = 120,
                Height = 35
            };

            btnLogin.Click += BtnLogin_Click;

            _lblStatus = new Label
            {
                ForeColor = Color.Red,
                AutoSize = true
            };

            table.Controls.Add(lblUser, 0, 0);
            table.Controls.Add(_txtUser, 1, 0);

            table.Controls.Add(lblPass, 0, 1);
            table.Controls.Add(_txtPass, 1, 1);

            table.Controls.Add(btnLogin, 1, 2);
            table.Controls.Add(_lblStatus, 1, 3);

            // ✅ 居中关键
            outer.Controls.Add(table, 0, 0);
            table.Anchor = AnchorStyles.None;

            panel.Controls.Add(outer);

            return panel;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            var user = await _loginService.LoginByAccount(
                _txtUser.Text.Trim(),
                _txtPass.Text.Trim(),
                _needMes);

            if (user == null)
            {
                _lblStatus.Text = "登录失败";
                return;
            }

            LoginSuccess?.Invoke(user);
        }

        public Control GetLoginControl() => _panel;

        public void Start() { }

        public void Stop() { }
    }
}
