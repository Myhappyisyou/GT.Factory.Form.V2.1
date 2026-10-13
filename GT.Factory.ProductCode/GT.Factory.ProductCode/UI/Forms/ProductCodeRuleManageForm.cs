//using GT.Factory.Common.Enums.ProductCodeEnums;
//using GT.Factory.Common.Helpers;
//using GT.Factory.Common.Models.ProductCode;
//using GT.Factory.Data.Core;
//using GT.Factory.ProductCode.Domain;
//using GT.Factory.ProductCode.Services;
//using GT.Factory.ProductCode.Repository;
//using GT.Factory.ProductCode.Service;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Windows.Forms;

//namespace GT.Factory.ProductCode.UI.Forms
//{
//    /// <summary>
//    /// 产品码规则管理窗口。
//    ///
//    ///
//    /// </summary>
//    public class ProductCodeRuleManageForm : Form
//    {
//        #region 字段

//        private readonly ProductCodeRuleService _service;

//        /// <summary>
//        /// 当前加载到内存中的所有产品码规则。
//        ///
//        /// 当前纯UI阶段使用List保存。
//        /// 后续接入SQL后，这里仍然可以作为当前页面的数据缓存。
//        /// </summary>
//        private List<CodeRule> _rules;

//        /// <summary>
//        /// 左侧产品码规则列表。
//        /// </summary>
//        private ListBox _ruleList;

//        /// <summary>
//        /// 右侧规则名称。
//        /// </summary>
//        private Label _ruleNameLabel;

//        /// <summary>
//        /// 右侧规则描述。
//        /// </summary>
//        private Label _descriptionLabel;

//        /// <summary>
//        /// 右侧规则状态。
//        /// </summary>
//        private Label _statusLabel;

//        /// <summary>
//        /// 右侧Segment数量。
//        /// </summary>
//        private Label _segmentCountLabel;

//        /// <summary>
//        /// Segment结构摘要列表。
//        /// </summary>
//        private ListBox _segmentSummary;

//        /// <summary>
//        /// 产品码示例预览文本框。
//        /// </summary>
//        private TextBox _previewText;

//        /// <summary>
//        /// 新建规则按钮。
//        /// </summary>
//        private Button _newButton;

//        /// <summary>
//        /// 编辑规则按钮。
//        /// </summary>
//        private Button _editButton;

//        /// <summary>
//        /// 删除规则按钮。
//        /// </summary>
//        private Button _deleteButton;

//        /// <summary>
//        /// 刷新规则按钮。
//        /// </summary>
//        private Button _refreshButton;


//        #endregion

//        #region 构造函数


//        /// <summary>
//        /// 创建产品码规则管理窗口。
//        /// </summary>
//        public ProductCodeRuleManageForm(ProductCodeRuleService service)
//        {
//            _service = service;

//            InitializeData();

//            InitializeUI();

//            BindEvents();

//            LoadRules();
//        }

//        #endregion

//        #region 初始化

//        /// <summary>
//        /// 初始化内存数据。
//        /// </summary>
//        private void InitializeData()
//        {
//            _rules =
//                new List<CodeRule>();
//        }

//        /// <summary>
//        /// 初始化整个窗口UI。
//        ///
//        /// 当前页面采用代码创建WinForms控件，
//        /// 不使用Designer。
//        /// </summary>
//        private void InitializeUI()
//        {
//            Text =
//                "产品码规则管理";

//            Width =
//                1100;

//            Height =
//                700;

//            MinimumSize =
//                new Size(900, 600);

//            StartPosition =
//                FormStartPosition.CenterScreen;

//            BackColor =
//                Color.White;

//            //------------------------------------------
//            // 创建主区域
//            //------------------------------------------

//            SplitContainer mainSplit = new SplitContainer();

//            mainSplit.Dock =
//                DockStyle.Fill;

//            mainSplit.Orientation =
//                Orientation.Vertical;

//            /*
//             * 左侧规则列表的最小宽度。
//             */
//            mainSplit.Panel1MinSize =
//                240;

//            /*
//             * 右侧规则详情的最小宽度。
//             */


