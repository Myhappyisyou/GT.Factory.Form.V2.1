using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Style
{
    public static class UITree
    {
        public static TreeView Create()
        {
            var tree = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,

                BackColor = UITheme.Card,
                ForeColor = UITheme.Text,
                Font = UITheme.BodyFont,

                FullRowSelect = true,
                HideSelection = false,
                HotTracking = true,

                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true,

                ItemHeight = 22
            };

            return tree;
        }
    }
}
