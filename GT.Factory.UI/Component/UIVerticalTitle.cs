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
    public class UIVerticalTitle : Label
    {
        public UIVerticalTitle(string text)
        {
            this.Text = SplitText(text);
            this.Dock = DockStyle.Fill;
            this.TextAlign = ContentAlignment.MiddleCenter;

            this.Font = UITheme.FontNormal;
            this.BackColor = UITheme.Card;
            this.ForeColor = UITheme.Text;
        }

        private string SplitText(string text)
        {
            return string.Join("\n", text.ToCharArray());
        }
    }
}
