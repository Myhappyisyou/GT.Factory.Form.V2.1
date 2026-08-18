using GT.Factory.UI.Component;
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
    /// <summary>
    /// 工具页面模板（简化稳定版）
    /// 结构：
    /// ┌──────── 工具栏 ────────┐
    /// ├──────── 内容区（Grid） ─┤
    /// └────────────────────────┘
    /// </summary>
    public class UIToolPage : UserControl
    {
        private Panel _topBar;
        private Panel _contentPanel;

        public FlowLayoutPanel ToolBar { get; private set; }
        public DataGridView Grid { get; private set; }

        private UILoadingOverlay _loading;
        private UIEmptyView _empty;
        private UIEmptyView _error;

        public UIToolPage()
        {
            InitLayout();
        }

        private void InitLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = UITheme.Background;

            // ===== 工具栏 =====
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = UITheme.Card,
                Padding = new Padding(0, 5, 0, 5)
            };

            ToolBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,   // ✅ 允许换行
                AutoScroll = false
            };

            _topBar.Controls.Add(ToolBar);

            // ===== 内容区 =====
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
            };

            Grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = true
            };
            UIGrid.BuildDataGrid(Grid);

            Grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ===== 状态层 =====
            _loading = new UILoadingOverlay();
            _empty = new UIEmptyView();
            _error = new UIEmptyView();

            _contentPanel.Controls.Add(Grid);
            _contentPanel.Controls.Add(_loading);
            _contentPanel.Controls.Add(_empty);
            _contentPanel.Controls.Add(_error);

            // ===== 添加 =====
            this.Controls.Add(_contentPanel);
            this.Controls.Add(_topBar);
        }

        public void AddControl(Control ctrl, bool fullRow = false)
        {
            ctrl.Height = UITheme.ControlHeight;

            ctrl.AutoSize = false;

            if (fullRow)
            {
                ctrl.AutoSize = false;

                ctrl.Width = ToolBar.ClientSize.Width - 10;

                ToolBar.Resize += (s, e) =>
                {
                    ctrl.Width = ToolBar.ClientSize.Width - 10;
                };
            }
            //else
            //{
            //    ctrl.AutoSize = true;
            //}

            ToolBar.Controls.Add(ctrl);
        }

        /// <summary>
        /// 添加按钮
        /// fullRow = true → 整行按钮
        /// </summary>
        public void AddButton(Button btn, bool fullRow = false)
        {
            btn.Padding = new Padding(12, 0, 12, 0);
            AddControl(btn, fullRow);
        }

        /// <summary>
        /// 状态切换
        /// </summary>
        public void SetState(UIPageState state, string message = null)
        {
            _loading.HideLoading();
            _empty.HideMessage();
            _error.HideMessage();
            Grid.Visible = false;

            switch (state)
            {
                case UIPageState.Loading:
                    _loading.ShowLoading(message ?? "加载中...");
                    break;

                case UIPageState.Empty:
                    _empty.ShowMessage(message ?? "暂无数据");
                    break;

                case UIPageState.Error:
                    _error.ShowMessage(message ?? "加载失败");
                    break;

                case UIPageState.Data:
                    Grid.Visible = true;
                    break;
            }
        }
    }
}
