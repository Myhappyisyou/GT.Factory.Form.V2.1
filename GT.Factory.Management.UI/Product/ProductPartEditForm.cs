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
    /// 产品部件编辑窗口。
    ///
    /// 同时用于：
    /// 1. 新增产品部件。
    /// 2. 编辑已有产品部件。
    ///
    /// 所属产品型号由外部 ProductPartManageForm 固定传入，
    /// 本窗口不允许用户修改 ProductModelId。
    /// </summary>
    public partial class ProductPartEditForm : Form
    {
        private readonly ProductPartService _service;
        private readonly GTProductModel _productModel;
        private readonly GTProductPart _part;

        private TextBox _txtPartCode;
        private TextBox _txtPartName;
        private TextBox _txtDescription;
        private CheckBox _chkIsActive;

        private Button _btnSave;
        private Button _btnCancel;

        /// <summary>
        /// 创建产品部件编辑窗口。
        /// </summary>
        /// <param name="service">产品部件服务。</param>
        /// <param name="productModel">所属产品型号。</param>
        /// <param name="part">编辑对象；新增时传 null。</param>
        public ProductPartEditForm(
            ProductPartService service,
            GTProductModel productModel,
            GTProductPart part)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            if (productModel == null)
            {
                throw new ArgumentNullException(
                    "productModel");
            }

            if (productModel.ProductModelId <= 0)
            {
                throw new ArgumentException(
                    "产品型号必须具有有效的 ProductModelId。",
                    "productModel");
            }

            _service = service;
            _productModel = productModel;
            _part = part;

            InitializeForm();
            InitializeControls();
            LoadPart();
        }

        /// <summary>
        /// 初始化窗体。
        /// </summary>
        private void InitializeForm()
        {
            Text = _part == null
                ? "新增产品部件"
                : "编辑产品部件";

            StartPosition =
                FormStartPosition.CenterParent;

            Width = 540;
            Height = 390;

            MinimumSize =
                new Size(540, 390);

            BackColor = UITheme.BgBase;
            Font = UITheme.FontNormal;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;
        }

        /// <summary>
        /// 创建编辑控件。
        /// </summary>
        private void InitializeControls()
        {
            Label title =
                UILabel.Title(
                    _part == null
                        ? "新增产品部件"
                        : "编辑产品部件");

            title.Dock = DockStyle.Top;
            title.Height = 45;
            title.Padding =
                new Padding(12, 8, 0, 0);

            Controls.Add(title);

            // ============================================================
            // 产品型号显示
            // ============================================================

            Label modelLabel =
                UILabel.FormLabel("产品型号");

            Label modelValue =
                UILabel.ValueLabel(
                    _productModel.ProductModel ??
                    string.Empty);

            modelValue.Width = 330;
            modelValue.TextAlign =
                ContentAlignment.MiddleLeft;

            Panel modelRow =
                UILayout.FormRow(
                    modelLabel,
                    modelValue);

            // ============================================================
            // 编辑控件
            // ============================================================

            _txtPartCode =
                UIInput.Text();

            _txtPartName =
                UIInput.Text();

            _txtDescription =
                UIInput.Text();

            _txtDescription.Multiline = true;
            _txtDescription.Height = 70;
            _txtDescription.ScrollBars =
                ScrollBars.Vertical;

            _chkIsActive =
                new CheckBox();

            _chkIsActive.Text = "启用";
            _chkIsActive.AutoSize = true;
            _chkIsActive.Height =
                UITheme.ControlHeight;

            _chkIsActive.ForeColor =
                UITheme.Text;

            _chkIsActive.Font =
                UITheme.FontNormal;

            Panel rowActive =
                UILayout.FormRow(
                    UILabel.FormLabel("状态"),
                    _chkIsActive);

            Panel rowDescription =
                UILayout.FormRow(
                    UILabel.FormLabel("描述"),
                    _txtDescription);

            Panel rowName =
                UILayout.FormRow(
                    UILabel.FormLabel("部件名称"),
                    _txtPartName);

            Panel rowCode =
                UILayout.FormRow(
                    UILabel.FormLabel("部件编码"),
                    _txtPartCode);

            // ============================================================
            // 表单容器
            // ============================================================

            Panel formPanel =
                new Panel();

            formPanel.Dock =
                DockStyle.Fill;

            formPanel.Padding =
                new Padding(12, 5, 12, 5);

            // DockStyle.Top 必须倒序添加。
            formPanel.Controls.Add(rowActive);
            formPanel.Controls.Add(rowDescription);
            formPanel.Controls.Add(rowName);
            formPanel.Controls.Add(rowCode);
            formPanel.Controls.Add(modelRow);

            Controls.Add(formPanel);

            // ============================================================
            // 操作按钮
            // ============================================================

            _btnSave =
                UIButton.Primary("保存");

            _btnCancel =
                UIButton.Ghost("取消");

            _btnSave.Click +=
                BtnSave_Click;

            _btnCancel.Click +=
                BtnCancel_Click;

            FlowLayoutPanel buttonBar =
                UILayout.ButtonBar(
                    _btnSave,
                    _btnCancel);

            buttonBar.Dock =
                DockStyle.Bottom;

            buttonBar.Height =
                UITheme.ControlHeight + 18;

            buttonBar.Padding =
                new Padding(0, 4, 12, 4);

            buttonBar.FlowDirection =
                FlowDirection.RightToLeft;

            Controls.Add(buttonBar);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;
        }

        /// <summary>
        /// 加载部件数据。
        /// </summary>
        private void LoadPart()
        {
            if (_part == null)
            {
                _chkIsActive.Checked = true;
                return;
            }

            _txtPartCode.Text =
                _part.PartCode ?? string.Empty;

            _txtPartName.Text =
                _part.PartName ?? string.Empty;

            _txtDescription.Text =
                _part.Description ?? string.Empty;

            _chkIsActive.Checked =
                _part.IsActive;

            _txtPartCode.Focus();
        }

        /// <summary>
        /// 保存产品部件。
        /// </summary>
        private void BtnSave_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                    _txtPartCode.Text))
                {
                    MessageBox.Show(
                        this,
                        "请输入部件编码。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    _txtPartCode.Focus();
                    return;
                }

                GTProductPart part;

                if (_part == null)
                {
                    part = new GTProductPart();

                    // 新增部件时，父级 ProductModel
                    // 是唯一合法的关联来源。
                    part.ProductModelId =
                        _productModel.ProductModelId;
                }
                else
                {
                    part = _part;

                    // 编辑时再次确保部件仍然属于
                    // 当前父级产品型号。
                    part.ProductModelId =
                        _productModel.ProductModelId;
                }

                part.PartCode =
                    _txtPartCode.Text.Trim();

                part.PartName =
                    _txtPartName.Text.Trim();

                part.Description =
                    _txtDescription.Text.Trim();

                part.IsActive =
                    _chkIsActive.Checked;

                if (_part == null)
                {
                    _service.Add(part);
                }
                else
                {
                    _service.Update(part);
                }

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "保存产品部件失败。\r\n\r\n" +
                    ex.Message,
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
            DialogResult =
                DialogResult.Cancel;

            Close();
        }
    }
}
