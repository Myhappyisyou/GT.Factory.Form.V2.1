using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Forms;
using GT_Common.MyEnum;
using GT_Common;
using GT_Common.DriverForm.Recipe.RecipeParameter;
using RecipeParameter.RecipeParameter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using BorderStyle = System.Windows.Forms.BorderStyle;
using Orientation = System.Windows.Forms.Orientation;
using TreeNode = System.Windows.Forms.TreeNode;
using TreeView = System.Windows.Forms.TreeView;

namespace RecipeParameter
{
    public partial class RecipeForm : Form
    {
        private TreeView treeView1;
        private PropertyGrid propertyGrid;
        private ToolStrip toolStrip;
        private ToolStripButton btnLoad, btnSave, btnAddGroup, btnAddParam, btnDelete, btnSwitchMode;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel toolStripStatusLabel1;

        private LimitConfig config = new LimitConfig();
        private LimitConfig originalConfig;   // 原始快照
        private ViewMode _viewMode = ViewMode.Operator;
        private DataGridView dgv;
        public RecipeForm()
        {
            Text = "上下限参数配置";
            this.BackColor = Color.FromArgb(245, 247, 250); // 浅灰蓝色系，柔和眼睛

            Width = 900;
            Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeComponent();
            InitializeUI();
            InitEvents();
            this.Load += (s, e) =>
            {
                ApplyUiTheme();
                LoadConfig();
                if (_viewMode == ViewMode.Operator)
                {
                    LoadOperatorView();
                }
            };
        }

        private void InitializeUI()
        {
            // ToolStrip
            toolStrip = new ToolStrip();
            btnLoad = new ToolStripButton("加载");
            btnSave = new ToolStripButton("保存");
            btnAddGroup = new ToolStripButton("添加组");
            btnAddParam = new ToolStripButton("添加参数");
            btnDelete = new ToolStripButton("删除");
            btnSwitchMode = new ToolStripButton("操作员模式");

            toolStrip.Items.AddRange(new[] { btnLoad, btnSave, btnAddGroup, btnAddParam, btnDelete, btnSwitchMode });

            btnSwitchMode.Click += (s, e) =>
            {
                btnSwitchMode.Text = _viewMode == ViewMode.Engineer
     ? "操作员模式"
     : "工程师模式";

                propertyGrid.SelectedObject = null;
                RefreshTree();

                _viewMode = _viewMode == ViewMode.Engineer
                    ? ViewMode.Operator
                    : ViewMode.Engineer;

                if (_viewMode == ViewMode.Engineer)
                {
                    dgv.Visible = false;
                    propertyGrid.Visible = true;
                }
                else
                {
                    propertyGrid.Visible = false;
                    dgv.Visible = true;

                    LoadOperatorView(); // ⭐关键
                }
            };

            foreach (ToolStripButton btn in toolStrip.Items.OfType<ToolStripButton>())
            {
                btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
                btn.Font = new Font("Microsoft YaHei", 9, FontStyle.Regular);
                btn.Margin = new Padding(3, 0, 3, 0);
                btn.ForeColor = Color.FromArgb(50, 50, 50);
            }

            // TreeView & PropertyGrid
            treeView1 = new TreeView {
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
            };
            treeView1.NodeMouseClick += (s, e) => treeView1.SelectedNode = e.Node;

            propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                CategoryForeColor = Color.FromArgb(0, 122, 204),
                LineColor = Color.FromArgb(200, 200, 200),
                ViewForeColor = Color.Black,

            };

           
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

           

            // SplitContainer
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 50 };
            split.Panel1.Controls.Add(treeView1);
            split.Panel2.Controls.Add(propertyGrid);
            split.Panel2.Controls.Add(dgv);
            propertyGrid.BringToFront(); // 默认工程师模式
            // 分隔条颜色
            split.BackColor = Color.FromArgb(200, 200, 200);
            // StatusStrip
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            statusStrip = new StatusStrip();
            statusStrip.Items.Add(toolStripStatusLabel1);

            // 加载布局
            Controls.Add(split);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            toolStrip.Dock = DockStyle.Top;
            statusStrip.Dock = DockStyle.Bottom;