//            /*
//             * SplitContainer在窗口创建阶段可能还没有最终Width。
//             *
//             * 因此不能在这里直接设置SplitterDistance。
//             *
//             * 等窗口显示后，根据实际Width计算。
//             */
//            this.Shown += delegate
//            {
//                int availableWidth =
//                    mainSplit.ClientSize.Width;

//                int minDistance =
//                    mainSplit.Panel1MinSize;

//                int maxDistance =
//                    availableWidth
//                    - mainSplit.Panel2MinSize;

//                /*
//                 * 如果当前窗口宽度不足以同时满足两个最小宽度，
//                 * 不强行设置SplitterDistance。
//                 */
//                if (maxDistance < minDistance)
//                {
//                    return;
//                }

//                /*
//                 * 默认左侧占约30%。
//                 *
//                 * 例如窗口宽度1000：
//                 *
//                 * 1000 * 30% = 300
//                 */
//                int distance =
//                    (int)(availableWidth * 0.30);

//                /*
//                 * 强制限制在合法范围内。
//                 */
//                if (distance < minDistance)
//                {
//                    distance =
//                        minDistance;
//                }

//                if (distance > maxDistance)
//                {
//                    distance =
//                        maxDistance;
//                }

//                mainSplit.SplitterDistance =
//                    distance;
//            };

//            //------------------------------------------
//            // 左侧规则列表
//            //------------------------------------------

//            Panel leftPanel =
//                CreateRuleListPanel();

//            mainSplit.Panel1.Controls.Add(
//                leftPanel);

//            //------------------------------------------
//            // 右侧规则详情
//            //------------------------------------------

//            Panel rightPanel =
//                CreateDetailPanel();

//            mainSplit.Panel2.Controls.Add(
//                rightPanel);

//            //------------------------------------------
//            // 底部操作按钮
//            //------------------------------------------

//            Panel buttonPanel =
//                CreateButtonPanel();

//            Controls.Add(
//                mainSplit);

//            Controls.Add(
//                buttonPanel);
//        }

//        /// <summary>
//        /// 创建左侧规则列表区域。
//        /// </summary>
//        private Panel CreateRuleListPanel()
//        {
//            Panel panel =
//                new Panel();

//            panel.Dock =
//                DockStyle.Fill;

//            panel.Padding =
//                new Padding(10);

//            //------------------------------------------
//            // 标题
//            //------------------------------------------

//            Label title =
//                new Label();

//            title.Text =
//                "产品码规则";

//            title.Dock =
//                DockStyle.Top;

//            title.Height =
//                35;

//            title.Font =
//                new Font(
//                    "Microsoft YaHei",
//                    11,
//                    FontStyle.Bold);

//            title.TextAlign =
//                ContentAlignment.MiddleLeft;

//            //------------------------------------------
//            // 规则列表
//            //------------------------------------------

//            _ruleList =
//                new ListBox();

//            _ruleList.Dock =
//                DockStyle.Fill;

//            _ruleList.Font =
//                new Font(
//                    "Microsoft YaHei",
//                    10);

//            _ruleList.IntegralHeight =
//                false;

//            panel.Controls.Add(
//                _ruleList);

//            panel.Controls.Add(
//                title);

//            return panel;
//        }

//        /// <summary>
//        /// 创建右侧规则详情区域。
//        /// </summary>
//        private Panel CreateDetailPanel()
//        {
//            Panel panel =
//                new Panel();

//            panel.Dock =
//                DockStyle.Fill;

//            panel.Padding =
//                new Padding(15);

//            //------------------------------------------
//            // 使用TableLayoutPanel组织详情区域
//            //------------------------------------------

//            TableLayoutPanel layout =
//                new TableLayoutPanel();

//            layout.Dock =
//                DockStyle.Fill;

//            layout.ColumnCount =
//                2;

//            layout.RowCount =
//                7;

//            layout.ColumnStyles.Add(
//                new ColumnStyle(
//                    SizeType.Absolute,
//                    100));

//            layout.ColumnStyles.Add(
//                new ColumnStyle(
//                    SizeType.Percent,
//                    100));

//            //------------------------------------------
//            // 规则名称
//            //------------------------------------------

