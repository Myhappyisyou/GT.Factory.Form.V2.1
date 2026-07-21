using GT_Common.Helper;
using GT_Common.MyEnum;
using GTJPN2503006.DriverForm.Aynettek;
using GTJPN2503006.Helper.BydMes;
using GTJPN2503006.Helper.BydUser;
using GTJPN2503006.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTJPN2503006
{
    public partial class CardLoginForm : Form
    {
        private CancellationTokenSource _listenCancellationTokenSource;
        private readonly IMesService _mesService;
        private readonly IUserService _userService;
        private readonly IRfidService _rfidService;
        private User currentUser;


        private TextBox txtCardNo;
        private Button btnLogin;
        private Label lblInfo;

        public User LoggedInUser { get; private set; }

        //  IRfidService rfidService, IUserService userService, IMesService mesService
        public CardLoginForm(IRfidService rfidService, IUserService userService, IMesService mesService, string addr, int port)
        {
            _rfidService = rfidService;
            _userService = userService;
            _mesService = mesService;


            Text = "刷卡登录";
            Width = 300;
            Height = 180;
            StartPosition = FormStartPosition.CenterScreen;

            lblInfo = new Label() { Text = "请刷卡或输入卡号：", Top = 20, Left = 20, AutoSize = true };
            txtCardNo = new TextBox() { Top = 50, Left = 20, Width = 200 };
            btnLogin = new Button() { Text = "登录", Top = 90, Left = 20, Width = 80 };

            btnLogin.Click += BtnLogin_Click;

            Controls.Add(lblInfo);
            Controls.Add(txtCardNo);
            Controls.Add(btnLogin);

            InitRfid( addr,  port);
        }

        //  初始化rfid
        private void InitRfid(string addr, int port)
        {
            if (!_rfidService.Init( addr,  port))
            {
               
            }
            else
            {
                StartListening();
            }
        }

        //  开启监听UID
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
                            txtCardNo.Text = uid;
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
                        });
                    },
                    _listenCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Helper.Logging.DisplayLog.Error("监听UID异常", ex);
            }
        }

        //  刷卡验证
        private void AuthenticateWithCard(string cardId)
        {
            currentUser = _userService.GetUserByCard(cardId);
            if (currentUser != null)
            {
                bool ok = true;
                LoggedInUser= currentUser;
                if (currentUser.LevelEnum == UserLevel.ADM || currentUser.LevelEnum == UserLevel.PE)
                {
                    if (!Shared.isOffline)
                    {
                        ok = _mesService.ValidateUser(out string MESren);
                    }
                }
              
                else
                {
                    _userService.InsertUserSwipeLog(currentUser);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("卡号不存在！");
            }
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string cardNo = txtCardNo.Text.Trim();

            if (string.IsNullOrEmpty(cardNo))
            {
                MessageBox.Show("卡号不能为空！");
                return;
            }
            currentUser = _userService.GetUserByCard(cardNo);

            if (currentUser == null)
            {
                MessageBox.Show("卡号不存在！");
                return;
            }

            LoggedInUser = currentUser;

            _userService.InsertUserSwipeLog(currentUser);

           

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
