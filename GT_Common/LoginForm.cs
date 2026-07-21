using GT_Common.Model;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GT_Common.Helper;
using GT_Common.DriverForm.Aynettek;
using GT_Common.Helper.BydMes;
using GT_Common.Helper.BydUser;
using GT_Common.Helper.Logging;
using System.Threading.Tasks;

namespace GT_Common
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtCard;
        private PictureBox pbTagSingle;
        private Button btnLogin;
        private Button btnExit;
        private Label lblStatus;
        private CancellationTokenSource _listenCancellationTokenSource;
        public User currentUser;

        private readonly IRfidService _rfidService;
        private readonly IUserService _userService;
        private readonly IMesService _mesService;
        private readonly ILocalUserService _localUserService;

        public LoginForm(IRfidService rfidService, IUserService userService, IMesService mesService, ILocalUserService localUserService,string addr, int port)
        {
            _rfidService = rfidService;
            _userService = userService;
            _mesService = mesService;
            _localUserService = localUserService;
            InitializeComponent();

            //  初始化UI界面
            InitUI();
            //  初始化rfid
            try
            {
                InitRfid(addr, port);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //  初始化UI界面
        private void InitUI()
        {
            this.Text = "登录界面";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 350;
            this.Height = 280;

            // 创建控件
            var lblUser = new Label { Text = "账号：", Left = 20, Top = 20, Width = 80 };
            var lblPass = new Label { Text = "密码：", Left = 20, Top = 60, Width = 80 };
            var lblCard = new Label { Text = "刷卡登录：", Left = 20, Top = 100, Width = 80 };

            txtUsername = new TextBox { Left = 100, Top = 20, Width = 200 };
            txtPassword = new TextBox { Left = 100, Top = 60, Width = 200, UseSystemPasswordChar = true };

            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    BtnLogin_Click(s, e);
            };

            txtCard = new TextBox { Left = 100, Top = 100, Width = 200, ReadOnly = true, TabStop = false };

            // 创建指示灯 PictureBox
            pbTagSingle = new PictureBox
            {
                Left = txtCard.Right + 10,  // 在文本框右侧10像素位置
                Top = 100,
                Width = 20,                 // 指示灯宽度
                Height = 20,                // 指示灯高度
                TabStop = false,
                BackColor = Color.Gray,     // 默认灰色
                BorderStyle = BorderStyle.FixedSingle
            };

            btnLogin = new Button { Text = "登录", Left = 100, Top = 160, Width = 80 };
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button { Text = "退出", Left = 220, Top = 160, Width = 80 };
            btnExit.Click += (s, e) => this.Close();

            lblStatus = new Label { Left = 20, Top = 220, Width = 300, ForeColor = Color.Red };

            // 添加控件
            this.Controls.AddRange(new Control[] { lblUser, lblPass, lblCard, pbTagSingle, txtUsername, txtPassword, txtCard,btnLogin, btnExit, lblStatus });

        }

        //  初始化rfid
        private void InitRfid(string addr, int port)
        {
            if (!_rfidService.Init( addr,  port))
            {
                this.SafeInvoke(() =>
                {
                    lblStatus.Text = "读卡器初始化失败";
                    txtCard.Enabled = false;
                });
            }
            else
            {
                StartListening();
            }
        }

        private void StartListening()
        {
            _listenCancellationTokenSource = new CancellationTokenSource();
            try
            {
                _rfidService.StartListening(
                    // 第一个回调：处理UID
                    uid =>
                    {
                        this.SafeInvoke(() =>
                        {
                            txtCard.Text = uid;
                            // 刷卡验证
                            AuthenticateWithCard(uid);
                        });
                    },
                    // 第二个回调：处理标签在线状态
                    tagOnline =>
                    {
                        this.SafeInvoke(() =>
                        {
                            // 根据标签在线状态更新指示灯
                            pbTagSingle.BackColor= tagOnline ? Color.Green:Color.Red;
                        });
                    },
                    _listenCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"读卡器读取失败--{ex.Message}", ex);
            }
        }

        //  刷卡验证
        private async void AuthenticateWithCard(string cardId)
        {
            try
            {
                currentUser = await TryGetUserByCard(cardId);

                if (currentUser != null)
                {
                    Shared.user = currentUser;
                    _localUserService.UpdateUserLog(currentUser);
                    bool isOffline = _mesService.ValidateUser(currentUser.JobNub, currentUser.UserPassword, out string err);

                    Shared.isOffline = !isOffline;

                    await _userService.InsertUserSwipeLog(currentUser);

                    this.DialogResult = DialogResult.OK;

                    this.Close();
                }
                else
                {
                    lblStatus.Text = "无效卡号";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
        }

        //  账号密码登录
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;

            try
            {
                string username = txtUsername.Text.Trim();

                string password = txtPassword.Text.Trim();

                currentUser = await TryGetUserByLogin(username, password);

                if (currentUser != null)
                {
                    Shared.user = currentUser;
                    //_localUserService.UpdateUserLog(currentUser);

                    //bool isOffline = _mesService.ValidateUser(currentUser.JobNub, currentUser.UserPassword, out string err);

                    //Shared.isOffline = !isOffline;

                    //await _userService.InsertUserSwipeLog(currentUser);

                    this.DialogResult = DialogResult.OK;

                    this.Close();
                }
                else
                {
                    lblStatus.Text = "账号或密码错误";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = ex.Message;
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private async Task<User> TryGetUserByCard(string cardId)
        {
            try
            {
                // 🔥 优先远程
                var user = await _userService.GetUserByCard(cardId);
                if (user != null) return user;
            }
            catch
            {
                // 忽略异常，走本地
            }

            // 🔥 兜底本地 Access
            return _localUserService.GetUserByCard(cardId);
        }

        private async Task<User> TryGetUserByLogin(string username, string password)
        {
            //try
            //{
            //    var user = await _userService.GetUserByLogin(username, password);
            //    if (user != null) return user;
            //}
            //catch
            //{
            //    // 服务不可用
            //}

            // 本地兜底
            return _localUserService.GetUserByLogin(username, password);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _listenCancellationTokenSource?.Cancel();
            _rfidService.Stop();
            base.OnFormClosing(e);
        }
    }
}
