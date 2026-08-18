using GT.Factory.UI.Enum;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.PageTemplate
{
    public class UIConfigPage : UserControl
    {
        public TreeView Tree { get; private set; }
        public DataGridView Grid { get; private set; }
        public FlowLayoutPanel ToolBar { get; private set; }
        public StatusStrip StatusBar { get; private set; }

        private ToolStripStatusLabel _statusLabel;

        public UIConfigPage(string title = null)
        {
            InitLayout(title);
        }

        private void InitLayout(string title)
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UITheme.Background;

            // ===== Split =====
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                //SplitterDistance = 260,
                Orientation = Orientation.Vertical
            };
            
            // ===== 左 Tree =====
            Tree = UITree.Create();
            split.Panel1.Controls.Add(Tree);

            // ===== 右侧 =====
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.Card
            };

            ToolBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            Grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true
            };
            UIGrid.Default(Grid);

            rightPanel.Controls.Add(Grid);
            rightPanel.Controls.Add(ToolBar);

            split.Panel2.Controls.Add(rightPanel);

            // ===== 状态栏 =====
            StatusBar = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel();
            StatusBar.Items.Add(_statusLabel);

            this.Controls.Add(split);
            this.Controls.Add(StatusBar);

            this.Load += (s, e) =>
            {
                // 窗体加载完成后设置 SplitterDistance
                split.SplitterDistance = 260;
            };

        }

        public void AddButton(Button btn)
        {
            ToolBar.Controls.Add(btn);
        }

        public void SetStatus(string text, UIStatusType type = UIStatusType.Info)
        {
            _statusLabel.Text = text;

            switch (type)
            {
                case UIStatusType.Success:
                    _statusLabel.ForeColor = UITheme.Success;
                    break;
                case UIStatusType.Warning:
                    _statusLabel.ForeColor = UITheme.Warning;
                    break;
                case UIStatusType.Error:
                    _statusLabel.ForeColor = UITheme.Danger;
                    break;
                default:
                    _statusLabel.ForeColor = UITheme.Text;
                    break;
            }
        }
    }
}
