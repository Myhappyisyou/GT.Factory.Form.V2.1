using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Layout
{
    public static class UILayout
    {
        /// <summary>
        /// 创建一行表单（左Label + 右输入）
        /// </summary>
        public static Panel FormRow(Control label, Control input)
        {
            var panel = new Panel
            {
                Height = UITheme.ControlHeight + 6,
                Dock = DockStyle.Top
            };

            // Label 左侧
            label.Location = new Point(0, 3);

            // 输入框右侧
            input.Location = new Point(label.Width + UITheme.ControlSpacing, 3);

            panel.Controls.Add(label);
            panel.Controls.Add(input);

            return panel;
        }

        /// <summary>
        /// 按钮横向布局
        /// </summary>
        public static FlowLayoutPanel ButtonBar(params Control[] buttons)
        {
            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            foreach (var btn in buttons)
                panel.Controls.Add(btn);

            return panel;
        }

        /// <summary>
        /// 分页控件布局（上一页 / 页码 / 下一页）
        /// </summary>
        public static FlowLayoutPanel Paging(Control prev, Control page, Control next)
        {
            var panel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            // 统一高度
            prev.Height = UITheme.ControlHeight;
            next.Height = UITheme.ControlHeight;

            page.Height = UITheme.ControlHeight;
            page.Width = 140;

            // 仅当 page 为 Label 或 Button 时设置 TextAlign
            if (page is Label label)
            {
                label.TextAlign = ContentAlignment.MiddleCenter;
            }
            else if (page is Button button)
            {
                button.TextAlign = ContentAlignment.MiddleCenter;
            }

            panel.Controls.Add(prev);
            panel.Controls.Add(page);
            panel.Controls.Add(next);

            return panel;
        }


        /// <summary>
        /// 魔法 1：开启双缓冲防闪烁 (Double Buffer)
        /// 原理：通过反射强行打开底层绘制缓存，彻底解决 DataGridView 滚动时的大面积闪烁和 Panel 重绘白屏。
        /// </summary>
        /// <param name="control">需要防闪烁的控件</param>
        public static void EnableDoubleBuffer(this Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, control, new object[] { true });
        }


        // 引入 Win32 API 控制底层重绘信号
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(System.IntPtr hWnd, int wMsg, bool wParam, int lParam);
        private const int WM_SETREDRAW = 11;

        /// <summary>
        /// 魔法 2 (起)：挂起绘制 (Suspend Drawing)
        /// 场景：当界面需要动态循环 Add 添加几十上百个控件时调用。
        /// 原理：直接拦截操作系统的 WM_SETREDRAW 消息，冻结界面，防止布局引擎每添加一个控件就计算一次。性能提升极大！
        /// </summary>
        public static void SuspendDrawing(this Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }

        /// <summary>
        /// 魔法 2 (承)：恢复绘制 (Resume Drawing)
        /// 场景：控件添加完毕后调用，界面瞬间渲染完成。
        /// </summary>
        public static void ResumeDrawing(this Control parent)
        {
            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }

    }
}
