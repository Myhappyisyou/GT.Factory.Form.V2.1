
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
        private Label lblStartTime, lblEndTime, lblCount, lblPageInfo;
        private DateTimePicker dtpStartTime, dtpEndTime;
        private Button btnSearch, btnExport, btnPrevPage, btnNextPage;
        private DataGridView dgvData;
        private Panel panelTop, panelBottom;

        public SearchCheckForm(string processNo)
        {
            InitializeComponent();
            InitUI();
            _processNo = processNo;
        }

        private void InitUI()
        {
            this.Text = "点检数据查询";
            this.MaximizeBox = true;

            //this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            var tableTop = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2,
                AutoSize = true,
            };
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var tableBottom = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                AutoSize = true,
            };
            tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
          
            // …继续按比例布局

            // 顶部 Panel
            panelTop = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
            this.Controls.Add(panelTop);

            // 底部 Panel
            panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };
            this.Controls.Add(panelBottom);

            // ======= 查询条件控件 =======
            lblStartTime = new Label
            {
                Text = "开始时间：",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(5, 3, 5, 3)
            };

            dtpStartTime = new DateTimePicker { Width = 150, ShowUpDown = true, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss", Margin = new Padding(5, 3, 5, 3) };

            lblEndTime = new Label
            {
                Text = "结束时间：",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(5, 3, 5, 3)
            };

            dtpEndTime = new DateTimePicker {  Width = 150, ShowUpDown = true, Format=DateTimePickerFormat.Custom,CustomFormat= "yyyy-MM-dd HH:mm:ss" , Margin = new Padding(5, 3, 5, 3) };

            btnSearch = new Button { Text = "查询", Width = 80 , Margin = new Padding(5, 3, 5, 3) };
            btnSearch.Click += btnSearch_Click;

            btnExport = new Button { Text = "导出", Width = 80 , Margin = new Padding(5, 3, 5, 3) };
            btnExport.Click += btnExport_Click;

            tableTop.Controls.Add(lblStartTime, 0, 0);
            tableTop.Controls.Add(dtpStartTime, 1, 0);
            tableTop.Controls.Add(lblEndTime, 2, 0);
            tableTop.Controls.Add(dtpEndTime, 3, 0);
            tableTop.Controls.Add(btnSearch, 4, 0);
            tableTop.Controls.Add(btnExport, 5, 0);

            panelTop.Controls.Add(tableTop);

            // ======= DataGridView =======
            dgvData = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Margin = new Padding(5, 3, 5, 3)
            };

            dgvData.EnableHeadersVisualStyles = false;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei", 9, FontStyle.Bold);
            dgvData.RowTemplate.Height = 28;
            dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            dgvData.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvData.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvData.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0); // 左右各5px
            //dgvData.CellFormatting += dgvData_CellFormatting;

            this.Controls.Add(dgvData);
            dgvData.BringToFront();

            // ======= 底部控件 =======
            btnPrevPage = new Button { Text = "上一页", Width = 80, Margin = new Padding(5, 3, 5, 3) };
            btnPrevPage.Click += btnPrevPage_Click;

            lblPageInfo = new Label
            {
                Text = "第 1 页 / 共 1 页",
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(5, 3, 5, 3)
            };

            btnNextPage = new Button { Text = "下一页", Width = 80, Margin = new Padding(5, 3, 5, 3) };
            btnNextPage.Click += btnNextPage_Click;

            lblCount = new Label
            {
                Text = "总数：0",
                AutoSize = false,
                Width = 100, 
                Height = 30, 
                Anchor = AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter, // 文字居中
                Margin = new Padding(5, 3, 5, 3),
                Padding = new Padding(5, 0, 5, 0),        // 左右留白
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White
            };

            tableBottom.Controls.Add(btnPrevPage, 0, 0);
            tableBottom.Controls.Add(lblPageInfo, 1, 0);
            tableBottom.Controls.Add(btnNextPage, 2, 0);
            tableBottom.Controls.Add(lblCount, 3, 0);

            panelBottom.Controls.Add(tableBottom);

            // ✅ 放在所有按钮创建完后
            foreach (var btn in new[] { btnSearch, btnExport, btnPrevPage, btnNextPage })
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.BackColor = Color.FromArgb(0, 122, 204);
                btn.ForeColor = Color.White;
                btn.Font = new Font("Microsoft YaHei", 9);
                btn.FlatAppearance.BorderSize = 0; // 去掉边框线
                btn.Cursor = Cursors.Hand;          // 鼠标悬停变手型
            }
        }

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
       
        private  void SearchForm_Load(object sender, EventArgs e)
        {
            SetDefaultShiftTime();
            EnableDoubleBuffer();
            UpdatePagingLabel();
        }

        private void SetDefaultShiftTime()
        {
            DateTime now = DateTime.Now;

            DateTime dayStart = now.Date.AddHours(8).AddMinutes(00);   // 08:30
            DateTime nightStart = now.Date.AddHours(20).AddMinutes(00); // 20:30

            if (now >= dayStart && now < nightStart)
            {
                // 早班
                dtpStartTime.Value = dayStart;
                dtpEndTime.Value = nightStart;
            }
            else
            {
                // 晚班
                if (now >= nightStart)
                {
                    // 当天晚上
                    dtpStartTime.Value = nightStart;
                    dtpEndTime.Value = dayStart.AddDays(1);
                }
                else
                {
                    // 凌晨（属于昨天晚班）
                    dtpStartTime.Value = nightStart.AddDays(-1);
                    dtpEndTime.Value = dayStart;
                }
            }
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
            btnSearch.Enabled = enabled;
            btnExport.Enabled = enabled;
            btnPrevPage.Enabled = enabled;
            btnNextPage.Enabled = enabled;

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
                DateTime start = dtpStartTime.Value;
                DateTime end = dtpEndTime.Value;
              
                if (start >= end)
                {
                    MessageBox.Show("开始时间不能大于或等于结束时间");
                    return;
                }

                ToggleUi(false);
                lblCount.Text = "查询中...";
                dgvData.DataSource = null;

                // 查询（后台线程）
                _allData = await Task.Run(() =>
                {

                    return RenameColumns(UploadSql.SelectCalibration(_processNo.Substring(0, 5), start, end), _columnMap);

                }, _cts.Token);

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
                lblCount.Text = $"总数: {_allData.Rows.Count}";
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
                        await Task.Run(() => WriteExcel(_currentPageData, saveFile.FileName));
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
                btnSearch.PerformClick();
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
            lblPageInfo.Text = $"第 {_currentPage} / {_totalPages} 页";
            btnPrevPage.Enabled = _currentPage > 1;
            btnNextPage.Enabled = _currentPage < _totalPages;
        }

        public static DataTable RenameColumns(DataSet ds, Dictionary<string, string> nameMap)
        {
            if (ds == null || ds.Tables.Count == 0) return null;
            var dt = ds.Tables[0];

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
