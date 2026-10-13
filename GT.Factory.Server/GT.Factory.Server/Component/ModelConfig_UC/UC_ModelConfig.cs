using GT.Factory.Common.Helpers;
using GT.Factory.Server.Core;
using GT.Factory.Server.Core.LanModelSync;
using GT.Factory.Server.Help;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GT.Factory.Server.Component.ModelConfig_UC
{
    public partial class UC_ModelConfig : UserControl, IPageLifecycle
    {
        private TreeView treeView;
        private PropertyGrid propertyGrid;
        private Button btnAdd, btnDelete, btnSave;
        private List<ProductModel> products = new List<ProductModel>();
        JsonService jsonService = new JsonService();

        public UC_ModelConfig()
        {
            InitializeComponent();
            InitUI();
            SetupEventHandlers();
            LoadProducts();
        }

        private void InitUI()
        {
            treeView = new TreeView { Dock = DockStyle.Left, Width = 200 };

            propertyGrid = new PropertyGrid { Dock = DockStyle.Fill,
                Name = "configPropertyGrid",
                ToolbarVisible = false,
                PropertySort = PropertySort.Categorized
            };
            Controls.Add(propertyGrid);
            Controls.Add(treeView);

            var panel = new Panel { Dock = DockStyle.Top, Height = 40 };
            btnAdd = new Button { Text = "添加产品", Left = 10, Width = 100, Top = 5, ForeColor = Color.White };
            btnAdd.Click += BtnAdd_Click;
            panel.Controls.Add(btnAdd);

            btnDelete = new Button { Text = "删除产品", Left = 120, Width = 100, Top = 5, ForeColor = Color.White };
            btnDelete.Click += BtnDelete_Click;
            panel.Controls.Add(btnDelete);

            btnSave = new Button { Text = "保存", Left = 230, Width = 100, Top = 5, ForeColor = Color.White };
            btnSave.Click += BtnSave_Click;
            panel.Controls.Add(btnSave);

            Controls.Add(panel);

            if (!Directory.Exists(PathCenter.Models)) Directory.CreateDirectory(PathCenter.Models);
        }

        private void SetupEventHandlers()
        {
            treeView.AfterSelect += TreeView_AfterSelect;
            propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;
        }

        private void LoadProducts()
        {
            treeView.Nodes.Clear();
            products.Clear();

            var files = Directory.GetFiles(PathCenter.Models, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var product = jsonService.LoadFromFile<ProductModel>(file);
                    products.Add(product);
                    treeView.Nodes.Add(TreeNodeBuilder.Build(product));
                }
                catch { } 
            }
        }

        private TreeNode CreateProductTreeNode(ProductModel product)
        {
            return TreeNodeBuilder.Build(product);
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

            if (e.Node?.Tag != null)
            {
                propertyGrid.SelectedObject = null;

                propertyGrid.SelectedObject = e.Node.Tag;
                propertyGrid.Refresh();

            }
        }

        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (treeView.SelectedNode == null) return;
            TreeNode root = GetRootNode(treeView.SelectedNode);
            TreeNodeBuilder.RefreshTreeNodeText(root);
            treeView.Refresh();

        }

        private void RefreshTreeNodeText(TreeNode node)
        {
            if (node?.Tag == null) return;

            var obj = node.Tag;

            if (node.Parent == null)
            {
                // 根节点，找 IsRootDisplay = true 的属性
                var rootProp = obj.GetType().GetProperties()
                    .Select(p => p.GetCustomAttribute<TreeNodeItemAttribute>())
                    .Where(attr => attr != null && attr.IsRootDisplay)
                    .FirstOrDefault();

                if (rootProp != null)
                {
                    var value = obj.GetType()
                                   .GetProperties()
                                   .First(p => p.GetCustomAttribute<TreeNodeItemAttribute>() == rootProp)
                                   .GetValue(obj);

                    node.Text = value?.ToString() ?? obj.ToString();
                }
                else
                {
                    node.Text = obj.ToString();
                }
            }
            else
            {
                // 子节点文本由 DisplayName 决定
                var parentObj = node.Parent.Tag;
                if (parentObj != null)
                {
                    var prop = parentObj.GetType()
                                .GetProperties()
                                .FirstOrDefault(p => p.GetValue(parentObj) == obj
                                                     && p.GetCustomAttribute<TreeNodeItemAttribute>() != null);

                    node.Text = prop?.GetCustomAttribute<TreeNodeItemAttribute>()?.DisplayName
                                ?? obj.ToString();
                }
                else
                {
                    node.Text = obj.ToString();
                }
            }

            foreach (TreeNode child in node.Nodes)
                RefreshTreeNodeText(child);
        }

        private TreeNode GetRootNode(TreeNode node)
        {
            if (node == null) return null;
            while (node.Parent != null)
            {
                node = node.Parent;
            }
            return node;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var product = new ProductModel
            {
                BaseInfo = new BaseInfo
                {
                    ProductCode = "",
                    ProductName = "新产品",
                    ProductNumber = "",
                    RecipeCode= RecipeCode.R1,
                },

                DetailInfo = new DetailInfo
                {
                    Info1 = GunpowderType.BK药,
                    Info2 = PipeType.小,
                    Info3 = ProductLength.L1,
                },
                //ParameterGroups = new List<ParameterGroup>()
            };

            products.Add(product);
            var node = CreateProductTreeNode(product);
            treeView.Nodes.Add(node);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null) return;

            // 找到根节点（ProductModel）
            TreeNode rootNode = GetRootNode(selectedNode);

            if (rootNode?.Tag is ProductModel product)
            {
                // 删除文件
                if (!string.IsNullOrEmpty(product.FilePath) && File.Exists(product.FilePath))
                {
                    File.Delete(product.FilePath);
                }

                // 从列表中移除
                products.Remove(product);

                // 从 TreeView 中移除
                treeView.Nodes.Remove(rootNode);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = treeView.SelectedNode;
            if (selectedNode == null) return;

            TreeNode rootNode = GetRootNode(selectedNode);
            if (rootNode?.Tag is ProductModel product)
            {
                string filePath = PathCenter.ModelFile(product.BaseInfo.ProductName,".json");
                product.FilePath = filePath;   // 关键！！！
                jsonService.SaveToFile(filePath, product);
            }
        }

        public void OnPageShown()
        {
            LoadProducts();
        }

        public void OnPageHidden()
        {

        }

    }
}
