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
    public partial class PasswordDialog : Form
    {
        private TextBox _txtPassword;

        public string Password => _txtPassword.Text;

        public PasswordDialog()
        {
            Text = "请输入密码";
            Width = 300;
            Height = 160;
            StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label
            {
                Text = "密码：",
                Left = 30,
                Top = 30,
                Width=60
            };

            _txtPassword = new TextBox
            {
                Left = 90,
                Top = 25,
                Width = 150,
                UseSystemPasswordChar = true
            };

            var btnOk = new Button
            {
                Text = "确定",
                Left = 90,
                Top = 70,
                Width = 70,
                DialogResult = DialogResult.OK
            };

            Controls.Add(lbl);
            Controls.Add(_txtPassword);
            Controls.Add(btnOk);

            AcceptButton = btnOk;
        }
    }
}
