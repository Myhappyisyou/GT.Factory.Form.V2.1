using EditJson;
using GT_Common.Helper.LanModelSync;
using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.ModelSelectForm
{
    public partial class ModelSelectForm : Form
    {
        private ComboBox cmbModel;
        private Button btnOk;
        private Button btnCancel;

        private JsonService _jsonService = new JsonService();
        private List<ProductModel> _models = new List<ProductModel>();

        public ProductModel SelectedModel { get; private set; }

        public ModelSelectForm()
        {
            InitUI();
            LoadModels();
        }

        private void InitUI()
        {
            this.Text = "选择型号";
            this.Size = new Size(400, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                Padding = new Padding(10)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            // 🔽 下拉框
            cmbModel = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12)
            };

            layout.Controls.Add(cmbModel, 0, 0);

            // 🔘 按钮
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            btnOk = new Button { Text = "确定", Width = 80 };
            btnCancel = new Button { Text = "取消", Width = 80 };

            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => this.Close();

            panel.Controls.Add(btnOk);
            panel.Controls.Add(btnCancel);

            layout.Controls.Add(panel, 0, 1);

            this.Controls.Add(layout);
        }

        private void LoadModels()
        {
            try
            {
                string dir = PathCenter.ConfigFile("model");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var files = Directory.GetFiles(dir, "*.json");

                _models.Clear();

                foreach (var file in files)
                {
                    try
                    {
                        var model = _jsonService.LoadFromFile<ProductModel>(file);
                        if (model != null)
                            _models.Add(model);
                    }
                    catch { }
                }

                // 👇 绑定显示
                cmbModel.DataSource = _models;
                cmbModel.DisplayMember = "BaseInfo.ProductName"; // ❗不支持嵌套，下面处理
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载型号失败：" + ex.Message);
            }

            // ⚠️ ComboBox 不支持嵌套属性，这里手动处理
            cmbModel.DataSource = _models
                .Select(m => new
                {
                    Name = m.BaseInfo.ProductName,
                    Model = m
                })
                .ToList();

            cmbModel.DisplayMember = "Name";
            cmbModel.ValueMember = "Model";
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (cmbModel.SelectedItem == null)
            {
                MessageBox.Show("请选择型号");
                return;
            }

            dynamic item = cmbModel.SelectedItem;
            SelectedModel = item.Model as ProductModel;

            if (SelectedModel != null)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
