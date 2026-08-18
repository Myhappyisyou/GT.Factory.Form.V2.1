
using GT.Factory.Common.Helper;
using GT.Factory.Common.Models;
using GT.Factory.Data.Core;
using GT.Factory.UI.Component;
using GT.Factory.UI.Enum;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using GT_Common.DriverForm.Login;
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
    public partial class SearchForm : Form
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
        private Dictionary<string, string> _columnMap = new Dictionary<string, string>();

        // 是否已初始化（构建）Grid列
        private bool _gridColumnsBuilt = false;
        // 是否切换数据库
        private int _dataSouce = 0;

        private UIFormRow<DateTimePicker> startRow;
        private UIFormRow<DateTimePicker> endRow;
        private UIFormRow<ComboBox> sourceRow;
        private UIFormRow<TextBox> barNoRow;
        private UIFormRow<TextBox> orderRow;
        private UIFormRow<ComboBox> resultRow;

        private DataGridView dgvData;


        public SearchForm(string processNo, DataRuntime dataRuntime)
        {
            InitializeComponent();

            this.Text = "数据查询";

            _processNo = processNo;
            _dataRuntime = dataRuntime;

            page = new UIQueryPage();

            this.Controls.Add(page);

            InitQueryUI();
            BindEvents();
            this.Load += SearchForm_Load;
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

            barNoRow = new UIFormRow<TextBox>("产品码", UIInput.Text());
            orderRow = new UIFormRow<TextBox>("工单号", UIInput.Text());

            resultRow = new UIFormRow<ComboBox>("结果", UIInput.Combo());

            resultRow.SetOptions(new Dictionary<string, string>
            {
                { "All", "全部" },
                { "OK", "合格" },
                { "NG", "不合格" }
            });

            resultRow.Input.SelectedIndex = 0;

            page.AddQueryControl(startRow);
            page.AddQueryControl(endRow);
            page.AddQueryControl(sourceRow);
            page.AddQueryControl(barNoRow);
            page.AddQueryControl(orderRow);
            page.AddQueryControl(resultRow);

            dgvData = page.Grid;
            dgvData.CellFormatting += dgvData_CellFormatting;
        }

        private void BindEvents()
        {
            page.QueryButtons.BtnSearch.Click += btnSearch_Click;
            page.QueryButtons.BtnExport.Click += btnExport_Click;

            barNoRow.Input.KeyDown += txtBarNo_KeyDown;

            page.Paging.BtnPrev.Click += btnPrevPage_Click;
            page.Paging.BtnNext.Click += btnNextPage_Click;
        }

        private async void SearchForm_Load(object sender, EventArgs e)
        {

            var (start, end) = ShiftTime.GetShiftTime();
            startRow.Input.Value = start;
            endRow.Input.Value = end; 

            EnableDoubleBuffer();

            // 预取列映射（一次即可）
            _columnMap = await _dataRuntime.Repositories.Processes.GetColumnMapAsync(_processNo.Substring(0, 5));

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

                string result = resultRow.Text;

                bool dataSource = sourceRow.Input.SelectedIndex == 0;

                string barNo = barNoRow.Text?.Trim();
                string vouNo = orderRow.Text?.Trim();

                if (_dataSouce != sourceRow.Input.SelectedIndex)
                    _gridColumnsBuilt = false;

                if (string.IsNullOrEmpty(barNo) && string.IsNullOrEmpty(vouNo) && start >= end)
                {
                    MessageBox.Show("开始时间不能大于或等于结束时间");
                    return;
                }

                ToggleUi(false);
                dgvData.DataSource = null;

                // 查询（后台线程）
                if (dataSource)
                {
                    List<dynamic> list;

                    if (string.IsNullOrEmpty(barNo) &&
                            string.IsNullOrEmpty(vouNo))
                    {
                        list = await _dataRuntime.ProcessService.SearchDataAsync(
                                new SearchDataPayload
                                {
                                    ProcessNo = _processNo.Substring(0, 5),
                                    StartTime = start,
                                    EndTime = end,
                                    OkFlag = result
                                });
                    }
                    else if (string.IsNullOrEmpty(barNo))
                    {
                        list = await _dataRuntime.ProcessService.SearchDataAsync(
                                new SearchDataPayload
                                {
                                    ProcessNo = _processNo.Substring(0, 5),
                                    StartTime = start,
                                    EndTime = end,
                                    OkFlag = result,
                                    FilterColumn = "vou_no",
                                    FilterValue = vouNo
                                });
                    }
                    else
                    {
                        list = await _dataRuntime.ProcessService.SearchDataAsync(
                                new SearchDataPayload
                                {
                                    ProcessNo = _processNo.Substring(0, 5),
                                    StartTime = start,
                                    EndTime = end,
                                    OkFlag = result,
                                    FilterColumn = "bar_no",
                                    FilterValue = barNo
                                });
                    }

                    _allData = LstToDataTableConverter.ToDataTable(list);

                    _allData = RenameColumns(_allData, _columnMap);
                }
                else
                {
                    List<AccessTestDataDto> listAccess;


                    if (string.IsNullOrEmpty(barNo) &&
                        string.IsNullOrEmpty(vouNo))
                    {
                        listAccess =
                            await _dataRuntime.AccessService
                            .SearchAccessDataAsync(
                                new SearchDataPayload
                                {
                                    ProcessNo = _processNo,
                                    StartTime = start,
                                    EndTime = end,
                                    OkFlag = result
                                });
                    }
                    else if (string.IsNullOrEmpty(barNo))
                    {
                        listAccess =
                            await _dataRuntime.AccessService
                            .SearchAccessDataAsync(
                                new SearchDataPayload
                                {
                                    ProcessNo = _processNo,
                                    StartTime = start,
                                    EndTime = null,
                                    OkFlag = result,
                                    FilterColumn = "当前工单号",
                                    FilterValue = vouNo
                                });
                    }
                    else
                    {
                        listAccess =
                            await _dataRuntime.AccessService
                            .SearchAccessDataAsync(
                                new SearchDataPayload
                                {
                                    ProcessNo = _processNo,
                                    StartTime = start,
                                    EndTime = null,
                                    OkFlag = result,
                                    FilterColumn = "产品条码",
                                    FilterValue = barNo
                                });
                    }

                    _allData =
                        LstToDataTableConverter
                        .ToDataTable(listAccess);
                }

                if (_allData == null || _allData.Rows.Count == 0)
                {
                    page.SetState(UIPageState.Empty, "没有查询到数据");
                    return;
                }

                page.SetState(UIPageState.Data);

                if (!_allData.Columns.Contains("序号"))
                {
                    _allData.Columns.Add("序号", typeof(int));
                }

                // ⭐ 赋值
                for (int i = 0; i < _allData.Rows.Count; i++)
                {
                    _allData.Rows[i]["序号"] = i + 1;
                }

                // ⭐ 放到第一列
                _allData.Columns["序号"].SetOrdinal(0);

                _currentPage = 1;
                _totalPages = Math.Max(1, (int)Math.Ceiling(_allData.Rows.Count / (double)PageSize));

                var rows = _allData.AsEnumerable();

                // 1. 去重（按条码）
                var distinctRows = rows
                    .GroupBy(r => GetColByIndex(r, dataSource ? 1 : 4))
                    .Select(g => g.First())
                    .ToList();

                // 2. 统计
                int okCount = distinctRows.Count(r => GetColByIndex(r, dataSource ? 3 : 7)?.ToString() == "OK");
                int ngCount = distinctRows.Count(r => GetColByIndex(r, dataSource ? 3 : 7)?.ToString() == "NG");

                // 首次构建列（之后不再构建）
                if (!_gridColumnsBuilt)
                {
                    BuildGridColumnsOnce(_allData);
                    _gridColumnsBuilt = true;
                }

                // 绑定第一页
                BindPage();
                page.StatBar.Update(okCount, ngCount, distinctRows.Count);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception ex)
            {
                page.SetState(UIPageState.Error, ex.Message);
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
                        await Task.Run(() => ExportData.WriteExcel(_columnMap,_currentPageData, saveFile.FileName));
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

        private void txtBarNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                page.QueryButtons.BtnSearch.PerformClick();
            }
        }

        string GetColByIndex(DataRow r, int index)
        {
            return r[index]?.ToString();
        }

        private void ColorRows()
        {
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                var val = row.Cells.Cast<DataGridViewCell>().FirstOrDefault(c => c.OwningColumn.DataPropertyName == "结果" || c.OwningColumn.DataPropertyName == "测试结果")?.Value?.ToString();
                switch (val)
                {
                    case "OK":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(20, 60, 40);  // 极暗的深绿
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 255, 150); // 荧光绿文字
                        break;
                    case "NG":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(70, 20, 25);  // 极暗的深红
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 100, 100); // 亮红文字
                        break;
                    case "进行中":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(20, 40, 70);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(100, 200, 255);
                        break;
                    case "取消":
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        break;
                }
            }
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // ✅ 1. 获取当前正在绘制的列的绑定字段名
            string colName = dgvData.Columns[e.ColumnIndex].DataPropertyName;

            // ✅ 2. 只有当绘制到“结果列”时，才进行着色（防止整行变色）
            if (colName == "ok_flag" || colName == "测试结果" || colName == "结果")
            {
                string result = e.Value?.ToString();

                switch (result)
                {
                    case "OK":
                        // 单元格暗绿色底 + 荧光绿文字
                        e.CellStyle.BackColor = Color.FromArgb(20, 60, 40);
                        e.CellStyle.ForeColor = Color.FromArgb(0, 255, 150);
                        break;
                    case "NG":
                        // 单元格暗红色底 + 亮红文字
                        e.CellStyle.BackColor = Color.FromArgb(70, 20, 25);
                        e.CellStyle.ForeColor = Color.FromArgb(255, 100, 100);
                        break;
                    case "进行中":
                        // 单元格暗蓝色底 + 亮蓝文字
                        e.CellStyle.BackColor = Color.FromArgb(20, 40, 70);
                        e.CellStyle.ForeColor = Color.FromArgb(100, 200, 255);
                        break;
                    case "取消":
                        e.CellStyle.ForeColor = Color.Gray;
                        break;
                    default:
                        // 保持原来的深色风格不变
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

            //ColorRows();

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
                for (int i = 0; i < 5 && i < dgvData.Columns.Count; i++)
                {
                    dgvData.Columns[i].Frozen = true;
                    // ✅ 使用略深于背景的颜色突出显示冻结列
                    dgvData.Columns[i].DefaultCellStyle.BackColor = Color.FromArgb(25, 35, 50);
                }
                if (dgvData.Columns.Count > 4)
                {
                    dgvData.Columns[4].DividerWidth = 2;
                }
            }
            finally
            {
                dgvData.ResumeLayout();
            }
        }
        #endregion

    }

}
