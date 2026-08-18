using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Apply
{
    public static class UIStyle
    {
        public static void ApplyWidget(Control root)
        {
            root.BackColor = UITheme.Background;

            foreach (Control c in root.Controls)
            {
                if (c is Panel p)
                {
                    p.BackColor = UITheme.Card;
                }
                else if (c is Label l)
                {
                    l.ForeColor = UITheme.Text;
                }

                if (c.HasChildren)
                    ApplyWidget(c);
            }
        }
    }
}
