using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Style
{
    /// <summary>
    /// 按钮样式工厂
    /// 所有按钮必须从这里创建，禁止直接 new Button
    /// </summary>
    public static class UIButton
    {
        /// <summary>
        /// 主按钮（蓝色）
        /// 适用场景：登录、查询、确认、保存
        /// </summary>
        public static Button Primary(string text)
        {
            var btn = new Button
            {
                Text = text,
                Height = UITheme.ControlHeight,
                Width = 100,
                BackColor = UITheme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UITheme.FontNormal,
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;

            // 鼠标悬浮效果
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 180, 255);

            return btn;
        }

        /// <summary>
        /// 默认按钮（灰色）
        /// </summary>
        public static Button Default(string text)
        {
            var btn = Primary(text);

            btn.BackColor = UITheme.Card;
            btn.FlatAppearance.BorderColor = UITheme.Border;
            btn.FlatAppearance.BorderSize = 1;

            return btn;
        }

        /// <summary>
        /// 渲染次级/幽灵按钮 (暗底色 + 高亮边框)
        /// 适用场景：取消、上一页、重置等非主流程操作
        /// </summary>
        /// <param name="btn">目标按钮控件</param>
        public static Button Ghost(string text)
        {
            var btn = Primary(text);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;              // 幽灵按钮保留 1px 细线边框
            btn.FlatAppearance.BorderColor = UITheme.CyanGlow; // 边框使用荧光青色
            btn.BackColor = UITheme.BgInput;        // 背景色与输入框一致（暗色）
            btn.ForeColor = UITheme.CyanGlow;
            btn.Font = UITheme.BodyFont;
            btn.Cursor = Cursors.Hand;

            // 悬停交互：鼠标放上去时背景亮起，文字变暗（反色效果），提升手感
            btn.FlatAppearance.MouseOverBackColor = UITheme.CyanGlow;
            btn.MouseDown += (s, e) => { btn.ForeColor = UITheme.BgBase; };
            btn.MouseLeave += (s, e) => { btn.ForeColor = UITheme.CyanGlow; };

            return btn;
        }
    }
}
