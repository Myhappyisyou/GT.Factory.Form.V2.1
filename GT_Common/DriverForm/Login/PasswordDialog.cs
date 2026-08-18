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
            InitUI();
        }

        private void InitUI()
        {
            Width = 420;
            Height = 260;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = IndustrialTheme.Background;
            ShowInTaskbar = false;

            // ✅ 外层卡片
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                BackColor = IndustrialTheme.Card
            };

            Controls.Add(card);

            // ✅ 标题
            var lblTitle = new Label
            {
                Text = "权限验证",
                Dock = DockStyle.Top,
                Height = 50,
                Font = IndustrialTheme.TitleFont,
                ForeColor = IndustrialTheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(lblTitle);

            // ✅ 提示文字
            var lblTip = new Label
            {
                Text = "请输入密码",
                Dock = DockStyle.Top,
                Height = 40,
                Font = IndustrialTheme.NormalFont,
                ForeColor = IndustrialTheme.Text,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(lblTip);

            // ✅ 输入框容器
            var inputPanel = new Panel
            {
                Width = 280,
                Height = 45,
                BackColor = Color.FromArgb(45, 55, 72),
                Left = (Width - 280) / 2 - 15,
                Top = 110
            };

            _txtPassword = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                Font = IndustrialTheme.NormalFont,
                ForeColor = Color.White,
                BackColor = inputPanel.BackColor,
                UseSystemPasswordChar = true
            };

            inputPanel.Padding = new Padding(10, 10, 10, 10);
            inputPanel.Controls.Add(_txtPassword);

            card.Controls.Add(inputPanel);

            // ✅ 按钮区域
            var btnCancel = new Button
            {
                Text = "取消",
                Width = 100,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 80, 100),
                ForeColor = Color.White,
                Font = IndustrialTheme.NormalFont,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            var btnOk = new Button
            {
                Text = "确认",
                Width = 100,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = IndustrialTheme.Primary,
                ForeColor = Color.White,
                Font = IndustrialTheme.NormalFont,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            btnCancel.Top = 170;
            btnOk.Top = 170;

            btnCancel.Left = Width / 2 - 120;
            btnOk.Left = Width / 2 + 20;

            card.Controls.Add(btnCancel);
            card.Controls.Add(btnOk);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    DialogResult = DialogResult.Cancel;
            };
        }
    }
}