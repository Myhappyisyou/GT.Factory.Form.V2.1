using GT_Common.Helper.UIHelp;
using GT_Common.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace GT_Common.Components
{
    public partial class WorkStatusUI : UserControl
    {
        public MergedDataGridView mergedGridView ;

        public WorkStatusUI()
        {
            this.AutoSize = true;
            InitializeComponent();

            InitFormLayout();
            EnableDoubleBuffer(mergedGridView);
            CreateFakeMergedTable();

            UiRefreshCenter.OnRefresh += RefreshUI;

            mergedGridView.SizeChanged += (s, e) => mergedGridView.ResizeRowHeights();

            // ① 永远不允许选中
            mergedGridView.SelectionChanged += (s, e) =>
            {
                mergedGridView.ClearSelection();
            };

            // ② 右键时，阻断 CurrentCell
            mergedGridView.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    mergedGridView.CurrentCell = null;
                    mergedGridView.ClearSelection();
                }
            };

            // ③ 合并单元格必须强刷
            mergedGridView.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                    mergedGridView.Invalidate();
            };

            //this.Layout += (s, e) => mergedGridView.ResizeRowHeights();
            // 延迟调用一次，确保控件加载完成后撑满
            this.Load += (s, e) => BeginInvoke((Action)(() => mergedGridView.ResizeRowHeights()));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        private void RefreshUI()
        {
            if (!IsHandleCreated || IsDisposed) return;

            if (!Visible || ParentForm?.WindowState == FormWindowState.Minimized)
                return;
            try
            {
                if (Shared.dataList[0].Col0 != DateTime.Now.ToString())
                    Shared.dataList[0].Col0 = DateTime.Now.ToString();
                if (Shared.dataList[1].Col1 != (Shared.user != null ? Shared.user.JobNub + "<<>>" + Shared.user.UserRole : ""))
                    Shared.dataList[1].Col1 = Shared.user != null ? Shared.user.JobNub + "<<>>" + Shared.user.UserRole : "";
                if (Shared.dataList[1].Col3 != (Shared.isOffline ? "单机" : "MES"))
                    Shared.dataList[1].Col3 = Shared.isOffline ? "单机" : "MES";
                if (Shared.dataList[2].Col1 != Shared.productModel.BaseInfo.ProductNumber)
                    Shared.dataList[2].Col1 = Shared.productModel.BaseInfo.ProductNumber;
                if (Shared.dataList[3].Col1 != Shared.productModel.BaseInfo.ProductName)
                    Shared.dataList[3].Col1 = Shared.productModel.BaseInfo.ProductName;
                if (Shared.dataList[4].Col1 != Shared.productModel.BaseInfo.ProductCode)
                    Shared.dataList[4].Col1 = Shared.productModel.BaseInfo.ProductCode;
                if (Shared.dataList[5].Col1 != Shared.fixtureBind)
                    Shared.dataList[5].Col1 = Shared.fixtureBind;
                if (Shared.dataList[6].Col0 != Shared.currentBarNo)
                    Shared.dataList[6].Col0 = Shared.currentBarNo;
            }
            catch
            {
                // 忽略异常，防止控件已释放时崩溃
            }
        }

        private void InitFormLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 1,
                BackColor = Color.White,
                //BorderStyle = BorderStyle.FixedSingle
            };

            mergedGridView = new MergedDataGridView()
            {
                Dock = DockStyle.Fill,
                MaxRowsToStretch = 8,
                ReadOnly = true,
                ScrollBars = ScrollBars.None,
                DataSource = Shared.dataList,
                BorderStyle = BorderStyle.Fixed3D,
                MultiSelect = false,
                TabStop = false,
                Enabled=false,
            };

            // 禁止用户选中行
            mergedGridView.DefaultCellStyle.SelectionBackColor = mergedGridView.DefaultCellStyle.BackColor;
            mergedGridView.DefaultCellStyle.SelectionForeColor = mergedGridView.DefaultCellStyle.ForeColor;

            mainLayout.Controls.Add(mergedGridView);

            this.Controls.Add(mainLayout);
        }

        private void CreateFakeMergedTable()
        {
            mergedGridView.DataSource = Shared.dataList;

            // 添加合并区域：第2行的列1~3（Col1~Col3）
            mergedGridView.AddMerge(0, 0, 1, 4); // 第2行，第1列起，合并3列

            mergedGridView.AddMerge(2, 1, 1, 3); // 第2行，第1列起，合并3列

            mergedGridView.AddMerge(3, 1, 1, 3); // 第2行，第1列起，合并3列
            mergedGridView.AddMerge(4, 1, 1, 3); // 第2行，第1列起，合并3列

            mergedGridView.AddMerge(5, 1, 1, 3); // 第2行，第1列起，合并3列
            mergedGridView.AddMerge(6, 0, 1, 4); // 第2行，第1列起，合并3列

            mergedGridView.RowBackColorProvider = row =>
            {
                return (row == 0 || row == 6) ? Color.LightBlue : (Color?)null;
            };

            mergedGridView.CellBackColorProvider = (row, col) =>
            {
                if (col == 3) // Col3
                {
                    var value = Shared.dataList[row].Col3;

                    if (value == "单机")
                        return Color.FromArgb(255, 199, 206);

                    if (value == "MES")
                        return Color.LightGreen;
                }

                if (col == 1 && row == 1) // Col3
                {

                        return Color.LightBlue;
                }
                return null;
            };
        }

        // 批量更新多行数据（复用对象+触发通知）
        public void BatchUpdateRows(IEnumerable<(int rowIndex, Dictionary<string, object> changes)> updates)
        {
            if (Shared.dataList == null) return;

            // 暂停布局刷新以提升性能
            mergedGridView.SuspendLayout();

            try
            {
                foreach (var (rowIndex, changes) in updates)
                {
                    if (rowIndex < 0 || rowIndex >= Shared.dataList.Count)
                        continue;

                    var row = Shared.dataList[rowIndex];
                    foreach (var kv in changes)
                    {
                        // 反射动态设置属性（避免硬编码）
                        var prop = row.GetType().GetProperty(kv.Key);
                        if (prop != null && prop.CanWrite)
                        {
                            var oldValue = prop.GetValue(row);
                            if (!Equals(oldValue, kv.Value))
                            {
                                prop.SetValue(row, kv.Value);
                                //row.OnPropertyChanged(kv.Key); // 手动触发通知
                            }
                        }
                    }
                }
            }
            finally
            {
                // 恢复布局并强制刷新
                mergedGridView.ResumeLayout();
            }
        }

        private void EnableDoubleBuffer(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    UiRefreshCenter.OnRefresh -= RefreshUI;
                    components?.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}
