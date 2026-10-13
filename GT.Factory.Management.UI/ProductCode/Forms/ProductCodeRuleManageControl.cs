using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Product.Service;
using GT.Factory.ProductCode.Service;
using GT.Factory.ProductCode.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Management.UI.ProductCode.Forms
{
    /// <summary>
    /// 产品码配置管理窗口。
    ///
    /// 当前页面采用：
    ///
    /// 产品型号
    ///     ↓
    /// 零部件
    ///     ↓
    /// 产品码规则
    ///
    /// 页面左侧、中间、右侧分别显示：
    ///
    /// 1. 产品型号
    /// 2. 零部件
    /// 3. 产品码规则
    ///
    /// 下方显示当前规则的详细信息。
    ///
    /// 本窗口负责：
    ///
    /// 1. 加载产品型号
    /// 2. 根据型号加载零部件
    /// 3. 根据零部件加载产品码规则
    /// 4. 新增规则
    /// 5. 编辑规则
    /// 6. 删除规则
    /// 7. 刷新页面
    ///
    /// 本窗口不负责：
    ///
    /// 1. Segment具体编辑
    /// 2. SQL
    /// 3. Repository
    /// 4. 产品码生成算法
    ///
    /// 这些职责分别由：
    ///
    /// ProductCodeRuleForm
    /// Service
    /// Repository
    ///
    /// 完成。
    /// </summary>
    public partial class ProductCodeRuleManageControl : UserControl
    {
        #region Service

        /// <summary>
        /// 产品型号业务服务。
        /// </summary>
        private readonly ProductModelService _productModelService;

        /// <summary>
        /// 产品零部件业务服务。
        /// </summary>
        private readonly ProductPartService _productPartService;

        /// <summary>
        /// 产品码规则业务服务。
        /// </summary>
        private readonly ProductCodeRuleService _ruleService;

        #endregion

        #region 当前数据

        /// <summary>
        /// 当前页面的所有产品型号。
        /// </summary>
        private List<GTProductModel> _productModels;

        /// <summary>
        /// 当前选中型号下的所有零部件。
        /// </summary>
        private List<GTProductPart> _productParts;

        /// <summary>
        /// 当前选中零部件下的所有规则。
        /// </summary>
        private List<CodeRule> _rules;

        /// <summary>
        /// 当前选中的产品型号。
        /// </summary>
        private GTProductModel _currentProductModel;

        /// <summary>
        /// 当前选中的零部件。
        /// </summary>
        private GTProductPart _currentProductPart;

        /// <summary>
        /// 当前选中的规则。
        /// </summary>
        private CodeRule _currentRule;

        #endregion

        #region 左侧列表

        /// <summary>
        /// 产品型号列表。
        /// </summary>
        private ListBox _productModelList;

        /// <summary>
        /// 零部件列表。
        /// </summary>
        private ListBox _partList;

        /// <summary>
        /// 产品码规则列表。
        /// </summary>
        private ListBox _ruleList;

        #endregion

        #region 规则详情

        /// <summary>
        /// 所属产品型号。
        /// </summary>
        private Label _productModelValue;

        /// <summary>
        /// 所属零部件。
        /// </summary>
        private Label _partValue;

        /// <summary>
        /// 规则名称。
        /// </summary>
        private Label _ruleNameValue;

        /// <summary>
        /// 规则描述。
        /// </summary>
        private Label _descriptionValue;

        /// <summary>
        /// 规则状态。
        /// </summary>
        private Label _statusValue;

        /// <summary>
        /// Segment数量。
        /// </summary>
        private Label _segmentCountValue;

        /// <summary>
        /// Segment结构摘要。
        /// </summary>
        private ListBox _segmentSummary;

        /// <summary>
        /// 产品码示例。
        /// </summary>
        private TextBox _previewText;

        #endregion

        #region 操作按钮

        /// <summary>
        /// 新增规则按钮。
        /// </summary>
        private Button _newButton;

        /// <summary>
        /// 编辑规则按钮。
        /// </summary>
        private Button _editButton;

        /// <summary>
        /// 删除规则按钮。
        /// </summary>
        private Button _deleteButton;

        /// <summary>
        /// 刷新按钮。
        /// </summary>
        private Button _refreshButton;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建产品码规则管理窗口。
        ///
        /// 三个 Service 从外部传入，
        /// Form 不负责创建数据库对象。
        /// </summary>
        public ProductCodeRuleManageControl(
            ProductModelService productModelService,
            ProductPartService productPartService,
            ProductCodeRuleService ruleService)
        {

            if (productModelService == null)
            {
                throw new ArgumentNullException(
                    "productModelService");
            }

            if (productPartService == null)
            {
                throw new ArgumentNullException(
                    "productPartService");
            }

            if (ruleService == null)
            {
                throw new ArgumentNullException(
                    "ruleService");
            }

            _productModelService =
                productModelService;

            _productPartService =
                productPartService;

            _ruleService =
                ruleService;

            InitializeData();
            InitializeUI();
            BindEvents();

            LoadProductModels();
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化页面数据。
        /// </summary>
        private void InitializeData()
        {
            _productModels =
                new List<GTProductModel>();

            _productParts =
                new List<GTProductPart>();

            _rules =
                new List<CodeRule>();
        }

        /// <summary>
        /// 初始化整个管理页面。
        ///
        /// 使用代码创建 WinForms 控件，
        /// 不使用 Designer。
        /// </summary>
        private void InitializeUI()
        {
            Text =
                "产品码配置管理";

            Width =
                1250;

            Height =
                780;

            MinimumSize =
                new Size(
                    1000,
                    650);

            //StartPosition =
            //    FormStartPosition.CenterScreen;

            BackColor =
                Color.White;

            Font =
                new Font(
                    "Microsoft YaHei",
                    9F);

            //--------------------------------------------------
            // 顶部标题
            //--------------------------------------------------

            Panel header =
                CreateHeaderPanel();

            //--------------------------------------------------
            // 中间三列
            //--------------------------------------------------

            TableLayoutPanel listArea =
                CreateListArea();

            //--------------------------------------------------
            // 下方规则详情
            //--------------------------------------------------

            Panel detailPanel =
                CreateDetailPanel();

            //--------------------------------------------------
            // 底部操作栏
            //--------------------------------------------------

            Panel buttonPanel =
                CreateButtonPanel();

            //--------------------------------------------------
            // 使用 Dock 从下往上布局
            //--------------------------------------------------

            Controls.Add(
                detailPanel);

            Controls.Add(
                listArea);

            Controls.Add(
                header);

            Controls.Add(
                buttonPanel);

            buttonPanel.Dock =
                DockStyle.Bottom;

            detailPanel.Dock =
                DockStyle.Bottom;

            listArea.Dock =
                DockStyle.Fill;

            header.Dock =
                DockStyle.Top;
        }

        /// <summary>
        /// 创建页面标题区域。
        /// </summary>
        private Panel CreateHeaderPanel()
        {
            Panel panel =
                new Panel();

            panel.Height =
                52;

            panel.Padding =
                new Padding(
                    12,
                    8,
                    12,
                    8);

            Label title =
                new Label();

            title.Text =
                "产品码配置管理";

            title.AutoSize =
                true;

            title.Font =
                new Font(
                    "Microsoft YaHei",
                    14F,
                    FontStyle.Bold);

            title.ForeColor =
                Color.FromArgb(
                    45,
                    75,
                    100);

            title.Location =
                new Point(
                    12,
                    12);

            panel.Controls.Add(
                title);

            return panel;
        }

        /// <summary>
        /// 创建型号、部件、规则三个列表区域。
        /// </summary>
        private TableLayoutPanel CreateListArea()
        {
            TableLayoutPanel table =
                new TableLayoutPanel();

            table.Dock =
                DockStyle.Fill;

            table.ColumnCount =
                3;

            table.RowCount =
                1;

            table.Padding =
                new Padding(
                    10,
                    0,
                    10,
                    8);

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.33F));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.33F));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    33.34F));

            //--------------------------------------------------
            // 产品型号
            //--------------------------------------------------

            Panel modelPanel =
                CreateListPanel(
                    "产品型号",
                    out _productModelList);

            //--------------------------------------------------
            // 零部件
            //--------------------------------------------------

            Panel partPanel =
                CreateListPanel(
                    "零部件",
                    out _partList);

            //--------------------------------------------------
            // 规则
            //--------------------------------------------------

            Panel rulePanel =
                CreateListPanel(
                    "产品码规则",
                    out _ruleList);

            table.Controls.Add(
                modelPanel,
                0,
                0);

            table.Controls.Add(
                partPanel,
                1,
                0);

            table.Controls.Add(
                rulePanel,
                2,
                0);

            return table;
        }

        /// <summary>
        /// 创建一个带标题的列表区域。
        /// </summary>
        private Panel CreateListPanel(
            string title,
            out ListBox list)
        {
            Panel panel =
                new Panel();

            panel.Dock =
                DockStyle.Fill;

            panel.Margin =
                new Padding(
                    4);

            panel.Padding =
                new Padding(
                    8);

            panel.BackColor =
                Color.FromArgb(
                    245,
                    249,
                    252);

            Label label =
                new Label();

            label.Text =
                title;

            label.Dock =
                DockStyle.Top;

            label.Height =
                30;

            label.Font =
                new Font(
                    "Microsoft YaHei",
                    10F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    55,
                    80,
                    100);

            list =
                new ListBox();

            list.Dock =
                DockStyle.Fill;

            list.BorderStyle =
                BorderStyle.FixedSingle;

            list.IntegralHeight =
                false;

            list.Font =
                new Font(
                    "Microsoft YaHei",
                    10F);

            list.BackColor =
                Color.White;

            panel.Controls.Add(
                list);

            panel.Controls.Add(
                label);

            return panel;
        }

        /// <summary>
        /// 创建规则详情区域。
        /// </summary>
        private Panel CreateDetailPanel()
        {
            Panel outer =
                new Panel();

            outer.Height =
                245;

            outer.Dock =
                DockStyle.Bottom;

            outer.Padding =
                new Padding(
                    10,
                    0,
                    10,
                    8);

            GroupBox group =
                new GroupBox();

            group.Text =
                "规则详情";

            group.Dock =
                DockStyle.Fill;

            group.Padding =
                new Padding(
                    10);

            TableLayoutPanel table =
                new TableLayoutPanel();

            table.Dock =
                DockStyle.Fill;

            table.ColumnCount =
                4;

            table.RowCount =
                5;

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    90));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    38));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    90));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    62));

            for (int i = 0;
                i < 5;
                i++)
            {
                table.RowStyles.Add(
                    new RowStyle(
                        SizeType.Absolute,
                        34));
            }

            //--------------------------------------------------
            // 第一行：型号 + 部件
            //--------------------------------------------------

            table.Controls.Add(
                CreateDetailLabel("所属型号"),
                0,
                0);

            _productModelValue =
                CreateValueLabel();

            table.Controls.Add(
                _productModelValue,
                1,
                0);

            table.Controls.Add(
                CreateDetailLabel("所属部件"),
                2,
                0);

            _partValue =
                CreateValueLabel();

            table.Controls.Add(
                _partValue,
                3,
                0);

            //--------------------------------------------------
            // 第二行：规则名称 + 状态
            //--------------------------------------------------

            table.Controls.Add(
                CreateDetailLabel("规则名称"),
                0,
                1);

            _ruleNameValue =
                CreateValueLabel();

            table.Controls.Add(
                _ruleNameValue,
                1,
                1);

            table.Controls.Add(
                CreateDetailLabel("状态"),
                2,
                1);

            _statusValue =
                CreateValueLabel();

            table.Controls.Add(
                _statusValue,
                3,
                1);

            //--------------------------------------------------
            // 第三行：描述
            //--------------------------------------------------

            table.Controls.Add(
                CreateDetailLabel("规则描述"),
                0,
                2);

            _descriptionValue =
                CreateValueLabel();

            table.Controls.Add(
                _descriptionValue,
                1,
                2);

            table.SetColumnSpan(
                _descriptionValue,
                3);

            //--------------------------------------------------
            // 第四行：Segment
            //--------------------------------------------------

            table.Controls.Add(
                CreateDetailLabel("Segment数量"),
                0,
                3);

            _segmentCountValue =
                CreateValueLabel();

            table.Controls.Add(
                _segmentCountValue,
                1,
                3);

            table.Controls.Add(
                CreateDetailLabel("结构"),
                2,
                3);

            _segmentSummary =
                new ListBox();

            _segmentSummary.Dock =
                DockStyle.Fill;

            _segmentSummary.BorderStyle =
                BorderStyle.FixedSingle;

            _segmentSummary.Font =
                new Font(
                    "Microsoft YaHei",
                    9F);

            table.Controls.Add(
                _segmentSummary,
                3,
                3);

            //--------------------------------------------------
            // 第五行：示例
            //--------------------------------------------------

            table.Controls.Add(
                CreateDetailLabel("示例预览"),
                0,
                4);

            _previewText =
                new TextBox();

            _previewText.Dock =
                DockStyle.Fill;

            _previewText.ReadOnly =
                true;

            _previewText.BackColor =
                Color.White;

            _previewText.Font =
                new Font(
                    "Consolas",
                    10F);

            table.Controls.Add(
                _previewText,
                1,
                4);

            table.SetColumnSpan(
                _previewText,
                3);

            group.Controls.Add(
                table);

            outer.Controls.Add(
                group);

            return outer;
        }

        /// <summary>
        /// 创建详情区域的字段标题。
        /// </summary>
        private Label CreateDetailLabel(
            string text)
        {
            Label label =
                new Label();

            label.Text =
                text;

            label.Dock =
                DockStyle.Fill;

            label.TextAlign =
                ContentAlignment.MiddleLeft;

            label.Font =
                new Font(
                    "Microsoft YaHei",
                    9F,
                    FontStyle.Bold);

            label.ForeColor =
                Color.FromArgb(
                    80,
                    90,
                    100);

            return label;
        }

        /// <summary>
        /// 创建详情区域的值标签。
        /// </summary>
        private Label CreateValueLabel()
        {
            Label label =
                new Label();

            label.Text =
                "";

            label.Dock =
                DockStyle.Fill;

            label.TextAlign =
                ContentAlignment.MiddleLeft;

            label.AutoEllipsis =
                true;

            label.ForeColor =
                Color.FromArgb(
                    45,
                    45,
                    45);

            return label;
        }

        /// <summary>
        /// 创建底部操作区域。
        /// </summary>
        private Panel CreateButtonPanel()
        {
            Panel panel =
                new Panel();

            panel.Height =
                52;

            panel.Padding =
                new Padding(
                    10,
                    5,
                    10,
                    5);

            //--------------------------------------------------
            // 新增
            //--------------------------------------------------

            _newButton =
                CreateButton(
                    "新增规则",
                    100);

            //--------------------------------------------------
            // 编辑
            //--------------------------------------------------

            _editButton =
                CreateButton(
                    "编辑",
                    80);

            //--------------------------------------------------
            // 删除
            //--------------------------------------------------

            _deleteButton =
                CreateButton(
                    "删除",
                    80);

            //--------------------------------------------------
            // 刷新
            //--------------------------------------------------

            _refreshButton =
                CreateButton(
                    "刷新",
                    80);

            FlowLayoutPanel buttons =
                new FlowLayoutPanel();

            buttons.Dock =
                DockStyle.Fill;

            buttons.FlowDirection =
                FlowDirection.LeftToRight;

            buttons.WrapContents =
                false;

            buttons.Controls.Add(
                _newButton);

            buttons.Controls.Add(
                _editButton);

            buttons.Controls.Add(
                _deleteButton);

            buttons.Controls.Add(
                _refreshButton);

            panel.Controls.Add(
                buttons);

            return panel;
        }

        /// <summary>
        /// 创建标准操作按钮。
        /// </summary>
        private Button CreateButton(
            string text,
            int width)
        {
            Button button =
                new Button();

            button.Text =
                text;

            button.Width =
                width;

            button.Height =
                32;

            button.Margin =
                new Padding(
                    4,
                    3,
                    4,
                    3);

            button.Font =
                new Font(
                    "Microsoft YaHei",
                    9F);

            return button;
        }

        #endregion

        #region 事件绑定

        /// <summary>
        /// 绑定页面事件。
        /// </summary>
        private void BindEvents()
        {
            _productModelList.SelectedIndexChanged +=
                ProductModelList_SelectedIndexChanged;

            _partList.SelectedIndexChanged +=
                PartList_SelectedIndexChanged;

            _ruleList.SelectedIndexChanged +=
                RuleList_SelectedIndexChanged;

            _ruleList.DoubleClick +=
                RuleList_DoubleClick;

            _newButton.Click +=
                NewButton_Click;

            _editButton.Click +=
                EditButton_Click;

            _deleteButton.Click +=
                DeleteButton_Click;

            _refreshButton.Click +=
                RefreshButton_Click;
        }

        #endregion

        #region 产品型号

        /// <summary>
        /// 加载所有产品型号。
        /// </summary>
        private void LoadProductModels()
        {
            try
            {
                _productModels =
                    _productModelService.GetAll();

                if (_productModels == null)
                {
                    _productModels =
                        new List<GTProductModel>();
                }

                RefreshProductModelList();
            }
            catch (Exception ex)
            {
                ShowLoadError(
                    "加载产品型号失败。",
                    ex);

                _productModels =
                    new List<GTProductModel>();

                RefreshProductModelList();
            }
        }

        /// <summary>
        /// 刷新产品型号列表。
        /// </summary>
        private void RefreshProductModelList()
        {
            int oldIndex =
                _productModelList.SelectedIndex;

            _productModelList.Items.Clear();

            for (int i = 0;
                i < _productModels.Count;
                i++)
            {
                GTProductModel model =
                    _productModels[i];

                _productModelList.Items.Add(
                    CreateProductModelDisplayName(
                        model));
            }

            if (_productModels.Count == 0)
            {
                _currentProductModel = null;

                _productParts =
                    new List<GTProductPart>();

                _currentProductPart = null;

                _rules =
                    new List<CodeRule>();

                _currentRule = null;

                _partList.Items.Clear();
                _ruleList.Items.Clear();

                ClearRuleDetail();

                return;
            }

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            if (oldIndex >= _productModels.Count)
            {
                oldIndex =
                    _productModels.Count - 1;
            }

            _productModelList.SelectedIndex =
                oldIndex;
        }

        /// <summary>
        /// 创建型号列表显示文本。
        /// </summary>
        private string CreateProductModelDisplayName(
            GTProductModel model)
        {
            if (model == null)
            {
                return "未命名型号";
            }

            if (string.IsNullOrWhiteSpace(
                model.ProductModel))
            {
                return "未命名型号";
            }

            if (string.IsNullOrWhiteSpace(
                model.ProductName))
            {
                return model.ProductModel;
            }

            return model.ProductModel
                + " - "
                + model.ProductName;
        }

        /// <summary>
        /// 产品型号选择变化。
        /// </summary>
        private void ProductModelList_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int index =
                _productModelList.SelectedIndex;

            if (index < 0 ||
                index >= _productModels.Count)
            {
                return;
            }

            _currentProductModel =
                _productModels[index];

            LoadParts(
                _currentProductModel.ProductModelId);
        }

        #endregion

        #region 零部件

        /// <summary>
        /// 加载当前型号下的零部件。
        /// </summary>
        private void LoadParts(
            int productModelId)
        {
            _currentProductPart = null;
            _currentRule = null;

            _partList.Items.Clear();
            _ruleList.Items.Clear();

            ClearRuleDetail();

            try
            {
                _productParts =
                    _productPartService
                        .GetByProductModelId(
                            productModelId);

                if (_productParts == null)
                {
                    _productParts =
                        new List<GTProductPart>();
                }

                for (int i = 0;
                    i < _productParts.Count;
                    i++)
                {
                    GTProductPart part =
                        _productParts[i];

                    _partList.Items.Add(
                        CreatePartDisplayName(
                            part));
                }

                if (_productParts.Count > 0)
                {
                    _partList.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowLoadError(
                    "加载产品零部件失败。",
                    ex);

                _productParts =
                    new List<GTProductPart>();
            }
        }

        /// <summary>
        /// 创建零部件显示名称。
        /// </summary>
        private string CreatePartDisplayName(
            GTProductPart part)
        {
            if (part == null)
            {
                return "未命名部件";
            }

            if (string.IsNullOrWhiteSpace(
                part.PartCode))
            {
                return string.IsNullOrWhiteSpace(
                    part.PartName)
                    ? "未命名部件"
                    : part.PartName;
            }

            if (string.IsNullOrWhiteSpace(
                part.PartName))
            {
                return part.PartCode;
            }

            return part.PartCode
                + " - "
                + part.PartName;
        }

        /// <summary>
        /// 零部件选择变化。
        /// </summary>
        private void PartList_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            int index =
                _partList.SelectedIndex;

            if (index < 0 ||
                index >= _productParts.Count)
            {
                return;
            }

            _currentProductPart =
                _productParts[index];

            LoadRules(
                _currentProductPart.PartId);
        }

        #endregion

        #region 产品码规则

        /// <summary>
        /// 加载当前零部件下的规则。
        /// </summary>
        private void LoadRules(
            int partId)
        {
            _currentRule = null;

            _ruleList.Items.Clear();

            ClearRuleDetail();

            try
            {
                _rules =
                    _ruleService
                        .LoadRulesByPartId(
                            partId);

                if (_rules == null)
                {
                    _rules =
                        new List<CodeRule>();
                }

                for (int i = 0;
                    i < _rules.Count;
                    i++)
                {
                    CodeRule rule =
                        _rules[i];

                    _ruleList.Items.Add(
                        CreateRuleDisplayName(
                            rule));
                }

                if (_rules.Count > 0)
                {
                    _ruleList.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowLoadError(
                    "加载产品码规则失败。",
                    ex);

                _rules =
                    new List<CodeRule>();
            }
        }

        /// <summary>
        /// 创建规则显示名称。
        /// </summary>
        private string CreateRuleDisplayName(
            CodeRule rule)
        {
            if (rule == null)
            {
                return "未命名规则";
            }

            string name =
                string.IsNullOrWhiteSpace(
                    rule.RuleName)
                    ? "未命名规则"
                    : rule.RuleName;

            string status =
                rule.IsActive
                    ? "启用"
                    : "禁用";

            return name
                + "  ["
                + status
                + "]";
        }

        /// <summary>
        /// 规则选择变化。
        /// </summary>
        private void RuleList_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            ShowSelectedRule();
        }

        /// <summary>
        /// 显示当前选中的规则。
        /// </summary>
        private void ShowSelectedRule()
        {
            int index =
                _ruleList.SelectedIndex;

            if (index < 0 ||
                index >= _rules.Count)
            {
                _currentRule = null;

                ClearRuleDetail();

                return;
            }

            _currentRule =
                _rules[index];

            ShowRuleDetail(
                _currentRule);
        }

        /// <summary>
        /// 显示规则详情。
        /// </summary>
        private void ShowRuleDetail(
            CodeRule rule)
        {
            if (rule == null)
            {
                ClearRuleDetail();
                return;
            }

            _productModelValue.Text =
                _currentProductModel == null
                    ? ""
                    : _currentProductModel.ProductModel;

            _partValue.Text =
                _currentProductPart == null
                    ? ""
                    : CreatePartDisplayName(
                        _currentProductPart);

            _ruleNameValue.Text =
                string.IsNullOrWhiteSpace(
                    rule.RuleName)
                    ? "未命名规则"
                    : rule.RuleName;

            _descriptionValue.Text =
                string.IsNullOrWhiteSpace(
                    rule.Description)
                    ? "无"
                    : rule.Description;

            _statusValue.Text =
                rule.IsActive
                    ? "启用"
                    : "禁用";

            int count =
                rule.Details == null
                    ? 0
                    : rule.Details.Count;

            _segmentCountValue.Text =
                count.ToString();

            RefreshSegmentSummary(
                rule);

            RefreshPreview(
                rule);
        }

        /// <summary>
        /// 清空规则详情。
        /// </summary>
        private void ClearRuleDetail()
        {
            _productModelValue.Text =
                _currentProductModel == null
                    ? ""
                    : _currentProductModel.ProductModel;

            _partValue.Text =
                _currentProductPart == null
                    ? ""
                    : CreatePartDisplayName(
                        _currentProductPart);

            _ruleNameValue.Text =
                "";

            _descriptionValue.Text =
                "";

            _statusValue.Text =
                "";

            _segmentCountValue.Text =
                "0";

            _segmentSummary.Items.Clear();

            _previewText.Text =
                "";
        }

        #endregion

        #region Segment摘要

        /// <summary>
        /// 刷新规则的 Segment 摘要。
        /// </summary>
        private void RefreshSegmentSummary(
            CodeRule rule)
        {
            _segmentSummary.Items.Clear();

            if (rule == null ||
                rule.Details == null)
            {
                return;
            }

            List<CodeRuleDetail> details =
                new List<CodeRuleDetail>(
                    rule.Details);

            details.Sort(
                delegate (
                    CodeRuleDetail x,
                    CodeRuleDetail y)
                {
                    return x.SegmentOrder.CompareTo(
                        y.SegmentOrder);
                });

            for (int i = 0;
                i < details.Count;
                i++)
            {
                CodeRuleDetail detail =
                    details[i];

                if (detail == null)
                {
                    continue;
                }

                string enabled =
                    detail.Enabled
                        ? ""
                        : " [禁用]";

                string typeName =
                    GetSegmentTypeName(
                        detail.SegmentType);

                _segmentSummary.Items.Add(
                    detail.SegmentOrder
                    + "  "
                    + typeName
                    + enabled);
            }
        }

        /// <summary>
        /// 获取 Segment 类型中文名称。
        /// </summary>
        private string GetSegmentTypeName(
            CodeSegmentType type)
        {
            switch (type)
            {
                case CodeSegmentType.FixedText:
                    return "固定文本";

                case CodeSegmentType.Date:
                    return "日期";

                case CodeSegmentType.Shift:
                    return "班次";


                case CodeSegmentType.SerialNumber:
                    return "流水号";


                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 刷新规则预览。
        ///
        /// 管理页面只需要展示一个示例，
        /// 不参与产品码生成逻辑。
        /// </summary>
        private void RefreshPreview(
            CodeRule rule)
        {
            if (rule == null ||
                rule.Details == null)
            {
                _previewText.Text = "";
                return;
            }

            /*
             * 管理页面不直接复制 CodePreviewGenerator 的生成逻辑。
             *
             * 这里优先显示规则结构，
             * 避免管理 Form 和真正产品码生成器出现两套算法。
             *
             * 如果当前项目已经有统一的 PreviewGenerator，
             * 可以在这里直接替换为调用生成器。
             */
            CodePreviewGenerator generator = new CodePreviewGenerator();

            _previewText.Text = CreatePreviewDescription(rule) + "----" + generator.GeneratePreview(rule);

            //_previewText.Text =
            //    CreatePreviewDescription(
            //        rule);
        }

        /// <summary>
        /// 创建简单的规则结构预览。
        ///
        /// 该方法只用于管理页面展示，
        /// 不作为实际产品码生成结果。
        /// </summary>
        private string CreatePreviewDescription(
            CodeRule rule)
        {
            if (rule == null ||
                rule.Details == null ||
                rule.Details.Count == 0)
            {
                return "";
            }

            List<CodeRuleDetail> details =
                new List<CodeRuleDetail>(
                    rule.Details);

            details.Sort(
                delegate (
                    CodeRuleDetail x,
                    CodeRuleDetail y)
                {
                    return x.SegmentOrder.CompareTo(
                        y.SegmentOrder);
                });

            string result =
                "";

            for (int i = 0;
                i < details.Count;
                i++)
            {
                CodeRuleDetail detail =
                    details[i];

                if (detail == null ||
                    !detail.Enabled)
                {
                    continue;
                }

                switch (detail.SegmentType)
                {
                    case CodeSegmentType.FixedText:
                        result += "[固定]";
                        break;

                    case CodeSegmentType.Date:
                        result += "[日期]";
                        break;

                    case CodeSegmentType.Shift:
                        result += "[班次]";
                        break;


                    case CodeSegmentType.SerialNumber:
                        result += "[流水号]";
                        break;


                    default:
                        result += "[?]";
                        break;
                }
            }

            return result;
        }

        #endregion

        #region 新增规则

        /// <summary>
        /// 点击新增规则。
        ///
        /// 新增必须基于当前选中的：
        ///
        /// 产品型号
        /// +
        /// 零部件
        ///
        /// 因此没有选择零部件时，
        /// 不允许新增规则。
        /// </summary>
        private void NewButton_Click(
            object sender,
            EventArgs e)
        {
            if (_currentProductModel == null)
            {
                MessageBox.Show(
                    "请先选择产品型号。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (_currentProductPart == null)
            {
                MessageBox.Show(
                    "请先选择产品零部件。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            ProductCodeRuleForm form =
                new ProductCodeRuleForm(
                    _currentProductModel,
                    _currentProductPart);

            DialogResult result =
                form.ShowDialog(this);

            if (result != DialogResult.OK)
            {
                return;
            }

            if (form.EditedRule == null)
            {
                return;
            }

            try
            {
                /*
                 * Form 内部已经设置 PartId。
                 *
                 * 这里再次强制设置一次，
                 * 防止以后编辑 Form 时误把归属关系破坏。
                 */
                form.EditedRule.PartId =
                    _currentProductPart.PartId;

                _ruleService.AddRule(
                    form.EditedRule);

                ReloadCurrentPart(
                    form.EditedRule.RuleId);
            }
            catch (Exception ex)
            {
                ShowOperationError(
                    "新增产品码规则失败。",
                    ex);
            }
        }

        #endregion

        #region 编辑规则

        /// <summary>
        /// 双击规则进入编辑。
        /// </summary>
        private void RuleList_DoubleClick(
            object sender,
            EventArgs e)
        {
            EditSelectedRule();
        }

        /// <summary>
        /// 点击编辑按钮。
        /// </summary>
        private void EditButton_Click(
            object sender,
            EventArgs e)
        {
            EditSelectedRule();
        }

        /// <summary>
        /// 编辑当前规则。
        /// </summary>
        private void EditSelectedRule()
        {
            if (_currentRule == null)
            {
                MessageBox.Show(
                    "请先选择一条产品码规则。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            ProductCodeRuleForm form =
                new ProductCodeRuleForm(
                    _currentRule);

            DialogResult result =
                form.ShowDialog(this);

            if (result != DialogResult.OK)
            {
                return;
            }

            if (form.EditedRule == null)
            {
                return;
            }

            try
            {
                /*
                 * 编辑普通规则时，
                 * 规则归属不能被编辑窗口改变。
                 */
                form.EditedRule.PartId =
                    _currentProductPart.PartId;

                _ruleService.UpdateRule(
                    form.EditedRule);

                ReloadCurrentPart(
                    form.EditedRule.RuleId);
            }
            catch (Exception ex)
            {
                ShowOperationError(
                    "修改产品码规则失败。",
                    ex);
            }
        }

        #endregion

        #region 删除规则

        /// <summary>
        /// 删除当前规则。
        /// </summary>
        private void DeleteButton_Click(
            object sender,
            EventArgs e)
        {
            if (_currentRule == null)
            {
                MessageBox.Show(
                    "请先选择一条产品码规则。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string ruleName =
                string.IsNullOrWhiteSpace(
                    _currentRule.RuleName)
                    ? "未命名规则"
                    : _currentRule.RuleName;

            DialogResult result =
                MessageBox.Show(
                    "确定删除产品码规则："
                    + ruleName
                    + "？\r\n\r\n"
                    + "删除后无法恢复。",
                    "删除规则",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            int deletedRuleId =
                _currentRule.RuleId;

            try
            {
                _ruleService.DeleteRule(
                    _currentRule);

                ReloadCurrentPart(
                    0);
            }
            catch (Exception ex)
            {
                ShowOperationError(
                    "删除产品码规则失败。",
                    ex);
            }
        }

        #endregion

        #region 刷新

        /// <summary>
        /// 刷新整个页面。
        ///
        /// 尽量恢复原来的：
        ///
        /// 型号
        /// 部件
        /// 规则
        ///
        /// 选择。
        /// </summary>
        private void RefreshButton_Click(
            object sender,
            EventArgs e)
        {
            int modelId =
                _currentProductModel == null
                    ? 0
                    : _currentProductModel.ProductModelId;

            int partId =
                _currentProductPart == null
                    ? 0
                    : _currentProductPart.PartId;

            int ruleId =
                _currentRule == null
                    ? 0
                    : _currentRule.RuleId;

            LoadProductModels();

            RestoreModelSelection(
                modelId);

            if (_currentProductModel != null)
            {
                LoadParts(
                    _currentProductModel.ProductModelId);

                RestorePartSelection(
                    partId);
            }

            if (_currentProductPart != null)
            {
                LoadRules(
                    _currentProductPart.PartId);

                RestoreRuleSelection(
                    ruleId);
            }
        }

        /// <summary>
        /// 重新加载当前零部件规则。
        /// </summary>
        private void ReloadCurrentPart(
            int ruleId)
        {
            if (_currentProductPart == null)
            {
                return;
            }

            LoadRules(
                _currentProductPart.PartId);

            RestoreRuleSelection(
                ruleId);
        }

        /// <summary>
        /// 恢复型号选择。
        /// </summary>
        private void RestoreModelSelection(
            int productModelId)
        {
            if (productModelId <= 0)
            {
                return;
            }

            for (int i = 0;
                i < _productModels.Count;
                i++)
            {
                if (_productModels[i].ProductModelId
                    == productModelId)
                {
                    _productModelList.SelectedIndex =
                        i;

                    return;
                }
            }
        }

        /// <summary>
        /// 恢复零部件选择。
        /// </summary>
        private void RestorePartSelection(
            int partId)
        {
            if (partId <= 0)
            {
                return;
            }

            for (int i = 0;
                i < _productParts.Count;
                i++)
            {
                if (_productParts[i].PartId
                    == partId)
                {
                    _partList.SelectedIndex =
                        i;

                    return;
                }
            }
        }

        /// <summary>
        /// 恢复规则选择。
        /// </summary>
        private void RestoreRuleSelection(
            int ruleId)
        {
            if (ruleId <= 0)
            {
                return;
            }

            for (int i = 0;
                i < _rules.Count;
                i++)
            {
                if (_rules[i].RuleId
                    == ruleId)
                {
                    _ruleList.SelectedIndex =
                        i;

                    return;
                }
            }
        }

        #endregion

        #region 错误处理

        /// <summary>
        /// 显示加载错误。
        /// </summary>
        private void ShowLoadError(
            string message,
            Exception ex)
        {
            MessageBox.Show(
                message
                + "\r\n\r\n"
                + ex.Message,
                "错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// 显示操作错误。
        /// </summary>
        private void ShowOperationError(
            string message,
            Exception ex)
        {
            MessageBox.Show(
                message
                + "\r\n\r\n"
                + ex.Message,
                "错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        #endregion
    }

}

