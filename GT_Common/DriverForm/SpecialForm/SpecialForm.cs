using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.SpecialForm
{
    public partial class SpecialForm : Form
    {
        private CheckBox chkModus;
        private CheckBox chkBind;
        private CheckBox chkTimeOut;
        private CheckBox chkBolckMesBind;

        public SpecialForm()
        {
            InitializeComponent();

            //  初始化UI界面
            InitUI();
        }

        //  初始化UI界面
        private void InitUI()
        {
            this.Text = "屏蔽界面";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 350;
            this.Height = 280;

            // 创建控件
            var lblModus = new Label { Text = "模式：", Left = 20, Top = 40, Width = 80 };

            chkModus = new CheckBox { Left = 100, Top = 40, Width = 200, Text = "MES/单机" };

            chkModus.Checked = Shared.isOffline;

            chkModus.CheckedChanged += (s, e) =>
            {
                Shared.isOffline = chkModus.Checked;
            };

            var lblBind = new Label { Text = "绑定：", Left = 20, Top = 80, Width = 80 };

            chkBind = new CheckBox { Left = 100, Top = 80, Width = 200, Text = "绑定/未绑定" };
            chkBind.Checked = Shared.isBind;
            chkBind.CheckedChanged += (s, e) =>
            {
                Shared.isBind = chkBind.Checked;
            };

            var lblTimeOut = new Label { Text = "静置时间：", Left = 20, Top = 120, Width = 80 };

            chkTimeOut = new CheckBox { Left = 100, Top = 120, Width = 200, Text = "超时/未超时" };
            chkTimeOut.Checked = Shared.isTimeOut;
            chkTimeOut.CheckedChanged += (s, e) =>
            {
                Shared.isTimeOut = chkTimeOut.Checked;
            };

            var lblBlockMesBind = new Label { Text = "MES装配屏蔽：", Left = 20, Top = 120, Width = 80 };

            chkBolckMesBind = new CheckBox { Left = 100, Top = 120, Width = 200, Text = "屏蔽/开启" };
            chkBolckMesBind.Checked = Shared.blockMesBind;
            chkBolckMesBind.CheckedChanged += (s, e) =>
            {
                Shared.blockMesBind = chkBolckMesBind.Checked;
            };

            // 添加控件
            this.Controls.AddRange(new Control[] { lblModus, chkModus, lblBind, chkBind,lblTimeOut, chkTimeOut, lblBlockMesBind, chkBolckMesBind });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }

    }
}
