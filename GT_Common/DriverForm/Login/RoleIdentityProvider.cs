using GT_Common.Model;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public class RoleIdentityProvider : IIdentityProvider
    {
        public string Name => "角色选择";

        public event Action<User> LoginSuccess;

        private Panel _panel;

        private readonly LoginService _loginService;

        /// <summary>
        /// 超过该等级必须输入密码
        /// </summary>
        private readonly UserLevel _passwordRequiredLevel;

        public RoleIdentityProvider(
            List<UserLevel> roles,
            LoginService loginService,
            UserLevel passwordRequiredLevel = UserLevel.ME)
        {
            _loginService = loginService;
            _passwordRequiredLevel = passwordRequiredLevel;

            _panel = BuildUI(roles);
        }

        private Panel BuildUI(List<UserLevel> roles)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = roles.Count + 1
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            for (int i = 0; i < roles.Count; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / roles.Count));

            var title = new Label
            {
                Text = "请选择登录角色",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微软雅黑", 16, FontStyle.Bold)
            };

            layout.Controls.Add(title, 0, 0);

            int row = 1;

            foreach (var level in roles)
            {
                var btn = CreateRoleButton(level);
                layout.Controls.Add(btn, 0, row++);
            }

            panel.Controls.Add(layout);

            return panel;
        }

        private Button CreateRoleButton(UserLevel level)
        {
            var btn = new Button
            {
                Text = GetDescription(level),
                Dock = DockStyle.Fill,
                Font = new Font("Microsoft YaHei UI", 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Margin = new Padding(100, 5, 100, 5),
                Height = 70,
                Padding = new Padding(0, 8, 0, 8),
                TextAlign = ContentAlignment.MiddleCenter,
                UseCompatibleTextRendering = true
            };

            btn.FlatAppearance.BorderColor = Color.Silver;

            btn.Click += async (s, e) =>
            {
                // ✅ 如果等级低于阈值，直接登录
                if (level < _passwordRequiredLevel)
                {
                    LoginSuccess?.Invoke(new User
                    {
                        UserName = level.ToString(),
                        LevelEnum = level
                    });

                    return;
                }

                // ✅ 高权限需要密码
                using (var pwdDialog = new PasswordDialog())
                {
                    if (pwdDialog.ShowDialog() == DialogResult.OK)
                    {
                        var user = await _loginService.LoginByAccount(
                            level.ToString(),
                            pwdDialog.Password,
                            false);

                        if (user != null)
                        {
                            LoginSuccess?.Invoke(user);
                        }
                        else
                        {
                            MessageBox.Show("密码错误", "提示",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
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
            var attr = field
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return attr?.Description ?? value.ToString();
        }
    }
}
