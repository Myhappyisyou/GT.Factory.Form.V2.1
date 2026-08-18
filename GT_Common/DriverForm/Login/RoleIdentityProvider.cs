
using GT.Factory.Common.Enums;
using GT.Factory.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public class RoleIdentityProvider : IIdentityProvider
    {
        public string Name => "角色选择";
        public event Action<User> LoginSuccess;

        private Panel _panel;
        private readonly LoginService _loginService;
        private readonly UserLevel _passwordRequiredLevel;

        public RoleIdentityProvider(List<UserLevel> roles, LoginService loginService, UserLevel passwordRequiredLevel = UserLevel.ME)
        {
            _loginService = loginService;
            _passwordRequiredLevel = passwordRequiredLevel;
            _panel = BuildUI(roles);
        }

        private Panel BuildUI(List<UserLevel> roles)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };

            var innerBox = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Anchor = AnchorStyles.None,
                BackColor = IndustrialTheme.Card,
                Padding = new Padding(40)
            };

            var title = new Label
            {
                Text = "选择您的操作权限",
                Font = IndustrialTheme.TitleFont,
                ForeColor = IndustrialTheme.Text,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30),
                Anchor = AnchorStyles.Top
            };

            innerBox.Controls.Add(title, 0, 0);

            var flow = new FlowLayoutPanel
            {
                AutoSize = true,
                MaximumSize = new Size(500, 0),
                FlowDirection = FlowDirection.TopDown,
                Anchor = AnchorStyles.Top
            };

            foreach (var level in roles)
            {
                flow.Controls.Add(CreateRoleButton(level));
            }

            innerBox.Controls.Add(flow, 0, 1);
            outer.Controls.Add(innerBox, 0, 0);
            panel.Controls.Add(outer);

            return panel;
        }

        private Button CreateRoleButton(UserLevel level)
        {
            var btn = new Button
            {
                Text = GetDescription(level),
                Font = IndustrialTheme.LargeFont,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(45, 55, 72), // 次级暗色
                ForeColor = IndustrialTheme.Text,
                Width = 300,
                Height = 60,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = IndustrialTheme.Primary;
            btn.FlatAppearance.MouseOverBackColor = IndustrialTheme.Primary; // 悬浮变成主色调蓝

            btn.Click += async (s, e) =>
            {
                if (level < _passwordRequiredLevel)
                {
                    LoginSuccess?.Invoke(new User { UserName = level.ToString(), LevelEnum = level });
                    return;
                }

                using (var pwdDialog = new PasswordDialog())
                {
                    if (UIHelper.ShowDialogWithMask(pwdDialog, _panel.FindForm()) == DialogResult.OK)
                    {
                        var user = await _loginService.LoginByRoleId(GetDescription(level), pwdDialog.Password, false);
                        if (user != null) LoginSuccess?.Invoke(user);
                        else MessageBox.Show("密码错误", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };

            return btn;
        }

        public Control GetLoginControl() => _panel;
        public void Start() { }
        public void Stop() { }

        private string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
            return attr?.Description ?? value.ToString();
        }
    }
}