//            layout.Controls.Add(
//                CreateTitleLabel("规则名称"),
//                0,
//                0);

//            _ruleNameLabel =
//                CreateValueLabel();

//            layout.Controls.Add(
//                _ruleNameLabel,
//                1,
//                0);

//            //------------------------------------------
//            // 规则描述
//            //------------------------------------------

//            layout.Controls.Add(
//                CreateTitleLabel("规则描述"),
//                0,
//                1);

//            _descriptionLabel =
//                CreateValueLabel();

//            layout.Controls.Add(
//                _descriptionLabel,
//                1,
//                1);

//            //------------------------------------------
//            // 规则状态
//            //------------------------------------------

//            layout.Controls.Add(
//                CreateTitleLabel("状态"),
//                0,
//                2);

//            _statusLabel =
//                CreateValueLabel();

//            layout.Controls.Add(
//                _statusLabel,
//                1,
//                2);

//            //------------------------------------------
//            // Segment数量
//            //------------------------------------------

//            layout.Controls.Add(
//                CreateTitleLabel("Segment数量"),
//                0,
//                3);

//            _segmentCountLabel =
//                CreateValueLabel();

//            layout.Controls.Add(
//                _segmentCountLabel,
//                1,
//                3);

//            //------------------------------------------
//            // Segment摘要标题
//            //------------------------------------------

//            layout.Controls.Add(
//                CreateTitleLabel("Segment结构"),
//                0,
//                4);

//            _segmentSummary =
//                new ListBox();

//            _segmentSummary.Dock =
//                DockStyle.Fill;

//            _segmentSummary.Font =
//                new Font(
//                    "Microsoft YaHei",
//                    10);

//            _segmentSummary.IntegralHeight =
//                false;

//            layout.Controls.Add(
//                _segmentSummary,
//                1,
//                4);

//            //------------------------------------------
//            // 示例标题
//            //------------------------------------------

//            layout.Controls.Add(
//                CreateTitleLabel("示例预览"),
//                0,
//                5);

//            _previewText =
//                new TextBox();

//            _previewText.Dock =
//                DockStyle.Fill;

//            _previewText.ReadOnly =
//                true;

//            _previewText.Font =
//                new Font(
//                    "Consolas",
//                    14,
//                    FontStyle.Bold);

//            _previewText.TextAlign =
//                HorizontalAlignment.Center;

//            _previewText.Multiline =
//                false;

//            layout.Controls.Add(
//                _previewText,
//                1,
//                5);

//            //------------------------------------------
//            // 空白行
//            //------------------------------------------

//            Label empty =
//                new Label();

//            layout.Controls.Add(
//                empty,
//                0,
//                6);

//            layout.Controls.Add(
//                new Panel(),
//                1,
//                6);

//            //------------------------------------------
//            // 设置行高
//            //------------------------------------------

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Absolute,
//                    40));

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Absolute,
//                    45));

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Absolute,
//                    40));

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Absolute,
//                    40));

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Percent,
//                    100));

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Absolute,
//                    55));

//            layout.RowStyles.Add(
//                new RowStyle(
//                    SizeType.Absolute,
//                    10));

//            panel.Controls.Add(
//                layout);

//            return panel;
//        }

//        /// <summary>
//        /// 创建详情区域左侧标题Label。
//        /// </summary>
//        private Label CreateTitleLabel(
//            string text)
//        {
//            Label label =
//                new Label();

//            label.Text =
//                text;

//            label.Dock =
//                DockStyle.Fill;

//            label.Font =
//                new Font(
//                    "Microsoft YaHei",
//                    10,
//                    FontStyle.Bold);

//            label.TextAlign =
//                ContentAlignment.MiddleLeft;

//            return label;
//        }

//        /// <summary>
//        /// 创建详情区域右侧值Label。
//        /// </summary>
//        private Label CreateValueLabel()
//        {
//            Label label =
//                new Label();

//            label.Dock =
//                DockStyle.Fill;

//            label.Font =
//                new Font(
//                    "Microsoft YaHei",
//                    10);

//            label.TextAlign =
//                ContentAlignment.MiddleLeft;

