using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Component
{

    /// <summary>
    /// GT.Factory 工业精工级 TabControl 控件（子类版）
    ///
    /// 彻底解决：
    ///   1. 右侧留白         → 自绘补齐
    ///   2. Tab 间分隔线     → UserPaint 接管，系统不再绘制
    ///   3. 内容区外边框     → UserPaint 接管，系统不再绘制
    ///   4. header 底部亮线  → 自绘覆盖
    ///   5. 顶部 2px 偏移    → fullRect 从 Y=0 强制撑满
    /// </summary>
    public class GTTabControl : TabControl
    {
        // ── 构造 ─────────────────────────────────────────────────────────────

        public GTTabControl()
        {
            // 接管全部绘制，系统边框 / 分隔线 / 亮线 全部消失
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            DrawMode = TabDrawMode.OwnerDrawFixed;
            ItemSize = new Size(126, 34);
            SizeMode = TabSizeMode.Fixed;
            Font = UITheme.BodyFont;
            Padding = new Point(0, 0); // 消除系统对 e.Bounds 的额外偏移
        }

        // ── 核心绘制入口 ─────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            // 清底：整个控件填 BgBase，从根上消灭所有系统残留色
            e.Graphics.Clear(UITheme.BgBase);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (TabPages.Count == 0) return;

            DrawTabHeaders(e.Graphics);
            DrawContentArea(e.Graphics);
        }

        // ── 绘制 Tab 标签头 ──────────────────────────────────────────────────

        private void DrawTabHeaders(Graphics g)
        {
            int headerH = ItemSize.Height;

            for (int i = 0; i < TabPages.Count; i++)
            {
                TabPage page = TabPages[i];
                bool selected = (SelectedIndex == i);

                // GetTabRect 返回的 Rect 在 Y 方向可能有系统偏移
                // 强制 Y=0，Height=ItemSize.Height，完全撑满 header 行
                Rectangle tabRect = GetTabRect(i);
                Rectangle fullRect = new Rectangle(
                    tabRect.X, 0, tabRect.Width, headerH
                );

                // ── 背景 ────────────────────────────────────────────────────
                Color bgColor = selected ? UITheme.BgCard : UITheme.BgInput;
                using (var bgBrush = new SolidBrush(bgColor))
                    g.FillRectangle(bgBrush, fullRect);

                // ── 选中：顶部 2px Accent 指示条 ────────────────────────────
                if (selected)
                {
                    using (var ab = new SolidBrush(UITheme.Accent))
                        g.FillRectangle(ab, fullRect.X, 0, fullRect.Width, 2);
                }

                // ── 文字 ────────────────────────────────────────────────────
                Color textColor = selected ? UITheme.TextMain : UITheme.TextMuted;
                using (var tb = new SolidBrush(textColor))
                using (var fmt = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                })
                {
                    g.DrawString(page.Text, UITheme.BodyFont, tb, fullRect, fmt);
                }
            }

            // ── 最后一个 Tab 右侧剩余区域 → BgBase 填满 ───────────────────
            Rectangle lastRect = GetTabRect(TabPages.Count - 1);
            int remainX = lastRect.Right;
            int remainW = Width - remainX;
            if (remainW > 0)
            {
                using (var bb = new SolidBrush(UITheme.BgBase))
                    g.FillRectangle(bb, remainX, 0, remainW, headerH);
            }
        }

        // ── 绘制内容区（无边框，纯色填充）───────────────────────────────────

        private void DrawContentArea(Graphics g)
        {
            int headerH = ItemSize.Height;
            var contentRect = new Rectangle(0, headerH, Width, Height - headerH);

            // 无边框，直接填充内容区背景色
            using (var brush = new SolidBrush(UITheme.BgCard))
                g.FillRectangle(brush, contentRect);
        }

        // ── 子页背景色同步（防止子页自己画出白色边框）───────────────────────

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is TabPage page)
            {
                page.BackColor = UITheme.BgBase;
                page.ForeColor = UITheme.Text;
                page.Font = UITheme.BodyFont;
                page.Padding = new Padding(8);
            }
        }
    }
}