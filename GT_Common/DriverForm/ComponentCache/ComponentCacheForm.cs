using GT_Common;
using GT_Common.DriverForm.CacheViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.ComponentCache
{

    public partial class ComponentCacheForm : Form
    {
        private DataGridView grid;
        private Button btnRefresh;

        private ComponentManager componentManager;

        public ComponentCacheForm()
        {
            this.componentManager = new ComponentManager();
            componentManager.Load();
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
            componentManager.Load();

            if (componentManager?._buffer?.Items == null) return;

            var data = componentManager._buffer.Items
                .Select(item => new
                {
                    AssemblyCode = item.AssemblyCode,
                    ComponentCode = item.ComponentCode,
                    Timestamp = item.Timestamp,
                })
                .ToList();

            grid.DataSource = data;
        }

    }

}
