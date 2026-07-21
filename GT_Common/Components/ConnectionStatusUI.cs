using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Components
{
    public enum IndicatorStatus
    {
        Normal,
        Warning,
        Error,
        Disabled
    }
    [DefaultProperty("Status")]
    public partial class ConnectionStatusUI : UserControl
    {
        private IndicatorStatus _status = IndicatorStatus.Normal;
        private string _labelText = "状态";
        private Color _normalColor = Color.LimeGreen;
        private Color _warningColor = Color.Orange;
        private Color _errorColor = Color.Red;
        private Color _disabledColor = Color.Gray;
        private int _indicatorSize = 30;
        private int _spacing = 0;
        private bool _showGlow = true;
        private bool _showHighlight = true;
        private bool _showStatusText = true;

        public ConnectionStatusUI()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);

            this.Size = new Size(40, 60);
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            this.ForeColor = Color.Black;
            this.BackColor = Color.Transparent;
            this.DoubleBuffered = true;
            this.Padding = new Padding(0 ,0, 0, 0);
            //this.Padding = new Padding(10, 15, 10, 15);
        }

        [Category("外观")]
        [Description("指示灯当前状态")]
        public IndicatorStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                Invalidate();
            }
        }

        [Category("外观")]
        [Description("标签显示的文本")]
        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                Invalidate();
            }
        }

        [Category("外观")]
        [Description("正常状态指示灯颜色")]
        public Color NormalColor
        {
            get => _normalColor;
            set
            {
                _normalColor = value;
                Invalidate();
            }
        }

        [Category("外观")]
        [Description("警告状态指示灯颜色")]
        public Color WarningColor
        {
            get => _warningColor;
            set
            {
                _warningColor = value;
                Invalidate();
            }
        }

        [Category("外观")]
        [Description("错误状态指示灯颜色")]
        public Color ErrorColor
        {
            get => _errorColor;
            set
            {
                _errorColor = value;
                Invalidate();
            }
        }

        [Category("外观")]
        [Description("禁用状态指示灯颜色")]
        public Color DisabledColor
        {
            get => _disabledColor;
            set
            {
                _disabledColor = value;
                Invalidate();
            }
        }

        [Category("布局")]
        [Description("指示灯直径大小")]
        public int IndicatorSize
        {
            get => _indicatorSize;
            set
            {
                _indicatorSize = Math.Max(20, value);
                Invalidate();
            }
        }

        [Category("布局")]
        [Description("指示灯与标签之间的间距")]
        public int Spacing
        {
            get => _spacing;
            set
            {
                _spacing = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("效果")]
        [Description("是否显示发光效果")]
        public bool ShowGlow
        {
            get => _showGlow;
            set
            {
                _showGlow = value;
                Invalidate();
            }
        }

        [Category("效果")]
        [Description("是否显示高光效果")]
        public bool ShowHighlight
        {
            get => _showHighlight;
            set
            {
                _showHighlight = value;
                Invalidate();
            }
        }

        [Category("效果")]
        [Description("是否显示状态文本")]
        public bool ShowStatusText
        {
            get => _showStatusText;
            set
            {
                _showStatusText = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 获取当前状态颜色
            Color statusColor = GetStatusColor();

            // 计算指示灯位置（垂直居中在控件上半部分）
            int indicatorX = (Width - _indicatorSize) / 2;
            int indicatorY = Padding.Top;
            Rectangle indicatorRect = new Rectangle(indicatorX, indicatorY, _indicatorSize, _indicatorSize);

            // 绘制发光效果
            if (_showGlow && _status != IndicatorStatus.Disabled)
            {
                using (GraphicsPath glowPath = new GraphicsPath())
                {
                    glowPath.AddEllipse(indicatorRect);
                    using (PathGradientBrush glowBrush = new PathGradientBrush(glowPath))
                    {
                        glowBrush.CenterColor = Color.FromArgb(100, statusColor);
                        glowBrush.SurroundColors = new Color[] { Color.Transparent };
                        g.FillEllipse(glowBrush,
                            indicatorRect.X - 10, indicatorRect.Y - 10,
                            indicatorRect.Width + 20, indicatorRect.Height + 20);
                    }
                }
            }

            // 绘制指示灯外圈
            using (Pen borderPen = new Pen(ControlPaint.Dark(statusColor, 0.3f), 2))
            {
                g.DrawEllipse(borderPen, indicatorRect);
            }

            // 绘制指示灯主体
            using (SolidBrush indicatorBrush = new SolidBrush(statusColor))
            {
                g.FillEllipse(indicatorBrush, indicatorRect);
            }

            // 添加高光效果
            if (_showHighlight)
            {
                Rectangle highlightRect = new Rectangle(
                    indicatorRect.X + indicatorRect.Width / 4,
                    indicatorRect.Y + indicatorRect.Height / 4,
                    indicatorRect.Width / 4,
                    indicatorRect.Height / 4);

                using (SolidBrush highlightBrush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.FillEllipse(highlightBrush, highlightRect);
                }
            }

            // 绘制标签文本（在指示灯下方）
            using (SolidBrush textBrush = new SolidBrush(ForeColor))
            {
                SizeF textSize = g.MeasureString(_labelText, Font);
                int textX = (Width - (int)textSize.Width) / 2;
                int textY = indicatorRect.Bottom + _spacing + 2;

                g.DrawString(_labelText, Font, textBrush, textX, textY);
            }

            //// 绘制状态文本（在标签文本下方）
            //if (_showStatusText)
            //{
            //    string statusText = GetStatusText();
            //    using (SolidBrush statusBrush = new SolidBrush(ForeColor))
            //    {
            //        SizeF statusSize = g.MeasureString(statusText, Font);
            //        int statusX = (Width - (int)statusSize.Width) / 2;
            //        int statusY = indicatorRect.Bottom + _spacing + (int)g.MeasureString(_labelText, Font).Height -10;

            //        g.DrawString(statusText, Font, statusBrush, statusX, statusY);
            //    }
            //}
        }

        private Color GetStatusColor()
        {
            switch (_status)
            {
                case IndicatorStatus.Normal: return _normalColor;
                case IndicatorStatus.Warning: return _warningColor;
                case IndicatorStatus.Error: return _errorColor;
                case IndicatorStatus.Disabled: return _disabledColor;
                default: return _normalColor;
            }
        }

        private string GetStatusText()
        {
            switch (_status)
            {
                case IndicatorStatus.Normal: return "正常";
                case IndicatorStatus.Warning: return "警告";
                case IndicatorStatus.Error: return "错误";
                case IndicatorStatus.Disabled: return "停用";
                default: return "未知";
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
