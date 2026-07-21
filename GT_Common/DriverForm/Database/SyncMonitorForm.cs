using GT_Common.Helper.Database.Abstractions;
using GT_Common.Helper.Database.Repository;
using GT_Common.Helper.Database.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Database
{
    /// <summary>
    /// 数据库诊断中心
    /// 
    /// 功能：
    /// 1️⃣ 主库健康检测
    /// 2️⃣ 缓存队列统计
    /// 3️⃣ 缓存明细展示
    /// 4️⃣ 手动同步
    /// 5️⃣ 清理失败数据
    /// 6️⃣ 测试SQL执行
    /// </summary>
    public partial class SyncMonitorForm : Form
    {
        private Label lblDbStatus;
        private Label lblPendingCount;
        private Label lblFailedCount;
        private Label lblTotalCount;
        private Label lblLastCheckTime;

        private Button btnSyncNow;
        private Button btnRefresh;
        private Button btnClearFailed;
        private Button btnTestPrimary;
        private Button btnTestCache;
        private Button btnExecuteSql;

        private DataGridView dgvCache;
        private DataGridView dgvSqlResult;

        private TextBox txtSql;

        private readonly DatabaseHealthMonitor _healthMonitor;
        private readonly SyncQueueRepository _syncRepo;
        private readonly SyncService _syncService;
        private readonly IDatabase _primary;

        public SyncMonitorForm(
            DatabaseHealthMonitor healthMonitor,
            SyncQueueRepository syncRepo,
            SyncService syncService,
            IDatabase primary)
        {
            _healthMonitor = healthMonitor;
            _syncRepo = syncRepo;
            _syncService = syncService;
            _primary = primary;

            InitializeLayout();
        }

        #region 布局初始化

        private void InitializeLayout()
        {
            this.Text = "数据库诊断中心";
            this.Width = 900;
            this.Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 10);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(15)
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));   // 状态区
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));  // 统计区（增加高度）
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));    // 缓存区（增加占比）
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));  // SQL区
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));    // SQL结果区

            #region 状态区

            var statusPanel = new Panel { Dock = DockStyle.Fill };

            lblDbStatus = new Label { AutoSize = true, Location = new Point(0, 0) };
            lblLastCheckTime = new Label { AutoSize = true, Location = new Point(0, 30) };

            statusPanel.Controls.Add(lblDbStatus);
            statusPanel.Controls.Add(lblLastCheckTime);

            #endregion

            #region 统计区 + 操作按钮

            var statLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(0, 5, 0, 5)
            };


            statLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            statLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            statLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));


            lblPendingCount = CreateLabel("待同步数量：0");
            lblFailedCount = CreateLabel("永久失败数量：0");
            lblTotalCount = CreateLabel("总缓存数量：0");

            btnSyncNow = CreateButton("立即同步");
            btnRefresh = CreateButton("刷新");
            btnClearFailed = CreateButton("清理失败");
            btnTestPrimary = CreateButton("测试主库连接");
            btnTestCache = CreateButton("测试缓存连接");

            btnSyncNow.Click += BtnSyncNow_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnClearFailed.Click += BtnClearFailed_Click;
            btnTestPrimary.Click += BtnTestPrimary_Click;
            btnTestCache.Click += BtnTestCache_Click;

            statLayout.Controls.Add(lblPendingCount, 0, 0);
            statLayout.Controls.Add(lblFailedCount, 1, 0);
            statLayout.Controls.Add(lblTotalCount, 2, 0);

            statLayout.Controls.Add(btnSyncNow, 0, 1);
            statLayout.Controls.Add(btnRefresh, 1, 1);
            statLayout.Controls.Add(btnClearFailed, 2, 1);

            statLayout.Controls.Add(btnTestPrimary, 0, 2);
            statLayout.Controls.Add(btnTestCache, 1, 2);

            #endregion

            #region 缓存区

            dgvCache = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ReadOnly = true,
                Margin = new Padding(0, 10, 0, 10)
            };

            // ✅ 强制列头高度
            dgvCache.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvCache.ColumnHeadersHeight = 45;   // 推荐 40~50

            dgvCache.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft YaHei UI", 10, FontStyle.Bold);

            dgvCache.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvCache.RowTemplate.Height = 30;
            dgvCache.DefaultCellStyle.Padding = new Padding(5);

            #endregion

            #region SQL测试区

            var sqlPanel = new Panel { Dock = DockStyle.Fill };

            txtSql = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Height = 80,
                ScrollBars = ScrollBars.Vertical
            };

            btnExecuteSql = CreateButton("执行SQL");
            btnExecuteSql.Dock = DockStyle.Right;
            btnExecuteSql.Width = 100;
            btnExecuteSql.Click += BtnExecuteSql_Click;

            sqlPanel.Controls.Add(txtSql);
            sqlPanel.Controls.Add(btnExecuteSql);

            #endregion

            #region SQL结果区

            dgvSqlResult = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // ✅ 强制列头高度
            dgvSqlResult.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgvSqlResult.ColumnHeadersHeight = 45;   // 推荐 40~50

            dgvSqlResult.ColumnHeadersDefaultCellStyle.Font =
                new Font("Microsoft YaHei UI", 10, FontStyle.Bold);

            dgvSqlResult.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            dgvSqlResult.RowTemplate.Height = 30;
            dgvSqlResult.DefaultCellStyle.Padding = new Padding(5);

            #endregion

            mainLayout.Controls.Add(statusPanel, 0, 0);
            mainLayout.Controls.Add(statLayout, 0, 1);
            mainLayout.Controls.Add(dgvCache, 0, 2);
            mainLayout.Controls.Add(sqlPanel, 0, 3);
            mainLayout.Controls.Add(dgvSqlResult, 0, 4);

            this.Controls.Add(mainLayout);

            this.Load += SyncMonitorForm_Load;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Padding = new Padding(5)
            };
        }

        private Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(5)
            };
        }

        #endregion

        #region 逻辑

        private async void SyncMonitorForm_Load(object sender, EventArgs e)
        {
            await RefreshStatusAsync();
        }

        private async Task RefreshStatusAsync()
        {
            bool healthy = await _healthMonitor.IsHealthyAsync();

            lblDbStatus.Text = healthy ? "✅ 主库正常" : "❌ 主库离线";
            lblDbStatus.ForeColor = healthy ? Color.Green : Color.Red;
            lblLastCheckTime.Text = $"最近检测：{DateTime.Now:HH:mm:ss}";

            var all = await _syncRepo.GetAllAsync();

            lblPendingCount.Text = $"待同步数量：{all.Count(x => x.Status == 0)}";
            lblFailedCount.Text = $"永久失败数量：{all.Count(x => x.Status == 2)}";
            lblTotalCount.Text = $"总缓存数量：{all.Count}";

            dgvCache.DataSource = all;
        }

        private async void BtnSyncNow_Click(object sender, EventArgs e)
        {
            await _syncService.SyncAsync();
            await RefreshStatusAsync();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshStatusAsync();
        }

        private async void BtnClearFailed_Click(object sender, EventArgs e)
        {
            var all = await _syncRepo.GetAllAsync();
            var failed = all.Where(x => x.Status == 2).ToList();

            foreach (var item in failed)
                await _syncRepo.DeleteAsync(item.Id);

            await RefreshStatusAsync();
        }

        private async void BtnTestPrimary_Click(object sender, EventArgs e)
        {
            bool healthy = await _healthMonitor.IsHealthyAsync();
            await RefreshStatusAsync();

            MessageBox.Show(healthy ? "主库连接正常 ✅" : "主库连接失败 ❌");
        }

        private async void BtnTestCache_Click(object sender, EventArgs e)
        {
            try
            {
                await _syncRepo.GetAllAsync();
                MessageBox.Show("缓存数据库连接正常 ✅");
            }
            catch (Exception ex)
            {
                MessageBox.Show("缓存数据库连接失败 ❌\n" + ex.Message);
            }
        }

        private async void BtnExecuteSql_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await _primary.QueryAsync<dynamic>(txtSql.Text);

                dgvSqlResult.DataSource = ToDataTable(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL执行失败:\n" + ex.Message);
            }
        }

        #endregion


        private DataTable ToDataTable(IEnumerable<dynamic> items)
        {
            var table = new DataTable();

            if (items == null || !items.Any())
                return table;

            var first = (IDictionary<string, object>)items.First();

            // ✅ 创建列
            foreach (var key in first.Keys)
            {
                table.Columns.Add(key);
            }

            // ✅ 添加行
            foreach (var item in items)
            {
                var dict = (IDictionary<string, object>)item;
                table.Rows.Add(dict.Values.ToArray());
            }

            return table;
        }
    }
}