//            label.AutoEllipsis =
//                true;

//            return label;
//        }

//        /// <summary>
//        /// 创建底部操作按钮区域。
//        /// </summary>
//        private Panel CreateButtonPanel()
//        {
//            FlowLayoutPanel panel =
//                new FlowLayoutPanel();

//            panel.Dock =
//                DockStyle.Bottom;

//            panel.Height =
//                55;

//            panel.Padding =
//                new Padding(
//                    10,
//                    8,
//                    10,
//                    8);

//            panel.FlowDirection =
//                FlowDirection.LeftToRight;

//            panel.WrapContents =
//                false;

//            //------------------------------------------
//            // 创建按钮
//            //------------------------------------------

//            _newButton =
//                CreateButton("新增规则");

//            _editButton =
//                CreateButton("编辑");

//            _deleteButton =
//                CreateButton("删除");

//            _refreshButton =
//                CreateButton("刷新");

//            //------------------------------------------
//            // 添加按钮
//            //------------------------------------------

//            panel.Controls.Add(
//                _newButton);

//            panel.Controls.Add(
//                _editButton);

//            panel.Controls.Add(
//                _deleteButton);

//            panel.Controls.Add(
//                _refreshButton);

//            return panel;
//        }

//        /// <summary>
//        /// 创建标准操作按钮。
//        /// </summary>
//        private Button CreateButton(
//            string text)
//        {
//            Button button =
//                new Button();

//            button.Text =
//                text;

//            button.Width =
//                100;

//            button.Height =
//                32;

//            button.Margin =
//                new Padding(
//                    5,
//                    0,
//                    5,
//                    0);

//            button.Font =
//                new Font(
//                    "Microsoft YaHei",
//                    9);

//            return button;
//        }

//        #endregion

//        #region 事件绑定

//        /// <summary>
//        /// 绑定页面事件。
//        /// </summary>
//        private void BindEvents()
//        {
//            //------------------------------------------
//            // 规则列表选择事件
//            //------------------------------------------

//            _ruleList.SelectedIndexChanged +=
//                RuleList_SelectedIndexChanged;

//            //------------------------------------------
//            // 双击规则进入编辑
//            //------------------------------------------

//            _ruleList.DoubleClick +=
//                RuleList_DoubleClick;

//            //------------------------------------------
//            // 新建
//            //------------------------------------------

//            _newButton.Click +=
//                NewButton_Click;

//            //------------------------------------------
//            // 编辑
//            //------------------------------------------

//            _editButton.Click +=
//                EditButton_Click;

//            //------------------------------------------
//            // 删除
//            //------------------------------------------

//            _deleteButton.Click +=
//                DeleteButton_Click;

//            //------------------------------------------
//            // 刷新
//            //------------------------------------------

//            _refreshButton.Click +=
//                RefreshButton_Click;
//        }

//        #endregion

//        #region 规则加载

//        /// <summary>
//        /// 加载所有规则。
//        ///
//        /// 当前阶段：
//        ///
//        /// 从ProductCodeRules.json读取。
//        ///
//        /// 如果文件不存在，
//        /// 则创建一个测试规则用于验证UI。
//        ///
//        /// 后续接入SQL时，
//        /// 这里替换为数据库查询即可。
//        /// </summary>
//        private void LoadRules()
//        {
//            try
//            {
//                _rules = _service.LoadRules();

//                RefreshRuleList();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(
//                    "加载产品码规则失败。\r\n\r\n"
//                    + ex.Message,
//                    "错误",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Error);

//                _rules =
//                    new List<CodeRule>();

//                RefreshRuleList();
//            }
//        }


//        /// <summary>
//        /// 将当前规则集合保存到JSON。
//        ///
//        /// 当前只作为UI阶段的数据持久化。
//        ///
//        /// 后续接入SQL后可以删除。
//        /// </summary>
//        private void CreateRule(CodeRule codeRule)
//        {
//            _service.AddRule(codeRule);

//        }

//        /// <summary>
//        /// 更新。
//        /// </summary>
//        private void UpdateRule(CodeRule codeRule)
//        {
//            _service.UpdateRule(codeRule);
//        }

