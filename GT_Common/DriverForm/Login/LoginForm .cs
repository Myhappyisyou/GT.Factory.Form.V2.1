using GT.Factory.Common.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public partial class LoginForm : Form
    {
        private Panel _header;
        private Panel _menuContainer; // 增加一个容器来完美居中菜单
        private FlowLayoutPanel _menuPanel;
        private Panel _contentPanel;
        private Timer _timer;

        private readonly List<IIdentityProvider> _providers;
        public User CurrentUser { get; private set; }

        public LoginForm(List<IIdentityProvider> providers)
        {
            _providers = providers;
            this.DoubleBuffered = true;
            InitUI();
            LoadProviders();
            this.FormClosing += LoginForm_FormClosing; // 必须绑定关闭事件释放读卡器

            this.KeyPreview = true;

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            };
        }

        private void InitUI()
        {
            Width = 700;  // 稍微加宽，保证菜单和卡片有足够的呼吸感
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = IndustrialTheme.Background;

            // ✅ 1. 顶部状态栏
            _header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = IndustrialTheme.Card
            };

            var lblTitle = new Label
            {
                Text = "GT 工业设备认证系统",
                Dock = DockStyle.Left,
                AutoSize = true,
                ForeColor = IndustrialTheme.Text,
                Font = IndustrialTheme.LargeFont,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            var lblTime = new Label
            {
                Dock = DockStyle.Right,
                Width = 220,
                ForeColor = IndustrialTheme.Primary,
                Font = IndustrialTheme.NormalFont,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnClose = new Button
            {
                Text = "✕",
                Dock = DockStyle.Right,
                Width = 60,
                FlatStyle = FlatStyle.Flat,
                BackColor = IndustrialTheme.Card,
                ForeColor = IndustrialTheme.Danger,
                Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnClose.FlatAppearance.BorderSize = 0;

            btnClose.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _timer.Start();

            // 绘制底部分割线
            _header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(40, 50, 70), 2), 0, _header.Height - 1, _header.Width, _header.Height - 1);

            _header.Controls.Add(lblTime);
            _header.Controls.Add(lblTitle);
            _header.Controls.Add(btnClose);
            //Controls.Add(_header);

            // ✅ 2. 菜单按钮区域 (使用容器包裹以实现绝对居中)
            _menuContainer = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = IndustrialTheme.Background };

            _menuPanel = new FlowLayoutPanel
            {
                Height = 60,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.None // 配合容器实现居中
            };

            _menuContainer.Controls.Add(_menuPanel);
            // 窗体大小改变时自动居中 _menuPanel
            _menuContainer.Resize += (s, e) => _menuPanel.Location = new Point((_menuContainer.Width - _menuPanel.Width) / 2, (_menuContainer.Height - _menuPanel.Height) / 2);

            //Controls.Add(_menuContainer);

            // ✅ 3. 内容区域
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = IndustrialTheme.Background,
                Padding = new Padding(20)
            };
            // ✅ 先添加 Fill
            Controls.Add(_contentPanel);

            // ✅ 再添加 Top
            Controls.Add(_menuContainer);

            // ✅ 最后添加 Top
            Controls.Add(_header);
        }

        private void LoadProviders()
        {
            foreach (var provider in _providers)
            {
                var btn = CreateMenuButton(provider.Name);

                btn.Click += (s, e) =>
                {
                    SwitchProvider(provider);
                    HighlightButton(btn);
                };

                _menuPanel.Controls.Add(btn);

                provider.LoginSuccess += OnLoginSuccess;

                //// ✅ 只初始化一次
                //provider.Start();

                var control = provider.GetLoginControl();
                control.Dock = DockStyle.Fill;
                control.Visible = false;

                _contentPanel.Controls.Add(control);
            }

            if (_providers.Any())
            {
                SwitchProvider(_providers.First());
            }
        }

        private void SwitchProvider(IIdentityProvider provider)
        {
            foreach (Control c in _contentPanel.Controls)
                c.Visible = false;

            provider.GetLoginControl().Visible = true;

            // ✅ 只有第一次才初始化
            provider.Start();
        }

        private Button CreateMenuButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Width = 180,
                Height = 55,
                FlatStyle = FlatStyle.Flat,
                BackColor = IndustrialTheme.Card,
                ForeColor = IndustrialTheme.Text,
                Font = IndustrialTheme.LargeFont,
                Margin = new Padding(10, 0, 10, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void HighlightButton(Button btn)
        {
            foreach (Control c in _menuPanel.Controls)
            {
                if (c is Button b)
                {
                    b.BackColor = IndustrialTheme.Card;
                    b.ForeColor = IndustrialTheme.Text;
                }
            }
            btn.BackColor = IndustrialTheme.Primary;
            btn.ForeColor = Color.White;
        }

        private void OnLoginSuccess(User user)
        {
            CurrentUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer?.Stop();
            foreach (var p in _providers) p.Stop(); // 安全退出
        }
    }
}
