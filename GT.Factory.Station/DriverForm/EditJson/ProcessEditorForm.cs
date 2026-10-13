using GT.Factory.Station.MyEnum;
using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.DriverForm.EditJson;
using GT.Factory.Station.DriverForm.EditJson.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GT.Factory.Station.EditJson.Components;

namespace GT.Factory.Station.EditJson
{
    public partial class ProcessEditorForm : Form
    {
        public ProcessBase Process { get; private set; }

        public ProcessEditorForm(ProcessBase process)
        {
            Process = process ?? throw new ArgumentNullException(nameof(process));
            InitializeComponent();
            InitializeUI();

            // 延迟设置 ComboBox 选中值 也米娜加载完成后items才会有值
            this.Load += ProcessEditorForm_Load;


        }

        private void ProcessEditorForm_Load(object sender, EventArgs e)
        {
            // 找到 ComboBox
            var cmb = this.Controls
                          .Find("cmbProcessType", true)
                          .FirstOrDefault() as ComboBox;
            if (cmb != null)
            {
                cmb.DataSource = Enum.GetValues(typeof(ProcessType));

                cmb.SelectedItem = Process.ProcessType;
            }

            LoadProcessData();
        }

        private void InitializeUI()
        {
            // 窗体设置
            Text = string.IsNullOrWhiteSpace(Process.TriggerAddress)
                    ? "添加新流程"
                    : $"编辑流程 - {Process.Name}"; StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(800, 600);
            //Icon = Properties.Resources.ProcessEditIcon;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 主布局
            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                RowStyles = {
                new RowStyle(SizeType.Percent, 100F),
                new RowStyle(SizeType.Absolute, 50F)
            },
                Padding = new Padding(10)
            };

            // Tab控件
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "processTabControl",
                //MinimumSize = new Size(700, 500)
            };
            mainTable.Controls.Add(tabControl, 0, 0);

