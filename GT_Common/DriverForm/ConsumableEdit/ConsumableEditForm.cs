using GT_Common;
using GT_Common.Helper;
using GT_Common.Helper.ClientTask;
using GT_Common.Helper.QueryClient;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using TaskContracts.Models;
using Consumables = TaskContracts.Models.Consumables;

namespace GT_Common.DriverForm.ConsumableEdit
{
    public partial class ConsumableEditForm : Form
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

        private string _process_no = null;

        private readonly ConsumablesApiClient _client;
        private readonly ClientTaskSender _clientTaskSender;

        public ConsumableEditForm(string process_no, string serverUrl, string taskServerUrl)
        {
            InitializeComponent();

            InitializeUI();

            InitializeContextMenu();

            InitializeDataGridView();

            _process_no = process_no;
            _client = new ConsumablesApiClient(serverUrl);
            _clientTaskSender = new ClientTaskSender(taskServerUrl);
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
            deviceGrid.CellFormatting += DataGridView1_CellFormatting;
            btnLoad.Click += async (s, e) => await LoadConfigAsync();
            btnSave.Click += async (s, e) => SaveConfigAsync();

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
            AddColumn("易损件所在工位", ProcessNameCol, 200);
            AddColumn("机台名称", StationNameCol, 200);
            AddColumn("易损件所在位置", LocationCol, 200);
            AddColumn("易损件名称", NameCol, 200);
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

            deleteMenuItem = new ToolStripMenuItem("删除", null, async (s, e) => DeleteSelectedRowsAsync());

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

        private async Task LoadConfigAsync()
        {
            try
            {
                deviceGrid.Columns.Clear();
                deviceGrid.EndEdit();

                // 异步等待，不阻塞 UI
                List<Consumables> consumables = await _client.GetProcessNoConsumablesAsync(_process_no);

                // 检查并提示耗材
                CommMethod.CheckAndAlertConsumables(consumables);

                // 绑定数据源
                deviceGrid.DataSource = new BindingList<Consumables>(consumables);

                // 允许添加行
                deviceGrid.AllowUserToAddRows = true;
                deviceGrid.Columns["ID"].ReadOnly = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}");
            }
        }

        //  保存
        private async Task SaveConfigAsync()
        {
            deviceGrid.EndEdit();

            if (!(deviceGrid.DataSource is BindingList<Consumables> bindingList)) return;

            foreach (DataGridViewRow row in deviceGrid.Rows)
            {
                if (row.IsNewRow) continue;
                try
                {
                    var consumable = new Consumables
                    {
                        ID = GetCellValue(row, ID).ToInt16(),
                        ProcessName = GetCellValue(row, ProcessNameCol),
                        StationName = GetCellValue(row, StationNameCol),
                        Location = GetCellValue(row, LocationCol),
                        Name = GetCellValue(row, NameCol),
                        TheoreticalCount = GetCellValue(row, TheoreticalCountCol).ToInt32(),
                        UsedCount = GetCellValue(row, UsedCountCol).ToInt32(),
                        RemainderCount = GetCellValue(row, RemainderCountCol).ToInt32(),
                    };

                    // 异步保存，不阻塞 UI
                    await _client.InsertOrUpdateConsumableAsync(consumable);

                    DatabaseSessionManager.EnsureDatabase();

                    var db = DbContext.CurrentDb;

                    UploadSql.Ac_UpdateConsumables(db, consumable.ToLocalConsumable());

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}");
                    return;
                }
            }

            // 保存完成后刷新列表
            await LoadConfigAsync();
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
            if (deviceGrid.CurrentCell != null && deviceGrid.CurrentCell.ColumnIndex != 0)
            {
                Clipboard.SetText(deviceGrid.CurrentCell.Value?.ToString() ?? "");
            }
        }

        private void PasteCell()
        {
            if (deviceGrid.CurrentCell != null && deviceGrid.CurrentCell.ColumnIndex != 0)
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
                clipboardRowCache[0] = "0";
            }
        }

        private void PasteRow()
        {
            if (clipboardRowCache == null || deviceGrid.CurrentRow == null)
                return;

            int colCount = Math.Min(clipboardRowCache.Length, deviceGrid.Columns.Count);
            for (int i = 0; i < colCount; i++)
            {
                deviceGrid.CurrentRow.Cells[i].Value = clipboardRowCache[i];
            }
            // 确保允许添加行
            deviceGrid.AllowUserToAddRows = true;
            deviceGrid.Columns["ID"].ReadOnly = true;
        }

        //  删除
        private async void DeleteSelectedRowsAsync()
        {
            deviceGrid.EndEdit();

            if (deviceGrid.DataSource is BindingList<Consumables> bindingList)
            {
                var toRemove = new List<Consumables>();
                foreach (DataGridViewRow row in deviceGrid.SelectedRows)
                {
                    if (!row.IsNewRow && row.DataBoundItem is Consumables item)
                    {
                        toRemove.Add(item);
                    }
                }

                foreach (var item in toRemove)
                {
                    try
                    {
                        await _client.DeleteConsumableAsync(item); // 异步删除
                        bindingList.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除失败: {ex.Message}");
                    }
                }
            }

            // 刷新列表
            await LoadConfigAsync();
        }
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var grid = (DataGridView)sender;
            var consumable = grid.Rows[e.RowIndex].DataBoundItem as Consumables;

            if (consumable == null) return;

            double percentage = consumable.GetRemainderPercentage();

            // 针对"剩余使用次数"列设置颜色
            if (grid.Columns[e.ColumnIndex].DataPropertyName == "RemainderCount")
            {
                if (percentage <= 5) // 严重 ≤5%
                {
                    e.CellStyle.BackColor = Color.LightCoral;
                    e.CellStyle.ForeColor = Color.DarkRed;
                    e.CellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                }
                else if (percentage <= 10) // 警告 5%-10%
                {
                    e.CellStyle.BackColor = Color.LightSalmon;
                    e.CellStyle.ForeColor = Color.OrangeRed;
                }
                else if (percentage <= 20) // 注意 10%-20%
                {
                    e.CellStyle.BackColor = Color.LightYellow;
                    e.CellStyle.ForeColor = Color.Goldenrod;
                }
            }

            //// 添加百分比显示
            //if (grid.Columns[e.ColumnIndex].DataPropertyName == "RemainderCount")
            //{
            //    e.Value = $"{consumable.RemainderCount} 次 ({percentage:F1}%)";
            //}
        }
    }
}
