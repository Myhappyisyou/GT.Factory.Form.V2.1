using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Batch
{
    public partial class BatchConfigForm : Form
    {
        private TreeView tvModels;
        private FlowLayoutPanel flpParts;
        private Button btnSave;
        private Button btnAddModel;
        private Button btnAddPart;

        private BindingList<ClientBatchConfig> configs;

        public BatchConfigForm()
        {
            InitializeComponent();
            InitUI();
            LoadConfigs();
        }

        private void InitUI()
        {
            this.Text = "批次配置管理";
            this.Width = 900;
            this.Height = 500;
            this.StartPosition = FormStartPosition.CenterParent;

            var splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 10, // 左侧树宽度
            };
            this.Controls.Add(splitMain);

            // 左侧 Panel (型号树 + 新增删除型号)
            var leftPanel = new Panel { Dock = DockStyle.Fill };
            splitMain.Panel1.Controls.Add(leftPanel);

            tvModels = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            tvModels.AfterSelect += TvModels_AfterSelect;
            leftPanel.Controls.Add(tvModels);

            btnAddModel = new Button
            {
                Text = "新增型号",
                Dock = DockStyle.Top,
                Height = 30
            };
            btnAddModel.Click += BtnAddModel_Click;
            leftPanel.Controls.Add(btnAddModel);

            // 右侧 Panel (零件卡片 + 新增零件 + 保存)
            var rightPanel = new Panel { Dock = DockStyle.Fill };
            splitMain.Panel2.Controls.Add(rightPanel);

            flpParts = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5)
            };
            rightPanel.Controls.Add(flpParts);

            btnAddPart = new Button
            {
                Text = "新增零件",
                Dock = DockStyle.Top,
                Height = 30
            };
            btnAddPart.Click += BtnAddPart_Click;
            rightPanel.Controls.Add(btnAddPart);

            btnSave = new Button
            {
                Text = "保存",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnSave.Click += BtnSave_Click;
            rightPanel.Controls.Add(btnSave);
        }

        private void LoadConfigs()
        {
            configs = new BindingList<ClientBatchConfig>(BatchConfigLoader.Load());
            PopulateTree();
        }

        private void PopulateTree()
        {
            tvModels.Nodes.Clear();
            var modelGroups = configs.GroupBy(c => c.Model);

            foreach (var g in modelGroups)
            {
                var node = new TreeNode(g.Key) { Tag = g.Key };
                foreach (var part in g)
                {
                    var child = new TreeNode(part.PartName) { Tag = part };
                    node.Nodes.Add(child);
                }

                // 右键菜单：删除型号
                var cms = new ContextMenuStrip();
                var delItem = new ToolStripMenuItem("删除型号");
                delItem.Click += (s, e) => DeleteModel(node);
                cms.Items.Add(delItem);
                node.ContextMenuStrip = cms;

                tvModels.Nodes.Add(node);
            }

            if (tvModels.Nodes.Count > 0)
                tvModels.SelectedNode = tvModels.Nodes[0];
        }

        private void DeleteModel(TreeNode node)
        {
            if (MessageBox.Show($"确定删除型号 {node.Text} 及其所有零件吗？", "删除确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string model = node.Tag as string;
                configs = new BindingList<ClientBatchConfig>(configs.Where(c => c.Model != model).ToList());
                PopulateTree();
                flpParts.Controls.Clear();
            }
        }

        private void TvModels_AfterSelect(object sender, TreeViewEventArgs e)
        {
            DisplayPartsForNode(e.Node);
        }

        private void DisplayPartsForNode(TreeNode node)
        {
            flpParts.Controls.Clear();

            List<ClientBatchConfig> parts;
            if (node.Level == 0)
            {
                string model = node.Tag as string;
                parts = configs.Where(c => c.Model == model).ToList();
            }
            else
            {
                parts = new List<ClientBatchConfig> { node.Tag as ClientBatchConfig };
            }

            foreach (var part in parts)
            {
                flpParts.Controls.Add(CreatePartCard(part));
            }
        }

        private Panel CreatePartCard(ClientBatchConfig part)
        {
            var panel = new Panel
            {
                Width = flpParts.ClientSize.Width - 25,
                Height = 100,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                Tag = part
            };

            int x = 5, y = 5, w = 150;

            panel.Controls.Add(new Label { Text = "零件名", Location = new Point(x, y), Width = 60 });
            var tbName = new TextBox { Text = part.PartName, Location = new Point(x + 60, y), Width = w };
            tbName.TextChanged += (s, e) => part.PartName = tbName.Text;
            panel.Controls.Add(tbName);

            panel.Controls.Add(new Label { Text = "物料编码", Location = new Point(x + 220, y), Width = 70 });
            var tbMat = new TextBox { Text = part.MaterialCode, Location = new Point(x + 290, y), Width = w };
            tbMat.TextChanged += (s, e) => part.MaterialCode = tbMat.Text;
            panel.Controls.Add(tbMat);

            panel.Controls.Add(new Label { Text = "PLC写", Location = new Point(x, y + 30), Width = 60 });
            var tbWrite = new TextBox { Text = part.PlcWriteTotolAddress, Location = new Point(x + 60, y + 30), Width = w };
            tbWrite.TextChanged += (s, e) => part.PlcWriteTotolAddress = tbWrite.Text;
            panel.Controls.Add(tbWrite);

            panel.Controls.Add(new Label { Text = "PLC读", Location = new Point(x + 220, y + 30), Width = 60 });
            var tbRead = new TextBox { Text = part.PlcReadUsedQtyAddress, Location = new Point(x + 290, y + 30), Width = w };
            tbRead.TextChanged += (s, e) => part.PlcReadUsedQtyAddress = tbRead.Text;
            panel.Controls.Add(tbRead);

            panel.Controls.Add(new Label { Text = "PLC写BYD", Location = new Point(x, y + 60), Width = 60 });
            var tbWriteByd = new TextBox { Text = part.PlcWriteBYDBatchNubAddress, Location = new Point(x + 60, y + 60), Width = w };
            tbWriteByd.TextChanged += (s, e) => part.PlcWriteBYDBatchNubAddress = tbWriteByd.Text;
            panel.Controls.Add(tbWriteByd);

            // 删除按钮
            var btnDel = new Button { Text = "删除", Size = new Size(60, 25), Location = new Point(x + 520, y + 30) };
            btnDel.Click += (s, e) =>
            {
                flpParts.Controls.Remove(panel);
                configs.Remove(part);
                PopulateTree();
            };
            panel.Controls.Add(btnDel);

            return panel;
        }

        // 弹窗输入新型号
        private void BtnAddModel_Click(object sender, EventArgs e)
        {
            using (var dlg = new Form { Width = 300, Height = 150, Text = "新增型号", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent })
            {
                var lbl = new Label { Text = "请输入新型号名:", Location = new Point(10, 20), AutoSize = true };
                var tb = new TextBox { Location = new Point(10, 50), Width = 260 };
                var btnOk = new Button { Text = "确定", DialogResult = DialogResult.OK, Location = new Point(40, 80), Width = 80 };
                var btnCancel = new Button { Text = "取消", DialogResult = DialogResult.Cancel, Location = new Point(160, 80), Width = 80 };

                dlg.Controls.Add(lbl);
                dlg.Controls.Add(tb);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancel);

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string modelName = tb.Text.Trim();
                    if (string.IsNullOrWhiteSpace(modelName)) return;

                    if (configs.Any(c => c.Model == modelName))
                    {
                        MessageBox.Show("型号已存在！");
                        return;
                    }

                    var newPart = new ClientBatchConfig { Model = modelName, PartName = "新零件" };
                    configs.Add(newPart);
                    PopulateTree();
                }
            }
        }

        private void BtnAddPart_Click(object sender, EventArgs e)
        {
            if (tvModels.SelectedNode == null || tvModels.SelectedNode.Level != 0)
            {
                MessageBox.Show("请先选择一个型号！");
                return;
            }

            string model = tvModels.SelectedNode.Tag as string;
            var newPart = new ClientBatchConfig { Model = model, PartName = "新零件" };
            configs.Add(newPart);
            flpParts.Controls.Add(CreatePartCard(newPart));
            PopulateTree();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            BatchConfigLoader.Save(configs.ToList());
            MessageBox.Show("保存成功！");
            PopulateTree();
        }
    }


}