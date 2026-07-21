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
    public partial class PlcReadConfigEditor : UserControl, IApplyChanges
    {
        private readonly PlcReadConfig _config;

        public PlcReadConfigEditor(PlcReadConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            InitializeUI();
            LoadData();
        }

        public void ApplyChanges()
        {
            SaveData(); // 调用内部逻辑，把界面写回到 _config
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            // 主面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                RowStyles = {
                   new RowStyle(SizeType.Percent,8F),
                    new RowStyle(SizeType.Percent,27F),
                     new RowStyle(SizeType.Percent,65F)
                }

            };

            // 启用复选框
            var chkEnabled = new CheckBox
            {
                Text = "启用PLC读取",
                Name = "chkEnabled",
                Checked = _config.IsEnabled,
                AutoSize = true,
                Dock = DockStyle.Top
            };
            mainPanel.Controls.Add(chkEnabled, 0, 0);

            // 批量上传配置
            var grpCombined = new GroupBox
            {
                Text = "批量上传配置",
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var combinedPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                RowStyles = {
                     new RowStyle(SizeType.Percent,40F),
                    new RowStyle(SizeType.Percent,30F),
                     new RowStyle(SizeType.Percent,30F)
                }
            };

            var chkCombinedEnabled = new CheckBox
            {
                Text = "启用批量上传",
                Name = "chkCombinedEnabled",
                Checked = _config.CombinedUpload?.IsEnabled ?? false,
                Dock = DockStyle.Top,

                AutoSize = true
            };
            combinedPanel.Controls.Add(chkCombinedEnabled, 0, 0);
            combinedPanel.SetColumnSpan(chkCombinedEnabled, 2);

            combinedPanel.Controls.Add(new Label { Text = "当前步骤:" }, 0, 1);
            var numCurrentStep = new NumericUpDown
            {
                Name = "numCurrentStep",
                Minimum = 0,
                Maximum = 100,
                Value = _config.CombinedUpload?.CurrentStep ?? 0,
                Dock = DockStyle.Fill
            };
            combinedPanel.Controls.Add(numCurrentStep, 1, 1);

            var chkFinalStep = new CheckBox
            {
                Text = "是否为最后一步",
                Name = "chkFinalStep",
                Checked = _config.CombinedUpload?.IsFinalStep ?? false,
                AutoSize = true
            };
            combinedPanel.Controls.Add(chkFinalStep, 0, 2);
            combinedPanel.SetColumnSpan(chkFinalStep, 2);

            grpCombined.Controls.Add(combinedPanel);
            mainPanel.Controls.Add(grpCombined, 0, 1);

            // 数据项列表
            var grpDataItems = new GroupBox
            {
                Text = "数据项配置",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var dataItemsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Name = "gridDataItems",
                AutoGenerateColumns = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                DataSource = _config.MeasureGroups
            };

            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "名称",
                DataPropertyName = "Name",
                Width = 150
            });

            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "地址",
                DataPropertyName = "Address",
                Width = 80
            });

            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "长度",
                DataPropertyName = "Length",
                Width = 80
            });

            dataItemsGrid.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "数据类型",
                DataPropertyName = "DataType",
                DataSource = new[] { "ubool", "short", "int", "float", "string" },
                Width = 100
            });

            dataItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "缩放系数",
                DataPropertyName = "ScalingFactor",
                Width = 80
            });

            grpDataItems.Controls.Add(dataItemsGrid);
            mainPanel.Controls.Add(grpDataItems, 0, 2);

            Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var chkCombinedEnabled = Controls.Find("chkCombinedEnabled", true)[0] as CheckBox;
            var numCurrentStep = Controls.Find("numCurrentStep", true)[0] as NumericUpDown;
            var chkFinalStep = Controls.Find("chkFinalStep", true)[0] as CheckBox;

            chkEnabled.Checked = _config.IsEnabled;

            if (_config.CombinedUpload != null)
            {
                chkCombinedEnabled.Checked = _config.CombinedUpload.IsEnabled;
                numCurrentStep.Value = _config.CombinedUpload.CurrentStep;
                chkFinalStep.Checked = _config.CombinedUpload.IsFinalStep;
            }
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            var chkCombinedEnabled = Controls.Find("chkCombinedEnabled", true)[0] as CheckBox;
            var numCurrentStep = Controls.Find("numCurrentStep", true)[0] as NumericUpDown;
            var chkFinalStep = Controls.Find("chkFinalStep", true)[0] as CheckBox;

            _config.IsEnabled = chkEnabled.Checked;

            if (_config.CombinedUpload == null)
            {
                _config.CombinedUpload = new CombinedUpload();
            }

            _config.CombinedUpload.IsEnabled = chkCombinedEnabled.Checked;
            _config.CombinedUpload.CurrentStep = (int)numCurrentStep.Value;
            _config.CombinedUpload.IsFinalStep = chkFinalStep.Checked;
        }
    }
}
