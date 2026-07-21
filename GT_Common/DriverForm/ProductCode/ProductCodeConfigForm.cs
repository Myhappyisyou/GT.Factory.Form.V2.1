using GT_Common.DriverForm.InputDialog;
using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GT_Common.DriverForm.ProductCode
{
    public partial class ProductCodeConfigForm : Form
    {
        private SplitContainer split;
        private TreeView tree;
        private DataGridView grid;
        private BindingList<ProductCodeRule> list;

        private FlowLayoutPanel panel;
        private Button btnSave;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnTest;

        private StatusStrip status;
        private ToolStripStatusLabel lblStatus;

        private ContextMenuStrip treeMenu;

        private string currentModel;
        private CodeType? currentType;

        public ProductCodeConfigForm()
        {
            InitUI();
            this.Load += Form_Load;
        }

        #region UI初始化
        private void InitUI()
        {
            this.Text = "产品码规则配置";
            this.Width = 1100;
            this.Height = 650;
            this.StartPosition = FormStartPosition.CenterScreen;

            #region Split（关键：固定模式）
            split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Vertical;

            //split.Panel1MinSize = 180;
            //split.Panel2MinSize = 400;
            #endregion

            #region Tree
            tree = new TreeView();
            tree.Dock = DockStyle.Fill;
            tree.AfterSelect += Tree_AfterSelect;

            treeMenu = new ContextMenuStrip();
            var addModel = new ToolStripMenuItem("新增型号");
            addModel.Click += AddModel_Click;

            var copyModel = new ToolStripMenuItem("复制型号");
            copyModel.Click += CopyModel_Click;

            treeMenu.Items.Add(addModel);
            treeMenu.Items.Add(copyModel);

            tree.ContextMenuStrip = treeMenu;
            #endregion

            #region Grid
            grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.AutoGenerateColumns = false;
            grid.AllowUserToAddRows = false;

            InitGridColumns();
            #endregion

            #region 按钮
            panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Bottom;
            panel.Height = 40;

            btnAdd = new Button { Text = "新增" };
            btnDelete = new Button { Text = "删除" };
            btnSave = new Button { Text = "保存" };
            btnTest = new Button { Text = "扫码测试" };
            btnDelete.Enabled = false;

            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSave.Click += BtnSave_Click;
            btnTest.Click += (s, e) => new ScanTestForm().ShowDialog();

            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnDelete);
            panel.Controls.Add(btnSave);
            panel.Controls.Add(btnTest);
            #endregion

            #region 状态栏
            status = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            status.Items.Add(lblStatus);
            #endregion

            split.Panel1.Controls.Add(tree);
            split.Panel2.Controls.Add(grid);
            split.Panel2.Controls.Add(panel);

            this.Controls.Add(split);
            this.Controls.Add(status);

            grid.SelectionChanged += (s, e) =>
            {
                btnDelete.Enabled = grid.SelectedRows.Count > 0;
            };

            grid.CellValueChanged += Grid_CellValueChanged;
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty)
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            this.Load += (s, e) =>
            {
                // 窗体加载完成后设置 SplitterDistance
                split.SplitterDistance = 260;
            };
        }
        #endregion

        #region Load
        private void Form_Load(object sender, EventArgs e)
        {
            LoadTree();
        }
        #endregion

        #region Grid列
        private void InitGridColumns()
        {
            grid.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = "CodeType",
                HeaderText = "码类型",
                DataSource = EnumBindHelper.GetCodeTypeItems(),
                DisplayMember = "Text",
                ValueMember = "Value",
                Width = 120
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Model",
                HeaderText = "型号",
                Width = 120
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "CodeMark",
                HeaderText = "码标识",
                Width = 120
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Length",
                HeaderText = "长度",
                Width = 80
            });

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PartName",
                HeaderText = "部件名称",
                Width = 150,
                ReadOnly = true
            });

            grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Enable",
                HeaderText = "启用",
                Width = 60
            });
        }
        #endregion

        #region Tree加载

        //private void LoadTree()
        //{
        //    tree.Nodes.Clear();

        //    var models = ProductCodeConfig.Instance.Rules
        //        .Select(x => x.Model)
        //        .Distinct()
        //        .ToList();

        //    foreach (var model in models)
        //    {
        //        var modelNode = new TreeNode(model);
        //        modelNode.Tag = new TreeTag { Model = model };

        //        foreach (CodeType type in Enum.GetValues(typeof(CodeType)))
        //        {
        //            var node = new TreeNode(EnumHelper.GetDescription(type));
        //            node.Tag = new TreeTag
        //            {
        //                Model = model,
        //                CodeType = type
        //            };

        //            modelNode.Nodes.Add(node);
        //        }

        //        tree.Nodes.Add(modelNode);
        //    }

        //    tree.ExpandAll();
        //}

        private void LoadTree()
        {
            tree.Nodes.Clear();

            var rules = ProductCodeConfig.Instance.Rules;

            var modelGroups = rules
                .GroupBy(x => x.Model)
                .ToList();

            foreach (var modelGroup in modelGroups)
            {
                var model = modelGroup.Key;

                var modelNode = new TreeNode(model);
                modelNode.Tag = new TreeTag { Model = model };

                var typeGroups = modelGroup
                    .GroupBy(x => x.CodeType)
                    .ToList();

                foreach (var typeGroup in typeGroups)
                {
                    var type = typeGroup.Key;

                    var node = new TreeNode(EnumHelper.GetDescription(type));
                    node.Tag = new TreeTag
                    {
                        Model = model,
                        CodeType = type
                    };

                    modelNode.Nodes.Add(node);
                }

                tree.Nodes.Add(modelNode);
            }

            tree.ExpandAll();
        }

        #endregion

        #region Tree联动
        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeTag tag = e.Node.Tag as TreeTag; 
            if (tag == null) return;

            currentModel = tag.Model;
            currentType = tag.CodeType;

            lblStatus.Text = $"当前：{currentModel} / {(currentType?.ToString() ?? "全部")}";

            RefreshGrid();
        }
        #endregion

        #region Grid刷新
        private void RefreshGrid()
        {
            var data = ProductCodeConfig.Instance.Rules.AsEnumerable();

            if (!string.IsNullOrEmpty(currentModel))
                data = data.Where(x => x.Model == currentModel);

            if (currentType.HasValue)
                data = data.Where(x => x.CodeType == currentType.Value);

            list = new BindingList<ProductCodeRule>(data.ToList());
            grid.DataSource = list;
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = grid.Rows[e.RowIndex];
            var rule = row.DataBoundItem as ProductCodeRule;
            if (rule == null) return;

            // 判断是否是 CodeType 列
            if (grid.Columns[e.ColumnIndex].DataPropertyName == "CodeType")
            {
                rule.PartName = CodeTypeHelper.GetPartName(rule.CodeType);

                grid.Refresh(); // 刷新显示
            }
        }
        #endregion

        #region 新增型号
        private void AddModel_Click(object sender, EventArgs e)
        {
            using (InputDialogForm dlg = new InputDialogForm("新增型号", "请输入型号"))
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                string model = dlg.InputText.Trim();
                if (string.IsNullOrEmpty(model)) return;

                if (ProductCodeConfig.Instance.Rules.Any(x => x.Model == model))
                {
                    MessageBox.Show("型号已存在");
                    return;
                }

                ProductCodeConfig.Instance.Rules.Add(new ProductCodeRule
                {
                    Model = model,
                    Enable = true
                });

                ProductCodeConfig.Save();
                LoadTree();
            }
        }
        #endregion

        #region 复制型号
        private void CopyModel_Click(object sender, EventArgs e)
        {
            if (tree.SelectedNode == null) return;

            var tag = tree.SelectedNode.Tag as TreeTag;
            if (tag == null || string.IsNullOrEmpty(tag.Model))
            {
                MessageBox.Show("请选择型号节点");
                return;
            }

            string sourceModel = tag.Model;

            using (InputDialogForm dlg = new InputDialogForm("复制型号", "请输入新型号"))
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                string newModel = dlg.InputText.Trim();
                if (string.IsNullOrEmpty(newModel)) return;

                var rules = ProductCodeConfig.Instance.Rules;

                if (rules.Any(x => x.Model == newModel))
                {
                    MessageBox.Show("型号已存在");
                    return;
                }

                // 🔥 关键：复制当前型号所有规则
                var copyList = rules
                    .Where(x => x.Model == sourceModel)
                    .Select(x => new ProductCodeRule
                    {
                        Model = newModel,
                        CodeType = x.CodeType,
                        CodeMark = x.CodeMark,
                        Length = x.Length,
                        PartName = x.PartName,
                        Enable = x.Enable
                    })
                    .ToList();

                rules.AddRange(copyList);

                ProductCodeConfig.Save();
                LoadTree();
            }
        }

        #endregion
        #region 操作
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentModel))
            {
                MessageBox.Show("请选择型号");
                return;
            }
            var type = currentType ?? CodeType.Shell;
            var newRule = new ProductCodeRule
            {
                Model = currentModel,
                CodeType = type,
                PartName = CodeTypeHelper.GetPartName(type), // 🔥 自动填
                Length = 20,
                Enable = true
            };

            list.Add(newRule);

            // 🔥 同步到全局
            ProductCodeConfig.Instance.Rules.Add(newRule);

            LoadTree(); // 刷新树
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null)
            {
                MessageBox.Show("请选择要删除的记录");
                return;
            }

            var rule = list[grid.CurrentRow.Index];

            var result = MessageBox.Show(
                $"确认删除？\n型号：{rule.Model}\n类型：{rule.CodeType}\n部件：{rule.PartName}",
                "删除确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            list.RemoveAt(grid.CurrentRow.Index);
            ProductCodeConfig.Instance.Rules.Remove(rule);

            LoadTree();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 1. 先拿到全量数据
            var all = ProductCodeConfig.Instance.Rules;

            // 2. 删除当前视图对应的数据（而不是覆盖）
            all.RemoveAll(x =>
                x.Model == currentModel &&
                (!currentType.HasValue || x.CodeType == currentType.Value));

            // 3. 加回当前 list
            all.AddRange(list);

            // 🔥 校验
            var errors = ProductCodeValidator.Validate(all);

            if (errors.Any())
            {
                MessageBox.Show(string.Join("\n", errors), "配置错误");
                return;
            }

            ProductCodeConfig.Save();

            lblStatus.Text = "保存成功 ✔";
            LoadTree();
        }
        #endregion
    }
}