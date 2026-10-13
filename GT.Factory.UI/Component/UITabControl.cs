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
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>
    /// GT.Factory 工业精工级 TabControl 控件。
    /// 解决痛点：
    /// 1. 最后一个 Tab 右侧不再有 Windows 系统白色突兀填充；
    /// 2. 选中/未选中标题均撑满 e.Bounds，无任何上下留白，风格统一紧凑；
    /// 3. 选中标题顶部 2px Accent 指示条区分当前页，未选中用 BgInput 底色区分。
    /// </summary>
    public static class UITabControl
    {
        #region 创建

        public static GTTabControl Create() => new GTTabControl(); // ✅ 返回子类


        public static TabControl Create(DockStyle dock)
        {
            var tab = Create();
            tab.Dock = dock;
            return tab;
        }

        #endregion

        #region Tab页

        public static TabPage AddTab(TabControl tab, string text)
        {
            if (tab == null)
                throw new ArgumentNullException(nameof(tab));

            var page = new TabPage(text);
            ApplyPageStyle(page);
            tab.TabPages.Add(page);
            return page;
        }

        public static void Select(TabControl tab, TabPage page)
        {
            if (tab == null || page == null)
                return;

            tab.SelectedTab = page;
        }

        #endregion

        #region 样式

        public static void ApplyStyle(TabControl tab)
        {
            if (tab == null)
                throw new ArgumentNullException(nameof(tab));

            tab.Dock = DockStyle.Fill;
            tab.Font = UITheme.BodyFont;
            tab.BackColor = UITheme.BgBase;
            tab.ForeColor = UITheme.Text;
            tab.DrawMode = TabDrawMode.OwnerDrawFixed;
            tab.ItemSize = new Size(126, 34);
            tab.SizeMode = TabSizeMode.Fixed;

            tab.DrawItem += Tab_DrawItem;
        }

        private static void ApplyPageStyle(TabPage page)
        {
            page.BackColor = UITheme.BgBase;
            page.ForeColor = UITheme.Text;
            page.Font = UITheme.BodyFont;
            page.Padding = new Padding(8);
        }

        #endregion

        #region 绘制 (工业留白自然美学核心)

        private static void Tab_DrawItem(object sender, DrawItemEventArgs e)
        {
            var tab = sender as TabControl;
            if (tab == null) return;

            var page = tab.TabPages[e.Index];
            Rectangle raw = e.Bounds;
            bool selected = (tab.SelectedIndex == e.Index);

            // ✅ 强制从 Y=0 撑满整个 header，消灭系统偏移留白
            Rectangle fullRaw = new Rectangle(
                raw.X, 0, raw.Width, tab.ItemSize.Height
            );

            // 背景填充 —— 用 fullRaw，从顶到底无缝隙
            Color backColor = selected ? UITheme.BgCard : UITheme.BgInput;
            using (var brush = new SolidBrush(backColor))
                e.Graphics.FillRectangle(brush, fullRaw);

            // 选中指示条
            //if (selected)
            //{
            //    using (var accentBrush = new SolidBrush(UITheme.Accent))
            //        e.Graphics.FillRectangle(accentBrush, fullRaw.X, 0, fullRaw.Width, 0);
            //}

            // 文字
            Color textColor = selected ? UITheme.TextMain : UITheme.TextMuted;
            using (var textBrush = new SolidBrush(textColor))
            using (var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            })
                e.Graphics.DrawString(page.Text, UITheme.BodyFont, textBrush, fullRaw, format);

            // ✅ 右侧补齐 —— 从 Y=0 开始，高度用 ItemSize.Height
            if (e.Index == tab.TabPages.Count - 1)
            {
                int remainLeft = fullRaw.Right;
                int remainWidth = tab.Width - remainLeft;
                if (remainWidth > 0)
                {
                    Rectangle remainRect = new Rectangle(
                        remainLeft,
                        0,                    // ✅ 从顶部开始
                        remainWidth,
                        tab.ItemSize.Height   // ✅ 固定满高度
                    );
                    using (var baseBrush = new SolidBrush(UITheme.BgBase))
                        e.Graphics.FillRectangle(baseBrush, remainRect);
                }
            }
        }
        #endregion
    }
}

//namespace GT.Factory.UI.Component
 //{

//    /// <summary>
//    /// GT.Factory统一Tab控件。
//    ///
//    /// 用于:
//    ///
//    /// 1. 创建标准TabControl
//    /// 2. 创建TabPage
//    /// 3. 添加页面
//    /// 4. 统一工业UI样式
//    ///
//    ///
//    /// 设计原则:
//    ///
//    /// GT.Factory.UI
//    ///       |
//    ///       +-- 负责Tab视觉规范
//    ///
//    /// 业务项目
//    ///       |
//    ///       +-- 负责页面内容
//    ///
//    ///
//    /// 示例:
//    ///
//    /// var tabs =
//    ///     UITabControl.Create();
//    ///
//    /// var frame =
//    ///     UITabControl.AddTab(
//    ///         tabs,
//    ///         "Frame");
//    ///
//    /// frame.Controls.Add(
//    ///     frameControl);
//    ///
//    /// </summary>
//    public static class UITabControl
//    {

