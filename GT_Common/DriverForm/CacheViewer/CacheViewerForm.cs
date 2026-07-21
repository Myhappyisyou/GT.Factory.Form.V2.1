using GT_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.CacheViewer
{
    public partial class CacheViewerForm : Form
    {
        private DataGridView grid;
        private Button btnRefresh;

        private UploadManager uploadManager;

        public CacheViewerForm()
        {
            this.uploadManager = new UploadManager(); 
            uploadManager.Load();
            Text = "缓存内容查看器";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.CellDoubleClick += Grid_CellDoubleClick; // 绑定双击事件

            btnRefresh = new Button
            {
                Text = "手动刷新",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.LightSteelBlue
            };
            btnRefresh.Click += (s, e) => RefreshGrid();

            Controls.Add(grid);
            Controls.Add(btnRefresh);

            // 定时自动刷新
            //refreshTimer = new Timer();
            //refreshTimer.Interval = 2000; // 2 秒刷新一次
            //refreshTimer.Tick += (s, e) => RefreshGrid();
            //refreshTimer.Start();

            Load += (s, e) => RefreshGrid();
        }

        private void RefreshGrid()
        {
            uploadManager.Load();

            if (uploadManager?.Buffer?.Items == null) return;

            var data = uploadManager.Buffer.Items
                .Select(item => new
                {
                    SN = item.Sn,
                    Do_time = item.Do_time,
                    Step = item.Step,
                    IsLastStep = item.IsLastStep,
                    Result = item.Result,
                    NgMsg = item.NgMsg,
                    DataCount = item.Data?.Length ?? 0,
                    DataPreview = string.Join(",", item.Data?.Take(5) ?? Array.Empty<string>()) // 预览前5个
                })
                .ToList();

            grid.DataSource = data;
        }

        private void Grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string sn = grid.Rows[e.RowIndex].Cells["SN"].Value?.ToString();
            int step = Convert.ToInt32(grid.Rows[e.RowIndex].Cells["Step"].Value);

            var item = uploadManager.Buffer.Items
                .FirstOrDefault(x => x.Sn == sn && x.Step == step);

            if (item != null)
            {
                var detailForm = new DataItemDetailForm(item);
                detailForm.ShowDialog();
            }
        }
    }
}
