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
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = UITheme.TextMain;
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



        /// <summary>
        /// 应用管理类表格样式。
        ///
        /// 用于产品、产品部件、产品码规则、参数配置、设备配置等
        /// 管理型页面。
        ///
        /// 与 Default() 的区别：
        /// 1. 管理表格默认只读。
        /// 2. 禁止直接新增、删除行。
        /// 3. 默认整行选择。
        /// 4. 默认居中显示。
        /// 5. 保持工业浅色主题。
        ///
        /// 注意：
        /// Management() 只负责 DataGridView 的通用行为和样式，
        /// 不负责数据绑定、查询、分页以及业务按钮。
        /// </summary>
        /// <param name="dgv">目标 DataGridView 控件</param>
        public static void Management(DataGridView dgv)
        {
            if (dgv == null)
            {
                return;
            }

            // ============================================================
            // 基础行为
            // ============================================================

            // 管理页面不允许直接在最后一行新增数据。
            dgv.AllowUserToAddRows = false;

            // 管理页面不允许直接删除数据。
            // 删除操作由页面按钮或业务服务完成。
            dgv.AllowUserToDeleteRows = false;

            // 不允许调整行高，保持表格统一。
            dgv.AllowUserToResizeRows = false;

            // 允许用户调整列宽。
            dgv.AllowUserToResizeColumns = true;

            // 禁止用户拖动列改变字段顺序。
            // 字段顺序由页面代码控制。
            dgv.AllowUserToOrderColumns = false;

            // 管理页面以“记录”为基本操作单位。
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 默认只允许选择一条记录。
            dgv.MultiSelect = false;

            // 管理列表默认只读。
            // 编辑应该通过独立编辑区域或编辑窗口完成。
            dgv.ReadOnly = true;

            // 不需要显示 DataGridView 左侧行号区域。
            dgv.RowHeadersVisible = false;


            // ============================================================
            // 基础外观
            // ============================================================

            // 使用当前 GT.Factory UITheme，而不是重新定义颜色。
            dgv.BackgroundColor = UITheme.Card;
            dgv.GridColor = UITheme.Border;

            // 关闭系统默认表头样式。
            dgv.EnableHeadersVisualStyles = false;

            // 使用统一的工业主题。
            dgv.BorderStyle = BorderStyle.FixedSingle;

            // ============================================================
            // 表头
            // ============================================================

            dgv.ColumnHeadersBorderStyle =
                DataGridViewHeaderBorderStyle.Single;

            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                UITheme.Card;

            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                UITheme.Text;

            dgv.ColumnHeadersDefaultCellStyle.Font =
                UITheme.FontNormal;

            // 表头文字统一居中。
            dgv.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // 管理表格表头高度统一。
            dgv.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.ColumnHeadersHeight = 36;


            // ============================================================
            // 数据行
            // ============================================================

            // 默认数据行使用 Card 背景。
            dgv.DefaultCellStyle.BackColor =
                UITheme.Card;

            dgv.DefaultCellStyle.ForeColor =
                UITheme.Text;

            // 管理类数据默认居中。
            // 如果某一列是描述、备注等长文本，
            // 页面可以在绑定完成后单独覆盖。
            dgv.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            // 选中行使用主题主色。
            dgv.DefaultCellStyle.SelectionBackColor =
                UITheme.Primary;

            dgv.DefaultCellStyle.SelectionForeColor =
                Color.White;

            // 不自动换行，避免一条数据把整行高度撑大。
            dgv.DefaultCellStyle.WrapMode =
                DataGridViewTriState.False;

            // 行高统一。
            dgv.RowTemplate.Height = 32;


            // ============================================================
            // 列宽
            // ============================================================

            // 管理表格默认填充整个可用区域。
            //
            // 页面如果有特殊字段需要固定宽度，
            // 可以在调用 Management() 之后覆盖：
            //
            // dgv.AutoSizeColumnsMode =
            //     DataGridViewAutoSizeColumnsMode.None;
            //
            // dgv.Columns["ProductModel"].Width = 150;
            //
            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