//        #region 创建

//        /// <summary>
//        /// 创建标准TabControl。
//        ///
//        /// 自动应用GT.Factory统一样式。
//        ///
//        /// </summary>
//        public static TabControl Create()
//        {

//            var tab =new TabControl();

//            ApplyStyle(tab);

//            return tab;

//        }

//        /// <summary>
//        /// 创建TabControl并设置Dock。
//        ///
//        /// </summary>
//        public static TabControl Create(DockStyle dock)
//        {

//            var tab =Create();

//            tab.Dock =dock;

//            return tab;
//        }

//        #endregion

//        #region Tab页

//        /// <summary>
//        /// 添加Tab页面。
//        ///
//        /// </summary>
//        public static TabPage AddTab(TabControl tab,string text)
//        {

//            if (tab == null)
//            {
//                throw new ArgumentNullException(nameof(tab));
//            }

//            var page =new TabPage(text);

//            ApplyPageStyle(page);

//            tab.TabPages.Add(page);

//            return page;

//        }

//        /// <summary>
//        /// 根据名称选择Tab。
//        ///
//        /// </summary>
//        public static void Select(TabControl tab,TabPage page)
//        {

//            if (tab == null ||page == null)
//            {
//                return;
//            }

//            tab.SelectedTab = page;

//        }

//        #endregion

//        #region 样式

//        /// <summary>
//        /// 应用TabControl统一样式。
//        ///
//        /// </summary>
//        public static void ApplyStyle(TabControl tab)
//        {
//            if (tab == null)
//                throw new ArgumentNullException(nameof(tab));

//            tab.Dock = DockStyle.Fill;
//            tab.Font = UITheme.BodyFont;
//            tab.BackColor = UITheme.BgBase;
//            tab.ForeColor = UITheme.Text;
//            tab.DrawMode = TabDrawMode.OwnerDrawFixed;
//            tab.ItemSize = new Size(120, 32);
//            tab.SizeMode = TabSizeMode.Fixed;

//            tab.DrawItem += Tab_DrawItem;
//        }


//        /// <summary>
//        /// TabPage统一样式。
//        ///
//        /// </summary>
//        private static void ApplyPageStyle(TabPage page)
//        {

//            page.BackColor =UITheme.BgBase;

//            page.ForeColor =UITheme.Text;

//            page.Font =UITheme.BodyFont;

//            page.Padding =new Padding(5);

//        }

//        #endregion

//        #region 绘制

//        /// <summary>
//        /// 自定义绘制Tab标题。
//        ///
//        /// 避免Windows默认蓝色Tab。
//        ///
//        /// </summary>
//        private static void Tab_DrawItem(object sender, DrawItemEventArgs e)
//        {
//            var tab = sender as TabControl;
//            if (tab == null) return;

//            var page = tab.TabPages[e.Index];
//            Rectangle rect = e.Bounds;
//            bool selected = (tab.SelectedIndex == e.Index);

//            // ── 1. 绘制当前标签背景 ──────────────────────────────────────
//            Color back = selected ? UITheme.BgCard : UITheme.BgInput;
//            using (var brush = new SolidBrush(back))
//                e.Graphics.FillRectangle(brush, rect);

//            // ── 2. 绘制当前标签文字 ──────────────────────────────────────
//            Color textColor = selected ? UITheme.TextMain : UITheme.TextMuted;
//            using (var brush = new SolidBrush(textColor))
//            {
//                var format = new StringFormat
//                {
//                    Alignment = StringAlignment.Center,
//                    LineAlignment = StringAlignment.Center
//                };
//                e.Graphics.DrawString(page.Text, UITheme.BodyFont, brush, rect, format);
//            }

//            // ── 3. 最后一个标签绘制完后，填充右侧剩余空白区域 ────────────
//            //
//            //  标签栏布局示意：
//            //  ┌──────┬──────┬──────┬──────────────────────────┐
//            //  │ Tab0 │ Tab1 │ Tab2 │  ← 这里是白色剩余区域     │
//            //  └──────┴──────┴──────┴──────────────────────────┘
//            //
//            //  当 e.Index 是最后一个标签时，计算右侧剩余宽度并用 BgBase 填充。
//            //
//            if (e.Index == tab.TabPages.Count - 1)
//            {
//                // 所有标签右边缘 X 坐标
//                int lastTabRight = rect.Right;

//                // 标签栏总宽度（控件宽度）
//                int controlWidth = tab.Width;

//                // 剩余区域：从最后一个标签右侧 → 控件右边缘
//                if (lastTabRight < controlWidth)
//                {
//                    Rectangle remainRect = new Rectangle(
//                        lastTabRight,           // X：紧接最后一个标签
//                        rect.Top,               // Y：与标签同高
//                        controlWidth - lastTabRight,  // 宽度：剩余空间
//                        rect.Height             // 高度：与标签相同
//                    );

//                    using (var brush = new SolidBrush(UITheme.BgBase))
//                        e.Graphics.FillRectangle(brush, remainRect);
//                }
//            }
//        }
//        #endregion


//    }

//}