            // 按钮面板
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 50
            };
            mainTable.Controls.Add(buttonPanel, 0, 1);

            // 初始化Tab页
            InitializeBasicTab(tabControl);

            // 根据流程类型添加特定Tab页
            if (Process.ProcessType ==ProcessType.StandardProcess)
            {
                InitializeStandardProcessTabs(tabControl, (StandardProcess)Process);
            }
            else if (Process.ProcessType == ProcessType.CheckResultProcesses)
            {
                InitializeCheckResultProcessTabs(tabControl, (CheckResultProcess)Process);
            }
            else if (Process is ReworkProcess reworkProcess)
            {
                InitializeReworkProcessTabs(tabControl, reworkProcess);
            }
            else if (Process is BarBindProcess bindProcess)
            {
                InitializeBindProcessTabs(tabControl, bindProcess);
            }
            else if (Process is CalibrationProcess calibProcess)
            {
                InitializeCalibrationProcessTabs(tabControl, calibProcess);
            }

            // 添加按钮
            var btnSave = new Button
            {
                Text = "保存",
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Right,
                Width = 80
            };
            btnSave.Click += (s, e) => SaveProcess();

            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Right,
                Left = 90,
                Width = 80
            };

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);

            Controls.Add(mainTable);

            // 设置Accept和Cancel按钮
            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private void InitializeBasicTab(TabControl tabControl)
        {
            var basicTab = new TabPage("基本信息");
            tabControl.TabPages.Add(basicTab);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            // 名称
            panel.Controls.Add(new Label
            {
                Text = "名称:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            }, 0, 0);

            var txtName = new TextBox
            {
                Name = "txtName",
                Dock = DockStyle.Fill,
                Text = Process.Name
            };
            panel.Controls.Add(txtName, 1, 0);

            // 信号名
            panel.Controls.Add(new Label
            {
                Text = "信号名:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            }, 0, 1);

            var txtSignalName = new TextBox
            {
                Name = "txtSignalName",
                Dock = DockStyle.Fill,
                Text = Process.SignalName
            };
            panel.Controls.Add(txtSignalName, 1, 1);

            // 触发地址
            panel.Controls.Add(new Label
            {
                Text = "触发地址:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            }, 0, 2);

            var txtTriggerAddress = new TextBox
            {
                Name = "txtTriggerAddress",  // ← 改名，避免和原来混淆
                Dock = DockStyle.Fill,
                Text = Process.TriggerAddress ?? string.Empty
            };
            panel.Controls.Add(txtTriggerAddress, 1, 2);

            // 工序号
            panel.Controls.Add(new Label
            {
                Text = "工序号:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            }, 0, 3);

            var txtProcessNo = new TextBox
            {
                Name = "txtProcessNo",
                Dock = DockStyle.Fill,
                Text = Process.ProcessNo
            };
            panel.Controls.Add(txtProcessNo, 1, 3);

            // 流程类型
            panel.Controls.Add(new Label
            {
                Text = "流程类型:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            }, 0, 4);

            var cmbProcessType = new ComboBox
            {
                Name = "cmbProcessType",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            panel.Controls.Add(cmbProcessType, 1, 4);

            
            // 完成反馈地址
            panel.Controls.Add(new Label
            {
                Text = "完成反馈地址:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            }, 0, 5);

            var txtFeedbackAddress = new TextBox
            {
                Name = "txtFeedbackAddress",
                Dock = DockStyle.Fill,
                Text = Process.FinishFeedbackAddress
            };
            panel.Controls.Add(txtFeedbackAddress, 1, 5);

            basicTab.Controls.Add(panel);

           
        }

        private void InitializeStandardProcessTabs(TabControl tabControl, StandardProcess process)
        {
            // 状态标志地址
            var statusTab = new TabPage("状态标志");
            var statusPanel = new PropertyGrid
            {
                Dock = DockStyle.Fill,
            };
            statusPanel.SelectedObject = new StatusFlagWrapper(process);

            statusPanel.Dock = DockStyle.Fill;
            statusTab.Controls.Add(statusPanel);
            tabControl.TabPages.Add(statusTab);

            // SN码配置
            if (process.SnConfig != null)
            {
                var snTab = new TabPage("SN码配置");
                var snEditor = new DataItemEditorForm(process.SnConfig);
                snEditor.Dock = DockStyle.Fill;
                snTab.Controls.Add(snEditor);
                tabControl.TabPages.Add(snTab);
            }

            // 治具配置
            if (process.FixtureConfig != null)
            {
                var fixtureTab = new TabPage("治具配置");
                var fixtureEditor = new FixtureConfigEditor(process.FixtureConfig);
                fixtureTab.Controls.Add(fixtureEditor);
                tabControl.TabPages.Add(fixtureTab);
            }

            // 总结果配置
            if (process.ResultConfig != null)
            {
                var resultConfigTab = new TabPage("总结果读取");
                var resultConfigEditor = new ResultConfigEditor(process.ResultConfig);
                resultConfigTab.Controls.Add(resultConfigEditor);
                tabControl.TabPages.Add(resultConfigTab);
            }


            // PLC读取配置
            if (process.PlcReadConfig != null)
            {
                var readTab = new TabPage("PLC读取");
                var readEditor = new PlcReadConfigEditor(process.PlcReadConfig);
                readTab.Controls.Add(readEditor);
                tabControl.TabPages.Add(readTab);
            }

            // 文件配置
            if (process.FileConfig != null)
            {
                var fileTab = new TabPage("文件配置");
                var fileEditor = new FileConfigEditor(process.FileConfig);
                fileTab.Controls.Add(fileEditor);
                tabControl.TabPages.Add(fileTab);
            }

            // 写入结果配置
            if (process.ResultWriteConfig != null)
            {
                var resultWriteTab = new TabPage("写入结果配置");
                var resultWriteConfigEditor = new ResultWriteConfigEditor(process.ResultWriteConfig);
                resultWriteTab.Controls.Add(resultWriteConfigEditor);
                tabControl.TabPages.Add(resultWriteTab);
            }
        }

        private void InitializeCheckResultProcessTabs(TabControl tabControl, CheckResultProcess process)
        {
           
            // SN码配置
            if (process.SnConfig != null)
            {
                var snTab = new TabPage("SN码配置");
                var snEditor = new DataItemEditorForm(process.SnConfig);
                snEditor.Dock = DockStyle.Fill;
                snTab.Controls.Add(snEditor);
                tabControl.TabPages.Add(snTab);
            }

            // 写入结果配置
            if (process.ResultWriteConfig != null)
            {
                var resultWriteTab = new TabPage("写入结果配置");
                var resultWriteConfigEditor = new ResultWriteConfigEditor(process.ResultWriteConfig);
                resultWriteTab.Controls.Add(resultWriteConfigEditor);
                tabControl.TabPages.Add(resultWriteTab);
            }

            // 过站检配置
            if (process.CheckResultInfo != null)
            {
                var snTab = new TabPage("过站检配置");
                var snEditor = new CheckResultInfoEditorForm(process.CheckResultInfo);
                snEditor.Dock = DockStyle.Fill;
                snTab.Controls.Add(snEditor);
                tabControl.TabPages.Add(snTab);
            }
        }

        private void InitializeReworkProcessTabs(TabControl tabControl, ReworkProcess process)
        {
            // PLC读取配置
            if (process.PlcWriteConfig != null)
            {
                var writeTab = new TabPage("写入数据配置");
                var readEditor = new PlcWriteConfigEditor(process.PlcWriteConfig);

                writeTab.Controls.Add(readEditor);
                tabControl.TabPages.Add(writeTab);
            }

        }

        private void InitializeBindProcessTabs(TabControl tabControl, BarBindProcess process)
        {
            // 绑定特定配置
            var partsTab = new TabPage("部件信息");
            var partsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = process.PartInfos,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true
            };
            partsTab.Controls.Add(partsGrid);
            tabControl.TabPages.Add(partsTab);
        }

        private void InitializeCalibrationProcessTabs(TabControl tabControl, CalibrationProcess process)
        {
            // 点检特定配置
            var typesTab = new TabPage("点检类型");
            var typesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = process.TypeStationIds,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true
            };
            typesTab.Controls.Add(typesGrid);
            tabControl.TabPages.Add(typesTab);
        }
        private void ApplyAllEditors(Control root)
        {
            if (root is IApplyChanges editor)
            {
                editor.ApplyChanges();
            }

            foreach (Control child in root.Controls)
            {
                ApplyAllEditors(child);
            }
        }


        private void LoadProcessData()
        {
            // 从控件加载数据
            var txtName = Controls.Find("txtName", true)[0] as TextBox;
            var txtSignalName = Controls.Find("txtSignalName", true)[0] as TextBox;
            var txtTriggerAddress = Controls.Find("txtTriggerAddress", true)[0] as TextBox;
            var txtProcessNo = Controls.Find("txtProcessNo", true)[0] as TextBox;
            var cmbProcessType = Controls.Find("cmbProcessType", true)[0] as ComboBox;
            var txtFeedbackAddress = Controls.Find("txtFeedbackAddress", true)[0] as TextBox;

            if (txtName != null) Process.Name = txtName.Text;
            if (txtSignalName != null) Process.SignalName = txtSignalName.Text;
            if (txtTriggerAddress != null)
                Process.TriggerAddress = txtTriggerAddress.Text?.Trim();
            if (txtProcessNo != null) Process.ProcessNo = txtProcessNo.Text;
            if (cmbProcessType != null) Process.ProcessType = (ProcessType)cmbProcessType.SelectedItem;
            if (txtFeedbackAddress != null) Process.FinishFeedbackAddress = txtFeedbackAddress.Text;
        }

        private void SaveProcess()
        {
            try
            {
                // 先让所有子编辑器保存数据
                ApplyAllEditors(this);
                LoadProcessData();
                Process.Validate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"验证失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
            }
        }

        public class StatusFlagWrapper
        {
            public StatusFlagWrapper(StandardProcess process)
            {
                _process = process;

                StatusFlagAddress = process.StatusFlagAddress;
            }

            private readonly StandardProcess _process;

            public string StatusFlagAddress
            {
                get => _process.StatusFlagAddress;
                set => _process.StatusFlagAddress = value;
            }
        }

    }
}
