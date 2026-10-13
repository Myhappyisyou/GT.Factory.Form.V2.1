using GT.Factory.Server.Core.Alarms;
using GT.Factory.Server.Core.UserModel;
using GT.Factory.Server.Help;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskContracts.Models;

namespace GT.Factory.Server
{
    public partial class UC_Consumable : UserControl
    {
        // 常量定义
        private const string ID = "ID";
        private const string ProcessNameCol = "ProcessName";
        private const string StationNameCol = "StationName";
        private const string LocationCol = "Location";
        private const string NameCol = "Name";
        private const string TheoreticalCountCol = "TheoreticalCount";
        private const string UsedCountCol = "UsedCount";
        private const string RemainderCountCol = "RemainderCount";

        private readonly Color HeaderForeColor = Color.White;
        private readonly Color HeaderBackColor = Color.FromArgb(46, 51, 73);

        private DataGridView deviceGrid;
        private ToolStrip toolStrip;
        private ToolStripButton btnLoad, btnSave;

        private ContextMenuStrip gridContextMenu;
        private ToolStripMenuItem copyCellMenuItem;
        private ToolStripMenuItem pasteCellMenuItem;
        private ToolStripMenuItem copyRowMenuItem;
        private ToolStripMenuItem pasteRowMenuItem;
        private ToolStripMenuItem deleteMenuItem;

        private string[] clipboardRowCache = null;

        private AccessMdbHelper db;

        string dbPath = @"D:\生产数据\Template.mdb"; // 目标电脑 IP 和共享名
        public UC_Consumable()
        {
            InitializeComponent();

            InitializeUI();

            InitializeContextMenu();

            InitializeDataGridView();

            db = new AccessMdbHelper(dbPath);

            string date = UploadSql.GetNetworkTime(db);

            var value = MonthlyAccessDbManager.GetCurrentMonthDbPath(date);

            db.DatabasePath = value.dbPath;
        }

        private void InitializeUI()
        {
            // ToolStrip
            toolStrip = new ToolStrip();
            btnLoad = new ToolStripButton("加载");
            btnSave = new ToolStripButton("保存");

            toolStrip.Items.AddRange(new[] { btnLoad, btnSave });

            deviceGrid = new DataGridView 
            { 
                Dock = DockStyle.Fill, 
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, 
                MultiSelect = true, 
                ColumnHeadersVisible = true, 
                RowHeadersVisible = false 
            };

            btnLoad.Click += (s, e) => LoadConfig();
            btnSave.Click += (s, e) => SaveConfig();

            // 加载布局

            this.Controls.Add(deviceGrid);

            Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;
        }

        //  初始化dgvAlarmData
        private void InitializeDataGridView()
        {
            deviceGrid.Columns.Clear();
            AddColumn("ID", ID, 120);
            AddColumn("易损件所在工位", ProcessNameCol, 120);
            AddColumn("机台名称", StationNameCol, 120);
            AddColumn("易损件所在位置", StationNameCol, 120);
            AddColumn("易损件名称", NameCol, 120);
            AddColumn("易损件理论使用次数", TheoreticalCountCol, 120);
            AddColumn("易损件已使用次数", UsedCountCol, 300);
            AddColumn("易损件剩余使用次数", RemainderCountCol, 300);
            deviceGrid.Columns["ID"].ReadOnly = true;
        }

        private void InitializeContextMenu()
        {
            gridContextMenu = new ContextMenuStrip();

            copyCellMenuItem = new ToolStripMenuItem("复制单元格", null, (s, e) => CopyCell());
            pasteCellMenuItem = new ToolStripMenuItem("粘贴单元格", null, (s, e) => PasteCell());
            copyRowMenuItem = new ToolStripMenuItem("复制整行", null, (s, e) => CopyRow());
            pasteRowMenuItem = new ToolStripMenuItem("粘贴整行", null, (s, e) => PasteRow());
            deleteMenuItem = new ToolStripMenuItem("删除", null, (s, e) => DeleteSelectedRows());
            gridContextMenu.Items.AddRange(new ToolStripItem[] {
                copyCellMenuItem, pasteCellMenuItem,deleteMenuItem,
                new ToolStripSeparator(),
                copyRowMenuItem, pasteRowMenuItem,deleteMenuItem
            });

            deviceGrid.ContextMenuStrip = gridContextMenu;

            deviceGrid.MouseDown += DeviceGrid_MouseDown;
        }

