using GT_Common.Helper;
using GT_Common.ProcessConfig;
using GT_Common.Helper.Alarms;
using GT_Common.Model;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GT_Common.DriverForm.Alarm
{
    public partial class AlarmForm : Form
    {

        // 常量定义
        private const string ID = "ID";

        private const string ProcessNoCol = "ProcessNo";
        private const string ProcessNameCol = "ProcessName";
        private const string AlarmStationCol = "AlarmStation";
        private const string AlarmGradeCol = "AlarmGrade";
        private const string PlcAddressCol = "PlcAddr";
        private const string AlarmParseCol = "Description";

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

        public AlarmForm()
        {
            InitializeComponent();

            InitializeUI();

            //InitializeContextMenu(); 

            InitializeDataGridView();
        }

        private void InitializeUI()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "报警配置编辑";
            this.BackColor = Color.White;
            this.Size = new Size(1140, 600);

            // ToolStrip 初始化
            toolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.System,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(46, 51, 73),
                ForeColor = Color.White,
                Padding = new Padding(5, 3, 5, 3),
                Font = new Font("微软雅黑", 10F, FontStyle.Regular)
            };

            btnLoad = new ToolStripButton("加载")
            {
                Image = SystemIcons.Information.ToBitmap(), // 你可以换成自定义图标
                ImageTransparentColor = Color.Magenta,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Margin = new Padding(3, 1, 3, 1)
            };

            btnSave = new ToolStripButton("保存")
            {
                Image = SystemIcons.Shield.ToBitmap(),
                ImageTransparentColor = Color.Magenta,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Margin = new Padding(3, 1, 3, 1)
            };

            // 鼠标悬停高亮效果
            btnLoad.MouseEnter += (s, e) => ((ToolStripButton)s).BackColor = Color.FromArgb(70, 80, 110);
            btnLoad.MouseLeave += (s, e) => ((ToolStripButton)s).BackColor = Color.FromArgb(46, 51, 73);
            btnSave.MouseEnter += (s, e) => ((ToolStripButton)s).BackColor = Color.FromArgb(70, 80, 110);
            btnSave.MouseLeave += (s, e) => ((ToolStripButton)s).BackColor = Color.FromArgb(46, 51, 73);

            toolStrip.Items.AddRange(new ToolStripItem[]
            {
                btnLoad,
                new ToolStripSeparator(),
                btnSave
            });

            deviceGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };

            // 表头样式
            deviceGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 51, 73);
            deviceGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            deviceGrid.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            btnLoad.Click += (s, e) => LoadConfig();
            btnSave.Click += (s, e) => SaveConfig();

            Controls.Add(deviceGrid);
            Controls.Add(toolStrip);
        }


        //private void InitializeUI()
        //{
        //    this.StartPosition = FormStartPosition.CenterScreen;

        //    // ToolStrip
        //    toolStrip = new ToolStrip();
        //    btnLoad = new ToolStripButton("加载");
        //    btnSave = new ToolStripButton("保存");
          
        //    toolStrip.Items.AddRange(new[] { btnLoad, btnSave });

        //    deviceGrid = new DataGridView { Dock = DockStyle.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = true,ColumnHeadersVisible=true,RowHeadersVisible=false };

        //    deviceGrid.EnableHeadersVisualStyles = false;
        //    deviceGrid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBackColor;
        //    deviceGrid.ColumnHeadersDefaultCellStyle.ForeColor = HeaderForeColor;
        //    deviceGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        //    deviceGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 120, 180);
        //    deviceGrid.DefaultCellStyle.SelectionForeColor = Color.White;
        //    deviceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;


        //    btnLoad.Click += (s, e) => LoadConfig();
        //    btnSave.Click += (s, e) => SaveConfig();

        //    // 加载布局
         
        //    this.Controls.Add(deviceGrid);

        //    Controls.Add(toolStrip);
        //    toolStrip.Dock = DockStyle.Top;
        //}

        //  初始化dgvAlarmData
        private void InitializeDataGridView()
        {
            deviceGrid.Columns.Clear();
            AddColumn("ID", ID, 120,true);

            AddColumn("工序号", ProcessNoCol, 120, true);
            AddColumn("机台名称", ProcessNameCol, 120, true);
            AddColumn("故障发生工位", AlarmStationCol, 120);
            AddColumn("故障类型", AlarmGradeCol, 120);
            AddColumn("PLC地址", PlcAddressCol, 120, true);
            AddColumn("报警信息", AlarmParseCol, 400);
        }

        private void InitializeContextMenu()
        {
            gridContextMenu = new ContextMenuStrip();

            copyCellMenuItem = new ToolStripMenuItem("复制单元格", null, (s, e) => CopyCell());
            pasteCellMenuItem = new ToolStripMenuItem("粘贴单元格", null, (s, e) => PasteCell());
            copyRowMenuItem = new ToolStripMenuItem("复制整行", null, (s, e) => CopyRow());
            pasteRowMenuItem = new ToolStripMenuItem("粘贴整行", null, (s, e) => PasteRow());
            deleteMenuItem = new ToolStripMenuItem("删除",null,(s, e) => DeleteSelectedRows());

            gridContextMenu.Items.AddRange(new ToolStripItem[] {
                copyCellMenuItem, pasteCellMenuItem,deleteMenuItem,
                new ToolStripSeparator(),
                copyRowMenuItem, pasteRowMenuItem,deleteMenuItem
            });

            deviceGrid.ContextMenuStrip = gridContextMenu;

            deviceGrid.MouseDown += DeviceGrid_MouseDown;
        }

        private void AddColumn(string headerText, string name, int width,bool readOnly=false)
        {
            var column = new DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Name = name,
                Width = width,
                DataPropertyName = name, // 绑定到数据源的属性
                ReadOnly = readOnly,
            };

            column.HeaderCell.Style.ForeColor = HeaderForeColor;
            column.HeaderCell.Style.BackColor = HeaderBackColor;

            deviceGrid.Columns.Add(column);
        }

        //private void LoadConfig()
        //{
        //    //deviceGrid.Columns.Clear();
        //    // 先保存当前行的编辑状态
        //    deviceGrid.EndEdit();

        //    // 不要清除列，保持列设置
        //    // deviceGrid.Columns.Clear();  // 移除这行

        //    // 加载数据
        //    var alarmParses = AlarmConfigManager.Load();
        //    deviceGrid.AutoGenerateColumns = false;

        //    // 绑定数据源时允许添加新行
        //    deviceGrid.DataSource = new BindingList<AlarmParse>(alarmParses);

        //    // 确保允许添加行
        //    deviceGrid.AllowUserToAddRows = false;
        //}

        private void LoadConfig()
        {
            try
            {
                deviceGrid.EndEdit();

                var alarmParses = AlarmConfigManager.Load() ?? new List<AlarmParse>();
                deviceGrid.AutoGenerateColumns = false;
                deviceGrid.DataSource = new BindingList<AlarmParse>(alarmParses);
                deviceGrid.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveConfig()
        {
            try
            {
                deviceGrid.EndEdit();

                var list = deviceGrid.DataSource as BindingList<AlarmParse>;
                if (list == null)
                    return;

                AlarmConfigManager.Save(list.ToList());
                MessageBox.Show("配置已保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //private void SaveConfig()
        //{
        //    deviceGrid.EndEdit();
        //    List<AlarmParse> alarmParses = new List<AlarmParse>();
        //    foreach (DataGridViewRow row in deviceGrid.Rows)
        //    {
        //        if (row.IsNewRow) continue;

        //        alarmParses.Add(new AlarmParse
        //        {
        //            Id = GetCellValue(row, ID).ToInt32(),
        //            ProcessNo = GetCellValue(row, ProcessNoCol),
        //            ProcessName = GetCellValue(row, ProcessNameCol),
        //            AlarmStation = GetCellValue(row, AlarmStationCol),
        //            AlarmGrade = GetCellValue(row, AlarmGradeCol),
        //            PlcAddr = GetCellValue(row, PlcAddressCol),
        //            Description = GetCellValue(row, AlarmParseCol)
        //        });
        //    }
        //    AlarmConfigManager.Save(alarmParses);
        //    //toolStripStatusLabel1.Text = "配置已保存";
        //}

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

            if (deviceGrid.DataSource is BindingList<AlarmParse> bindingList)
            {
                // 收集要删除的项
                var toRemove = new List<AlarmParse>();
                foreach (DataGridViewRow row in deviceGrid.SelectedRows)
                {
                    if (!row.IsNewRow && row.DataBoundItem is AlarmParse item)
                        toRemove.Add(item);
                }

                foreach (var item in toRemove)
                {
                    bindingList.Remove(item);
                }
            }
        }


    }

}
