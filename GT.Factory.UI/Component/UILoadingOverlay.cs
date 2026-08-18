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
    /// 加载遮罩（查询中）
    /// </summary>
    public class UILoadingOverlay : Panel
    {
        private Label lbl;

        public UILoadingOverlay()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(120, 0, 0, 0); // 半透明遮罩
            this.Visible = false;

            lbl = new Label
            {
                Text = "正在查询，请稍候...",
                ForeColor = Color.White,
                Font = UITheme.FontLarge,
                AutoSize = true
            };

            this.Controls.Add(lbl);

            this.Resize += (s, e) =>
            {
                lbl.Location = new Point(
                    (this.Width - lbl.Width) / 2,
                    (this.Height - lbl.Height) / 2
                );
            };
        }

        public void ShowLoading(string text = "加载中...")
        {
            lbl.Text = text;
            this.Visible = true;
            this.BringToFront();
        }

        public void HideLoading()
        {
            this.Visible = false;
        }
    }
}
