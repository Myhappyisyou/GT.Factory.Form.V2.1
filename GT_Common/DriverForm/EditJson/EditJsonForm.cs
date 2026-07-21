using GT_Common.ProcessConfig;
using GT_Common.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using Control = System.Windows.Forms.Control;
using Formatting = Newtonsoft.Json.Formatting;
using RichTextBox = System.Windows.Forms.RichTextBox;
using TabControl = System.Windows.Forms.TabControl;
using ToolTip = System.Windows.Forms.ToolTip;
using TreeView = System.Windows.Forms.TreeView;

namespace EditJson
{
    public partial class EditJsonForm : Form
    {
        private PlcConfig _plcConfig;
        private readonly JsonService _jsonService = new JsonService();
        private readonly ConfigValidator _validator = new ConfigValidator();
        private readonly string _configFilePath = PathCenter.ConfigFile("PlcConfig.json");
        private readonly string _backupFolder = "Backups";

        // 控件声明
        private TreeView treeView;
        private PropertyGrid propertyGrid;
        private RichTextBox jsonTextBox;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolTip toolTip = new ToolTip();
        private SplitContainer mainSplitContainer;
        private TabControl tabControl;

        public EditJsonForm()
        {
            InitializeComponent();
            InitializeUiComponents();
            SetupEventHandlers();
            LoadConfiguration();
            // 不直接调用 ApplyUiTheme()
            this.Load += EditJsonForm_Load;
        }

        private void EditJsonForm_Load(object sender, EventArgs e)
        {
            ApplyUiTheme();
        }

        //  页面布局
        private void InitializeUiComponents()
        {
            // 主窗体设置
            Text = "PLC 配置管理器";
            //WindowState = FormWindowState.Maximized;
            //Icon = Properties.Resources.AppIcon;
            this.Size = new Size(900, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250); // 浅灰蓝色系，柔和眼睛

            // 创建主布局
            mainSplitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                //SplitterDistance = 300,
                Name = "mainSplitContainer"
            };
            //mainSplitContainer.SplitterDistance = 200;

            Controls.Add(mainSplitContainer);

            // 初始化树形视图
            InitializeTreeView(mainSplitContainer.Panel1);

            // 初始化详情面板
            InitializeDetailPanel(mainSplitContainer.Panel2);

            Load += (s, e) =>
            {
                mainSplitContainer.SplitterDistance = 200;
            };    //在窗体加载后设置 SplitterDistance 设置分隔条的位置（即左边 Panel1 的宽度，单位是像素）

            // 初始化工具栏
            InitializeToolStrip();

            // 初始化状态栏
            InitializeStatusBar();

        }

        //  树形视图
        private void InitializeTreeView(Control parent)
        {
            treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,

                Name = "configTreeView",
                ShowNodeToolTips = true,
                HotTracking = true,

                HideSelection = false,
                ShowPlusMinus = true,
                ShowRootLines = true,
                ShowLines = true,
                Font = new Font("Microsoft YaHei", 9),
                BackColor = Color.White,
                ImageList = CreateImageList(),
                ContextMenuStrip = CreateTreeViewContextMenu()
            };
            treeView.NodeMouseClick += (s, e) => treeView.SelectedNode = e.Node;

