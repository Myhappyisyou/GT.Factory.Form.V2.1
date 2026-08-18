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
    /// <summary>
    /// 统计组件（工业稳定版）
    /// </summary>
    public class UIStatBar : FlowLayoutPanel
    {
        private Label lblOK;
        private Label lblNG;
        private Label lblTotal;

        public UIStatBar()
        {
            this.Height = UITheme.ControlHeight;
            this.AutoSize = false; // ✅ 关键！
            this.WrapContents = false;
            this.FlowDirection = FlowDirection.LeftToRight;
            this.Width = 360; // ✅ 关键：固定宽度（必须）

            lblOK = CreateStat("OK", UITheme.Success);
            lblNG = CreateStat("NG", UITheme.Danger);
            lblTotal = CreateStat("总数", UITheme.Primary);

            this.Controls.Add(lblOK);
            this.Controls.Add(lblNG);
            this.Controls.Add(lblTotal);
        }

        private Label CreateStat(string text, Color color)
        {
            return new Label
            {
                Text = $"{text}: 0",
                Width = 110,
                Height = UITheme.ControlHeight,
                BackColor = color,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(5, 0, 5, 0),
                AutoSize = false, // ✅ 关键！
                Font = UITheme.FontNormal
            };
        }

        public void Update(int ok, int ng, int total)
        {
            lblOK.Text = $"OK: {ok}";
            lblNG.Text = $"NG: {ng}";
            lblTotal.Text = $"总数: {total}";
        }
    }
}
