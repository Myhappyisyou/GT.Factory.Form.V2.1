using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Util
{
    public class DraggableTabControl : TabControl
    {
        private TabPage _dragTab;

        public event Action TabOrderChanged;


        public DraggableTabControl()
        {
            AllowDrop = true;
            MouseDown += Tab_MouseDown;
            MouseMove += Tab_MouseMove;
            DragOver += Tab_DragOver;
            DragDrop += Tab_DragDrop;
        }

        private void Tab_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < TabPages.Count; i++)
            {
                if (GetTabRect(i).Contains(e.Location))
                {
                    _dragTab = TabPages[i];
                    break;
                }
            }
        }

        private void Tab_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _dragTab != null)
            {
                DoDragDrop(_dragTab, DragDropEffects.Move);
            }
        }

        private void Tab_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void Tab_DragDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(typeof(TabPage)) is TabPage src))
                return;

            Point pt = PointToClient(new Point(e.X, e.Y));

            for (int i = 0; i < TabPages.Count; i++)
            {
                if (GetTabRect(i).Contains(pt))
                {
                    if (src == TabPages[i]) return;

                    int srcIndex = TabPages.IndexOf(src);
                    int dstIndex = i;

                    TabPages.Remove(src);
                    TabPages.Insert(dstIndex, src);
                    SelectedTab = src;
                    // ⭐ 通知外部顺序已改变
                    TabOrderChanged?.Invoke();
                    break;
                }
            }

            _dragTab = null;

        }
    }

}