//        /// <summary>
//        /// 删除。
//        /// </summary>
//        private void DeleteRule(CodeRule codeRule)
//        {
//            _service.DeleteRule(codeRule);

//        }

//        #endregion

//        #region 列表刷新

//        /// <summary>
//        /// 刷新左侧规则列表。
//        ///
//        /// 刷新后尽量保持原来的选择项。
//        /// </summary>
//        private void RefreshRuleList()
//        {
//            int oldIndex =
//                _ruleList.SelectedIndex;

//            _ruleList.Items.Clear();

//            //------------------------------------------
//            // 重新加载规则名称
//            //------------------------------------------

//            for (int i = 0;
//                i < _rules.Count;
//                i++)
//            {
//                CodeRule rule =
//                    _rules[i];

//                string displayName =
//                    CreateRuleDisplayName(
//                        rule);

//                _ruleList.Items.Add(
//                    displayName);
//            }

//            //------------------------------------------
//            // 恢复选择
//            //------------------------------------------

//            if (_rules.Count == 0)
//            {
//                ClearRuleDetail();

//                return;
//            }

//            if (oldIndex < 0)
//            {
//                oldIndex = 0;
//            }

//            if (oldIndex >= _rules.Count)
//            {
//                oldIndex =
//                    _rules.Count - 1;
//            }

//            _ruleList.SelectedIndex =
//                oldIndex;
//        }

//        /// <summary>
//        /// 创建规则列表显示文本。
//        ///
//        /// 当前显示：
//        ///
//        /// 规则名称
//        ///
//        /// 后续如果需要，可以加入状态或Segment数量。
//        /// </summary>
//        private string CreateRuleDisplayName(
//            CodeRule rule)
//        {
//            if (rule == null)
//            {
//                return "未命名规则";
//            }

//            if (string.IsNullOrWhiteSpace(
//                rule.RuleName))
//            {
//                return "未命名规则";
//            }

//            return rule.RuleName;
//        }

//        #endregion

//        #region 规则详情

//        /// <summary>
//        /// 规则列表选择改变时，
//        /// 更新右侧规则详情。
//        /// </summary>
//        private void RuleList_SelectedIndexChanged(
//            object sender,
//            EventArgs e)
//        {
//            ShowSelectedRule();
//        }

//        /// <summary>
//        /// 显示当前选中的规则。
//        /// </summary>
//        private void ShowSelectedRule()
//        {
//            int index = _ruleList.SelectedIndex;

//            if (index < 0)
//            {
//                ClearRuleDetail();

//                return;
//            }

//            if (index >= _rules.Count)
//            {
//                ClearRuleDetail();

//                return;
//            }

//            CodeRule rule = _rules[index];

//            ShowRuleDetail(rule);
//        }

//        /// <summary>
//        /// 显示指定规则的详细信息。
//        /// </summary>
//        private void ShowRuleDetail(
//            CodeRule rule)
//        {
//            if (rule == null)
//            {
//                ClearRuleDetail();

//                return;
//            }

//            //------------------------------------------
//            // 基本信息
//            //------------------------------------------

//            _ruleNameLabel.Text =
//                string.IsNullOrWhiteSpace(
//                    rule.RuleName)
//                ?
//                "未命名规则"
//                :
//                rule.RuleName;

//            _descriptionLabel.Text =
//                string.IsNullOrWhiteSpace(
//                    rule.Description)
//                ?
//                "无"
//                :
//                rule.Description;

//            _statusLabel.Text =
//                rule.IsActive
//                ?
//                "启用"
//                :
//                "禁用";

//            // Segment数量

//            int count =
//                rule.Details == null
//                ?
//                0
//                :
//                rule.Details.Count;

//            _segmentCountLabel.Text = count.ToString();

//            // Segment摘要

//            RefreshSegmentSummary(rule);

//            // 示例预览

//            RefreshPreview(rule);
//        }

//        /// <summary>
//        /// 清空右侧规则详情。
//        /// </summary>
//        private void ClearRuleDetail()
//        {
//            _ruleNameLabel.Text = "";

//            _descriptionLabel.Text = "";

