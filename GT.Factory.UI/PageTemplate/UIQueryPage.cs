using GT.Factory.UI.Component;
using GT.Factory.UI.Enum;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.PageTemplate
{
    ///┌──────── Header（可选）────────┐
    ///├──────── 查询区（条件 + 按钮） ─┤
    ///├──────── 表格区（DataGrid） ───┤
    ///├──────── 底部（分页 + 统计） ──┤
    ///└──────────────────────────────┘
    /// <summary>
    /// 标准查询页面模板（工业终端）
    /// 包含：查询区 / 表格 / 分页 / 统计
    /// </summary>
    public class UIQueryPage : UserControl
    {
        // ===== 区域 =====
        private Panel _topPanel;
        private Panel _gridPanel;
        private Panel _bottomPanel;

        private FlowLayoutPanel _queryContainer;

        // ===== 组件 =====
        public UIQueryButtons QueryButtons { get; private set; }
        public DataGridView Grid { get; private set; }
        public UIPaging Paging { get; private set; }
        public UIStatBar StatBar { get; private set; }

        private UILoadingOverlay _loading;
        private UIEmptyView _empty;
        private UIEmptyView _error;
        private UISkeletonView _skeleton;

        public UIQueryPage()
        {
            InitLayout();
        }

        /// <summary>
        /// 初始化整体布局
        /// </summary>
        private void InitLayout()
        {
            //this.MaximizeBox = true;
            //this.StartPosition = FormStartPosition.CenterScreen;
            this.Dock = DockStyle.Fill;
            this.BackColor = UITheme.Background;

          
            // ===== 查询区 =====
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = UITheme.Card,
                Padding = new Padding(10)
            };

            _queryContainer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                AutoSize = false,
                FlowDirection = FlowDirection.LeftToRight
            };

            QueryButtons = new UIQueryButtons();

            _topPanel.Controls.Add(_queryContainer);
            _topPanel.Controls.Add(QueryButtons);
            QueryButtons.Dock = DockStyle.Right;

            // ===== 表格区 =====
            _gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            Grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false
            };

            UIGrid.Default(Grid);

            _loading = new UILoadingOverlay();
            _empty = new UIEmptyView();
            _error = new UIEmptyView();
            _skeleton = new UISkeletonView();

            _gridPanel.Controls.Add(_skeleton);
            _gridPanel.Controls.Add(_error);
            _gridPanel.Controls.Add(_empty);
            _gridPanel.Controls.Add(_loading);

            _gridPanel.Controls.Add(Grid);

            // ===== 底部区 =====
            _bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10,5,10,5),
                BackColor = UITheme.Card
            };

            var bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3
            };

            // ✅ 列布局
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));    // 左（分页）
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // 中间撑开
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));    // 右（统计）

            // ✅ 行布局（必须）
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            Paging = new UIPaging();
            StatBar = new UIStatBar();

            // ✅ 加入布局
            bottomLayout.Controls.Add(Paging, 0, 0);
            bottomLayout.Controls.Add(new Panel(), 1, 0); // 占位
            bottomLayout.Controls.Add(StatBar, 2, 0);

            // ✅ 对齐方式（关键）
            Paging.Anchor = AnchorStyles.Left;
            StatBar.Anchor = AnchorStyles.Right;

            _bottomPanel.Controls.Add(bottomLayout);

            // ===== 添加到页面 =====
            this.Controls.Add(_gridPanel);

            this.Controls.Add(_bottomPanel);
            this.Controls.Add(_topPanel);
        }

        /// <summary>
        /// 添加查询条件（自动布局）
        /// </summary>
        public void AddQueryControl(Control control)
        {
            _queryContainer.Controls.Add(control);
        }

        /// <summary>
        /// 查询按钮是否可见
        /// </summary>
        public void EnableQueryBtn(bool enable)
        {
            QueryButtons.Visible = enable;
        }

        /// <summary>
        /// 翻页是否可见
        /// </summary>
        public void EnablePaging(bool enable)
        {
            Paging.Visible = enable;
        }

        /// <summary>
        /// 统计是否可见
        /// </summary>
        public void EnableStat(bool enable)
        {
            StatBar.Visible = enable;
        }

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
