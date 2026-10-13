using GT.Factory.Logging.Logging;
using System;
using System.Windows.Forms;

namespace GT.Factory.Server
{
    public partial class PassWordFrom : Form
    {
        public PassWordFrom()
        {
            InitializeComponent();

            // 设置在父窗口中心
            //this.StartPosition = FormStartPosition.CenterParent;

            // 或者设置在屏幕中心
            this.StartPosition = FormStartPosition.CenterScreen;

        }

        /// <summary>
        /// 确定
        /// </summary>
        private void Btnconfirm_Click(object sender, EventArgs e)
        {
            //while (txtpassword.Text != "123")
            //{
            //    DisplayLog.ShowLogWarn("密码错误,请重新输入！");
            //    //MessageBox.Show("密码错误,请重新输入！");
            //    txtpassword.Clear();
            //    return;
            //}
            //this.Close();
            if (txtpassword.Text.Trim() == "123")
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("密码错误");
                DisplayLog.Warn("密码错误,请重新输入！",true);
                txtpassword.Clear();
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Btncancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