//            _statusLabel.Text = "";

//            _segmentCountLabel.Text = "0";

//            _segmentSummary.Items.Clear();

//            _previewText.Text = "";
//        }

//        #endregion

//        #region Segment摘要

//        /// <summary>
//        /// 刷新Segment结构摘要。
//        ///
//        /// 当前只显示Segment类型以及部分主要配置。
//        ///
//        /// Segment详细编辑仍然由
//        /// CodeSegmentEditorControl负责。
//        /// </summary>
//        private void RefreshSegmentSummary(
//            CodeRule rule)
//        {
//            _segmentSummary.Items.Clear();

//            if (rule == null)
//            {
//                return;
//            }

//            if (rule.Details == null)
//            {
//                return;
//            }

//            // 按Sort排序显示

//            List<CodeRuleDetail> details =
//                new List<CodeRuleDetail>(
//                    rule.Details);

//            details.Sort(
//                delegate (
//                    CodeRuleDetail x,
//                    CodeRuleDetail y)
//                {
//                    return x.SegmentOrder.CompareTo(y.SegmentOrder);
//                });

//            // 创建摘要

//            for (int i = 0; i < details.Count; i++)
//            {
//                CodeRuleDetail detail = details[i];

//                if (detail == null)
//                {
//                    continue;
//                }

//                string summary =
//                    CreateSegmentSummary(
//                        detail);

//                _segmentSummary.Items.Add(
//                    summary);
//            }
//        }

//        /// <summary>
//        /// 创建单个Segment的摘要文本。
//        /// </summary>
//        private string CreateSegmentSummary(
//            CodeRuleDetail detail)
//        {
//            string typeName =
//                GetSegmentTypeName(
//                    detail.SegmentType);

//            string configSummary =
//                GetSegmentConfigSummary(
//                    detail);

//            string enabledText =
//                detail.Enabled
//                ?
//                ""
//                :
//                " [禁用]";

//            if (string.IsNullOrWhiteSpace(
//                configSummary))
//            {
//                return detail.SegmentOrder
//                    + "  "
//                    + typeName
//                    + enabledText;
//            }

//            return detail.SegmentOrder
//                + "  "
//                + typeName
//                + "  "
//                + configSummary
//                + enabledText;
//        }

//        /// <summary>
//        /// 将Segment枚举转换成用户可以理解的中文名称。
//        /// </summary>
//        private string GetSegmentTypeName(
//            CodeSegmentType type)
//        {
//            switch (type)
//            {
//                case CodeSegmentType.FixedText:

//                    return "固定文本";

//                case CodeSegmentType.Date:

//                    return "日期";

//                case CodeSegmentType.Shift:

//                    return "班次";


//                case CodeSegmentType.SerialNumber:

//                    return "流水号";


//                default:

//                    return "未知";
//            }
//        }

//        /// <summary>
//        /// 根据Segment类型提取简单配置摘要。
//        ///
//        /// 当前只用于管理页面显示。
//        ///
//        /// 不负责完整ConfigJson解析。
//        /// </summary>
//        private string GetSegmentConfigSummary(
//            CodeRuleDetail detail)
//        {
//            if (detail == null)
//            {
//                return "";
//            }

//            if (string.IsNullOrWhiteSpace(
//                detail.ConfigJson))
//            {
//                return "";
//            }

//            try
//            {
//                Newtonsoft.Json.Linq.JObject config =
//                    JsonConvert.DeserializeObject<
//                        Newtonsoft.Json.Linq.JObject>(
//                            detail.ConfigJson);

//                if (config == null)
//                {
//                    return "";
//                }

//                switch (detail.SegmentType)
//                {
//                    case CodeSegmentType.FixedText:

//                        return GetJsonStringValue(
//                            config,
//                            "Text");

//                    case CodeSegmentType.Date:

//                        return GetDateFormat(config);
//                    case CodeSegmentType.SerialNumber:

//                        string length =
//                            GetJsonStringValue(
//                                config,
//                                "Length");

//                        if (!string.IsNullOrWhiteSpace(
//                            length))
//                        {
//                            return "长度 "
//                                + length;
//                        }

