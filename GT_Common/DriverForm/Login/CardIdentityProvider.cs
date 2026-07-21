using GT_Common.DriverForm.Aynettek;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
        private Label _lblTitle;
        private Label _lblStatus;
        private PictureBox _pbStatus;

        public CardIdentityProvider(
            LoginService loginService,
            IRfidService rfidService,
            bool needMesValidation)
        {
            _loginService = loginService;
            _rfidService = rfidService;
            _needMes = needMesValidation;

            _panel = BuildUI();
        }

        private Panel BuildUI()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // 使用 FlowLayoutPanel 替代 TableLayoutPanel，更灵活
            var flowLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(10)
            };

            // 标题区域 - 使用固定高度确保显示完整
            _lblTitle = new Label
            {
                Text = "请刷卡登录",
                AutoSize = false,
                Height = 60,
                Width = 300, // 设置合适宽度
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("微软雅黑", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Anchor = AnchorStyles.None
            };

            // 状态指示灯容器
            var statusPanel = new Panel
            {
                Height = 50,
                Width = 300,
                AutoScroll = false
            };

            _pbStatus = new PictureBox
            {
                Height = 16,
                Width = 16,
                BackColor = Color.Gray,
                Location = new Point(60, 17),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            _lblStatus = new Label
            {
                Text = "读卡器未初始化",
                AutoSize = false,
                Height = 50,
                Width = 220,
                Location = new Point(85, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.Red
            };

            statusPanel.Controls.Add(_pbStatus);
            statusPanel.Controls.Add(_lblStatus);

            // 添加控件到流式布局
            flowLayout.Controls.Add(_lblTitle);
            flowLayout.Controls.Add(statusPanel);

            // 居中显示
            flowLayout.Controls[0].Anchor = AnchorStyles.None;
            flowLayout.Controls[1].Anchor = AnchorStyles.None;

            panel.Controls.Add(flowLayout);

            return panel;
        }

        public void Start()
        {
            bool initOk = false;

            try
            {
                initOk = _rfidService.Init(
                    LocalConfig.Instance.UIDIP,
                    LocalConfig.Instance.UIDPort);
            }
            catch (Exception ex)
            {
                UpdateStatus("读卡器异常: " + ex.Message, false);
                return;
            }

            if (!initOk)
            {
                UpdateStatus("读卡器连接失败", false);
                return;
            }

            UpdateStatus("读卡器已连接，请刷卡", true);

            _rfidService.StartListening(async uid =>
            {
                var user = await _loginService.LoginByCard(uid, _needMes);

                if (user != null)
                    LoginSuccess?.Invoke(user);

            },
            tagOnline =>
            {
                // 更新在线状态指示灯
                _panel.Invoke(new Action(() =>
                {
                    _pbStatus.BackColor = tagOnline ? Color.Green : Color.Red;
                }));
            },
            CancellationToken.None);
        }

        private void UpdateStatus(string text, bool connected)
        {
            if (_panel.InvokeRequired)
            {
                _panel.Invoke(new Action(() =>
                {
                    _lblStatus.Text = text;
                    _lblStatus.ForeColor = connected ? Color.Green : Color.Red;
                    _pbStatus.BackColor = connected ? Color.Green : Color.Red;
                }));
            }
            else
            {
                _lblStatus.Text = text;
                _lblStatus.ForeColor = connected ? Color.Green : Color.Red;
                _pbStatus.BackColor = connected ? Color.Green : Color.Red;
            }
        }

        public void Stop()
        {
            _rfidService.Stop();
        }

        public Control GetLoginControl() => _panel;
    }
}