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
    /// 表格统一样式
    /// </summary>
    public static class UIGrid
    {
        /// <summary>
        /// 应用工业主题到 DataGridView
        /// </summary>
        public static void Default(DataGridView dgv)
        {
            dgv.BackgroundColor = UITheme.Card;
            dgv.GridColor = UITheme.Border;

            dgv.EnableHeadersVisualStyles = false;

            // 表头
            dgv.ColumnHeadersDefaultCellStyle.BackColor = UITheme.Card;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = UITheme.Text;
            dgv.ColumnHeadersDefaultCellStyle.Font = UITheme.FontNormal;

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 行
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 40, 60);
            dgv.DefaultCellStyle.ForeColor = UITheme.Text;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 45, 65);

            // 选中
            dgv.DefaultCellStyle.SelectionBackColor = UITheme.Primary;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.RowTemplate.Height = 32;
        }


        /// <summary>
        /// 渲染工业赛博风格数据表 (DataGridView)
        /// 特点：去除了所有原生丑陋的高光和边框，提供沉浸式暗黑网格
        /// </summary>
        /// <param name="dgv">目标 DataGridView 控件</param>
        public static void BuildDataGrid(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;          // 必须设为 false，否则无法修改表头颜色
            dgv.BorderStyle = BorderStyle.None;
            dgv.BackgroundColor = UITheme.BgBase;   // 大背景融于底色
            dgv.GridColor = UITheme.BgInput;        // 网格线比底色稍微亮一点，柔和分割

            // 1. 表头专属样式
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = UITheme.BgCard;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = UITheme.TextMuted;
            dgv.ColumnHeadersDefaultCellStyle.Font = UITheme.SmallFont;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.ColumnHeadersHeight = 40;

            // 2. 数据行通用样式
            dgv.DefaultCellStyle.BackColor = UITheme.BgBase;
            dgv.DefaultCellStyle.ForeColor = UITheme.TextMain;
            dgv.DefaultCellStyle.SelectionBackColor = UITheme.BgInput; // 选中时不瞎眼
            dgv.DefaultCellStyle.SelectionForeColor = UITheme.CyanGlow;
            dgv.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0); // 让文字不要紧贴边线

            // 3. 斑马线交替颜色，提升长表格的可读性
            dgv.AlternatingRowsDefaultCellStyle.BackColor = UITheme.BgCard;
            dgv.RowTemplate.Height = 40; // 触摸屏环境中，行高必须大于等于 40px 以防误触
        }



        public static void Trace(DataGridView dgv)
        {

            BuildDataGrid(dgv);


            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;


            dgv.AllowUserToResizeColumns = true;


            dgv.Columns["Time"].Width = 160;

            dgv.Columns["Direction"].Width = 70;

            dgv.Columns["Id"].Width = 50;

            dgv.Columns["Frame"].Width = 150;

            dgv.Columns["Data"].Width = 220;

            dgv.Columns["Checksum"].Width = 80;

        }
    }
}