            dgv.Visible = true;
            propertyGrid.Visible = false;

        }

        private void ApplyUiTheme()
        {
            // 窗体背景
            this.BackColor = Color.FromArgb(245, 247, 250);

            // 工具栏
            toolStrip.BackColor = Color.FromArgb(240, 240, 240);
            toolStrip.Padding = new Padding(5, 2, 5, 2);
            toolStrip.Font = new Font("Microsoft YaHei", 9);
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;

            // 状态栏
            statusStrip.BackColor = Color.FromArgb(230, 230, 230);
            toolStripStatusLabel1.ForeColor = Color.Black;

            // SplitContainer Panel 背景
            treeView1.BackColor = Color.FromArgb(235, 240, 245);
            treeView1.ForeColor = Color.FromArgb(50, 50, 50);
            treeView1.FullRowSelect = true;
            treeView1.HotTracking = true;
            propertyGrid.BackColor = Color.White;
            propertyGrid.ForeColor = Color.Black;
        }

        private void InitEvents()
        {
            btnLoad.Click += (s, e) => LoadConfig();
            btnSave.Click += (s, e) => SaveConfig();
            btnAddGroup.Click += (s, e) => AddGroup();
            btnAddParam.Click += (s, e) => AddParameterDialog();
            btnDelete.Click += (s, e) => DeleteSelected();

            treeView1.AfterSelect += (s, e) =>
            {
                if (e.Node.Tag is ParameterBase param)
                    propertyGrid.SelectedObject = new ParameterWrapper(param);
                else if (e.Node.Tag is ProcessGroup group)
                    propertyGrid.SelectedObject = group;
                else
                    propertyGrid.SelectedObject = null;
            };


            propertyGrid.PropertyValueChanged += (s, e) =>
            {
              
                if (treeView1.SelectedNode?.Tag is ParameterBase param)
                {
                    if ((int)Shared.user.LevelEnum < (int)param.EditableRole)
                    {
                        MessageBox.Show("权限不足，无法修改此参数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        propertyGrid.Refresh(); // 回滚显示
                        return;
                    }

                    if (param is LimitParameter limit && !limit.IsValid)
                        toolStripStatusLabel1.Text = $"⚠ 参数 '{param.Name}' 上下限非法";
                    else
                        toolStripStatusLabel1.Text = "参数已更新";
                }
                else if (treeView1.SelectedNode?.Tag is ProcessGroup group)
                {
                    treeView1.SelectedNode.Text = group.GroupName;
                    toolStripStatusLabel1.Text = "工艺组名称已更新";
                }


                var selectedNode = treeView1.SelectedNode;
                if (selectedNode?.Tag == null) return;

                string recipeName = Shared.productModel.BaseInfo?.ProductName ?? "Unknown";
              
            };
        }

        private void LoadConfig()
        {
            config = LimitConfigManager.Load();

            originalConfig = DeepClone(config);  // 一定要深拷贝！

            treeView1.Nodes.Clear();
            foreach (var group in config.Groups)
            {
                var groupNode = new TreeNode(group.GroupName) { Tag = group };
                foreach (var param in group.Parameters)
                {
                    var paramNode = new TreeNode(param.Name) { Tag = param };
                    groupNode.Nodes.Add(paramNode);
                }
                treeView1.Nodes.Add(groupNode);
            }
            treeView1.ExpandAll();
        }

        private void SaveConfig()
        {
            var changes = CompareConfigs(originalConfig, config);

            foreach (var c in changes)
                RecipeChangeLogger.LogChange(c);  // 写日志

            LimitConfigManager.Save(config);

            originalConfig = DeepClone(config); // 更新快照
            string tips = changes.Count != 0 ? "请重新选型更新PLC数据" : "";
            toolStripStatusLabel1.Text = $"配置已保存，共记录 {changes.Count} 条修改日志.{tips}";
        }


        private void RefreshTree()
        {
            treeView1.Nodes.Clear();
            foreach (var group in config.Groups)
            {
                var groupNode = new TreeNode(group.GroupName) { Tag = group };
                foreach (var param in group.Parameters)
                {
                    var paramNode = new TreeNode(param.Name) { Tag = param };

                    // 权限可视化
                    if (param is ParameterBase pb && (int)Shared.user.LevelEnum < (int)pb.EditableRole)
                        paramNode.ForeColor = Color.Gray; // 不可编辑
                    else
                        paramNode.ForeColor = Color.Black; // 可编辑

                    groupNode.Nodes.Add(paramNode);
                }
                treeView1.Nodes.Add(groupNode);
            }
            treeView1.ExpandAll();
        }

        //  深拷贝方法
        public static T DeepClone<T>(T obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }



        private void AddGroup()
        {
            var group = new ProcessGroup { GroupName = "新工艺组" };
            config.Groups.Add(group);
            RefreshTree();

            // 找到刚添加的组对应的 TreeNode 并选中它
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Tag == group)
                {
                    treeView1.SelectedNode = node;
                    node.EnsureVisible();  // 确保可见（可选）
                    break;
                }
            }
            toolStripStatusLabel1.Text = "添加了一个新工艺组";
        }

        private void AddParameterDialog()
        {
            var node = treeView1.SelectedNode;
            ProcessGroup targetGroup = null;

            if (node == null)
            {
                MessageBox.Show("请选择一个工艺组后再添加参数");
                return;
            }

            if (node.Tag is ProcessGroup group)
            {
                targetGroup = group;
            }
            else if (node.Tag is ParameterBase)
            {
                // 取父节点的工艺组
                targetGroup = node.Parent?.Tag as ProcessGroup;
            }

            if (targetGroup == null)
            {
                MessageBox.Show("请选择一个工艺组后再添加参数");
                return;
            }

            var dlg = new ParameterTypeSelectorForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ParameterBase param = dlg.CreateParameter();
                targetGroup.Parameters.Add(param);

                RefreshTree();

                // 选中新添加的参数节点
                foreach (TreeNode groupNode in treeView1.Nodes)
                {
                    if (groupNode.Tag == targetGroup)
                    {
                        foreach (TreeNode paramNode in groupNode.Nodes)
                        {
                            if (paramNode.Tag == param)
                            {
                                treeView1.SelectedNode = paramNode;
                                paramNode.EnsureVisible();
                                break;
                            }
                        }
                        break;
                    }
                }
                toolStripStatusLabel1.Text = $"添加参数：{param.Name}";
            }
        }

        private void DeleteSelected()
        {
            var node = treeView1.SelectedNode;
            if (node?.Tag is ParameterBase param)
            {
                var group = node.Parent?.Tag as ProcessGroup;
                group?.Parameters.Remove(param);
                toolStripStatusLabel1.Text = $"已删除参数：{param.Name}";
            }
            else if (node?.Tag is ProcessGroup group)
            {
                config.Groups.Remove(group);
                toolStripStatusLabel1.Text = $"已删除组：{group.GroupName}";
            }
            RefreshTree();
        }

        private List<RecipeChangeLog> CompareConfigs(LimitConfig oldCfg, LimitConfig newCfg)
        {
            List<RecipeChangeLog> logs = new List<RecipeChangeLog>();
            string user = Shared.user.UserName;
            string recipeName = Shared.productModel.BaseInfo?.ProductName ?? "Unknown";

            foreach (var newGroup in newCfg.Groups)
            {
                var oldGroup = oldCfg.Groups.FirstOrDefault(g => g.GroupName == newGroup.GroupName);
                if (oldGroup == null) continue;

                foreach (var newParam in newGroup.Parameters)
                {
                    var oldParam = oldGroup.Parameters.FirstOrDefault(p => p.Name == newParam.Name);
                    if (oldParam == null) continue;

                    // 逐属性对比
                    var props = newParam.GetType().GetProperties();

                    foreach (var prop in props)
                    {
                        var oldVal = prop.GetValue(oldParam)?.ToString();
                        var newVal = prop.GetValue(newParam)?.ToString();
                        if (oldVal == newVal) continue;

                        logs.Add(new RecipeChangeLog
                        {
                            ChangeTime = DateTime.Now,
                            UserName = user,
                            RecipeName = recipeName,
                            ParameterPath = $"{newGroup.GroupName}/{newParam.Name}/{prop.Name}",
                            OldValue = oldVal,
                            NewValue = newVal,
                            Source = "Save"
                        });
                    }
                }
            }

            return logs;
        }


        private void LoadOperatorView()
        {
            var list = new List<OperatorRow>();

            foreach (var group in config.Groups)
            {
                foreach (var param in group.Parameters)
                {
                    if (param is LimitParameter l)
                    {
                        list.Add(new OperatorRow
                        {
                            Name = param.Name,
                            Down = l.LowerLimit.Value?.ToString(),
                            Up = l.UpperLimit.Value?.ToString(),
                            Unit = l.Unit,
                        });
                    }
                }
            }

            dgv.DataSource = list;

            dgv.Columns["Name"].HeaderText = "参数";
            dgv.Columns["Down"].HeaderText = "下限";
            dgv.Columns["Up"].HeaderText = "上限";
            dgv.Columns["Unit"].HeaderText = "单位";

        }

    }

    class OperatorRow
    {
        public string Name { get; set; }
        public string Down { get; set; }
        public string Up { get; set; }
        public string Unit { get; set; }
    }
}
