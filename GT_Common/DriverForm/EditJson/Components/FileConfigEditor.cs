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
    public partial class FileConfigEditor : UserControl, IApplyChanges
    {
        private readonly FileConfig _config;

        public FileConfigEditor(FileConfig config)
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
                RowCount = 2
            };

            // 启用复选框
            var chkEnabled = new CheckBox
            {
                Text = "启用文件配置",
                Name = "chkEnabled",
                Checked = _config.IsEnabled,
                AutoSize = true,
                Dock = DockStyle.Top
            };
            mainPanel.Controls.Add(chkEnabled, 0, 0);

            // 文件项列表
            var grpFileItems = new GroupBox
            {
                Text = "文件项配置",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var fileItemsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                Name = "gridFileItems",
                AutoGenerateColumns = false,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                DataSource = _config.FileItems
            };

            fileItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "地址",
                DataPropertyName = "Address",
                Width = 80
            });

            fileItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "长度",
                DataPropertyName = "Length",
                Width = 80
            });

            fileItemsGrid.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "文件类型",
                DataPropertyName = "FileType",
                DataSource = new[] { "img", "txt", "csv", "bin" },
                Width = 100
            });

            fileItemsGrid.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "扩展名",
                DataPropertyName = "FileExtension",
                DataSource = new[] { "jpg", "png", "bmp", "txt", "csv" },
                Width = 80
            });

            fileItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "文件名",
                DataPropertyName = "FileName",
                Width = 150
            });

            fileItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "文件夹",
                DataPropertyName = "FolderName",
                Width = 150
            });

            fileItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "相机名称",
                DataPropertyName = "CameraName",
                Width = 120
            });

            fileItemsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "相机类型",
                DataPropertyName = "CameraType",
                Width = 80
            });

            grpFileItems.Controls.Add(fileItemsGrid);
            mainPanel.Controls.Add(grpFileItems, 0, 1);

            Controls.Add(mainPanel);
        }

        private void LoadData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            chkEnabled.Checked = _config.IsEnabled;
        }

        public void SaveData()
        {
            var chkEnabled = Controls.Find("chkEnabled", true)[0] as CheckBox;
            _config.IsEnabled = chkEnabled.Checked;
        }
    }
}
