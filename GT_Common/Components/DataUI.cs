using GT_Common;
using GT_Common.DriverForm.DataDetail;
using GT_Common.Helper.Logging;
using GT_Common.Helper.UIHelp;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Components
{
    public partial class DataUI : UserControl
    {
        private readonly Font _customFont = new Font("Segoe UI", 11f);
        private readonly Font _lvHeaderFont = new Font("Segoe UI", 11.5f, FontStyle.Bold);
        private readonly string _processName;

        private ListView lvInfo;
        private int[] _columnMaxWidth;
        private List<(string ColumnName, Func<TestDispItem, string> ValueGetter)> _columnConfigs;

        // 控制状态
        private bool _isLayoutBusy = false;
        private bool _isLoaded = false;
        private bool _pendingResize = false;

        // === 刷新控制参数 ===
        private const int MaxItemsPerRefresh = 50;     // 每次最多处理50条（核心）
        private const int MaxQueueLimit = 1000;        // 队列最大长度（防爆）
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private const int RefreshIntervalMs = 50;      // 刷新节流（20FPS）

        // 全局真实序号
        private long _globalIndex = 1;

        // 控制最大显示条数
        private const int MaxDisplayCount = 200;

        public DataUI(string processName)
        {
            _processName = processName;
            InitializeComponent();
            InitFormLayout();
            InitEventHandlers();
            EnableDoubleBuffer(lvInfo);

            UiRefreshCenter.OnRefresh += RefreshUI;

            this.Load += (s, e) =>
            {
                _isLoaded = true;
                BeginInvoke(new  Action(() =>
                {
                    lvInfo.Refresh();
                }));
            };
        }

        #region === 初始化 ===
        private void InitFormLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = false,
                BackColor = Color.White,
                Padding = new Padding(1)
            };

            lvInfo = new ListView
            {
                Dock = DockStyle.Fill,
                Font = _customFont,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Scrollable = true,
                OwnerDraw = true,
                BorderStyle = BorderStyle.None,
                ForeColor = Color.Black,
            };

            mainPanel.Controls.Add(lvInfo);
            Controls.Add(mainPanel);
        }

        private void InitEventHandlers()
        {
            lvInfo.DoubleClick += LvInfo_DoubleClick;
            lvInfo.DrawColumnHeader += LvInfo_DrawColumnHeader;
            lvInfo.DrawItem += (s, e) => e.DrawDefault = true;
            lvInfo.DrawSubItem += (s, e) => e.DrawDefault = true;

            //this.Resize += (s, e) =>
            //{
            //    // 强拦截：最小化直接不处理
            //    if (ParentForm != null && ParentForm.WindowState == FormWindowState.Minimized)
            //        return;

            //    if (!_isLoaded || _isLayoutBusy)
            //        return;

            //    // 防抖 + 延迟执行（等窗体完全渲染完再计算）
            //    _pendingResize = true;
            //    BeginInvoke(new Action(() =>
            //    {
            //        // 二次校验：防止执行时又被最小化
            //        if (ParentForm != null && ParentForm.WindowState == FormWindowState.Minimized)
            //        {
            //            _pendingResize = false;
            //            return;
            //        }

            //        //SafeResizeColumns();
            //        _pendingResize = false;
            //    }));
            //};
        }

        private void EnableDoubleBuffer(Control control)
        {
            var prop = typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            prop?.SetValue(control, true);
        }

        #endregion

        #region === 表头 ===

        private void LvInfo_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var bgBrush = new SolidBrush(Color.FromArgb(64, 64, 70)))
            using (var textBrush = new SolidBrush(Color.Gainsboro))
            using (var borderPen = new Pen(Color.FromArgb(90, 90, 95)))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
                e.Graphics.DrawLine(borderPen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(e.Header.Text, _lvHeaderFont, textBrush, e.Bounds, sf);
            }
        }

        #endregion

        #region === 列配置 ===

        public void SetColumns(List<(string ColumnName, int Width, Func<TestDispItem, string> ValueGetter)> columnConfigs)
        {
            _isLayoutBusy = true;
            _columnConfigs = columnConfigs.Select(c => (c.ColumnName, c.ValueGetter)).ToList();

            lvInfo.BeginUpdate();
            lvInfo.Columns.Clear();

            foreach (var col in columnConfigs)
            {
                lvInfo.Columns.Add(col.ColumnName, col.Width, HorizontalAlignment.Center);
            }

            lvInfo.EndUpdate();

            _isLayoutBusy = false;
        }

        //public void SetColumns(List<(string ColumnName, Func<TestDispItem, string> ValueGetter)> columnConfigs)
        //{
        //    _isLayoutBusy = true;
        //    _columnConfigs = columnConfigs;

        //    lvInfo.BeginUpdate();
        //    lvInfo.Columns.Clear();

        //    foreach (var col in _columnConfigs)
        //        lvInfo.Columns.Add(col.ColumnName, 100, HorizontalAlignment.Center);

        //    _columnMaxWidth = new int[_columnConfigs.Count];
        //    for (int i = 0; i < _columnConfigs.Count; i++)
        //    {
        //        int w;
        //        if (_columnConfigs[i].ColumnName == "序号")
        //            w = TextRenderer.MeasureText("2000", lvInfo.Font).Width + 20;
        //        else
        //            w = TextRenderer.MeasureText(_columnConfigs[i].ColumnName, lvInfo.Font).Width + 10;

        //        _columnMaxWidth[i] = Math.Max(w, 100);

        //        // ==========================================
        //        // 👇 👇 关键：直接赋值固定宽度，不再变化
        //        lvInfo.Columns[i].Width = _columnMaxWidth[i];
        //        // ==========================================
        //    }

        //    lvInfo.EndUpdate();
        //    _isLayoutBusy = false;
        //}

        #endregion

        #region === 安全列宽计算 ===

        //private void SafeResizeColumns()
        //{
        //    // 1. 最小化直接跳过（最强拦截）
        //    if (ParentForm != null && ParentForm.WindowState == FormWindowState.Minimized)
        //        return;

        //    // 2. 状态无效直接跳过
        //    if (!_isLoaded || _isLayoutBusy || lvInfo.Columns.Count == 0 || _columnMaxWidth == null)
        //        return;

        //    // 3. 等待控件渲染完成（关键！解决还原瞬间宽度错误）
        //    if (!lvInfo.IsHandleCreated || lvInfo.ClientSize.Width <= 0)
        //        return;

        //    // 4. 取真实可用宽度（必须等布局完成）
        //    int totalWidth = lvInfo.ClientSize.Width;

        //    // 5. 滚动条计算
        //    bool hasScroll = false;
        //    if (lvInfo.Items.Count > 0)
        //    {
        //        int contentHeight = lvInfo.Items.Count * lvInfo.Font.Height + 40;
        //        hasScroll = contentHeight > lvInfo.ClientSize.Height;
        //    }
        //    if (hasScroll)
        //        totalWidth -= SystemInformation.VerticalScrollBarWidth;

        //    // 6. 比例计算（防止除零）
        //    int totalMin = _columnMaxWidth.Sum();
        //    if (totalMin <= 0) return;

        //    float ratio = 1f;
        //    if (totalWidth > totalMin)
        //        ratio = (float)totalWidth / totalMin;

        //    // 7. 最终赋值（稳定不抖动）
        //    for (int i = 0; i < lvInfo.Columns.Count; i++)
        //    {
        //        int w = (int)(_columnMaxWidth[i] * ratio);
        //        lvInfo.Columns[i].Width = Math.Max(w, 60);
        //    }
        //}

        #endregion


        #region === 刷新逻辑 ===

        private void RefreshUI()
        {
            // === 1. 状态过滤（最重要） ===
            if (UiState.IsPaused || !IsHandleCreated || IsDisposed)
                return;

            if (!Visible || ParentForm?.WindowState == FormWindowState.Minimized)
                return;

            // === 2. 刷新节流（防止UI过载） ===
            var now = DateTime.Now;
            if ((now - _lastRefreshTime).TotalMilliseconds < RefreshIntervalMs)
                return;

            _lastRefreshTime = now;

            var queue = Shared.Instance.dispItem;
            if (queue.IsEmpty) return;

            // === 3. 队列保护（防止最小化期间爆炸） ===
            while (queue.Count > MaxQueueLimit)
            {
                queue.TryDequeue(out _); // 丢弃最旧数据
            }

            // === 4. 分批消费（核心解决UI卡死） ===
            lvInfo.BeginUpdate();
            try
            {
                int processed = 0;
                TestDispItem item;

                while (processed < MaxItemsPerRefresh &&
                       queue.TryDequeue(out item))
                {
                    if (item == null)
                    {
                        lvInfo.Items.Clear();
                        continue;
                    }

                    AddListViewItem(item);
                    processed++;
                }
            }
            finally
            {
                lvInfo.EndUpdate();
            }
        }

        private void AddListViewItem(TestDispItem item)
        {
            var lvItem = new ListViewItem(_globalIndex.ToString());
            _globalIndex++;

            for (int i = 1; i < _columnConfigs.Count; i++)
                lvItem.SubItems.Add(_columnConfigs[i].ValueGetter(item) ?? "");

            lvItem.BackColor = (item.Ok_flag == "NG" || item.MesResult == "NG") ? Color.FromArgb(255, 200, 200) : Color.White;
            lvInfo.Items.Add(lvItem);

            if (lvInfo.Items.Count > 1)
                lvInfo.EnsureVisible(lvInfo.Items.Count - 1);

            if (lvInfo.Items.Count > MaxDisplayCount)
                lvInfo.Items.RemoveAt(0);
        }

        #endregion

        #region === 双击逻辑 ===
        private void LvInfo_DoubleClick(object sender, EventArgs e)
        {
            if (lvInfo.SelectedItems.Count == 0) return;

            var selectedItem = lvInfo.SelectedItems[0];
            string barNo = selectedItem.SubItems.Count > 1 ? selectedItem.SubItems[2].Text : string.Empty;

            // 假设 SubItems 从 0 开始索引
            string okFlag = selectedItem.SubItems.Count >= 2
                ? selectedItem.SubItems[selectedItem.SubItems.Count - 2].Text
                : string.Empty;

            List<SaveItem> items;
            try
            {
                string text = GetSubItemByColumnName(selectedItem, "完成时间");
                string[] formats =
                {
                    "yyyy/M/d H:mm:ss", "yyyy/M/d HH:mm:ss",
                    "yyyy/MM/dd H:mm:ss", "yyyy/MM/dd HH:mm:ss"
                };

                if (!DateTime.TryParseExact(text, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startTime))
                {
                    DisplayLog.Error($"时间解析失败：{text}", null);
                    return;
                }

                var ds = UploadSql.SearchAccessData(_processName, startTime, null, null, "产品条码", barNo);
                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show($"未找到条码 {barNo} 的测试数据");
                    return;
                }

                var dt = ds.Tables[0];
                items = dt.AsEnumerable().Select(r => new SaveItem
                {
                    Bar_no = r["产品条码"].ToString(),
                    Order_no = r["当前工单号"].ToString(),
                    Process_no = r["工位名称"].ToString(),
                    Operator_no = r["操作人员"].ToString(),
                    Do_time = r["测试时间"].ToString(),
                    Ok_flag = r["测试结果"].ToString(),
                    Test_beat = r["测试节拍"]?.ToString(),
                    Test_item_name = r["测试项名称"]?.ToString(),
                    Test_item_up = r["测试项上限"]?.ToString(),
                    Test_item_down = r["测试项下限"]?.ToString(),
                    Test_item_value = r["测试项实际值"]?.ToString(),
                    Test_item_unit = GetUnit(r["测试项名称"]?.ToString()),
                }).ToList();
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询失败 {barNo}", ex);
                MessageBox.Show("查询异常");
                return;
            }

            string partBar = UploadSql.QueryPartBarByMainBar("OP070", barNo);
            new DataDetailForm(items, okFlag, barNo, partBar).ShowDialog();
        }

        private string GetSubItemByColumnName(ListViewItem item, string columnName)
        {
            for (int i = 0; i < lvInfo.Columns.Count; i++)
            {
                if (lvInfo.Columns[i].Text == columnName)
                    return item.SubItems[i].Text;
            }
            return string.Empty;
        }

        public static string GetUnit(string text)
        {
            var match = System.Text.RegularExpressions.Regex.Match(text, @"\((.*?)\)");
            return match.Success ? match.Groups[1].Value : "";
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UiRefreshCenter.OnRefresh -= RefreshUI;
                _customFont?.Dispose();
                _lvHeaderFont?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}