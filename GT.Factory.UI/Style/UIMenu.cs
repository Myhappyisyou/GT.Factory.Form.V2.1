using GT.Factory.UI.Theme;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.UI.Style
{
    /// <summary>
    /// GT.Factory统一菜单UI。
    ///
    /// 用于:
    ///
    /// 1. MenuStrip创建
    /// 2. 一级菜单创建
    /// 3. 菜单项创建
    /// 4. 分隔线创建
    /// 5. 菜单统一工业UI样式
    ///
    ///
    /// 设计原则:
    ///
    /// GT.Factory.UI
    ///     ↓
    /// 负责菜单视觉规范
    ///
    /// 业务项目
    ///     ↓
    /// 负责菜单内容和业务事件
    ///
    ///
    /// 例如:
    ///
    /// var menu = UIMenu.Create();
    ///
    /// var project =
    ///     UIMenu.AddMenu(
    ///         menu,
    ///         "工程");
    ///
    /// UIMenu.AddItem(
    ///     project,
    ///     "打开工程...",
    ///     OpenProject);
    ///
    /// </summary>
    public static class UIMenu
    {

        #region 常量

        /// <summary>
        /// 菜单默认高度
        /// </summary>
        private const int MenuHeight = 30;


        /// <summary>
        /// 一级菜单左右内边距
        /// </summary>
        private const int MainMenuHorizontalPadding = 10;


        /// <summary>
        /// 子菜单左右内边距
        /// </summary>
        private const int ItemHorizontalPadding = 12;


        /// <summary>
        /// 子菜单上下内边距
        /// </summary>
        private const int ItemVerticalPadding = 6;

        #endregion


        #region 创建


        /// <summary>
        /// 创建标准MenuStrip。
        ///
        /// 使用GT.Factory统一UITheme。
        ///
        /// </summary>
        public static MenuStrip Create()
        {
            var menu =
                new MenuStrip();

            ApplyStyle(
                menu);

            return menu;
        }


        /// <summary>
        /// 创建MenuStrip并指定Dock。
        ///
        /// </summary>
        public static MenuStrip Create(
            DockStyle dock)
        {
            var menu =
                Create();

            menu.Dock =
                dock;

            return menu;
        }


        #endregion


        #region 样式


        /// <summary>
        /// 应用统一MenuStrip样式。
        ///
        /// </summary>
        public static void ApplyStyle(
            MenuStrip menu)
        {
            if (menu == null)
                throw new ArgumentNullException(
                    nameof(menu));


            //
            // 基础布局
            //
            menu.Dock =
                DockStyle.Top;


            menu.Height =
                MenuHeight;


            menu.AutoSize =
                false;


            menu.Padding =
                new Padding(
                    4,
                    0,
                    4,
                    0);


            //
            // 使用统一字体
            //
            menu.Font =
                UITheme.BodyFont;


            //
            // 使用统一背景
            //
            menu.BackColor =
                UITheme.Card;


            //
            // 使用统一文字颜色
            //
            menu.ForeColor =
                UITheme.Text;


            //
            // 禁止系统默认阴影
            //
            menu.ShowItemToolTips =
                true;


            //
            // Renderer
            //
            menu.Renderer =
                new UIMenuRenderer();
        }


        /// <summary>
        /// 应用一级菜单样式。
        ///
        /// </summary>
        public static void ApplyMenuStyle(
            ToolStripMenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException(
                    nameof(item));


            item.Font =
                UITheme.BodyFont;


            item.ForeColor =
                UITheme.Text;


            item.BackColor =
                UITheme.Card;


            item.Padding =
                new Padding(
                    MainMenuHorizontalPadding,
                    4,
                    MainMenuHorizontalPadding,
                    4);


            item.Margin =
                new Padding(
                    2,
                    0,
                    2,
                    0);
        }


        /// <summary>
        /// 应用普通菜单项样式。
        ///
        /// </summary>
        public static void ApplyItemStyle(
            ToolStripMenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException(
                    nameof(item));


            item.Font =
                UITheme.BodyFont;


            item.ForeColor =
                UITheme.Text;


            item.BackColor =
                UITheme.Card;


            item.Padding =
                new Padding(
                    ItemHorizontalPadding,
                    ItemVerticalPadding,
                    ItemHorizontalPadding,
                    ItemVerticalPadding);


            item.Margin =
                new Padding(
                    0);
        }


        #endregion


        #region 一级菜单


        /// <summary>
        /// 添加一级菜单。
        ///
        /// 例如:
        ///
        /// 工程
        /// 通信
        /// 视图
        /// 帮助
        ///
        /// </summary>
        public static ToolStripMenuItem AddMenu(
            MenuStrip menu,
            string text)
        {
            if (menu == null)
                throw new ArgumentNullException(
                    nameof(menu));


            var item =
                new ToolStripMenuItem(
                    text);


            ApplyMenuStyle(
                item);


            menu.Items.Add(
                item);


            return item;
        }


        /// <summary>
        /// 添加一级菜单。
        ///
        /// 同时绑定点击事件。
        ///
        /// 一般一级菜单不需要Click事件，
        /// 保留该接口方便特殊场景。
        ///
        /// </summary>
        public static ToolStripMenuItem AddMenu(
            MenuStrip menu,
            string text,
            EventHandler click)
        {
            var item =
                AddMenu(
                    menu,
                    text);


            if (click != null)
            {
                item.Click +=
                    click;
            }


            return item;
        }


        #endregion


        #region 菜单项


        /// <summary>
        /// 添加普通菜单项。
        ///
        /// </summary>
        public static ToolStripMenuItem AddItem(
            ToolStripMenuItem parent,
            string text,
            EventHandler click = null)
        {
            if (parent == null)
                throw new ArgumentNullException(
                    nameof(parent));


            var item =
                new ToolStripMenuItem(
                    text);


            ApplyItemStyle(
                item);


            if (click != null)
            {
                item.Click +=
                    click;
            }


            parent.DropDownItems.Add(
                item);


            return item;
        }


        /// <summary>
        /// 添加带快捷键的菜单项。
        ///
        /// 例如:
        ///
        /// Ctrl + O
        ///
        /// </summary>
        public static ToolStripMenuItem AddItem(
            ToolStripMenuItem parent,
            string text,
            Keys shortcut,
            EventHandler click = null)
        {
            var item =
                AddItem(
                    parent,
                    text,
                    click);


            item.ShortcutKeys =
                shortcut;


            return item;
        }


        #endregion


        #region 分隔线


        /// <summary>
        /// 添加菜单分隔线。
        ///
        /// </summary>
        public static ToolStripSeparator AddSeparator(
            ToolStripMenuItem parent)
        {
            if (parent == null)
                throw new ArgumentNullException(
                    nameof(parent));


            var separator =
                new ToolStripSeparator();


            parent.DropDownItems.Add(
                separator);


            return separator;
        }


        #endregion


        #region 子菜单


        /// <summary>
        /// 添加二级菜单。
        ///
        /// 例如:
        ///
        /// 工程
        ///   └── 最近打开
        ///
        /// </summary>
        public static ToolStripMenuItem AddSubMenu(
            ToolStripMenuItem parent,
            string text)
        {
            if (parent == null)
                throw new ArgumentNullException(
                    nameof(parent));


            var item =
                new ToolStripMenuItem(
                    text);


            ApplyItemStyle(
                item);


            parent.DropDownItems.Add(
                item);


            return item;
        }


        #endregion


        #region 状态


        /// <summary>
        /// 设置菜单项启用状态。
        ///
        /// </summary>
        public static void SetEnabled(
            ToolStripMenuItem item,
            bool enabled)
        {
            if (item == null)
                return;


            item.Enabled =
                enabled;
        }


        /// <summary>
        /// 设置菜单项显示状态。
        ///
        /// </summary>
        public static void SetVisible(
            ToolStripMenuItem item,
            bool visible)
        {
            if (item == null)
                return;


            item.Visible =
                visible;
        }


        /// <summary>
        /// 设置菜单项选中状态。
        ///
        /// 适用于:
        ///
        /// 视图
        /// 显示Signal
        /// 显示Trace
        ///
        /// </summary>
        public static void SetChecked(
            ToolStripMenuItem item,
            bool checkedState)
        {
            if (item == null)
                return;


            item.Checked =
                checkedState;
        }


        #endregion


        #region Renderer


        /// <summary>
        /// GT.Factory菜单Renderer。
        ///
        /// 用于控制:
        ///
        /// Hover
        /// Selected
        /// Separator
        /// Menu背景
        ///
        /// 避免使用Windows默认的
        /// 深色/高对比菜单风格。
        ///
        /// </summary>
        private sealed class UIMenuRenderer :
            ToolStripProfessionalRenderer
        {


            /// <summary>
            /// 创建Renderer。
            ///
            /// </summary>
            public UIMenuRenderer()
                : base(
                    new UIMenuColorTable())
            {

                RoundedEdges =
                    false;

            }


            /// <summary>
            /// 绘制菜单项背景。
            ///
            /// 完全由GT.Factory控制菜单背景。
            ///
            /// 不调用:
            ///
            /// base.OnRenderMenuItemBackground()
            ///
            ///
            /// 原因:
            ///
            /// ProfessionalRenderer默认会再次绘制
            /// Windows系统菜单选中背景。
            ///
            /// 如果继续调用base，
            /// 前面使用UITheme绘制的颜色可能被系统
            /// 默认颜色覆盖。
            ///
            /// 因此这里完全接管背景绘制。
            /// </summary>
            protected override void
                OnRenderMenuItemBackground(
                    ToolStripItemRenderEventArgs e)
            {
                if (e == null ||
                    e.Item == null)
                {
                    return;
                }


                //
                // 获取菜单项绘制区域
                //
                var rect =
                    new Rectangle(
                        Point.Empty,
                        e.Item.Size);


                //
                // 当前菜单项是否处于Hover / Selected状态
                //
                bool selected =
                    e.Item.Selected &&
                    e.Item.Enabled;


                //
                // 根据状态决定背景颜色
                //
                Color backColor =
                    selected
                    ?
                    UITheme.BgHover
                    :
                    UITheme.Card;


                //
                // 绘制背景
                //
                using (var brush =
                    new SolidBrush(
                        backColor))
                {
                    e.Graphics.FillRectangle(
                        brush,
                        rect);
                }


                //
                // Selected状态绘制边框
                //
                if (selected)
                {
                    using (var pen =
                        new Pen(
                            UITheme.Border))
                    {
                        var borderRect =
                            new Rectangle(
                                rect.X,
                                rect.Y,
                                rect.Width - 1,
                                rect.Height - 1);


                        e.Graphics.DrawRectangle(
                            pen,
                            borderRect);
                    }
                }


                //
                // 注意:
                //
                // 这里不能再调用:
                //
                // base.OnRenderMenuItemBackground(e);
                //
                // 否则ProfessionalRenderer会再次绘制背景，
                // 从而可能覆盖GT.Factory自己的颜色。
            }


            /// <summary>
            /// 绘制分隔线。
            ///
            /// </summary>
            protected override void
                OnRenderSeparator(
                    ToolStripSeparatorRenderEventArgs e)
            {
                var rect =
                    new Rectangle(
                        6,
                        0,
                        e.Item.Width - 12,
                        e.Item.Height);


                using (var pen =
                    new Pen(
                        UITheme.Border))
                {
                    int y =
                        rect.Top +
                        rect.Height / 2;


                    e.Graphics.DrawLine(
                        pen,
                        rect.Left,
                        y,
                        rect.Right,
                        y);
                }
            }

        }


        /// <summary>
        /// 菜单颜色表。
        ///
        /// </summary>
        /// <summary>
        /// LIN / GT.Factory统一菜单颜色表
        ///
        /// 负责控制:
        ///
        /// 1. MenuStrip菜单栏
        /// 2. 一级菜单选中状态
        /// 3. 下拉菜单背景
        /// 4. 下拉菜单图片区域
        /// 5. 菜单边框
        ///
        /// 注意:
        ///
        /// 不直接使用系统默认颜色。
        ///
        /// 所有颜色统一来自:
        ///
        /// UITheme
        ///
        /// 保证不同窗口的菜单视觉风格一致。
        /// </summary>
        private sealed class UIMenuColorTable :
            ProfessionalColorTable
        {

            /// <summary>
            /// 创建菜单颜色表
            ///
            /// 禁止使用Windows系统默认颜色。
            ///
            /// 否则在不同Windows主题下，
            /// MenuStrip可能出现白色、蓝色等
            /// 与工业UI主题不一致的颜色。
            /// </summary>
            public UIMenuColorTable()
            {
                UseSystemColors =
                    false;
            }


            /// <summary>
            /// 菜单整体边框颜色。
            ///
            /// 主要用于:
            ///
            /// MenuStrip
            /// DropDown
            /// Menu边缘
            /// </summary>
            public override Color MenuBorder
            {
                get
                {
                    return UITheme.Border;
                }
            }


            /// <summary>
            /// 菜单项边框颜色。
            ///
            /// 当菜单项处于:
            ///
            /// Selected
            /// Pressed
            ///
            /// 状态时，
            /// 用于控制菜单项边缘颜色。
            /// </summary>
            public override Color MenuItemBorder
            {
                get
                {
                    return UITheme.Border;
                }
            }


            /// <summary>
            /// 菜单项选中颜色。
            ///
            /// 用于一级菜单或下拉菜单
            /// 被鼠标选中时的背景颜色。
            ///
            /// 例如:
            ///
            /// 工程
            ///
            /// 点击后不会使用Windows默认白色，
            /// 而是使用GT.Factory统一背景色。
            /// </summary>
            public override Color MenuItemSelected
            {
                get
                {
                    return UITheme.BgBase;
                }
            }


            /// <summary>
            /// 菜单项选中状态渐变起始颜色。
            ///
            /// WinForms ProfessionalRenderer
            /// 默认可能使用渐变。
            ///
            /// 这里使用统一颜色，
            /// 避免出现明显的系统渐变效果。
            /// </summary>
            public override Color MenuItemSelectedGradientBegin
            {
                get
                {
                    return UITheme.BgBase;
                }
            }


            /// <summary>
            /// 菜单项选中状态渐变结束颜色。
            ///
            /// 与:
            ///
            /// MenuItemSelectedGradientBegin
            ///
            /// 使用相同颜色，
            /// 保持菜单选中区域为纯色。
            /// </summary>
            public override Color MenuItemSelectedGradientEnd
            {
                get
                {
                    return UITheme.BgBase;
                }
            }


            /// <summary>
            /// MenuStrip左侧背景颜色。
            ///
            /// MenuStrip采用统一Card颜色，
            /// 与GT.Factory其它UI组件保持一致。
            /// </summary>
            public override Color MenuStripGradientBegin
            {
                get
                {
                    return UITheme.Card;
                }
            }


            /// <summary>
            /// MenuStrip右侧背景颜色。
            ///
            /// 与MenuStripGradientBegin保持一致，
            /// 防止MenuStrip出现系统默认渐变。
            /// </summary>
            public override Color MenuStripGradientEnd
            {
                get
                {
                    return UITheme.Card;
                }
            }


            /// <summary>
            /// 下拉菜单整体背景颜色。
            ///
            /// 例如:
            ///
            /// 工程
            ///   ├ 新建工程
            ///   ├ 打开工程
            ///   └ 退出
            ///
            /// 整个下拉区域使用UITheme.Card。
            /// </summary>
            public override Color ToolStripDropDownBackground
            {
                get
                {
                    return UITheme.Card;
                }
            }


            /// <summary>
            /// 下拉菜单左侧图片区域背景。
            ///
            /// 即MenuItem左侧预留的:
            ///
            /// Icon
            /// Image
            ///
            /// 区域。
            ///
            /// 当前统一使用Card背景。
            /// </summary>
            public override Color ImageMarginGradientBegin
            {
                get
                {
                    return UITheme.Card;
                }
            }


            /// <summary>
            /// 下拉菜单图片区域中间背景。
            ///
            /// 与其它ImageMargin颜色保持一致，
            /// 避免出现渐变。
            /// </summary>
            public override Color ImageMarginGradientMiddle
            {
                get
                {
                    return UITheme.Card;
                }
            }


            /// <summary>
            /// 下拉菜单图片区域结束背景。
            ///
            /// 与其它ImageMargin颜色保持一致，
            /// 保持整体菜单视觉统一。
            /// </summary>
            public override Color ImageMarginGradientEnd
            {
                get
                {
                    return UITheme.Card;
                }
            }

        }

        #endregion

    }
}