            parent.Controls.Add(treeView);
        }

        //  图标
        private ImageList CreateImageList()
        {
            var imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new System.Drawing.Size(16, 16)
            };

            //imageList.Images.Add("Root", Properties.Resources.PlcConfig);
            //imageList.Images.Add("Basic", Properties.Resources.Settings);
            //imageList.Images.Add("Process", Properties.Resources.Process);
            //imageList.Images.Add("Standard", Properties.Resources.StandardProcess);
            //imageList.Images.Add("Bind", Properties.Resources.BindProcess);
            //imageList.Images.Add("Calibration", Properties.Resources.Calibration);
            //imageList.Images.Add("Alarm", Properties.Resources.Alarm);
            //imageList.Images.Add("Equipment", Properties.Resources.Equipment);

            return imageList;
        }

        //  树状菜单
        private ContextMenuStrip CreateTreeViewContextMenu()
        {
            var menu = new ContextMenuStrip();

            var addItem = new ToolStripMenuItem("添加");
            addItem.Click += (s, e) => AddSelectedItem();

            var editItem = new ToolStripMenuItem("编辑");
            editItem.Click += (s, e) => EditSelectedItem();

            var deleteItem = new ToolStripMenuItem("删除");
            deleteItem.Click += (s, e) => DeleteSelectedItem();

            var refreshItem = new ToolStripMenuItem("刷新");
            refreshItem.Click += (s, e) => LoadConfiguration();

            menu.Items.Add(addItem);
            menu.Items.Add(editItem);
            menu.Items.Add(deleteItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(refreshItem);

            return menu;
        }

        //  初始化属性详情面板
        private void InitializeDetailPanel(Control parent)
        {
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "detailTabControl",
                Padding= new Point(10, 5),
                Font= new Font("Microsoft YaHei", 9),
                Appearance= TabAppearance.Normal,
                BackColor = Color.WhiteSmoke
            };
          

            // 属性网格Tab
            var propTab = new TabPage("属性");
            propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                Name = "configPropertyGrid",
                ToolbarVisible = false,
                PropertySort = PropertySort.Categorized,
                HelpVisible = true,
                BackColor = Color.White,
                Font = new Font("Microsoft YaHei", 9)
            };
            propTab.Controls.Add(propertyGrid);
            tabControl.TabPages.Add(propTab);

            // JSON视图Tab
            var jsonTab = new TabPage("JSON");
            jsonTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Name = "jsonTextBox",
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(220, 220, 220),
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Consolas", 10),
                ReadOnly = true
            };
            jsonTab.Controls.Add(jsonTextBox);
            tabControl.TabPages.Add(jsonTab);

            parent.Controls.Add(tabControl);
        }

        //  初始化工具栏
        private void InitializeToolStrip()
        {
            toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                Name = "mainToolStrip",
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.Professional,
                BackColor = Color.FromArgb(240, 240, 240),
                ImageScalingSize = new Size(20, 20),
                Padding = new Padding(5, 2, 5, 2),
                Font = new Font("Microsoft YaHei", 9),
            };

            var btnNew = new ToolStripButton
            {
                Text = "新建",
                //Image = Properties.Resources.NewDocument,
                ToolTipText = "创建新配置 (Ctrl+N)"
            };
            btnNew.Click += (s, e) => NewConfiguration();

            var btnOpen = new ToolStripButton
            {
                Text = "打开",
                //Image = Properties.Resources.Open,
                ToolTipText = "打开配置文件 (Ctrl+O)"
            };
            btnOpen.Click += (s, e) => OpenConfiguration();

            var btnSave = new ToolStripButton
            {
                Text = "保存",
                //Image = Properties.Resources.Save,
                ToolTipText = "保存配置 (Ctrl+S)"
            };
            btnSave.Click += (s, e) => SaveConfiguration();

            var btnValidate = new ToolStripButton
            {
                Text = "验证",
                //Image = Properties.Resources.Validate,
                ToolTipText = "验证配置 (Ctrl+V)"
            };
            btnValidate.Click += (s, e) => ValidateConfiguration();

            toolStrip.Items.Add(btnNew);
            toolStrip.Items.Add(btnOpen);
            toolStrip.Items.Add(btnSave);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(btnValidate);

            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is ToolStripButton btn)
                {
                    btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    btn.Margin = new Padding(3, 0, 3, 0);
                    btn.ForeColor = Color.FromArgb(50, 50, 50);
                }
            }


            Controls.Add(toolStrip);
        }

        //  初始化状态栏
        private void InitializeStatusBar()
        {
            statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                Name = "mainStatusStrip",
                BackColor = Color.FromArgb(250, 250, 250),
                SizingGrip = false, // 右下角去掉拖动三角
                Font = new Font("Microsoft YaHei", 9),
            };

            var lblStatus = new ToolStripStatusLabel
            {
                Text = "就绪",
                Name = "lblStatus",
                Spring = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var lblVersion = new ToolStripStatusLabel
            {
                Text = $"版本: {Application.ProductVersion}",
                Name = "lblVersion"
            };

            statusStrip.Items.Add(lblStatus);
            statusStrip.Items.Add(lblVersion);

            Controls.Add(statusStrip);
        }

        private void ApplyUiTheme()
        {
            // 主窗体背景
            this.BackColor = Color.FromArgb(245, 247, 250); // 浅灰蓝色

            // SplitContainer 左右面板
            mainSplitContainer.Panel1.BackColor = Color.FromArgb(235, 240, 245); // 左：树视图
            mainSplitContainer.Panel2.BackColor = Color.White;                   // 右：详情面板

            // TabControl & 内容区
            foreach (TabPage tab in tabControl.TabPages)
            {
                tab.BackColor = Color.WhiteSmoke;
            }
            propertyGrid.BackColor = Color.White;
            jsonTextBox.BackColor = Color.FromArgb(30, 30, 30);
            jsonTextBox.ForeColor = Color.FromArgb(220, 220, 220);

            // 工具栏 & 状态栏
            toolStrip.BackColor = Color.FromArgb(240, 240, 240);
            statusStrip.BackColor = Color.FromArgb(240, 240, 240);

            // 树视图
            treeView.BackColor = Color.FromArgb(235, 240, 245);
            treeView.ForeColor = Color.FromArgb(50, 50, 50);
            treeView.FullRowSelect = true;
            treeView.HotTracking = true;

            // 分隔条颜色
            mainSplitContainer.BackColor = Color.FromArgb(200, 200, 200);
        }


        private void SetupEventHandlers()
        {
            treeView.AfterSelect += TreeView_AfterSelect;
            propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
        }

        private void LoadConfiguration()
        {
            try
            {
                _plcConfig = _jsonService.LoadFromFile<PlcConfig>(_configFilePath) ?? CreateDefaultConfig();
                UpdateTreeView();
                UpdateStatus("配置已加载");
            }
            catch (Exception ex)
            {
                ShowError($"加载配置失败: {ex.Message}");
            }
        }

        private PlcConfig CreateDefaultConfig()
        {
            return new PlcConfig
            {
                Ip = "127.0.0.1",
                SectionName = "DM",
                AlarmConfig = new AlarmConfig(),                  
                EquipmentStata = new EquipmentStata(),            
                StandardProcess = new List<StandardProcess>(),
                CalibrationProcesses = new List<CalibrationProcess>(),
                BarBindProcess = new List<BarBindProcess>()
            };
        }

        private void UpdateTreeView()
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            var rootNode = new TreeNode("PLC 配置")
            {
                Name = "RootNode",
                ImageKey = "Root",
                SelectedImageKey = "Root",
                Tag = _plcConfig
            };

            // 基本配置节点
            var basicNode = new TreeNode("基本配置")
            {
                Name = "BasicConfigNode",
                ImageKey = "Basic",
                SelectedImageKey = "Basic",
                Tag = _plcConfig
            };
            rootNode.Nodes.Add(basicNode);

            // 固定配置
            if (_plcConfig.FixedInformation != null)
            {
                var alarmNode = new TreeNode("固定配置")
                {
                    Name = "FixedInformationNode",
                    ImageKey = "FixedInformation",
                    Tag = _plcConfig.FixedInformation
                };
                rootNode.Nodes.Add(alarmNode);
            }

            // 报警配置
            if (_plcConfig.AlarmConfig != null)
            {
                var alarmNode = new TreeNode("报警配置")
                {
                    Name = "AlarmConfigNode",
                    ImageKey = "Alarm",
                    //SelectedImageKey = "Alarm",
                    Tag = _plcConfig.AlarmConfig
                };
                rootNode.Nodes.Add(alarmNode);
            }

            // 设备状态节点
            if (_plcConfig.EquipmentStata != null)
            {
                var equipmentNode = new TreeNode("设备状态")
                {
                    Name = "EquipmentStateNode",
                    ImageKey = "Equipment",
                    SelectedImageKey = "Equipment",
                    Tag = _plcConfig.EquipmentStata
                };
                rootNode.Nodes.Add(equipmentNode);
            }

            // 标准流程节点
            var stdProcessNode = new TreeNode("标准流程")
            {
                Name = "StandardProcessesNode",
                ImageKey = "Standard",
                SelectedImageKey = "Standard"
            };
            foreach (var process in _plcConfig.StandardProcess)
            {
                var node = new TreeNode(process.Name)
                {
                    Name = $"StdProcess_{process.Name}",
                    ImageKey = "Process",
                    SelectedImageKey = "Process",
                    Tag = process,
                    ToolTipText = $"信号: {process.SignalName}\n触发地址: {process.TriggerAddress}"
                };
                stdProcessNode.Nodes.Add(node);
            }
            rootNode.Nodes.Add(stdProcessNode);

            // 过站检流程节点
            if (_plcConfig.CheckResultProcess != null)
            {
                // 绑定流程节点
                var bindProcessNode = new TreeNode("过站检流程")
                {
                    Name = "CheckResultProcessesNode",
                    ImageKey = "CheckResult",
                    SelectedImageKey = "CheckResult"
                };
                foreach (var process in _plcConfig.CheckResultProcess)
                {
                    var node = new TreeNode(process.Name)
                    {
                        Name = $"CheckResult_{process.Name}",
                        ImageKey = "Process",
                        SelectedImageKey = "Process",
                        Tag = process,
                        ToolTipText = $"信号: {process.SignalName}\n触发地址: {process.TriggerAddress}"
                    };
                    bindProcessNode.Nodes.Add(node);
                }
                rootNode.Nodes.Add(bindProcessNode);
            }

            // 绑定流程节点
            if (_plcConfig.BarBindProcess != null)
            {
                // 绑定流程节点
                var bindProcessNode = new TreeNode("绑定流程")
                {
                    Name = "BindProcessesNode",
                    ImageKey = "Bind",
                    SelectedImageKey = "Bind"
                };
                foreach (var process in _plcConfig.BarBindProcess)
                {
                    var node = new TreeNode(process.Name)
                    {
                        Name = $"BindProcess_{process.Name}",
                        ImageKey = "Process",
                        SelectedImageKey = "Process",
                        Tag = process,
                        ToolTipText = $"信号: {process.SignalName}\n触发地址: {process.TriggerAddress}"
                    };
                    bindProcessNode.Nodes.Add(node);
                }
                rootNode.Nodes.Add(bindProcessNode);
            }
        

            // 点检流程节点
            var calibProcessNode = new TreeNode("点检流程")
            {
                Name = "CalibrationProcessesNode",
                ImageKey = "Calibration",
                SelectedImageKey = "Calibration"
            };
            foreach (var process in _plcConfig.CalibrationProcesses)
            {
                var node = new TreeNode(process.Name)
                {
                    Name = $"CalibProcess_{process.Name}",
                    ImageKey = "Process",
                    SelectedImageKey = "Process",
                    Tag = process,
                    ToolTipText = $"信号: {process.SignalName}\n触发地址: {process.TriggerAddress}"
                };
                calibProcessNode.Nodes.Add(node);
            }
            rootNode.Nodes.Add(calibProcessNode);

            treeView.Nodes.Add(rootNode);
            rootNode.Expand();
            treeView.EndUpdate();
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag != null)
            {
               
                propertyGrid.SelectedObject = e.Node.Tag;
               
                jsonTextBox.Text = JsonConvert.SerializeObject(e.Node.Tag, Formatting.Indented);
                propertyGrid.Refresh(); // 强制刷新


            }
        }

        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem?.Value != null)
            {
                UpdateStatus($"属性已更改: {e.ChangedItem.Label}");
                jsonTextBox.Text = JsonConvert.SerializeObject(propertyGrid.SelectedObject, Formatting.Indented);
            }
        }

        private void NewConfiguration()
        {
            if (ConfirmUnsavedChanges())
            {
                _plcConfig = CreateDefaultConfig();
                UpdateTreeView();
                UpdateStatus("已创建新配置");
            }
        }

        private void OpenConfiguration()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*";
                dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(_configFilePath));

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _plcConfig = _jsonService.LoadFromFile<PlcConfig>(dialog.FileName);
                        UpdateTreeView();
                        UpdateStatus($"已加载: {dialog.FileName}");
                    }
                    catch (Exception ex)
                    {
                        ShowError($"打开文件失败: {ex.Message}");
                    }
                }
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                // 创建备份目录
                if (!Directory.Exists(_backupFolder))
                {
                    Directory.CreateDirectory(_backupFolder);
                }

                // 创建备份
                if (File.Exists(_configFilePath))
                {
                    var backupPath = Path.Combine(_backupFolder,
                        $"PlcConfig_{DateTime.Now:yyyyMMddHHmmss}.json");
                    File.Copy(_configFilePath, backupPath, true);
                }

                _jsonService.SaveToFile(_configFilePath, _plcConfig);
                UpdateStatus("配置已保存");
            }
            catch (Exception ex)
            {
                ShowError($"保存配置失败: {ex.Message}");
            }
        }

        private void ValidateConfiguration()
        {
            try
            {
                var result = _validator.Validate(_plcConfig);
                //using (var dialog = new Dialog(result))
                //{
                //    dialog.ShowDialog();
                //}

                if (result.IsValid)
                {
                    UpdateStatus("配置验证通过");
                }
                else
                {
                    UpdateStatus("配置验证发现错误", ToolTipIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ShowError($"验证失败: {ex.Message}");
            }
        }

        private void AddSelectedItem()
        {
            var selectedNode = treeView.SelectedNode;
            if (selectedNode == null) return;

            if (selectedNode.Name == "StandardProcessesNode")
            {
                var process = new StandardProcess
                {
                    SnConfig = new SnConfig { IsEnabled = false },
                    FixtureConfig = new FixtureConfig { IsEnabled = false },
                    ResultConfig = new ResultConfig { IsEnabled = false },
                    PlcReadConfig = new PlcReadConfig
                    {
                        IsEnabled = false,
                        MeasureGroups = new List<PlcMeasureGroup>(),
                        CombinedUpload = new CombinedUpload { IsEnabled = false }
                    },
                    PlcWriteConfig = new PlcWriteConfig
                    {
                        IsEnabled = false,
                        WriteItems = new List<WriteItemConfig>()
                    },
                    FileConfig = new FileConfig
                    {
                        IsEnabled = false,
                        FileItems = new List<FileItemConfig>()
                    },
                    MethodConfig = new MethodConfig
                    {
                        IsNeed = false,
                        Methods = Array.Empty<MethodInformation>()
                    },
                    ResultWriteConfig = new ResultWriteConfig
                    {
                        IsEnabled = false,
                        Address = string.Empty // 注意：虽然未启用，但最好初始化为空字符串，避免空引用
                    }
                };
                using (var editor = new ProcessEditorForm(process))
                {
                    if (editor.ShowDialog() == DialogResult.OK && editor.Process != null)
                    {
                        _plcConfig.StandardProcess.Add((StandardProcess)editor.Process);
                        UpdateTreeView();
                        UpdateStatus("已添加标准流程");
                    }
                }
            }
            else if (selectedNode.Name == "CheckResultProcessesNode")
            {
                using (var editor = new ProcessEditorForm(new CheckResultProcess()))
                {
                    if (editor.ShowDialog() == DialogResult.OK && editor.Process != null)
                    {
                        _plcConfig.CheckResultProcess.Add((CheckResultProcess)editor.Process);
                        UpdateTreeView();
                        UpdateStatus("已添加过站检流程");
                    }
                }
            }
            else if (selectedNode.Name == "BindProcessesNode")
            {
                using (var editor = new ProcessEditorForm(new BarBindProcess()))
                {
                    if (editor.ShowDialog() == DialogResult.OK && editor.Process != null)
                    {
                        _plcConfig.BarBindProcess.Add((BarBindProcess)editor.Process);
                        UpdateTreeView();
                        UpdateStatus("已添加绑定流程");
                    }
                }
            }
            else if (selectedNode.Name == "CalibrationProcessesNode")
            {
                using (var editor = new ProcessEditorForm(new CalibrationProcess()))
                {
                    if (editor.ShowDialog() == DialogResult.OK && editor.Process != null)
                    {
                        _plcConfig.CalibrationProcesses.Add((CalibrationProcess)editor.Process);
                        UpdateTreeView();
                        UpdateStatus("已添加点检流程");
                    }
                }
            }
        }

        private void EditSelectedItem()
        {
            var selectedNode = treeView.SelectedNode;
            if (selectedNode?.Tag == null) return;

            if (selectedNode.Tag is ProcessBase process)
            {
                using (var editor = new ProcessEditorForm(process))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        UpdateTreeView();
                        UpdateStatus($"已更新: {process.Name}");
                    }
                }
            }
        }

        private void DeleteSelectedItem()
        {
            var selectedNode = treeView.SelectedNode;
            if (selectedNode?.Tag == null || selectedNode.Parent == null) return;

            if (MessageBox.Show($"确定要删除 '{selectedNode.Text}' 吗?", "确认删除",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (selectedNode.Tag is StandardProcess stdProcess)
                {
                    _plcConfig.StandardProcess.Remove(stdProcess);
                }
                else if (selectedNode.Tag is BarBindProcess bindProcess)
                {
                    _plcConfig.BarBindProcess.Remove(bindProcess);
                }
                else if (selectedNode.Tag is CalibrationProcess calibProcess)
                {
                    _plcConfig.CalibrationProcesses.Remove(calibProcess);
                }

                UpdateTreeView();
                UpdateStatus("已删除选定项");
            }
        }

        private bool ConfirmUnsavedChanges()
        {
            // TODO: 实现检查是否有未保存的更改
            return true;
        }

        private void UpdateStatus(string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            var statusLabel = (ToolStripStatusLabel)statusStrip.Items["lblStatus"];
            statusLabel.Text = message;

            // 可以添加更详细的提示
            toolTip.SetToolTip(statusStrip, message);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateStatus(message, ToolTipIcon.Error);
        }

      
    }
}