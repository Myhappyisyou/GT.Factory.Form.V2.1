using GT_Common.Helper;
using GT_Common.MyEnum;
using GT_Common.Util.LableStatus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Components
{
    public partial class ActionTipsUI : UserControl
    {
        private readonly Font _customFont = new Font("Segoe UI", 10f);
        private Timer _timer;
        private Label lbStatus;

        public ActionTipsUI()
        {
            AutoSize = true;

            InitializeComponent();
            InitFormLayout();
            //UiRefreshCenter.OnRefresh += RefreshUI;

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick; // 分离 Tick 回调
        }

        private void InitFormLayout()
        {
            var mainPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 248, 252),
                Padding = new Padding(6),
                BorderStyle = BorderStyle.FixedSingle,
            };

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Color.White,
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lbTitleName = new Label()
            {
                AutoSize = false,
                Text = "操作\n提示",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(230, 235, 240),
                ForeColor = Color.FromArgb(40, 40, 40),
            };

            lbStatus = new Label()
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BorderStyle = BorderStyle.FixedSingle,
                Font = _customFont,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 30, 30),
            };

            mainLayout.Controls.Add(lbTitleName, 0, 0);
            mainLayout.Controls.Add(lbStatus, 1, 0);

            mainPanel.Controls.Add(mainLayout);
            Controls.Add(mainPanel);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _timer?.Start();
        }

        private void RefreshUI()
        {
            if (!IsHandleCreated || IsDisposed) return;

            UpdateActionStatus();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // 安全保护
            if (!IsHandleCreated || IsDisposed)
                return;

            UpdateActionStatus();
        }

       
        private void UpdateActionStatus()
        {
            try
            {
                //if (Shared.lsActionTips == null || Shared.lsActionTips.Count == 0)
                //{
                //    lbStatus.Text = "无动作提示数据";
                //    return;
                //}

                //var tip = Shared.lsActionTips
                //    .FirstOrDefault(t => t.Code == Shared.currentActionStatus);

                //lbStatus.Text = tip != null && !string.IsNullOrWhiteSpace(tip.Tips)
                //    ? tip.Tips
                //    : $"未知提示：{Shared.currentActionStatus}";

                if (Shared.actionTip == null )
                {
                    lbStatus.Text = "待生产";
                    return;
                }

                lbStatus.Text = Shared.actionTip;
                lbStatus.ForeColor = StatusStyle.GetFontColor(Shared.actionTipStatus);

                //Shared.actionTipStatus = (ProductStatus)ctx.WritePlcResult;

                //Shared.actionTip = $"{ctx.MainBar}\n条码验证--{StatusStyle.GetDisplayText(Shared.actionTipStatus)}";

                //Shared.actionTipStatus = (ProductStatus)context.WritePlcResult;

                //Shared.actionTip = $"{context.MainBar}\n条码上传--{StatusStyle.GetDisplayText(Shared.actionTipStatus)}";
            }
            catch
            {
                lbStatus.Text = "提示信息更新错误";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // 先释放 Timer
                    if (_timer != null)
                    {
                        _timer.Tick -= Timer_Tick;
                        _timer.Stop();
                        _timer.Dispose();
                        _timer = null;
                    }
                }
                catch { }

                //UiRefreshCenter.OnRefresh -= RefreshUI;
                // Designer 自动生成的控件释放
                components?.Dispose();
                lbStatus?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

}
