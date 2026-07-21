using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Helper.Logging
{
    public class RichTextBoxLogDisplay : ILogDisplay
    {
        private readonly RichTextBox _richTextBox;
        private static readonly object _rtbLock = new object();
        private int _logCount = 0;

        public RichTextBoxLogDisplay(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        public void Show(LogLevel level, string message)
        {
            // UI日志
          
                Color color;
                switch (level)
                {
                    case LogLevel.Info:
                        color = Color.Black;
                        break;
                    case LogLevel.Warning:
                        color = Color.Blue;
                        break;
                    case LogLevel.Error:
                        color = Color.Red;
                        break;
                    default:
                        color = Color.Black;
                        break;
                }

                DispColorText(_richTextBox, message, color);
        }

        private  void DispColorText(RichTextBox rtbox, string dispText, Color color, int maxLines = 5000, int linesToKeep = 4900)
        {
            if (rtbox == null || rtbox.IsDisposed) return;

            lock (_rtbLock)
            {
                try
                {
                    if (rtbox.InvokeRequired)
                    {
                        rtbox.BeginInvoke((Action)(() => SafeAppendText(rtbox, dispText, color, maxLines, linesToKeep)));
                    }
                    else
                    {
                        SafeAppendText(rtbox, dispText, color, maxLines, linesToKeep);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"日志显示错误: {ex.Message}");
                }
            }
        }

        private  void SafeAppendText(RichTextBox rtbox, string text, Color color, int maxLines, int linesToKeep)
        {
            _logCount++;
            // 检查是否在查看最新内容
            bool wasAtBottom = IsScrollAtBottom(rtbox);

            // 清理旧日志
            if (_logCount % 50 == 0 && rtbox.Lines.Length > maxLines)
            {
                rtbox.SuspendLayout();
                try
                {
                    int removeCount = rtbox.Lines.Length - linesToKeep;
                    int charsToRemove = rtbox.GetFirstCharIndexFromLine(removeCount);
                    rtbox.Select(0, charsToRemove);
                    rtbox.SelectedText = string.Empty;
                }
                finally
                {
                    rtbox.ResumeLayout();
                }
            }

            //_logCount++;
            //if (_logCount % 50 == 0 && !_richTextBox.IsDisposed)
            //{
            //    if (_richTextBox.TextLength > 500_000) // 保留最后 400_000 字符
            //    {
            //        _richTextBox.Select(0, _richTextBox.TextLength - 400_000);
            //        _richTextBox.SelectedText = string.Empty;
            //    }
            //}

            // 添加新日志
            rtbox.SelectionColor = color;
            rtbox.AppendText($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} : {text}\r\n");

            // 自动滚动到底部（如果之前已经在底部）
            if (wasAtBottom)
            {
                rtbox.SelectionStart = rtbox.Text.Length;
                rtbox.ScrollToCaret();
            }
        }

        private  bool IsScrollAtBottom(RichTextBox rtbox)
        {
            if (rtbox.TextLength == 0) return true;

            // 获取最后可见字符的位置
            Point bottomPoint = new Point(1, rtbox.ClientSize.Height - 1);
            int lastVisibleCharIndex = rtbox.GetCharIndexFromPosition(bottomPoint);

            // 判断是否已经显示到最后
            return lastVisibleCharIndex >= rtbox.TextLength - 1;
        }

    }

}