//                        return "";


//                    case CodeSegmentType.Shift:

//                        return "自动班次";

//                    default:

//                        return "";
//                }
//            }
//            catch
//            {
//                /*
//                 * 管理页面只是摘要显示。
//                 *
//                 * ConfigJson异常时不阻断整个规则管理页面。
//                 */
//                return "";
//            }
//        }

//        /// <summary>
//        /// 从JSON对象读取字符串形式的属性值。
//        /// </summary>
//        private string GetJsonStringValue(
//            Newtonsoft.Json.Linq.JObject config,
//            string propertyName)
//        {
//            if (config == null)
//            {
//                return "";
//            }

//            Newtonsoft.Json.Linq.JToken token = config[propertyName];

//            if (token == null)
//            {
//                return "";
//            }

//            return token.ToString();
//        }

//        /// <summary>
//        /// 获取日期配置摘要。
//        ///
//        /// 新配置使用 Parts，例如：
//        /// Year:Full + Month:TwoDigit + Day:TwoDigit。
//        /// </summary>
//        private string GetDateFormat(JObject config)
//        {
//            if (config == null || config["Parts"] == null)
//            {
//                return "";
//            }

//            try
//            {
//                GT.Factory.ProductCode.Domain.Config.DateConfig date =
//                    JsonConvert.DeserializeObject<GT.Factory.ProductCode.Domain.Config.DateConfig>(config.ToString());

//                if (date == null || date.Parts == null)
//                {
//                    return "";
//                }

//                List<string> parts = new List<string>();
//                foreach (GT.Factory.ProductCode.Domain.Config.DatePartConfig part in date.Parts)
//                {
//                    if (part == null)
//                    {
//                        continue;
//                    }
//                    parts.Add(part.Part + ":" + part.Mode);
//                }

//                return string.Join(" + ", parts.ToArray());
//            }
//            catch
//            {
//                return "";
//            }
//        }

//        #endregion

//        #region 示例预览

//        /// <summary>
//        /// 刷新规则示例预览。
//        ///
//        /// UI预览和正式生成共用 Processor，避免两套规则。
//        /// </summary>
//        private void RefreshPreview(CodeRule rule)
//        {
//            if (rule == null)
//            {
//                _previewText.Text = "";
//                return;
//            }

//            try
//            {
//                CodePreviewGenerator generator = new CodePreviewGenerator();

//                _previewText.Text = generator.GeneratePreview(rule);
//            }
//            catch (Exception ex)
//            {
//                _previewText.Text = "配置错误：" + ex.Message;
//            }
//        }

//        #endregion


//        /// <summary>
//        /// 刷新规则示例预览。
//        ///
//        /// 当前阶段：
//        ///
//        /// 根据Segment配置生成一个UI示例。
//        ///
//        /// 这里只是管理页面预览，
//        /// 不代表正式产品码生成逻辑。
//        ///
//        /// 正式生成逻辑后续由独立Generator负责。
//        /// </summary>
//        #region 新建规则

//        /// <summary>
//        /// 点击新增规则。
//        /// </summary>
//        private void NewButton_Click(object sender, EventArgs e)
//        {
//            ProductCodeRuleForm form = new ProductCodeRuleForm();

//            DialogResult result = form.ShowDialog(this);

//            if (result != DialogResult.OK)
//            {
//                return;
//            }

//            if (form.EditedRule == null)
//            {
//                return;
//            }

//            // 将新规则加入当前集合

//            _rules.Add(form.EditedRule);

//            // 保存整个规则集合

//            CreateRule(form.EditedRule);

//            // 刷新列表

//            RefreshRuleList();

//            // 自动选择新规则

//            _ruleList.SelectedIndex = _rules.Count - 1;
//        }

//        #endregion

//        #region 编辑规则

//        /// <summary>
//        /// 点击编辑按钮。
//        /// </summary>
//        private void EditButton_Click(
//            object sender,
//            EventArgs e)
//        {
//            EditSelectedRule();
//        }

//        /// <summary>
//        /// 双击规则进入编辑。
//        /// </summary>
//        private void RuleList_DoubleClick(
//            object sender,
//            EventArgs e)
//        {
//            EditSelectedRule();
//        }

