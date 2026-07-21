using GT_Common.Util;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GT_Common.Helper;
using GT_Common.Model;
using Newtonsoft.Json;
using System.IO;
using GT_Common.Helper.UIHelp;

namespace GT_Common.Components
{
    public partial class CurrentProductDataUI : UserControl
    {
        private readonly Font _customFont = new Font("Segoe UI", 15f);
        private readonly Font _txtSnFont = new Font("Segoe UI", 20f);
        private Dictionary<string, DataGridView> _gridViews = new Dictionary<string, DataGridView>();
        private Dictionary<string, TextBox> _dcTxtSn = new Dictionary<string, TextBox>();
        private TabControl _tabControl;

        // 添加主题颜色定义
        private  Color TabSelectedColor = Color.FromArgb(0, 122, 204);        // 选中状态 - 蓝色
        private  Color TabSelectedTextColor = Color.White;                    // 选中文本 - 白色
        private  Color TabUnselectedColor = Color.FromArgb(240, 240, 240);   // 未选中状态 - 浅灰色
        private  Color TabUnselectedTextColor = Color.FromArgb(68, 68, 68);  // 未选中文本 - 深灰色
        private  Color TabHoverColor = Color.FromArgb(220, 220, 220);        // 悬停状态 - 中灰色
        private  Color TabBorderColor = Color.FromArgb(200, 200, 200);       // 边框颜色

        public CurrentProductDataUI()
        {
            InitializeComponent();

            this.AutoSize = true;

            // 应用主题（三选一）
            ApplyProfessionalBlueTheme();  // 
                                           // ApplyDarkTheme();           // 或者深色主题
                                           // ApplyGreenTheme();          // 或者绿色主题

            InitFormLayout();

            UiRefreshCenter.OnRefresh += RefreshUI;

        }

        private void InitFormLayout()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            _tabControl = new DraggableTabControl
            {
                Dock = DockStyle.Fill,
            };

            ((DraggableTabControl)_tabControl).TabOrderChanged += SaveTabOrder;

            mainPanel.Controls.Add(_tabControl);

            this.Controls.Add(mainPanel);
        }

        private void InitGridColumnWidth(DataGridView gridView)
        {
            gridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            // 第一列自适应
            gridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            gridView.Columns[0].MinimumWidth = 200;

            // 第一列留白
            gridView.Columns[0].DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);

            // 其他列填充
            for (int i = 1; i < gridView.Columns.Count; i++)
            {
                gridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void RefreshUI()
        {
            if (!IsHandleCreated || IsDisposed) return;

            if (!Visible || ParentForm?.WindowState == FormWindowState.Minimized)
                return;

            List<KeyValuePair<string, currentProductDataModel>> snapshot;

            lock (Shared._dicCurrentTestItemMDsLock)
            {
                if (Shared.dicCurrentTestItemMDs == null || Shared.dicCurrentTestItemMDs.Count == 0)
                    return;

                snapshot = Shared.dicCurrentTestItemMDs.ToList();
            }

            foreach (var kvp in snapshot)
            {
                string stationName = kvp.Key;
                var data = kvp.Value;

                if (!_gridViews.ContainsKey(stationName))
                {
                    CreateStationTab(stationName, data);
                }
                else
                {
                    UpdateStationTab(stationName, data);
                }
            }
        }

        private void CreateStationTab(string stationName, currentProductDataModel data)
        {
            var tabPage = new TabPage(stationName) { Name = stationName };

            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
            };

            layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var txtSn = new TextBox()
            {
                Dock = DockStyle.Fill,
                Font = _txtSnFont,
                Text = data.Sn,
                //BackColor = Color.Transparent,
                ForeColor = TabSelectedColor,  // 使用主题色
                ReadOnly = true,
            };
            layoutPanel.Controls.Add(txtSn, 0, 0);
            _dcTxtSn[stationName] = txtSn;

            var gridView = new DataGridView
            {
                Font = _customFont,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Both,
                DataSource = data.Data,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.Red, // ⚡ 确保网格线可见
                CellBorderStyle = DataGridViewCellBorderStyle.Single, // ⚡ 显示横纵线

            };

            // 绑定事件
            //gridView.DataBindingComplete += Grid_DataBindingComplete;
            gridView.DataBindingComplete += (s, e) =>
            {
                InitGridColumnWidth(gridView);
            };

            gridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 禁止用户选中行
            gridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 51, 76);

