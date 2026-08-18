using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Component
{
    public class UIPasswordInput : UserControl
    {
        private TextBox _txt;
        private Button _btnToggle;
        private Panel _panel;

        private bool _showPassword = false;

        public string TextValue
        {
            get => _txt.Text;
            set => _txt.Text = value;
        }

        public TextBox InnerTextBox => _txt;

        public UIPasswordInput()
        {
            InitUI();
        }

        private void InitUI()
        {
            this.Height = UITheme.ControlHeight;
            this.BackColor = UITheme.Border; // 外边框色
            this.Padding = new Padding(1);

            _panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.Card
            };

            // ✅ 输入框
            _txt = UIInput.Text();
            _txt.AutoSize = false;

            _txt.BorderStyle = BorderStyle.None;
            _txt.Dock = DockStyle.Fill;
            _txt.UseSystemPasswordChar = true;

            // ✅ 按钮（优化版）
            //_btnToggle = new Button
            //{
            //    Dock = DockStyle.Right,
            //    Width = 32,
            //    Text = "👁",
            //    FlatStyle = FlatStyle.Flat,
            //    BackColor = UITheme.Card,
            //    ForeColor = UITheme.Text,
            //    Cursor = Cursors.Hand,
            //    TabStop = false
            //};

            _btnToggle = UIButton.Ghost("👁");
            _btnToggle.Width = UITheme.ControlHeight;
            _btnToggle.Height = UITheme.ControlHeight - 2;

            _btnToggle.Dock = DockStyle.Right;
            _btnToggle.FlatAppearance.BorderSize = 0;
            _btnToggle.FlatAppearance.MouseOverBackColor =UITheme.BgCard;
            _btnToggle.FlatAppearance.MouseDownBackColor = UITheme.CyanGlow;

            _btnToggle.Click += (s, e) => TogglePassword();

            // ✅ 组装
            _panel.Controls.Add(_txt);
            _panel.Controls.Add(_btnToggle);

            this.Controls.Add(_panel);

            // ✅ Hover 边框效果（可选但推荐）
            this.MouseEnter += (s, e) =>
            {
                this.BackColor = UITheme.Primary;
            };

            this.MouseLeave += (s, e) =>
            {
                this.BackColor = UITheme.Text;
            };
        }

        private void TogglePassword()
        {
            _showPassword = !_showPassword;

            _txt.UseSystemPasswordChar = !_showPassword;

            _btnToggle.Text = _showPassword ? "🙈" : "👁";
        }
    }
}