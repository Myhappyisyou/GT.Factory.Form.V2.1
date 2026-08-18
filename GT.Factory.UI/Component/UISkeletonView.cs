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
    public class UISkeletonView : Panel
    {
        public UISkeletonView()
        {
            this.Dock = DockStyle.Fill;
            this.Visible = false;
            this.BackColor = UITheme.Card;

            this.Paint += (s, e) =>
            {
                var g = e.Graphics;

                for (int i = 0; i < 8; i++)
                {
                    g.FillRectangle(
                        new SolidBrush(Color.FromArgb(50, 60, 80)),
                        20,
                        20 + i * 40,
                        this.Width - 40,
                        25);
                }
            };
        }

        public void ShowSkeleton()
        {
            this.Visible = true;
            this.BringToFront();
        }

        public void HideSkeleton()
        {
            this.Visible = false;
        }
    }
}
