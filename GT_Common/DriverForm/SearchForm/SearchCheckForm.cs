
using GT.Factory.Common.Helper;
using GT.Factory.Data.Core;
using GT.Factory.UI.Component;
using GT.Factory.UI.Enum;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.SearchForm
{
    public partial class SearchCheckForm : Form
    {
        private UIQueryPage page;

        private readonly DataRuntime _dataRuntime;

        private readonly string _processNo;

        private DataTable _allData;                 // 全量数据
        private DataTable _currentPageData;         // 当前页数据
        private CancellationTokenSource _cts;

        // 分页
        private const int PageSize = 2000;
        private int _currentPage = 1;
        private int _totalPages = 1;

        // 列映射（英 -> 中）
        private Dictionary<string, string> _columnMap = new Dictionary<string, string>
        {
            { "bar_no", "SN码" },
            { "do_time", "时间" },
            { "test_item", "测试项" },
            { "test_item_up", "上限" },
            { "test_item_down", "下限" },
            { "test_item_value", "测试值" },
            { "flag", "结果" }
        };

        // 是否已初始化（构建）Grid列
        private bool _gridColumnsBuilt = false;
        // 是否切换数据库
        private int _dataSouce = 0;

        private UIFormRow<DateTimePicker> startRow;
        private UIFormRow<DateTimePicker> endRow;
        private UIFormRow<ComboBox> sourceRow;

        private DataGridView dgvData;

        public SearchCheckForm(string processNo, DataRuntime dataRuntime)
        {
            InitializeComponent();

            this.Text = "点检数据查询";

            page = new UIQueryPage();

            this.Controls.Add(page);

            InitQueryUI();
            BindEvents();
            this.Load += SearchForm_Load;
            _processNo = processNo;
            _dataRuntime = dataRuntime;

        }

        private void InitQueryUI()
        {
            startRow = new UIFormRow<DateTimePicker>("开始时间", UIInput.DateTime());
            endRow = new UIFormRow<DateTimePicker>("结束时间", UIInput.DateTime());

            sourceRow = new UIFormRow<ComboBox>("数据源", UIInput.Combo());

            sourceRow.SetOptions(new Dictionary<string, string>
            {
                { "Sql Server", "服务器" },
                { "Local", "本地" }
            });
            sourceRow.Input.SelectedIndex = 0;

            page.AddQueryControl(startRow);
            page.AddQueryControl(endRow);
            page.AddQueryControl(sourceRow);

            dgvData = page.Grid;
            dgvData.CellFormatting += dgvData_CellFormatting;

            page.EnableStat(false);
        }

        private void BindEvents()
        {
            page.QueryButtons.BtnSearch.Click += btnSearch_Click;
            page.QueryButtons.BtnExport.Click += btnExport_Click;

            page.Paging.BtnPrev.Click += btnPrevPage_Click;
            page.Paging.BtnNext.Click += btnNextPage_Click;
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {

            var (start, end) = ShiftTime.GetShiftTime();
            startRow.Input.Value = start;
            endRow.Input.Value = end;

            EnableDoubleBuffer();

            UpdatePagingLabel();
        }


        #region UI/性能基础
        /// <summary>
        /// DataGridView 双缓冲
        /// </summary>
        private void EnableDoubleBuffer()
        {
            Type dgvType = dgvData.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi?.SetValue(dgvData, true, null);
        }

        private void ToggleUi(bool enabled)
        {
            page.QueryButtons.BtnSearch.Enabled = enabled;
            page.QueryButtons.BtnExport.Enabled = enabled;
            page.Paging.BtnPrev.Enabled = enabled;
            page.Paging.BtnPrev.Enabled = enabled;

            UseWaitCursor = !enabled;
            Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
        }
        #endregion

        #region 事件
        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // 取消上一次
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            try
            {
                page.SetState(UIPageState.Loading);

                DateTime start = startRow.Input.Value;
                DateTime end = endRow.Input.Value;

                if (start >= end)
                {
                    MessageBox.Show("开始时间不能大于或等于结束时间");
                    return;
                }

                ToggleUi(false);

                dgvData.DataSource = null;

                // 查询（后台线程）

                List<dynamic> list = await _dataRuntime.ProcessService.SelectCalibrationAsync(_processNo.Substring(0, 5), start, end);

                _allData = LstToDataTableConverter.ToDataTable(list);

                _allData = RenameColumns(_allData, _columnMap);

                if (_allData == null || _allData.Rows.Count == 0)
                {
                    page.SetState(UIPageState.Empty, "没有查询到数据");
                    return;
                }

                page.SetState(UIPageState.Data);

                _currentPage = 1;

                _totalPages = Math.Max(1, (int)Math.Ceiling(_allData.Rows.Count / (double)PageSize));

                // 首次构建列（之后不再构建）
                if (!_gridColumnsBuilt)
                {
                    BuildGridColumnsOnce(_allData);
                    _gridColumnsBuilt = true;
                }

                // 绑定第一页
                BindPage();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询过程中发生错误：{ex.Message}");
            }
            finally
            {
                ToggleUi(true);
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (_currentPage <= 1) return;
            _currentPage--;
            BindPage();
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_currentPage >= _totalPages) return;
            _currentPage++;
            BindPage();
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            if (_currentPageData == null || _currentPageData.Rows.Count == 0)
            {
                MessageBox.Show("没有可导出的数据");
                return;
            }

            using (var saveFile = new SaveFileDialog())
            {
                saveFile.Filter = "Excel文件|*.xls;*.xlsx";
                saveFile.FileName = string.Format("查询结果_{0:yyyyMMddHHmmss}.xls", DateTime.Now);

                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ToggleUi(false);
                        await Task.Run(() => ExportData.WriteExcel(_columnMap, _currentPageData, saveFile.FileName));
                        MessageBox.Show("已生成 Excel 文件！");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败：{ex.Message}");
                    }
                    finally
                    {
                        ToggleUi(true);
                    }
                }
            }
        }

        private void ColorRows()
        {
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                var val = row.Cells
                    .Cast<DataGridViewCell>()
                    .FirstOrDefault(c => c.OwningColumn.DataPropertyName == "结果")?.Value?.ToString();
                switch (val)
                {
                    // 使用更柔和的背景色 + 深色文字
                    case "OK":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(232, 245, 232);  // 柔和浅绿
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(51, 51, 51);     // 深灰
                        break;

                    case "NG":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(250, 235, 235);  // 柔和浅红
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(51, 51, 51);     // 深灰
                        break;

                    case "进行中":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(220, 240, 255);
                        row.DefaultCellStyle.ForeColor = Color.DarkBlue;
                        break;

                    case "取消":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        break;
                }
            }
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgvData.Columns[e.ColumnIndex].DataPropertyName == "ok_flag" || dgvData.Columns[e.ColumnIndex].DataPropertyName == "测试结果") && e.RowIndex >= 0)
            {
                var cell = dgvData.Rows[e.RowIndex].Cells[e.ColumnIndex];

                cell.Style.BackColor = dgvData.DefaultCellStyle.BackColor;
                cell.Style.ForeColor = dgvData.DefaultCellStyle.ForeColor;

                switch (cell.Value?.ToString())
                {
                    case "OK":
                        cell.Style.BackColor = Color.FromArgb(232, 245, 232);  // 柔和浅绿
                        cell.Style.ForeColor = Color.FromArgb(51, 51, 51);     // 深灰
                        break;
                    case "进行中":
                        cell.Style.BackColor = Color.LightBlue;
                        cell.Style.ForeColor = Color.Red;
                        break;
                    case "NG":
                        cell.Style.BackColor = Color.FromArgb(250, 235, 235);  // 柔和浅红
                        cell.Style.ForeColor = Color.FromArgb(51, 51, 51);     // 深灰
                        break;
                    case "取消":
                        cell.Style.BackColor = Color.LightGray;
                        cell.Style.ForeColor = Color.LightGray;
                        break;
                    default:
                        cell.Style.BackColor = Color.LightGray;
                        cell.Style.ForeColor = Color.LightGray;
                        break;
                }
            }
        }
        #endregion

        #region 绑定/分页
        private void BindPage()
        {
            if (_allData == null)
            {
                dgvData.DataSource = null;
                UpdatePagingLabel();
                return;
            }

            var start = (_currentPage - 1) * PageSize;
            var end = Math.Min(start + PageSize, _allData.Rows.Count);

            _currentPageData = _allData.Clone();
            for (int i = start; i < end; i++)
                _currentPageData.ImportRow(_allData.Rows[i]);

            dgvData.DataSource = _currentPageData;

            ColorRows();

            UpdatePagingLabel();
        }

        private void UpdatePagingLabel()
        {
            page.Paging.Update(_currentPage, _totalPages);
        }

        public static DataTable RenameColumns(DataTable dt, Dictionary<string, string> nameMap)
        {
            if (dt == null)
                return null;

            foreach (var kv in nameMap)
            {
                if (dt.Columns.Contains(kv.Key))
                {
                    dt.Columns[kv.Key].ColumnName = kv.Value;
                }
            }
            return dt;
        }

        /// <summary>
        /// 只在第一次有数据时建列，后面翻页只换 DataSource
        /// </summary>
        private void BuildGridColumnsOnce(DataTable table)
        {
            dgvData.AutoGenerateColumns = false;
            dgvData.Columns.Clear();

            foreach (DataColumn col in table.Columns)
            {
                string header = _columnMap.TryGetValue(col.ColumnName, out var cn)
                    ? cn
                    : col.ColumnName; // 没映射就用英文原名

                dgvData.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = col.ColumnName, // 必须英文列名
                    HeaderText = header,               // 显示中文
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                });
            }

            // 固定前4列
            SetupFixedColumns();
        }

        /// <summary>
        /// 固定列样式
        /// </summary>
        private void SetupFixedColumns()
        {
            dgvData.SuspendLayout();
            try
            {
                for (int i = 0; i < 3 && i < dgvData.Columns.Count; i++)
                {
                    dgvData.Columns[i].Frozen = true;
                    dgvData.Columns[i].DefaultCellStyle.BackColor = Color.LightGray;
                }
                if (dgvData.Columns.Count > 2)
                    dgvData.Columns[2].DividerWidth = 2;

                //if (dgvData.Columns.Count > 0) dgvData.Columns[0].Width = 250;
                //if (dgvData.Columns.Count > 5) dgvData.Columns[4].Width = 150;
            }
            finally
            {
                dgvData.ResumeLayout();
            }
        }
        #endregion

        #region 导出
        /// <summary>
        /// 直接以 TSV 方式写 Excel（简单粗暴但够用）
        /// </summary>
        public void WriteExcel(DataTable dt, string path)
        {
            using (var sw = new System.IO.StreamWriter(path, false, Encoding.GetEncoding("gb2312")))
            {
                var sb = new StringBuilder();

                // 表头
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var colName = dt.Columns[i].ColumnName;
                    if (_columnMap.TryGetValue(colName, out var cn))
                        sb.Append(cn);
                    else
                        sb.Append(colName);

                    if (i < dt.Columns.Count - 1) sb.Append('\t');
                }
                sb.AppendLine();

                // 数据
                foreach (DataRow row in dt.Rows)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        var val = row[j]?.ToString() ?? "";
                        sb.Append(val);
                        if (j < dt.Columns.Count - 1) sb.Append('\t');
                    }
                    sb.AppendLine();
                }

                sw.Write(sb.ToString());
                sw.Flush();
            }

        }
        #endregion
    }
}
