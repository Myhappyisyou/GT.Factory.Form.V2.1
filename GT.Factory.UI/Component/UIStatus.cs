using GT.Factory.UI.Theme;
using System;

using System.Drawing;

namespace GT.Factory.UI.Component
{
    /// <summary>
    /// 通用状态显示工具。
    ///
    /// 用于工业界面中常见的：
    ///     True  = 正常 / 有效 / 开启
    ///     False = 异常 / 无效 / 关闭
    ///
    /// UI 层统一负责状态图标的表现形式，
    /// 业务项目不再直接依赖具体图片资源。
    /// </summary>
    public static class UIStatus
    {
        private static readonly Image _greenImage = CreateStatusImage(UITheme.Success);
        private static readonly Image _redImage = CreateStatusImage(UITheme.Danger);

        /// <summary>
        /// 根据状态获取对应状态图片。
        /// </summary>
        /// <param name="status">True 表示正常，False 表示异常。</param>
        /// <returns>对应的状态图片。</returns>
        public static Image GetImage(bool status)
        {
            return status ? _greenImage : _redImage;
        }

        /// <summary>
        /// 创建统一尺寸的圆形状态指示灯。
        /// </summary>
        private static Image CreateStatusImage(Color color)
        {
            Bitmap bitmap = new Bitmap(16, 16);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (Brush brush = new SolidBrush(color))
                {
                    graphics.FillEllipse(brush, 1, 1, 14, 14);
                }

                using (Pen pen = new Pen(Color.DarkGray, 1))
                {
                    graphics.DrawEllipse(pen, 1, 1, 14, 14);
                }
            }

            return bitmap;
        }
    }
}