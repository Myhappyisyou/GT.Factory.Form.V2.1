using GT.Factory.UI.Style;
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
    /// <summary>
    /// 分页组件（稳定版）
    /// </summary>
    public class UIPaging : Panel
    {
        public Button BtnPrev { get; }
        public Button BtnNext { get; }
        private Label lblPage;

        public UIPaging()
        {
            this.Height = UITheme.ControlHeight;
            this.Width = 360; // ✅ 稍微加宽一点更好看

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3
            };

            // ✅ 三等分
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));

            BtnPrev = UIButton.Default("上一页");
            BtnNext = UIButton.Default("下一页");
            lblPage = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = UITheme.Text,
                Font = UITheme.FontNormal
            };

            // ✅ 正确布局
            BtnPrev.Dock = DockStyle.Fill;
            BtnNext.Dock = DockStyle.Fill;

            // ✅ 加入布局
            table.Controls.Add(BtnPrev, 0, 0);
            table.Controls.Add(lblPage, 1, 0);
            table.Controls.Add(BtnNext, 2, 0);

            this.Controls.Add(table);
        }

        public void Update(int current, int total)
        {
            lblPage.Text = $"第 {current} / {total} 页";
        }
    }
}
