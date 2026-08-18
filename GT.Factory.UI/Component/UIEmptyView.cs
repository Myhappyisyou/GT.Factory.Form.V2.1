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
    /// 空数据提示
    /// </summary>
    public class UIEmptyView : Panel
    {
        private Label lbl;

        public UIEmptyView()
        {
            this.Dock = DockStyle.Fill;
            this.Visible = false;

            lbl = new Label
            {
                Text = "暂无数据",
                ForeColor = UITheme.SubText,
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

        public void ShowMessage(string text = "暂无数据")
        {
            lbl.Text = text;
            this.Visible = true;
            this.BringToFront();
        }

        public void HideMessage()
        {
            this.Visible = false;
        }
    }
}