        private void AddColumn(string headerText, string name, int width)
        {
            var column = new DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Name = name,
                Width = width,
                DataPropertyName = name // 绑定到数据源的属性
            };

            column.HeaderCell.Style.ForeColor = HeaderForeColor;
            column.HeaderCell.Style.BackColor = HeaderBackColor;

            deviceGrid.Columns.Add(column);
        }

        private void LoadConfig()
        {
            deviceGrid.Columns.Clear();
            // 先保存当前行的编辑状态
            deviceGrid.EndEdit();

            // 绑定数据源时允许添加新行
            deviceGrid.DataSource = new BindingList<Consumables>(UploadSql.SelectConsumables(db));

            // 确保允许添加行
            deviceGrid.AllowUserToAddRows = true;
            deviceGrid.Columns["ID"].ReadOnly = true;
        }

        private void SaveConfig()
        {
            deviceGrid.EndEdit();
            Consumables consumables;
            foreach (DataGridViewRow row in deviceGrid.Rows)
            {
                if (row.IsNewRow) continue;
                try
                {
                    consumables = new Consumables
                    {
                        ID = GetCellValue(row, ID).ToInt16(),
                        ProcessName = GetCellValue(row, ProcessNameCol),
                        StationName = GetCellValue(row, StationNameCol),
                        Location = GetCellValue(row, StationNameCol),
                        Name = GetCellValue(row, NameCol),
                        TheoreticalCount = GetCellValue(row, TheoreticalCountCol).ToInt32(),
                        UsedCount = GetCellValue(row, UsedCountCol).ToInt32(),
                        RemainderCount = GetCellValue(row, RemainderCountCol).ToInt32(),
                    };
                    if (consumables.ID==0)
                    {
                        UploadSql.InsertConsumables(db, consumables);
                    }
                    else
                    {
                        UploadSql.InsertOrUpdateConsumablesInfor(db, consumables);
                    }
                }
                catch (Exception)
                {

                    throw;
                }
    
            }
            LoadConfig();
        }

        private string GetCellValue(DataGridViewRow row, string columnName)
        {
            return row.Cells[columnName]?.Value?.ToString() ?? string.Empty;
        }

        private void DeviceGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = deviceGrid.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    deviceGrid.ClearSelection();
                    deviceGrid.Rows[hit.RowIndex].Selected = true;
                    deviceGrid.CurrentCell = deviceGrid.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
                }
            }
        }

        private void CopyCell()
        {
            if (deviceGrid.CurrentCell != null)
            {
                Clipboard.SetText(deviceGrid.CurrentCell.Value?.ToString() ?? "");
            }
        }

        private void PasteCell()
        {
            if (deviceGrid.CurrentCell != null)
            {
                string pasteText = Clipboard.GetText();
                deviceGrid.CurrentCell.Value = pasteText;
            }
        }

        private void CopyRow()
        {
            if (deviceGrid.CurrentRow != null && !deviceGrid.CurrentRow.IsNewRow)
            {
                clipboardRowCache = deviceGrid.Columns
                    .Cast<DataGridViewColumn>()
                    .Select(col => deviceGrid.CurrentRow.Cells[col.Name].Value?.ToString() ?? "")
                    .ToArray();
            }
        }

        private void PasteRow()
        {
            if (clipboardRowCache == null || deviceGrid.CurrentRow == null || deviceGrid.CurrentRow.IsNewRow)
                return;

            int colCount = Math.Min(clipboardRowCache.Length, deviceGrid.Columns.Count);
            for (int i = 0; i < colCount; i++)
            {
                deviceGrid.CurrentRow.Cells[i].Value = clipboardRowCache[i];
            }
        }

        private void DeleteSelectedRows()
        {
            deviceGrid.EndEdit();

            if (deviceGrid.DataSource is BindingList<Consumables> bindingList)
            {
                // 收集要删除的项
                var toRemove = new List<Consumables>();
                foreach (DataGridViewRow row in deviceGrid.SelectedRows)
                {
                    if (!row.IsNewRow && row.DataBoundItem is Consumables item)
                    {
                        toRemove.Add(item);
                        UploadSql.DeleteConsumableInfor(db, item);
                    }
                }

                foreach (var item in toRemove)
                {
                    bindingList.Remove(item);
                }
            }
            LoadConfig();
        }
    }
}
