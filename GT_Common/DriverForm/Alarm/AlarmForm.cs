using GT.Factory.Common.Models;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using GT_Common.Helper;
using GT_Common.Helper.Alarms;
using GT_Common.ProcessConfig;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
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

        //private DataGridView page.Grid;

        private ContextMenuStrip gridContextMenu;
        private ToolStripMenuItem copyCellMenuItem;
        private ToolStripMenuItem pasteCellMenuItem;
        private ToolStripMenuItem copyRowMenuItem;
        private ToolStripMenuItem pasteRowMenuItem;
        private ToolStripMenuItem deleteMenuItem;

        private string[] clipboardRowCache = null;

        private UIToolPage page;


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

            page = new UIToolPage();

            page.Grid.ReadOnly = false;

            var btnLoad = UIButton.Primary("加载");

            btnLoad.Click += (s, e) => LoadConfig();

            var btnSave = UIButton.Primary("保存");
            btnSave.Click += (s, e) => SaveConfig();

            page.AddButton(btnLoad);
            page.AddButton(btnSave);

            Controls.Add(page);


            //page.Grid = new DataGridView
            //{
            //    Dock = DockStyle.Fill,
            //    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            //    MultiSelect = true,
            //    ColumnHeadersVisible = true,
            //    RowHeadersVisible = false,
            //    BackgroundColor = Color.White,
            //    BorderStyle = BorderStyle.None,
            //    EnableHeadersVisualStyles = false
            //};

            //// 表头样式
            //page.Grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(46, 51, 73);
            //page.Grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            //page.Grid.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 9F, FontStyle.Bold);

            //btnLoad.Click += (s, e) => LoadConfig();
            //btnSave.Click += (s, e) => SaveConfig();

            //Controls.Add(page.Grid);
        }


        //  初始化dgvAlarmData
        private void InitializeDataGridView()
        {
            page.Grid.Columns.Clear();
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

            page.Grid.ContextMenuStrip = gridContextMenu;

            page.Grid.MouseDown += DeviceGrid_MouseDown;
        }

        private void AddColumn(string headerText, string name, int width, bool readOnly = false)
        {
            var column = new DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Name = name,
                Width = width,
                DataPropertyName = name, // 绑定到数据源的属性
                ReadOnly = readOnly,
            };

            //column.HeaderCell.Style.ForeColor = HeaderForeColor;
            //column.HeaderCell.Style.BackColor = HeaderBackColor;

            page.Grid.Columns.Add(column);
        }

        private void LoadConfig()
        {
            try
            {
                page.Grid.EndEdit();

                var alarmParses = AlarmConfigManager.Load() ?? new List<AlarmParse>();
                page.Grid.AutoGenerateColumns = false;
                page.Grid.DataSource = new BindingList<AlarmParse>(alarmParses);
                page.Grid.AllowUserToAddRows = false;
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
                page.Grid.EndEdit();

                var list = page.Grid.DataSource as BindingList<AlarmParse>;
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

        private string GetCellValue(DataGridViewRow row, string columnName)
        {
            return row.Cells[columnName]?.Value?.ToString() ?? string.Empty;
        }


        private void DeviceGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = page.Grid.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    page.Grid.ClearSelection();
                    page.Grid.Rows[hit.RowIndex].Selected = true;
                    page.Grid.CurrentCell = page.Grid.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
                }
            }
        }


        private void CopyCell()
        {
            if (page.Grid.CurrentCell != null)
            {
                Clipboard.SetText(page.Grid.CurrentCell.Value?.ToString() ?? "");
            }
        }

        private void PasteCell()
        {
            if (page.Grid.CurrentCell != null)
            {
                string pasteText = Clipboard.GetText();
                page.Grid.CurrentCell.Value = pasteText;
            }
        }

        private void CopyRow()
        {
            if (page.Grid.CurrentRow != null && !page.Grid.CurrentRow.IsNewRow)
            {
                clipboardRowCache = page.Grid.Columns
                    .Cast<DataGridViewColumn>()
                    .Select(col => page.Grid.CurrentRow.Cells[col.Name].Value?.ToString() ?? "")
                    .ToArray();
            }
        }

        private void PasteRow()
        {
            if (clipboardRowCache == null || page.Grid.CurrentRow == null || page.Grid.CurrentRow.IsNewRow)
                return;

            int colCount = Math.Min(clipboardRowCache.Length, page.Grid.Columns.Count);
            for (int i = 0; i < colCount; i++)
            {
                page.Grid.CurrentRow.Cells[i].Value = clipboardRowCache[i];
            }
        }

        private void DeleteSelectedRows()
        {
            page.Grid.EndEdit();

            if (page.Grid.DataSource is BindingList<AlarmParse> bindingList)
            {
                // 收集要删除的项
                var toRemove = new List<AlarmParse>();
                foreach (DataGridViewRow row in page.Grid.SelectedRows)
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