            // 专业深色表头
            gridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(51, 51, 76);
            gridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            gridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridView.ColumnHeadersDefaultCellStyle.Padding = new Padding(3);

            // 表头边框
            gridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            gridView.ColumnHeadersHeight = 40;

            // 行样式
            gridView.RowsDefaultCellStyle.Font = new Font("Segoe UI", 10);
            gridView.RowsDefaultCellStyle.Padding = new Padding(2);

            // 网格线
            gridView.GridColor = Color.White;
            gridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // 禁用表头选择
            gridView.EnableHeadersVisualStyles = false;
            gridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // 禁用整个控件的选择
            gridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridView.MultiSelect = false;

            EnableDoubleBuffer(gridView);

            gridView.DataBindingComplete += (s, args) =>
            {
                if (gridView.Columns.Count >= 5)
                {
                    gridView.Columns[0].HeaderText = "测试项";
                    gridView.Columns[1].HeaderText = "上限";
                    gridView.Columns[2].HeaderText = "测试值";
                    gridView.Columns[3].HeaderText = "下限";
                    gridView.Columns[4].HeaderText = "结果";
                    // 设置列名
                    gridView.Columns[4].Name = "Result";
                }
                gridView.ClearSelection();
            };

            gridView.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return; // 跳过表头

                var row = gridView.Rows[e.RowIndex];

                // 背景色
                Color bgColor = Color.White;
                var resultValue = row.Cells[4].Value?.ToString();
                if (!string.IsNullOrEmpty(resultValue) && resultValue.Contains("NG"))
                    bgColor = Color.IndianRed;

                using (var brush = new SolidBrush(bgColor))
                    e.Graphics.FillRectangle(brush, e.CellBounds);

                // 文本
                Color textColor = bgColor == Color.IndianRed ? Color.White : Color.Black;
                TextRenderer.DrawText(
                    e.Graphics,
                    e.FormattedValue?.ToString(),
                    e.CellStyle.Font,
                    e.CellBounds,
                    textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                );

