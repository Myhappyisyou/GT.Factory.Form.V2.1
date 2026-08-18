using GT.Factory.UI.Component;
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
    /// 输入控件样式工厂（TextBox / ComboBox / DateTime）
    /// </summary>
    public static class UIInput
    {
        /// <summary>
        /// 文本输入框
        /// </summary>
        public static TextBox Text()
        {
            return new TextBox
            {
                AutoSize = true,
                Height = UITheme.ControlHeight,
                BackColor = UITheme.Input,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UITheme.FontNormal
            };
        }

        /// <summary>
        /// 下拉框
        /// </summary>
        public static ComboBox Combo()
        {
            return new ComboBox
            {
                AutoSize = true,
                Height = UITheme.ControlHeight,
                BackColor = UITheme.Input,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UITheme.FontNormal,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        /// <summary>
        /// 日期时间选择
        /// </summary>
        public static DateTimePicker DateTime()
        {
            return new DateTimePicker
            {
                AutoSize = true,
                Height = UITheme.ControlHeight,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm:ss",
                Font = UITheme.FontNormal
            };
        }

        /// <summary>
        /// 密码输入
        /// </summary>
        public static UIPasswordInput Password()
        {
            return new UIPasswordInput();
        }
    }
}