//        /// <summary>
//        /// 编辑当前选中的规则。
//        /// </summary>
//        private void EditSelectedRule()
//        {
//            int index = _ruleList.SelectedIndex;

//            if (index < 0)
//            {
//                MessageBox.Show(
//                    "请先选择一条规则。",
//                    "提示",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Information);

//                return;
//            }

//            if (index >= _rules.Count)
//            {
//                return;
//            }

//            CodeRule rule = _rules[index];

//            //------------------------------------------
//            // 打开单条规则编辑窗口
//            //------------------------------------------

//            ProductCodeRuleForm form =
//                new ProductCodeRuleForm(
//                    rule);

//            DialogResult result =
//                form.ShowDialog(
//                    this);

//            if (result != DialogResult.OK)
//            {
//                return;
//            }

//            if (form.EditedRule == null)
//            {
//                return;
//            }

//            // 用编辑后的规则替换原规则

//            _rules[index] = form.EditedRule;

//            // 保存

//            UpdateRule(_rules[index]);

//            // 刷新

//            RefreshRuleList();

//            // 恢复当前选择

//            if (index < _ruleList.Items.Count)
//            {
//                _ruleList.SelectedIndex = index;
//            }
//        }

//        #endregion

//        #region 删除规则

//        /// <summary>
//        /// 删除当前选中的规则。
//        /// </summary>
//        private void DeleteButton_Click(object sender, EventArgs e)
//        {
//            int index = _ruleList.SelectedIndex;

//            if (index < 0)
//            {
//                MessageBox.Show(
//                    "请先选择一条规则。",
//                    "提示",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Information);

//                return;
//            }

//            if (index >= _rules.Count)
//            {
//                return;
//            }

//            CodeRule rule = _rules[index];

//            string ruleName =
//                string.IsNullOrWhiteSpace(
//                    rule.RuleName)
//                ?
//                "未命名规则"
//                :
//                rule.RuleName;

//            // 删除确认

//            DialogResult result =
//                MessageBox.Show(
//                    "确定要删除规则："
//                    + ruleName
//                    + "？\r\n\r\n"
//                    + "删除后将无法恢复。",
//                    "删除规则",
//                    MessageBoxButtons.YesNo,
//                    MessageBoxIcon.Warning);

//            if (result != DialogResult.Yes)
//            {
//                return;
//            }

//            // 从内存集合删除

//            _rules.RemoveAt(index);

//            // 删除

//            DeleteRule(rule);

//            // 刷新列表

//            RefreshRuleList();

//            // 尽量保持合理选择

//            if (_rules.Count > 0)
//            {
//                if (index >= _rules.Count)
//                {
//                    index = _rules.Count - 1;
//                }

//                _ruleList.SelectedIndex = index;
//            }
//        }

//        #endregion

//        #region 刷新

//        /// <summary>
//        /// 点击刷新按钮。
//        ///
//        /// 当前阶段重新读取JSON文件。
//        ///
//        /// 后续接入SQL时，
//        /// 这里将重新查询数据库。
//        /// </summary>
//        private void RefreshButton_Click(
//            object sender,
//            EventArgs e)
//        {
//            LoadRules();
//        }

//        #endregion
//    }
//}


using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Common.Models.ProductModel;
using GT.Factory.Management.UI.Product;
using GT.Factory.Product.Service;
using GT.Factory.ProductCode.Service;
using GT.Factory.ProductCode.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.ProductCode.UI.Forms
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
    public class ProductCodeRuleManageForm : Form
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
        public ProductCodeRuleManageForm(
            ProductModelService productModelService,
            ProductPartService productPartService,
            ProductCodeRuleService ruleService)
        {

            //using (ProductModelManageForm form =
            // new ProductModelManageForm(productModelService, productPartService))
            //{
            //    if (form.ShowDialog(this) == DialogResult.OK)
            //    {

            //    }
            //}


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

            StartPosition =
                FormStartPosition.CenterScreen;

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

            _previewText.Text = CreatePreviewDescription(rule) +"----"+ generator.GeneratePreview(rule);

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
