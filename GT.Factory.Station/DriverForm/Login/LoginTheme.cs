using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Station.DriverForm.Login
{
    public static class LoginTheme
    {
        public static Color Primary = Color.FromArgb(24, 144, 255);
        public static Color Background = Color.FromArgb(240, 242, 245);
        public static Font TitleFont = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
        public static Font NormalFont = new Font("Microsoft YaHei UI", 12F);
    }

    public static class IndustrialTheme
    {
        public static Color Background = Color.FromArgb(18, 25, 38);     // 深蓝黑
        public static Color Card = Color.FromArgb(28, 38, 55);           // 卡片蓝
        public static Color Primary = Color.FromArgb(0, 168, 255);       // 科技蓝
        public static Color Success = Color.FromArgb(0, 200, 83);        // 绿色
        public static Color Danger = Color.FromArgb(255, 82, 82);        // 红色
        public static Color Text = Color.White;
        public static Font TitleFont = new Font("Microsoft YaHei UI", 22F, FontStyle.Bold);
        public static Font LargeFont = new Font("Microsoft YaHei UI", 16F);
        public static Font NormalFont = new Font("Microsoft YaHei UI", 14F);
    }

    public static class UIHelper
    {
        public static DialogResult ShowDialogWithMask(Form dialog, Form parentForm)
        {
            using (Form mask = new Form())
            {
                mask.StartPosition = FormStartPosition.Manual;
                mask.FormBorderStyle = FormBorderStyle.None;
                mask.BackColor = Color.Black;
                mask.Opacity = 0.6; // 60%透明黑
                mask.ShowInTaskbar = false;
                mask.Size = parentForm.Size;
                mask.Location = parentForm.Location;

                mask.Show(parentForm);
                dialog.StartPosition = FormStartPosition.CenterParent;
                DialogResult result = dialog.ShowDialog(mask);
                mask.Close();

                return result;
            }
        }
    }
}
