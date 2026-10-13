using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Product.Service;
using GT.Factory.UI.Layout;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.Management.UI.Product
{
    /// <summary>
    /// 产品型号编辑窗口。
    ///
    /// 同时用于：
    /// 1. 新增产品型号。
    /// 2. 编辑已有产品型号。
    ///
    /// 当 model == null 时为新增模式。
    /// 当 model != null 时为编辑模式。
    /// </summary>
    public partial class ProductModelEditForm : Form
    {
        private readonly ProductModelService _service;
        private readonly GTProductModel _model;

        private TextBox _txtProductModel;
        private TextBox _txtProductName;
        private TextBox _txtDescription;
        private CheckBox _chkIsActive;

        private Button _btnSave;
        private Button _btnCancel;

        /// <summary>
        /// 创建产品型号编辑窗口。
        /// </summary>
        /// <param name="service">产品型号服务。</param>
        /// <param name="model">编辑对象；新增时传 null。</param>
        public ProductModelEditForm(
            ProductModelService service,
            GTProductModel model)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            _service = service;
            _model = model;

            InitializeForm();
            InitializeControls();
            LoadModel();
        }

        /// <summary>
        /// 初始化窗体。
        /// </summary>
        private void InitializeForm()
        {
            Text = _model == null
                ? "新增产品型号"
                : "编辑产品型号";

            StartPosition = FormStartPosition.CenterParent;

            Width = 520;
            Height = 360;

            MinimumSize = new Size(520, 360);

            BackColor = UITheme.BgBase;
            Font = UITheme.FontNormal;

            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
        }

        /// <summary>
        /// 创建编辑控件。
        /// </summary>
        private void InitializeControls()
        {
            Label title = UILabel.Title(
                _model == null ? "新增产品型号" : "编辑产品型号");

            title.Dock = DockStyle.Top;
            title.Height = 45;
            title.Padding = new Padding(12, 8, 0, 0);


            // ============================================================
            // 输入控件
            // ============================================================

            _txtProductModel = UIInput.Text();
            _txtProductName = UIInput.Text();
            _txtDescription = UIInput.Text();

            _txtDescription.Multiline = true;
            _txtDescription.Height = 70;
            _txtDescription.ScrollBars = ScrollBars.Vertical;

            _chkIsActive = new CheckBox();

            _chkIsActive.Text = "启用";
            _chkIsActive.AutoSize = true;
            _chkIsActive.Height = UITheme.ControlHeight;
            _chkIsActive.ForeColor = UITheme.Text;
            _chkIsActive.Font = UITheme.FontNormal;

            // ============================================================
            // 表单行
            //
            // FormRow 使用 DockStyle.Top。
            // 因此这里采用倒序 Add，确保视觉顺序正确。
            // ============================================================

            Panel formPanel = new Panel();

            formPanel.Dock = DockStyle.Fill;
            formPanel.Padding = new Padding(12, 5, 12, 5);

            Panel rowActive = UILayout.FormRow(
                UILabel.FormLabel("状态"),
                _chkIsActive);

            Panel rowDescription = UILayout.FormRow(
                UILabel.FormLabel("描述"),
                _txtDescription);

            Panel rowName = UILayout.FormRow(
                UILabel.FormLabel("产品名称"),
                _txtProductName);

            Panel rowModel = UILayout.FormRow(
                UILabel.FormLabel("产品型号"),
                _txtProductModel);

            // DockStyle.Top 的控件必须按照从下到上的顺序加入。
            formPanel.Controls.Add(rowActive);
            formPanel.Controls.Add(rowDescription);
            formPanel.Controls.Add(rowName);
            formPanel.Controls.Add(rowModel);


            // ============================================================
            // 按钮
            // ============================================================

            _btnSave = UIButton.Primary("保存");
            _btnCancel = UIButton.Ghost("取消");

            _btnSave.Click += BtnSave_Click;
            _btnCancel.Click += BtnCancel_Click;

            FlowLayoutPanel buttonBar =
                UILayout.ButtonBar(_btnSave, _btnCancel);

            buttonBar.Dock = DockStyle.Bottom;
            buttonBar.Height = UITheme.ControlHeight + 18;
            buttonBar.Padding = new Padding(0, 4, 12, 4);

            buttonBar.FlowDirection =
                FlowDirection.RightToLeft;

            Controls.Add(formPanel);

            Controls.Add(buttonBar);
            Controls.Add(title);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;
        }

        /// <summary>
        /// 将编辑对象加载到控件。
        /// </summary>
        private void LoadModel()
        {
            if (_model == null)
            {
                _chkIsActive.Checked = true;
                return;
            }

            _txtProductModel.Text =
                _model.ProductModel ?? string.Empty;

            _txtProductName.Text =
                _model.ProductName ?? string.Empty;

            _txtDescription.Text =
                _model.Description ?? string.Empty;

            _chkIsActive.Checked =
                _model.IsActive;

            // 编辑时不允许修改数据库主键。
            // 产品型号编码本身仍然允许修改，
            // 由 Service 负责重复检查。
            _txtProductModel.Focus();
        }

        /// <summary>
        /// 保存产品型号。
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    _txtProductModel.Text))
                {
                    MessageBox.Show(
                        this,
                        "请输入产品型号。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _txtProductModel.Focus();
                    return;
                }

                GTProductModel model;

                if (_model == null)
                {
                    model = new GTProductModel();
                }
                else
                {
                    model = _model;
                }

                model.ProductModel =
                    _txtProductModel.Text.Trim();

                model.ProductName =
                    _txtProductName.Text.Trim();

                model.Description =
                    _txtDescription.Text.Trim();

                model.IsActive =
                    _chkIsActive.Checked;

                if (_model == null)
                {
                    _service.Add(model);
                }
                else
                {
                    _service.Update(model);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "保存产品型号失败。\r\n\r\n" + ex.Message,
                    "保存失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 取消编辑。
        /// </summary>
        private void BtnCancel_Click(
            object sender,
            EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