                // 绘制网格线
                using (var pen = new Pen(Color.LightGray))
                {
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1); // 下边线
                    e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);   // 右边线
                }

                e.Handled = true;
            };

            gridView.DefaultCellStyle.SelectionBackColor = gridView.DefaultCellStyle.BackColor;
            gridView.DefaultCellStyle.SelectionForeColor = gridView.DefaultCellStyle.ForeColor;

            _gridViews[stationName] = gridView;

            layoutPanel.Controls.Add(gridView, 0, 1);
            tabPage.Controls.Add(layoutPanel);

            var order = LoadTabOrder();
            int insertIndex = order.IndexOf(stationName);
            if (insertIndex < 0 || insertIndex > _tabControl.TabPages.Count)
                insertIndex = _tabControl.TabPages.Count; // 新增在最后

            _tabControl.TabPages.Insert(insertIndex, tabPage);

            // 强制重绘 TabControl
            _tabControl.Invalidate();

        }

        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var gridView = sender as DataGridView;

            InitGridColumnWidth(gridView);

            if (gridView.Columns.Count >= 5)
            {
                gridView.Columns[0].HeaderText = "测试项";
                gridView.Columns[1].HeaderText = "上限";
                gridView.Columns[2].HeaderText = "测试值";
                gridView.Columns[3].HeaderText = "下限";
                gridView.Columns[4].HeaderText = "结果";
                gridView.Columns[4].Name = "Result";
            }

            gridView.ClearSelection();
        }

        private void UpdateStationTab(string stationName, currentProductDataModel data)
        {
            // 更新SN文本
            var txtSn = _dcTxtSn[stationName];
            if (txtSn.Text != data.Sn)
                txtSn.Text = data.Sn;

            // 更新表格数据，不重新绑定
            var gridView = _gridViews[stationName];
            var bindingList = gridView.DataSource as BindingList<TestItemMD>;
            if (bindingList != null)
            {
                for (int i = 0; i < data.Data.Count; i++)
                {
                    if (i < bindingList.Count)
                    {
                        // 更新已有行
                        bindingList[i].Col0 = data.Data[i].Col0;
                        bindingList[i].Col1 = data.Data[i].Col1;
                        bindingList[i].Col2 = data.Data[i].Col2;
                        bindingList[i].Col3 = data.Data[i].Col3;
                        bindingList[i].Col4 = data.Data[i].Col4;
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Data.Count && i < gridView.Rows.Count; i++)
                {
                    gridView.Rows[i].Cells[0].Value = data.Data[i].Col0;
                    gridView.Rows[i].Cells[1].Value = data.Data[i].Col1;
                    gridView.Rows[i].Cells[2].Value = data.Data[i].Col2;
                    gridView.Rows[i].Cells[3].Value = data.Data[i].Col3;
                    gridView.Rows[i].Cells[4].Value = data.Data[i].Col4;
                }
            }

            gridView.Invalidate(); // 仅重绘，不闪烁
        }

        // 启用双缓冲
        private void EnableDoubleBuffer(DataGridView dgv)
        {
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.SetProperty,
                null, dgv, new object[] { true });
        }

        // ✅ 可选：专业蓝色主题
        private void ApplyProfessionalBlueTheme()
        {
            TabSelectedColor = Color.FromArgb(41, 128, 185);     // 专业蓝
            TabSelectedTextColor = Color.White;
            TabUnselectedColor = Color.FromArgb(245, 245, 245);  // 浅灰
            TabUnselectedTextColor = Color.FromArgb(102, 102, 102);
            TabHoverColor = Color.FromArgb(224, 237, 247);       // 浅蓝灰
            TabBorderColor = Color.FromArgb(221, 221, 221);
        }

        // ✅ 可选：深色主题
        private void ApplyDarkTheme()
        {
            TabSelectedColor = Color.FromArgb(51, 51, 76);       // 深蓝紫
            TabSelectedTextColor = Color.White;
            TabUnselectedColor = Color.FromArgb(68, 68, 68);     // 深灰
            TabUnselectedTextColor = Color.FromArgb(200, 200, 200);
            TabHoverColor = Color.FromArgb(85, 85, 85);          // 中灰
            TabBorderColor = Color.FromArgb(100, 100, 100);
        }

        // ✅ 可选：绿色主题
        private void ApplyGreenTheme()
        {
            TabSelectedColor = Color.FromArgb(76, 175, 80);      // 成功绿
            TabSelectedTextColor = Color.White;
            TabUnselectedColor = Color.FromArgb(245, 245, 245);
            TabUnselectedTextColor = Color.FromArgb(102, 102, 102);
            TabHoverColor = Color.FromArgb(232, 245, 233);       // 浅绿
            TabBorderColor = Color.FromArgb(221, 221, 221);
        }

        #region tab排序

        //  保存
        void SaveTabOrder()
        {
            var order = _tabControl.TabPages
                .Cast<TabPage>()
                .Select(t => t.Name)
                .ToList();

            File.WriteAllText(PathCenter.ConfigFile("Taborder.json"),
                JsonConvert.SerializeObject(order));
        }

        //  加载
        List<string> LoadTabOrder()
        {
            if (!File.Exists(PathCenter.ConfigFile("Taborder.json")))
                return new List<string>();

            return JsonConvert.DeserializeObject<List<string>>(
                File.ReadAllText(PathCenter.ConfigFile("Taborder.json")));
        }

        void RestoreTabOrder(List<string> order)
        {
            if (order == null || order.Count == 0) return;

            var map = _tabControl.TabPages
                .Cast<TabPage>()
                .ToDictionary(t => t.Name);

            var selected = _tabControl.SelectedTab;

            _tabControl.TabPages.Clear();

            // 按历史顺序恢复
            foreach (var name in order)
            {
                if (map.TryGetValue(name, out var page))
                    _tabControl.TabPages.Add(page);
            }

            // 新增的站点补到最后
            foreach (var page in map.Values)
            {
                if (!_tabControl.TabPages.Contains(page))
                    _tabControl.TabPages.Add(page);
            }

            if (selected != null && _tabControl.TabPages.Contains(selected))
                _tabControl.SelectedTab = selected;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
               
                UiRefreshCenter.OnRefresh -= RefreshUI;

                components?.Dispose();
                // 释放 Fonts
                _customFont?.Dispose();
                _txtSnFont?.Dispose();

                // 释放 DataGridView
                if (_gridViews != null)
                {
                    foreach (var dgv in _gridViews.Values)
                    {
                        dgv.DataBindingComplete -= null; // 如果有事件，要解绑
                        dgv.RowPrePaint -= null;
                        dgv.Dispose();
                    }
                    _gridViews.Clear();
                }

                // 释放 TextBox
                if (_dcTxtSn != null)
                {
                    foreach (var txt in _dcTxtSn.Values)
                    {
                        txt.Dispose();
                    }
                    _dcTxtSn.Clear();
                }
            }

            base.Dispose(disposing);
        }
    }
}
