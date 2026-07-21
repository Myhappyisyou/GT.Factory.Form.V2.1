using GT_Common.Model;
using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public partial class RoleLoginView : UserControl, ILoginView
    {
        public event Action<User> LoginSuccess;

        public void Initialize(LoginConfig config)
        {
            this.Dock = DockStyle.Fill;

            int top = 40;

            foreach (var role in config.Roles)
            {
                var btn = new Button
                {
                    Text = GetDescription(role),
                    Left = 100,
                    Top = top,
                    Width = 150,
                    Height = 35
                };

                btn.Click += (s, e) =>
                {
                    var user = new User
                    {
                        UserName = role.ToString(),
                        LevelEnum = role
                    };

                    Shared.isOffline = true;
                    LoginSuccess?.Invoke(user);
                };

                Controls.Add(btn);
                top += 50;
            }
        }

        public static string GetDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;

            return attr?.Description ?? value.ToString();
        }
    }
}
