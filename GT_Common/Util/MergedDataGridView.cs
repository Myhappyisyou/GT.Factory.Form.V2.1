using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace GT_Common.Util
{
    public class MergedCell
    {
        public int StartRow { get; set; }
        public int StartColumn { get; set; }
        public int RowSpan { get; set; } = 1;
        public int ColumnSpan { get; set; } = 1;

        public bool Contains(int row, int col)
        {
            return row >= StartRow && row < StartRow + RowSpan &&
                   col >= StartColumn && col < StartColumn + ColumnSpan;
        }

        public bool IsMainCell(int row, int col)
        {
            return row == StartRow && col == StartColumn;
        }
    }

    public class MergedDataGridView : DataGridView
    {
        public List<MergedCell> MergedCells { get; private set; } = new List<MergedCell>();
        public Func<int, int, Color?> CellBackColorProvider { get; set; }
        public Func<int, Color?> RowBackColorProvider { get; set; }
        public Func<int, Color?> ColumnBackColorProvider { get; set; }
        public Func<int, int, Color?> CellForeColorProvider { get; set; }
        public Func<int, Color?> RowForeColorProvider { get; set; }
        public Func<int, Color?> ColumnForeColorProvider { get; set; }

        public Color BorderColor { get; set; } = Color.LightGray;
        /// <summary>
        /// 当行数小于等于该值时才尝试拉伸行高填满控件
        /// </summary>
        public int MaxRowsToStretch { get; set; } = 8;

        private int _lastRowCount = -1;
        private int _lastClientHeight = -1;

        public MergedDataGridView()
        {
            CellPainting += OnCellPainting;
            CellBeginEdit += OnCellBeginEdit;
            CellFormatting += OnCellFormatting;
            RowHeadersVisible = false;
            ColumnHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToResizeRows = false;
            //Enabled = false;
            ReadOnly = true;            // 禁止编辑
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            SizeChanged += (s, e) => ResizeRowHeights();
            HandleCreated += (s, e) => BeginInvoke((Action)(() => ResizeRowHeights()));
        }

        public void AddMerge(int row, int col, int rowSpan, int colSpan)
        {
            MergedCells.Add(new MergedCell
            {
                StartRow = row,
                StartColumn = col,
                RowSpan = rowSpan,
                ColumnSpan = colSpan
            });
            Invalidate();
        }

        public void ClearMerges()
        {
            MergedCells.Clear();
            Invalidate();
        }

        public void ResizeRowHeights()
        {
            if (RowCount == 0) return;

            int headerHeight = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
            int totalAvailableHeight = ClientSize.Height - headerHeight;

            if (totalAvailableHeight == _lastClientHeight && RowCount == _lastRowCount)
                return; // 没变化就不处理

            _lastClientHeight = totalAvailableHeight;
            _lastRowCount = RowCount;

            if (RowCount < MaxRowsToStretch) // 少量数据填满
            {
                int baseHeight = totalAvailableHeight / RowCount;       // 每行基本高度
                int remainder = totalAvailableHeight - baseHeight * RowCount; // 剩余像素

                for (int i = 0; i < RowCount; i++)
                {
                    // 最后一行吸收余数像素
                    Rows[i].Height = (i == RowCount - 1) ? baseHeight + remainder : baseHeight;
                }

                // 隐藏滚动条
                ScrollBars = ScrollBars.None;
            }
            else
            {
                foreach (DataGridViewRow row in Rows)
                    row.Height = 22; // 默认行高，让滚动条出现
                ScrollBars = ScrollBars.Vertical;
            }
        }

        private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            foreach (var merge in MergedCells)
            {
                if (!merge.Contains(e.RowIndex, e.ColumnIndex)) continue;
                e.Handled = true;

                if (!merge.IsMainCell(e.RowIndex, e.ColumnIndex)) return;

                Rectangle rect = GetCellDisplayRectangle(merge.StartColumn, merge.StartRow, true);

                for (int i = 1; i < merge.ColumnSpan; i++)
                    rect.Width += GetCellDisplayRectangle(merge.StartColumn + i, merge.StartRow, true).Width;

                for (int j = 1; j < merge.RowSpan; j++)
                    rect.Height += GetCellDisplayRectangle(merge.StartColumn, merge.StartRow + j, true).Height;

                Color? backColor = CellBackColorProvider?.Invoke(merge.StartRow, merge.StartColumn) ??
                                   RowBackColorProvider?.Invoke(merge.StartRow) ??
                                   ColumnBackColorProvider?.Invoke(merge.StartColumn) ??
                                   (this[merge.StartColumn, merge.StartRow].Style.BackColor.IsEmpty ?
                                    this.DefaultCellStyle.BackColor :
                                    this[merge.StartColumn, merge.StartRow].Style.BackColor);

                Color foreColor = CellForeColorProvider?.Invoke(merge.StartRow, merge.StartColumn)
                                   ?? RowForeColorProvider?.Invoke(merge.StartRow)
                                   ?? ColumnForeColorProvider?.Invoke(merge.StartColumn)
                                   ?? (this[merge.StartColumn, merge.StartRow].Style.ForeColor.IsEmpty
                                       ? this.DefaultCellStyle.ForeColor
                                       : this[merge.StartColumn, merge.StartRow].Style.ForeColor);

                using (SolidBrush back = new SolidBrush(backColor.Value))
                {
                    e.Graphics.FillRectangle(back, rect);
                }

                using (Pen pen = new Pen(BorderColor))
                {
                    e.Graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                }

                string text = this[merge.StartColumn, merge.StartRow].FormattedValue?.ToString();

                //TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, rect, e.CellStyle.ForeColor,
                //    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, rect, foreColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // 2. 再处理普通单元格的背景色
            //      if (e.RowIndex >= 0 && e.ColumnIndex >= 0) // 排除行头/列头
            //      {
            //          Color? backColor = CellBackColorProvider?.Invoke(e.RowIndex, e.ColumnIndex) ??
            //              RowBackColorProvider?.Invoke(e.RowIndex)
            //                            ?? ColumnBackColorProvider?.Invoke(e.ColumnIndex);
            //          Color? foreColor = CellForeColorProvider?.Invoke(e.RowIndex, e.ColumnIndex)
            //?? RowForeColorProvider?.Invoke(e.RowIndex)
            //?? ColumnForeColorProvider?.Invoke(e.ColumnIndex);


            //          using (Pen p = new Pen(BorderColor))
            //              e.Graphics.DrawRectangle(p, e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width - 1, e.CellBounds.Height - 1);

            //          if (backColor.HasValue || foreColor.HasValue)
            //          {
            //              //e.Graphics.FillRectangle(new SolidBrush(backColor.Value), e.CellBounds);
            //              if (backColor.HasValue)
            //                  e.Graphics.FillRectangle(new SolidBrush(backColor.Value), e.CellBounds);

            //              if (foreColor.HasValue)
            //                  e.CellStyle.ForeColor = foreColor.Value; // ⭐关键

            //              e.PaintContent(e.CellBounds);
            //              e.Handled = true;
            //          }
            //      }
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                Color back = CellBackColorProvider?.Invoke(e.RowIndex, e.ColumnIndex)
                             ?? RowBackColorProvider?.Invoke(e.RowIndex)
                             ?? ColumnBackColorProvider?.Invoke(e.ColumnIndex)
                             ?? this.DefaultCellStyle.BackColor;

                Color fore = CellForeColorProvider?.Invoke(e.RowIndex, e.ColumnIndex)
                             ?? RowForeColorProvider?.Invoke(e.RowIndex)
                             ?? ColumnForeColorProvider?.Invoke(e.ColumnIndex)
                             ?? this.DefaultCellStyle.ForeColor;

                using (var brush = new SolidBrush(back))
                {
                    e.Graphics.FillRectangle(brush, e.CellBounds);
                }

                using (var pen = new Pen(BorderColor))
                {
                    e.Graphics.DrawRectangle(pen, e.CellBounds.X, e.CellBounds.Y, e.CellBounds.Width - 1, e.CellBounds.Height - 1);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    e.FormattedValue?.ToString(),
                    e.CellStyle.Font,
                    e.CellBounds,
                    fore,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        private void OnCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            foreach (var merge in MergedCells)
            {
                if (merge.Contains(e.RowIndex, e.ColumnIndex) && !merge.IsMainCell(e.RowIndex, e.ColumnIndex))
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void OnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            foreach (var merge in MergedCells)
            {
                if (merge.Contains(e.RowIndex, e.ColumnIndex) && !merge.IsMainCell(e.RowIndex, e.ColumnIndex))
                {
                    e.Value = this[merge.StartColumn, merge.StartRow].Value;
                    e.FormattingApplied = true;

                    return;
                }
            }
        }
    }
}
