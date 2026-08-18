using GT.Factory.Common.Models;
using GT_Common.DriverForm.Aynettek;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Login
{
    public class CardIdentityProvider : IIdentityProvider
    {
        public string Name => "刷卡登录";
        public event Action<User> LoginSuccess;

        private readonly LoginService _loginService;
        private readonly IRfidService _rfidService;
        private readonly bool _needMes;

        private Panel _panel;
        private Label _lblStatus;
        private PictureBox _pbStatus;
        private bool _initialized = false;

        public CardIdentityProvider(LoginService loginService, IRfidService rfidService, bool needMesValidation)
        {
            _loginService = loginService;
            _rfidService = rfidService;
            _needMes = needMesValidation;
            _panel = BuildUI();
        }

        private Panel BuildUI()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // 整体居中网格
            var outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 1 };

            var innerBox = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Anchor = AnchorStyles.None,
                BackColor = IndustrialTheme.Card,
                Padding = new Padding(60, 40, 60, 40)
            };

            var lblTitle = new Label
            {
                Text = "请放置员工卡",
                Font = IndustrialTheme.TitleFont,
                ForeColor = IndustrialTheme.Primary,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30),
                Anchor = AnchorStyles.Top
            };

            var statusPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Anchor = AnchorStyles.Top };

            _pbStatus = new PictureBox { Width = 20, Height = 20, Margin = new Padding(0, 5, 15, 0), BackColor = Color.Gray };
            _pbStatus.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(_pbStatus.BackColor), 0, 0, _pbStatus.Width - 1, _pbStatus.Height - 1);
            };

            _lblStatus = new Label { Text = "读卡器未初始化", Font = IndustrialTheme.LargeFont, ForeColor = IndustrialTheme.Danger, AutoSize = true };

            statusPanel.Controls.Add(_pbStatus);
            statusPanel.Controls.Add(_lblStatus);

            innerBox.Controls.Add(lblTitle, 0, 0);
            innerBox.Controls.Add(statusPanel, 0, 1);

            outer.Controls.Add(innerBox, 0, 0);
            panel.Controls.Add(outer);
            return panel;
        }


        public void Start()
        {
            if (_initialized)
                return;

            _initialized = true;

            Task.Run(() =>
            {
                InitRfid();
            });
        }

        public void InitRfid()
        {
            bool initOk = false;
            try { initOk = _rfidService.Init(LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort); }
            catch (Exception ex) { UpdateStatus("读卡器异常: " + ex.Message, false); return; }

            if (!initOk) { UpdateStatus("读卡器连接失败", false); return; }

            UpdateStatus("设备就绪，等待刷卡...", true);

            _rfidService.StartListening(async uid =>
            {
                var user = await _loginService.LoginByCard(uid, _needMes);
                if (user != null) LoginSuccess?.Invoke(user);
            },
            tagOnline =>
            {
                if (_panel.InvokeRequired) _panel.Invoke(new Action(() => UpdateStatus(tagOnline ? "设备就绪，等待刷卡..." : "设备已断开", tagOnline)));
                else UpdateStatus(tagOnline ? "设备就绪，等待刷卡..." : "设备已断开", tagOnline);
            }, CancellationToken.None);
        }

        private void UpdateStatus(string text, bool connected)
        {
            if (_panel.InvokeRequired)
            {
                _panel.Invoke(new Action(() => {
                    _lblStatus.Text = text;
                    _lblStatus.ForeColor = connected ? IndustrialTheme.Success : IndustrialTheme.Danger;
                    _pbStatus.BackColor = connected ? IndustrialTheme.Success : IndustrialTheme.Danger;
                    _pbStatus.Invalidate();
                }));
            }
            else
            {
                _lblStatus.Text = text;
                _lblStatus.ForeColor = connected ? IndustrialTheme.Success : IndustrialTheme.Danger;
                _pbStatus.BackColor = connected ? IndustrialTheme.Success : IndustrialTheme.Danger;
                _pbStatus.Invalidate();
            }
        }

        public void Stop() => _rfidService.Stop();
        public Control GetLoginControl() => _panel;
    }
}