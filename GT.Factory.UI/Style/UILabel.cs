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
    /// 标签样式工厂
    /// </summary>
    public static class UILabel
    {
        /// <summary>
        /// 表单标签（右对齐）
        /// </summary>
        public static Label FormLabel(string text, int width = 90)
        {
            return new Label
            {
                Text = text,
                Width = width,
                Height = UITheme.ControlHeight,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = UITheme.Text,
                Font = UITheme.FontNormal
            };
        }

        /// <summary>
        /// 普通显示文本
        /// </summary>
        public static Label ValueLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = false,
                Height = UITheme.ControlHeight,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = UITheme.Text,
                Font = UITheme.FontNormal
            };
        }



        public static Label Title(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = UITheme.TextMain,
                Font = UITheme.HeaderFont
            };
        }
    }
}
