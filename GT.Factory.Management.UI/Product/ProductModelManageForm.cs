using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Product.Service;
using GT.Factory.UI.Layout;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GT.Factory.Management.UI.Product
{
    /// <summary>
    /// 产品型号管理页面。
    ///
    /// 页面职责：
    /// 1. 查询并显示所有产品型号。
    /// 2. 新增产品型号。
    /// 3. 编辑产品型号。
    /// 4. 删除产品型号。
    /// 5. 进入当前产品型号的部件管理。
    ///
    /// 注意：
    /// 本页面只负责 UI 和用户交互，
    /// 数据操作统一通过 ProductModelService 完成。
    /// </summary>
    public partial class ProductModelManageForm : Form
    {
        private readonly ProductModelService _modelService;

        private readonly ProductPartService _partService;

        private DataGridView _grid;

        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnRefresh;
        private Button _btnParts;

        /// <summary>
        /// 当前产品型号数据。
        ///
        /// 不直接从 DataGridView 中重新创建 Model，
        /// 而是保留查询结果，方便获取完整对象。
        /// </summary>
        private List<GTProductModel> _models;

        /// <summary>
        /// 创建产品型号管理页面。
        /// </summary>
        /// <param name="service">产品型号服务。</param>
        public ProductModelManageForm(ProductModelService modelService, ProductPartService partService)
        {
            if (modelService == null)
            {
                throw new ArgumentNullException("modelService");
            }
            if (partService == null)
            {
                throw new ArgumentNullException("partService");
            }
            _modelService = modelService;
            _partService = partService;
            InitializeForm();
            InitializeControls();
            LoadData();
        }

        /// <summary>
        /// 初始化窗体基础属性。
        /// </summary>
        private void InitializeForm()
        {
            Text = "产品型号管理";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1000;
            Height = 600;
            MinimumSize = new Size(800, 500);

            BackColor = UITheme.BgBase;
            Font = UITheme.FontNormal;

            // 防止窗体和表格大量刷新时闪烁。
            this.EnableDoubleBuffer();
        }

        /// <summary>
        /// 创建页面控件。
        /// </summary>
        private void InitializeControls()
        {
            // ============================================================
            // 标题
            // ============================================================

            Label title = UILabel.Title("产品型号管理");
            title.Dock = DockStyle.Top;
            title.Height = 45;
            title.Padding = new Padding(12, 8, 0, 0);


            // ============================================================
            // 操作按钮
            // ============================================================

            _btnAdd = UIButton.Primary("新增");
            _btnEdit = UIButton.Default("编辑");
            _btnDelete = UIButton.Default("删除");
            _btnRefresh = UIButton.Default("刷新");
            _btnParts = UIButton.Primary("部件管理");

            _btnAdd.Click += BtnAdd_Click;
            _btnEdit.Click += BtnEdit_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnRefresh.Click += BtnRefresh_Click;
            _btnParts.Click += BtnParts_Click;

            FlowLayoutPanel buttonBar = UILayout.ButtonBar(
                _btnAdd,
                _btnEdit,
                _btnDelete,
                _btnRefresh,
                _btnParts);

            buttonBar.Dock = DockStyle.Top;
            buttonBar.Height = UITheme.ControlHeight + 12;
            buttonBar.Padding = new Padding(12, 4, 0, 4);


            // ============================================================
            // 数据表格
            // ============================================================

            _grid = new DataGridView();

            UIGrid.Management(_grid);

            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;

            // 双击记录直接进入编辑。
            _grid.CellDoubleClick += Grid_CellDoubleClick;


            // ============================================================
            // 建立字段
            // ============================================================
            Controls.Add(_grid);
            Controls.Add(buttonBar);

            Controls.Add(title);

            AddGridColumns();
        }

        /// <summary>
        /// 创建产品型号表格列。
        /// </summary>
        private void AddGridColumns()
        {
            _grid.Columns.Clear();

            DataGridViewTextBoxColumn modelColumn =
                new DataGridViewTextBoxColumn();

            modelColumn.Name = "ProductModel";
            modelColumn.HeaderText = "产品型号";
            modelColumn.DataPropertyName = "ProductModel";
            modelColumn.FillWeight = 120;

            DataGridViewTextBoxColumn nameColumn =
                new DataGridViewTextBoxColumn();

            nameColumn.Name = "ProductName";
            nameColumn.HeaderText = "产品名称";
            nameColumn.DataPropertyName = "ProductName";
            nameColumn.FillWeight = 120;

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
            createTimeColumn.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

            DataGridViewTextBoxColumn updateTimeColumn =
                new DataGridViewTextBoxColumn();

            updateTimeColumn.Name = "UpdateTime";
            updateTimeColumn.HeaderText = "修改时间";
            updateTimeColumn.DataPropertyName = "UpdateTime";
            updateTimeColumn.FillWeight = 110;
            updateTimeColumn.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";

            _grid.Columns.Add(modelColumn);
            _grid.Columns.Add(nameColumn);
            _grid.Columns.Add(descriptionColumn);
            _grid.Columns.Add(activeColumn);
            _grid.Columns.Add(createTimeColumn);
            _grid.Columns.Add(updateTimeColumn);
        }

        /// <summary>
        /// 加载产品型号数据。
        /// </summary>
        private void LoadData()
        {
            try
            {
                _models = _modelService.GetAll();

                _grid.DataSource = null;
                _grid.DataSource = _models;

                UpdateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "加载产品型号失败。\r\n\r\n" + ex.Message,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 更新按钮状态。
        /// </summary>
        private void UpdateButtonState()
        {
            bool hasSelection = GetSelectedModel() != null;

            _btnEdit.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
            _btnParts.Enabled = hasSelection;
        }

        /// <summary>
        /// 获取当前选中的产品型号。
        /// </summary>
        private GTProductModel GetSelectedModel()
        {
            if (_grid == null ||
                _grid.SelectedRows.Count == 0)
            {
                return null;
            }

            int index = _grid.SelectedRows[0].Index;

            if (_models == null ||
                index < 0 ||
                index >= _models.Count)
            {
                return null;
            }

            return _models[index];
        }

        /// <summary>
        /// 新增产品型号。
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (ProductModelEditForm form =
                new ProductModelEditForm(_modelService, null))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        /// <summary>
        /// 编辑产品型号。
        /// </summary>
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            GTProductModel model = GetSelectedModel();

            if (model == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一个产品型号。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (ProductModelEditForm form =
                new ProductModelEditForm(_modelService, model))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        /// <summary>
        /// 删除产品型号。
        ///
        /// 数据库存在部件外键时可能拒绝物理删除，
        /// 此时将错误信息直接提示给用户。
        /// </summary>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            GTProductModel model = GetSelectedModel();

            if (model == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一个产品型号。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult result = MessageBox.Show(
                this,
                "确定删除产品型号：\r\n\r\n" +
                model.ProductModel +
                "\r\n\r\n注意：如果该型号已经存在产品部件，数据库可能拒绝删除。",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                _partService.Delete(model.ProductModelId);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "删除产品型号失败。\r\n\r\n" + ex.Message,
                    "删除失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 刷新产品型号。
        /// </summary>
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 打开当前产品型号的部件管理。
        /// </summary>
        private void BtnParts_Click(object sender, EventArgs e)
        {
            GTProductModel model = GetSelectedModel();

            if (model == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一个产品型号。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // ProductPartManageForm 只接收当前父级产品型号，
            // 防止部件被错误关联到其他产品型号。
            using (ProductPartManageForm form =
                new ProductPartManageForm(model,_partService))
            {
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// 双击产品型号直接编辑。
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
        /// 表格选择发生变化时更新按钮状态。
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _grid.SelectionChanged += Grid_SelectionChanged;

            UpdateButtonState();
        }

        /// <summary>
        /// 表格选中行变化。
        /// </summary>
        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            UpdateButtonState();
        }
    }
}
