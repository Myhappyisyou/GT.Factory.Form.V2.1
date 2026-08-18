using GT_Common.ProcessConfig;
using GT_Common.DriverForm.EditJson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditJson
{
    /// <summary>
    /// PLC读取配置编辑器。
    ///
    /// PLC读取配置结构：
    ///
    /// PlcReadConfig
    ///     ├── IsEnabled
    ///     ├── CombinedUpload
    ///     │
    ///     └── MeasureGroups
    ///             ├── PlcMeasureGroup
    ///             │      ├── FieldName
    ///             │      ├── Name
    ///             │      ├── Unit
    ///             │      ├── Upper
    ///             │      ├── Lower
    ///             │      ├── Value
    ///             │      └── Result
    ///             │
    ///             └── ...
    ///
    /// 不再直接在 DataGridView 中编辑 PlcChannel。
    /// </summary>
    public partial class PlcReadConfigEditor : UserControl, IApplyChanges
    {
        private readonly PlcReadConfig _config;

        private CheckBox _chkEnabled;

        private CheckBox _chkCombinedEnabled;
        private NumericUpDown _numCurrentStep;
        private CheckBox _chkFinalStep;

        private DataGridView _gridMeasureGroups;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;

        public PlcReadConfigEditor(PlcReadConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            InitializeComponent();

            InitializeUI();
            LoadData();
        }

        public void ApplyChanges()
        {
            SaveData();
        }

        #region UI

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            BuildMainLayout();
        }

        private void BuildMainLayout()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(8)
            };

            mainPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35));

            mainPanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 125));

            mainPanel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            // =========================================================
            // 1. PLC读取启用
            // =========================================================

            _chkEnabled = new CheckBox
            {
                Text = "启用PLC读取",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            mainPanel.Controls.Add(
                _chkEnabled,
                0,
                0);

            // =========================================================
            // 2. CombinedUpload
            // =========================================================

            var grpCombined = BuildCombinedUploadPanel();

            mainPanel.Controls.Add(
                grpCombined,
                0,
                1);

            // =========================================================
            // 3. MeasureGroups
            // =========================================================

            var grpMeasureGroups = BuildMeasureGroupsPanel();

            mainPanel.Controls.Add(
                grpMeasureGroups,
                0,
                2);

            Controls.Add(mainPanel);
        }

        private GroupBox BuildCombinedUploadPanel()
        {
            var group = new GroupBox
            {
                Text = "批量上传配置",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2
            };

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.AutoSize));

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30));

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.AutoSize));

            panel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 30));

            panel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            panel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            // 启用批量上传

            _chkCombinedEnabled = new CheckBox
            {
                Text = "启用批量上传",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            panel.Controls.Add(
                _chkCombinedEnabled,
                0,
                0);

            panel.SetColumnSpan(
                _chkCombinedEnabled,
                2);

            // 当前步骤

            var lblCurrentStep = new Label
            {
                Text = "当前步骤:",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            panel.Controls.Add(
                lblCurrentStep,
                0,
                1);

            _numCurrentStep = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 9999,
                Dock = DockStyle.Left,
                Width = 100
            };

            panel.Controls.Add(
                _numCurrentStep,
                1,
                1);

            // 最后一步

            _chkFinalStep = new CheckBox
            {
                Text = "是否最后一步",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            panel.Controls.Add(
                _chkFinalStep,
                2,
                1);

            group.Controls.Add(panel);

            return group;
        }

        private GroupBox BuildMeasureGroupsPanel()
        {
            var group = new GroupBox
            {
                Text = "业务测点配置",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            panel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            panel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 42));

            // =========================================================
            // DataGridView
            // =========================================================

            _gridMeasureGroups = new DataGridView
            {
                Dock = DockStyle.Fill,

                AutoGenerateColumns = false,

                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,

                ReadOnly = true,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                RowHeadersVisible = false,

                AutoSizeRowsMode =
                    DataGridViewAutoSizeRowsMode.None,

                AllowUserToResizeRows = false,

                BackgroundColor = SystemColors.Window,

                BorderStyle = BorderStyle.FixedSingle
            };

            BuildMeasureGroupColumns();

            _gridMeasureGroups.SelectionChanged +=
                GridMeasureGroups_SelectionChanged;

            panel.Controls.Add(
                _gridMeasureGroups,
                0,
                0);

            // =========================================================
            // Button
            // =========================================================

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            _btnDelete = new Button
            {
                Text = "删除",
                Width = 80,
                Height = 28
            };

            _btnEdit = new Button
            {
                Text = "编辑",
                Width = 80,
                Height = 28
            };

            _btnAdd = new Button
            {
                Text = "新增",
                Width = 80,
                Height = 28
            };

            _btnDelete.Click += BtnDelete_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnAdd.Click += BtnAdd_Click;

            buttonPanel.Controls.Add(_btnDelete);
            buttonPanel.Controls.Add(_btnEdit);
            buttonPanel.Controls.Add(_btnAdd);

            panel.Controls.Add(
                buttonPanel,
                0,
                1);

            group.Controls.Add(panel);

            return group;
        }

        private void BuildMeasureGroupColumns()
        {
            _gridMeasureGroups.Columns.Clear();

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "FieldName",
                    HeaderText = "业务主键",
                    DataPropertyName = "FieldName",
                    Width = 150
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Name",
                    HeaderText = "显示名称",
                    DataPropertyName = "Name",
                    Width = 220
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Unit",
                    HeaderText = "单位",
                    DataPropertyName = "Unit",
                    Width = 80
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Upper",
                    HeaderText = "上限",
                    Width = 130
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Lower",
                    HeaderText = "下限",
                    Width = 130
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Value",
                    HeaderText = "实际值",
                    Width = 130
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Result",
                    HeaderText = "结果",
                    Width = 130
                });

            _gridMeasureGroups.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "配置状态",
                    Width = 100
                });
        }

        #endregion

        #region Load

        private void LoadData()
        {
            _chkEnabled.Checked = _config.IsEnabled;

            // CombinedUpload

            if (_config.CombinedUpload != null)
            {
                _chkCombinedEnabled.Checked =
                    _config.CombinedUpload.IsEnabled;

                _numCurrentStep.Value =
                    Math.Max(
                        _numCurrentStep.Minimum,
                        Math.Min(
                            _numCurrentStep.Maximum,
                            _config.CombinedUpload.CurrentStep));

                _chkFinalStep.Checked =
                    _config.CombinedUpload.IsFinalStep;
            }
            else
            {
                _chkCombinedEnabled.Checked = false;
                _numCurrentStep.Value = 0;
                _chkFinalStep.Checked = false;
            }

            RefreshMeasureGroups();
        }

        private void RefreshMeasureGroups()
        {
            _gridMeasureGroups.Rows.Clear();

            if (_config.MeasureGroups == null)
                return;

            foreach (var group in _config.MeasureGroups)
            {
                int rowIndex =
                    _gridMeasureGroups.Rows.Add();

                var row =
                    _gridMeasureGroups.Rows[rowIndex];

                row.Cells["FieldName"].Value =
                    group.FieldName;

                row.Cells["Name"].Value =
                    group.Name;

                row.Cells["Unit"].Value =
                    group.Unit;

                row.Cells["Upper"].Value =
                    FormatChannel(group.Upper);

                row.Cells["Lower"].Value =
                    FormatChannel(group.Lower);

                row.Cells["Value"].Value =
                    FormatChannel(group.Value);

                row.Cells["Result"].Value =
                    FormatChannel(group.Result);

                row.Cells["Status"].Value =
                    GetGroupStatus(group);

                row.Tag = group;
            }

            if (_gridMeasureGroups.Rows.Count > 0)
            {
                _gridMeasureGroups.Rows[0].Selected = true;
            }

            UpdateButtonState();
        }

        private string FormatChannel(PlcChannel channel)
        {
            if (channel == null || !channel.IsConfigured())
                return "未配置";

            return string.Format(
                "{0} / {1} / {2}",
                channel.Address,
                channel.DataType,
                channel.Length);
        }

        private string GetGroupStatus(PlcMeasureGroup group)
        {
            if (group == null)
                return "无效";

            if (group.Value == null ||
                !group.Value.IsConfigured())
            {
                return "缺少实际值";
            }

            return "已配置";
        }

        #endregion

        #region Save

        private void SaveData()
        {
            _config.IsEnabled =
                _chkEnabled.Checked;

            if (_config.CombinedUpload == null)
            {
                _config.CombinedUpload =
                    new CombinedUpload();
            }

            _config.CombinedUpload.IsEnabled =
                _chkCombinedEnabled.Checked;

            _config.CombinedUpload.CurrentStep =
                (int)_numCurrentStep.Value;

            _config.CombinedUpload.IsFinalStep =
                _chkFinalStep.Checked;
        }

        #endregion

        #region Add

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var group =
                new PlcMeasureGroup();

            using (var dialog =
                new PlcMeasureGroupEditor(group, true))
            {
                if (dialog.ShowDialog(
                    FindForm()) != DialogResult.OK)
                {
                    return;
                }
            }

            if (_config.MeasureGroups == null)
            {
                _config.MeasureGroups =
                    new List<PlcMeasureGroup>();
            }

            // 防止 FieldName 重复

            if (_config.MeasureGroups.Any(
                x => string.Equals(
                    x.FieldName,
                    group.FieldName,
                    StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    "业务主键已经存在，请修改业务主键。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            _config.MeasureGroups.Add(group);

            RefreshMeasureGroups();

            SelectGroup(group);
        }

        #endregion

        #region Edit

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var group = GetSelectedGroup();

            if (group == null)
            {
                MessageBox.Show(
                    "请选择一个测点。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // 编辑前记录原 FieldName

            string oldFieldName =
                group.FieldName;

            using (var dialog =
                new PlcMeasureGroupEditor(group, false))
            {
                if (dialog.ShowDialog(
                    FindForm()) != DialogResult.OK)
                {
                    return;
                }
            }

            // 检查 FieldName 是否重复

            bool duplicated =
                _config.MeasureGroups
                    .Where(x => !ReferenceEquals(x, group))
                    .Any(x =>
                        string.Equals(
                            x.FieldName,
                            group.FieldName,
                            StringComparison.OrdinalIgnoreCase));

            if (duplicated)
            {
                // 恢复原来的 FieldName

                group.FieldName =
                    oldFieldName;

                MessageBox.Show(
                    "业务主键已经存在，请修改业务主键。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            RefreshMeasureGroups();

            SelectGroup(group);
        }

        #endregion

        #region Delete

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var group = GetSelectedGroup();

            if (group == null)
            {
                MessageBox.Show(
                    "请选择一个测点。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string name =
                string.IsNullOrWhiteSpace(group.Name)
                    ? group.FieldName
                    : group.Name;

            var result =
                MessageBox.Show(
                    $"确定要删除测点“{name}”吗？",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            _config.MeasureGroups.Remove(group);

            RefreshMeasureGroups();
        }

        #endregion

        #region Selection

        private PlcMeasureGroup GetSelectedGroup()
        {
            if (_gridMeasureGroups == null)
                return null;

            if (_gridMeasureGroups.SelectedRows.Count == 0)
                return null;

            return
                _gridMeasureGroups
                    .SelectedRows[0]
                    .Tag as PlcMeasureGroup;
        }

        private void SelectGroup(
            PlcMeasureGroup group)
        {
            foreach (
                DataGridViewRow row
                in _gridMeasureGroups.Rows)
            {
                if (ReferenceEquals(
                    row.Tag,
                    group))
                {
                    row.Selected = true;

                    _gridMeasureGroups
                        .CurrentCell =
                        row.Cells[0];

                    break;
                }
            }
        }

        private void GridMeasureGroups_SelectionChanged(
            object sender,
            EventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            bool hasSelection =
                GetSelectedGroup() != null;

            _btnEdit.Enabled =
                hasSelection;

            _btnDelete.Enabled =
                hasSelection;
        }

        #endregion
    }
}

//namespace EditJson
//{
//    public partial class PlcReadConfigEditor : UserControl, IApplyChanges
//    {
//        private readonly PlcReadConfig _config;

//        public PlcReadConfigEditor(PlcReadConfig config)
//        {
//            _config = config ?? throw new ArgumentNullException(nameof(config));
//            InitializeComponent();
//            InitializeUI();
//            LoadData();
//        }

//        public void ApplyChanges()
//        {
//            SaveData(); // 调用内部逻辑，把界面写回到 _config
//        }

//        private void InitializeUI()
//        {
//            Dock = DockStyle.Fill;

//            // 主面板
//            var mainPanel = new TableLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                ColumnCount = 1,
//                RowCount = 3,
//                RowStyles = {
//                   new RowStyle(SizeType.Percent,8F),
//                    new RowStyle(SizeType.Percent,27F),
//                     new RowStyle(SizeType.Percent,65F)
//                }

//            };

//            // 启用复选框
//            var chkEnabled = new CheckBox
//            {
//                Text = "启用PLC读取",
//                Name = "chkEnabled",
//                Checked = _config.IsEnabled,
//                AutoSize = true,
//                Dock = DockStyle.Top
//            };
//            mainPanel.Controls.Add(chkEnabled, 0, 0);

//            // 批量上传配置
//            var grpCombined = new GroupBox
//            {
//                Text = "批量上传配置",
//                Dock = DockStyle.Top,
//                Padding = new Padding(10)
//            };

//            var combinedPanel = new TableLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                ColumnCount = 2,
//                RowCount = 3,
//                RowStyles = {
//                     new RowStyle(SizeType.Percent,40F),
//                    new RowStyle(SizeType.Percent,30F),
//                     new RowStyle(SizeType.Percent,30F)
//                }
//            };

//            var chkCombinedEnabled = new CheckBox
//            {
//                Text = "启用批量上传",
//                Name = "chkCombinedEnabled",
//                Checked = _config.CombinedUpload?.IsEnabled ?? false,
//                Dock = DockStyle.Top,

//                AutoSize = true
//            };
//            combinedPanel.Controls.Add(chkCombinedEnabled, 0, 0);
//            combinedPanel.SetColumnSpan(chkCombinedEnabled, 2);

//            combinedPanel.Controls.Add(new Label { Text = "当前步骤:" }, 0, 1);
//            var numCurrentStep = new NumericUpDown
//            {
//                Name = "numCurrentStep",
//                Minimum = 0,
//                Maximum = 100,
//                Value = _config.CombinedUpload?.CurrentStep ?? 0,
//                Dock = DockStyle.Fill
//            };
//            combinedPanel.Controls.Add(numCurrentStep, 1, 1);

//            var chkFinalStep = new CheckBox
//            {
//                Text = "是否为最后一步",
//                Name = "chkFinalStep",
//                Checked = _config.CombinedUpload?.IsFinalStep ?? false,
//                AutoSize = true
//            };
//            combinedPanel.Controls.Add(chkFinalStep, 0, 2);
//            combinedPanel.SetColumnSpan(chkFinalStep, 2);

//            grpCombined.Controls.Add(combinedPanel);
//            mainPanel.Controls.Add(grpCombined, 0, 1);

//            // 数据项列表
//            var grpDataItems = new GroupBox
//            {
//                Text = "数据项配置",
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10)
//            };

//            var dataItemsGrid = new DataGridView
//            {
//                Dock = DockStyle.Fill,
//                Name = "gridDataItems",
//                AutoGenerateColumns = false,
//                AllowUserToAddRows = true,
//                AllowUserToDeleteRows = true,
//                DataSource = _config.MeasureGroups
//            };

//            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
//            {
//                HeaderText = "名称",
//                DataPropertyName = "Name",
//                Width = 150
//            });

//            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
//            {
//                HeaderText = "地址",
//                DataPropertyName = "Address",
//                Width = 80
//            });

//            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
//            {
//                HeaderText = "长度",
//                DataPropertyName = "Length",
//                Width = 80
//            });

//            dataItemsGrid.Columns.Add(new DataGridViewComboBoxColumn
//            {
//                HeaderText = "数据类型",
//                DataPropertyName = "DataType",
//                DataSource = new[] { "ubool", "short", "int", "float", "string" },
//                Width = 100
//            });

//            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
//            {
//                HeaderText = "缩放系数",
//                DataPropertyName = "ScalingFactor",
//                Width = 80
//            });

//            grpDataItems.Controls.Add(dataItemsGrid);
//            mainPanel.Controls.Add(grpDataItems, 0, 2);

//            Controls.Add(mainPanel);
//        }

//        private void LoadData()
//        {
//            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
//            var chkCombinedEnabled = Controls.Find("chkCombinedEnabled", true)[0] as CheckBox;
//            var numCurrentStep = Controls.Find("numCurrentStep", true)[0] as NumericUpDown;
//            var chkFinalStep = Controls.Find("chkFinalStep", true)[0] as CheckBox;

//            chkEnabled.Checked = _config.IsEnabled;

//            if (_config.CombinedUpload != null)
//            {
//                chkCombinedEnabled.Checked = _config.CombinedUpload.IsEnabled;
//                numCurrentStep.Value = _config.CombinedUpload.CurrentStep;
//                chkFinalStep.Checked = _config.CombinedUpload.IsFinalStep;
//            }
//        }

//        public void SaveData()
//        {
//            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
//            var chkCombinedEnabled = Controls.Find("chkCombinedEnabled", true)[0] as CheckBox;
//            var numCurrentStep = Controls.Find("numCurrentStep", true)[0] as NumericUpDown;
//            var chkFinalStep = Controls.Find("chkFinalStep", true)[0] as CheckBox;

//            _config.IsEnabled = chkEnabled.Checked;

//            if (_config.CombinedUpload == null)
//            {
//                _config.CombinedUpload = new CombinedUpload();
//            }

//            _config.CombinedUpload.IsEnabled = chkCombinedEnabled.Checked;
//            _config.CombinedUpload.CurrentStep = (int)numCurrentStep.Value;
//            _config.CombinedUpload.IsFinalStep = chkFinalStep.Checked;
//        }
//    }
//}
