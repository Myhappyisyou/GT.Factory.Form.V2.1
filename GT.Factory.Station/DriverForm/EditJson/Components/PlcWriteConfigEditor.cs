using GT.Factory.Station.EditJson;
using GT.Factory.Station.MyEnum;
using GT.Factory.Station.ProcessConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GT.Factory.Station.DriverForm.EditJson.Components
{
    public partial class PlcWriteConfigEditor : UserControl, IApplyChanges
    {
        private readonly PlcWriteConfig _config;

        public PlcWriteConfigEditor(PlcWriteConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeUI();
            LoadData();
        }

        public void ApplyChanges()
        {
            SaveData();
        }

        private void InitializeUI()
        {
            this.SuspendLayout();
            Dock = DockStyle.Fill;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 35F),  // 启用复选框
                    new RowStyle(SizeType.Percent, 100F)   // DataGridView
                }
            };

            // ========== 启用复选框 ==========
            var chkEnabled = new CheckBox
            {
                Text = "启用PLC写入配置",
                Name = "chkEnabled",
                Checked = _config.IsEnabled,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            chkEnabled.CheckedChanged += ChkEnabled_CheckedChanged;
            mainPanel.Controls.Add(chkEnabled, 0, 0);

            // ========== DataGridView 写入项列表 ==========
            var grid = new DataGridView
            {
                Name = "gridWriteItems",
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            // 列：名称
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "名称",
                FillWeight = 25
            });

            // 列：写入地址（string）
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAddress",
                HeaderText = "写入地址",
                FillWeight = 25
            });

            // 列：数据长度
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colLength",
                HeaderText = "长度",
                FillWeight = 15
            });

            // 列：数据类型（ComboBox）
            var colDataType = new DataGridViewComboBoxColumn
            {
                Name = "colDataType",
                HeaderText = "数据类型",
                FillWeight = 35,
                DataSource = Enum.GetValues(typeof(PlcDataType)),
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            grid.Columns.Add(colDataType);

            mainPanel.Controls.Add(grid, 0, 1);

            Controls.Add(mainPanel);

            this.ResumeLayout();
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var grid = Controls.Find("gridWriteItems", true)[0] as DataGridView;

            chkEnabled.Checked = _config.IsEnabled;

            grid.Rows.Clear();

            if (_config.WriteItems == null)
                return;

            foreach (var item in _config.WriteItems)
            {
                grid.Rows.Add(
                    item.Name ?? string.Empty,
                    item.Address ?? string.Empty,
                    item.Length.ToString(),
                    item.DataType
                );
            }

            UpdateGridEnabled(chkEnabled.Checked, grid);
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var grid = Controls.Find("gridWriteItems", true)[0] as DataGridView;

            _config.IsEnabled = chkEnabled.Checked;

            _config.WriteItems = new List<WriteItemConfig>();

            foreach (DataGridViewRow row in grid.Rows)
            {
                // 跳过空白新增行
                if (row.IsNewRow) continue;

                var name = row.Cells["colName"].Value?.ToString()?.Trim();
                var address = row.Cells["colAddress"].Value?.ToString()?.Trim();
                var lengthStr = row.Cells["colLength"].Value?.ToString();
                var dataTypeVal = row.Cells["colDataType"].Value;

                // 跳过完全空白的行
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(address))
                    continue;

                int length = 0;
                int.TryParse(lengthStr, out length);

                PlcDataType dataType = PlcDataType.Short;
                if (dataTypeVal is PlcDataType dt)
                    dataType = dt;
                else if (dataTypeVal != null)
                    Enum.TryParse(dataTypeVal.ToString(), out dataType);

                _config.WriteItems.Add(new WriteItemConfig
                {
                    Name = name,
                    Address = address,
                    Length = length,
                    DataType = dataType
                });
            }
        }

        // ========== 启用状态切换 ==========
        private void ChkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            var chkEnabled = sender as CheckBox;
            var grid = Controls.Find("gridWriteItems", true)[0] as DataGridView;
            UpdateGridEnabled(chkEnabled.Checked, grid);
        }

        private void UpdateGridEnabled(bool enabled, DataGridView grid)
        {
            grid.Enabled = enabled;
            grid.AllowUserToAddRows = enabled;
            grid.AllowUserToDeleteRows = enabled;
        }
    }
}
