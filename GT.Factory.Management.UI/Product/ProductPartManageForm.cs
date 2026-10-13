using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Product.Repository;
using GT.Factory.Product.Service;
using GT.Factory.UI.Layout;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.Management.UI.Product
{
    /// <summary>
    /// 产品部件管理窗口。
    ///
    /// 本窗口必须依附一个产品型号打开。
    ///
    /// 数据关系：
    ///
    /// ProductModel
    ///     ↓
    /// ProductPart
    ///
    /// 因此 ProductModel 是本窗口的固定父级，
    /// 用户不能在这里切换产品型号。
    /// </summary>
    public partial class ProductPartManageForm : Form
    {
        private readonly GTProductModel _productModel;
        private readonly ProductPartService _service;

        // ===== 区域 =====
        private DataGridView _grid;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;

        private List<GTProductPart> _parts;

        /// <summary>
        /// 创建产品部件管理窗口。
        /// </summary>
        /// <param name="productModel">
        /// 当前部件所属的产品型号。
        /// </param>
        public ProductPartManageForm(
            GTProductModel productModel, ProductPartService partService)
        {
            if (productModel == null)
            {
                throw new ArgumentNullException("productModel");
            }

            if (productModel.ProductModelId <= 0)
            {
                throw new ArgumentException(
                    "产品型号必须具有有效的 ProductModelId。",
                    "productModel");
            }

            _productModel = productModel;

            _service = partService;

            InitializeForm();
            InitializeControls();
            LoadData();
        }

        /// <summary>
        /// 初始化窗体。
        /// </summary>
        private void InitializeForm()
        {
            Text =
                "部件管理 - " +
                (_productModel.ProductModel ?? string.Empty);

            StartPosition = FormStartPosition.CenterScreen;

            Width = 950;
            Height = 560;

            MinimumSize = new Size(800, 480);

            BackColor = UITheme.BgBase;
            Font = UITheme.FontNormal;

            this.EnableDoubleBuffer();
        }

        /// <summary>
        /// 创建页面控件。
        /// </summary>
        private void InitializeControls()
        {
            // ============================================================
            // 产品型号信息
            // ============================================================
           
            Label title = UILabel.Title(
                "产品部件管理");

            title.Dock = DockStyle.Top;
            title.Height = 42;
            title.Padding = new Padding(12, 8, 0, 0);

            //Controls.Add(title);

            Panel modelPanel = new Panel();

            modelPanel.Dock = DockStyle.Top;
            modelPanel.Height = UITheme.ControlHeight + 12;
            modelPanel.Padding = new Padding(12, 2, 12, 2);

            Label modelLabel =
                UILabel.FormLabel("产品型号", 90);

            Label modelValue =
                UILabel.ValueLabel(
                    _productModel.ProductModel ?? string.Empty);

            modelValue.Width = 300;
            modelValue.TextAlign =
                ContentAlignment.MiddleLeft;

            modelLabel.Location =
                new Point(0, 3);

            modelValue.Location =
                new Point(
                    modelLabel.Width +
                    UITheme.ControlSpacing,
                    3);

            modelPanel.Controls.Add(modelValue);
            modelPanel.Controls.Add(modelLabel);


            // ============================================================
            // 操作按钮
            // ============================================================
          

            _btnAdd = UIButton.Primary("新增");
            _btnEdit = UIButton.Default("编辑");
            _btnDelete = UIButton.Default("删除");
            _btnRefresh = UIButton.Default("刷新");

            _btnAdd.Click += BtnAdd_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnRefresh.Click += BtnRefresh_Click;

            FlowLayoutPanel buttonBar =
                UILayout.ButtonBar(
                    _btnAdd,
                    _btnEdit,
                    _btnDelete,
                    _btnRefresh);

            buttonBar.Dock = DockStyle.Top;
            buttonBar.Height = UITheme.ControlHeight + 12;
            buttonBar.Padding = new Padding(12, 4, 0, 4);


            //Controls.Add(buttonBar);

            // ============================================================
            // 数据表格
            // ============================================================
           
            _grid = new DataGridView();

            UIGrid.Management(_grid);


            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;

            _grid.CellDoubleClick +=
                Grid_CellDoubleClick;

            _grid.SelectionChanged +=
                Grid_SelectionChanged;


            Controls.Add(_grid);
            Controls.Add(buttonBar);
            Controls.Add(modelPanel);

            AddGridColumns();
           
        }

        /// <summary>
        /// 创建部件表格列。
        /// </summary>
        private void AddGridColumns()
        {
            _grid.Columns.Clear();

            DataGridViewTextBoxColumn codeColumn =
                new DataGridViewTextBoxColumn();

            codeColumn.Name = "PartCode";
            codeColumn.HeaderText = "部件编码";
            codeColumn.DataPropertyName = "PartCode";
            codeColumn.FillWeight = 120;

            DataGridViewTextBoxColumn nameColumn =
                new DataGridViewTextBoxColumn();

            nameColumn.Name = "PartName";
            nameColumn.HeaderText = "部件名称";
            nameColumn.DataPropertyName = "PartName";
            nameColumn.FillWeight = 110;

            DataGridViewTextBoxColumn descriptionColumn =
                new DataGridViewTextBoxColumn();

            descriptionColumn.Name = "Description";
            descriptionColumn.HeaderText = "描述";
            descriptionColumn.DataPropertyName = "Description";
            descriptionColumn.FillWeight = 180;

            DataGridViewCheckBoxColumn activeColumn =
                new DataGridViewCheckBoxColumn();

            activeColumn.Name = "IsActive";
            activeColumn.HeaderText = "启用";
            activeColumn.DataPropertyName = "IsActive";
            activeColumn.FillWeight = 60;

            DataGridViewTextBoxColumn createTimeColumn =
                new DataGridViewTextBoxColumn();

            createTimeColumn.Name = "CreateTime";
            createTimeColumn.HeaderText = "创建时间";
            createTimeColumn.DataPropertyName = "CreateTime";
            createTimeColumn.FillWeight = 110;
            createTimeColumn.DefaultCellStyle.Format =
                "yyyy-MM-dd HH:mm:ss";

            DataGridViewTextBoxColumn updateTimeColumn =
                new DataGridViewTextBoxColumn();

            updateTimeColumn.Name = "UpdateTime";
            updateTimeColumn.HeaderText = "修改时间";
            updateTimeColumn.DataPropertyName = "UpdateTime";
            updateTimeColumn.FillWeight = 110;
            updateTimeColumn.DefaultCellStyle.Format =
                "yyyy-MM-dd HH:mm:ss";

            _grid.Columns.Add(codeColumn);
            _grid.Columns.Add(nameColumn);
            _grid.Columns.Add(descriptionColumn);
            _grid.Columns.Add(activeColumn);
            _grid.Columns.Add(createTimeColumn);
            _grid.Columns.Add(updateTimeColumn);
        }

        /// <summary>
        /// 查询当前产品型号下的全部部件。
        /// </summary>
        private void LoadData()
        {
            try
            {
                _parts =
                    _service.GetByProductModelId(
                        _productModel.ProductModelId);

                _grid.DataSource = null;
                _grid.DataSource = _parts;

                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "加载产品部件失败。\r\n\r\n" +
                    ex.Message,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取当前选中的部件。
        /// </summary>
        private GTProductPart GetSelectedPart()
        {
            if (_grid == null ||
                _grid.SelectedRows.Count == 0)
            {
                return null;
            }

            int index =
                _grid.SelectedRows[0].Index;

            if (_parts == null ||
                index < 0 ||
                index >= _parts.Count)
            {
                return null;
            }

            return _parts[index];
        }

        /// <summary>
        /// 更新按钮状态。
        /// </summary>
        private void UpdateButtonState()
        {
            bool hasSelection =
                GetSelectedPart() != null;

            _btnEdit.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        /// <summary>
        /// 新增部件。
        /// </summary>
        private void BtnAdd_Click(
            object sender,
            EventArgs e)
        {
            using (ProductPartEditForm form =
                new ProductPartEditForm(
                    _service,
                    _productModel,
                    null))
            {
                if (form.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        /// <summary>
        /// 编辑部件。
        /// </summary>
        private void BtnEdit_Click(
            object sender,
            EventArgs e)
        {
            GTProductPart part =
                GetSelectedPart();

            if (part == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一个产品部件。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (ProductPartEditForm form =
                new ProductPartEditForm(
                    _service,
                    _productModel,
                    part))
            {
                if (form.ShowDialog(this) ==
                    DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        /// <summary>
        /// 删除部件。
        /// </summary>
        private void BtnDelete_Click(
            object sender,
            EventArgs e)
        {
            GTProductPart part =
                GetSelectedPart();

            if (part == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一个产品部件。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    this,
                    "确定删除部件：\r\n\r\n" +
                    part.PartCode +
                    "\r\n\r\n" +
                    "如果该部件已经存在产品码规则，" +
                    "数据库可能拒绝删除。",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _service.Delete(part.PartId);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "删除产品部件失败。\r\n\r\n" +
                    ex.Message,
                    "删除失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新。
        /// </summary>
        private void BtnRefresh_Click(
            object sender,
            EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 双击部件进入编辑。
        /// </summary>
        private void Grid_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            BtnEdit_Click(sender, EventArgs.Empty);
        }

        /// <summary>
        /// 表格选择变化。
        /// </summary>
        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            UpdateButtonState();
        }
    }
}
