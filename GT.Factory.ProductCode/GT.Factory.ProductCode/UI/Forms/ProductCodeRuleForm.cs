//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Windows.Forms;
//using GT.Factory.Common.Models.ProductCode;
//using GT.Factory.ProductCode.UI.Controls;
//using Newtonsoft.Json;


//namespace GT.Factory.ProductCode.UI.Forms
//{
//    /// <summary>
//    /// 产品码规则编辑窗口。
//    ///
//    /// 当前版本：
//    ///
//    /// 纯UI版本 V1.1
//    ///
//    /// 主要职责：
//    ///
//    /// 1. 创建新规则
//    /// 2. 编辑已有规则
//    /// 3. 编辑规则基本信息
//    /// 4. 管理Segment
//    /// 5. 编辑Segment配置
//    /// 6. 实时预览产品码
//    /// 7. 将编辑结果返回给规则管理页面
//    ///
//    /// 不负责：
//    ///
//    /// 1. 加载所有规则
//    /// 2. 删除规则
//    /// 3. 数据库
//    /// 4. SQL
//    /// 5. MES
//    /// 6. 流水号申请
//    /// 7. 全局规则文件管理
//    ///
//    /// 说明：
//    ///
//    /// ProductCodeRuleManageForm负责规则集合的管理。
//    ///
//    /// 本窗口只负责“单条规则编辑”。
//    /// </summary>
//    public class ProductCodeRuleForm : Form
//    {
//        /// <summary>
//        /// 当前正在编辑的规则。
//        ///
//        /// 对已有规则编辑时，
//        /// 这里保存的是原规则的副本，
//        /// 不直接修改管理页面中的原对象。
//        /// </summary>
//        private CodeRule _rule;

//        /// <summary>
//        /// 规则基本信息控件。
//        /// </summary>
//        private CodeRuleInfoControl _ruleInfo;

//        /// <summary>
//        /// Segment列表控件。
//        ///
//        /// 负责：
//        ///
//        /// 1. 新增
//        /// 2. 删除
//        /// 3. 上移
//        /// 4. 下移
//        /// 5. Segment选择
//        /// </summary>
//        private CodeSegmentListControl _segmentList;

//        /// <summary>
//        /// Segment配置编辑器。
//        /// </summary>
//        private CodeSegmentEditorControl _editor;

//        /// <summary>
//        /// 产品码预览控件。
//        /// </summary>
//        private CodePreviewControl _preview;

//        /// <summary>
//        /// 保存按钮。
//        ///
//        /// 保存不是直接写数据库，
//        /// 而是将当前编辑结果返回给调用者。
//        /// </summary>
//        private Button _saveButton;

//        /// <summary>
//        /// 取消按钮。
//        /// </summary>
//        private Button _cancelButton;

//        /// <summary>
//        /// 测试生成按钮。
//        /// </summary>
//        private Button _testButton;

//        /// <summary>
//        /// 编辑完成后返回给调用者的规则。
//        ///
//        /// 当DialogResult为OK时，
//        /// 调用者可以从这里取得完整规则。
//        /// </summary>
//        public CodeRule EditedRule
//        {
//            get
//            {
//                return _rule;
//            }
//        }


//        #region 构造函数

//        /// <summary>
//        /// 创建一个新的产品码规则。
//        ///
//        /// 该构造函数用于：
//        ///
//        /// ProductCodeRuleManageForm
//        /// → 新建规则
//        /// </summary>
//        public ProductCodeRuleForm()
//        {
//            InitializeData();

//            InitializeUI();

//            BindEvents();
//        }


//        /// <summary>
//        /// 编辑已有产品码规则。
//        ///
//        /// 这里不会直接修改传入的rule。
//        ///
//        /// 而是创建一个副本进行编辑。
//        ///
//        /// 用户点击取消时，
//        /// 原规则不会受到影响。
//        /// </summary>
//        /// <param name="rule">需要编辑的已有规则。</param>
//        public ProductCodeRuleForm(
//            CodeRule rule)
//        {
//            if (rule == null)
//            {
//                throw new ArgumentNullException(
//                    "rule");
//            }

//            /*
//             * 使用JSON完成深复制。
//             *
//             * 当前CodeRule本身就是需要JSON保存的模型，
//             * 因此这种方式简单且不会额外引入新的复制框架。
//             */
//            string json =
//                JsonConvert.SerializeObject(rule);

//            _rule =
//                JsonConvert.DeserializeObject<CodeRule>(json);

//            /*
//             * 理论上不会为空，
//             * 这里仍然进行保护。
//             */
//            if (_rule == null)
//            {
//                throw new InvalidOperationException(
//                    "产品码规则复制失败。");
//            }

//            /*
//             * 防止Details为空导致后续新增Segment异常。
//             */
//            if (_rule.Details == null)
//            {
//                _rule.Details =
//                    new List<CodeRuleDetail>();
//            }

//            InitializeUI();

//            BindEvents();
//        }

//        #endregion


//        #region 初始化数据

//        /// <summary>
//        /// 创建新规则的默认数据。
//        /// </summary>
//        private void InitializeData()
//        {
//            _rule = new CodeRule();

//            _rule.RuleName = "New Product Code";

//            _rule.Description = "产品码规则";

//            _rule.Details = new List<CodeRuleDetail>();
//        }

//        #endregion


//        #region UI

//        /// <summary>
//        /// 初始化规则编辑页面。
//        /// </summary>
//        private void InitializeUI()
//        {
//            Text = "产品码规则编辑";

//            Width = 1100;

//            Height = 700;

//            StartPosition = FormStartPosition.CenterScreen;

//            /*
//             * 防止窗体尺寸过小时，
//             * 中间编辑区域无法正常显示。
//             */
//            MinimumSize = new Size(900, 600);


//            //--------------------------------------------------
//            // 顶部规则信息
//            //--------------------------------------------------

//            _ruleInfo = new CodeRuleInfoControl();

//            _ruleInfo.Dock = DockStyle.Top;

//            _ruleInfo.Padding = new Padding(10);
//            _ruleInfo.Height = 120;

//            _ruleInfo.BindRule(_rule);


//            //--------------------------------------------------
//            // 中间区域
//            //--------------------------------------------------

//            SplitContainer split = new SplitContainer();

//            _ruleInfo.Padding = new Padding(10);

//            split.Dock = DockStyle.Fill;

//            /*
//             * SplitterDistance在窗体创建阶段
//             * 可能受到ClientSize影响。
//             *
//             * 因此继续在Load事件中设置。
//             */
//            this.Load += (s, e) =>
//            {
//                split.SplitterDistance = 340;
//            };


//            //--------------------------------------------------
//            // 左侧Segment列表
//            //--------------------------------------------------

//            _segmentList =
//                new CodeSegmentListControl();

//            _segmentList.Dock = DockStyle.Fill;

//            /*
//             * 使用WinForms原生Margin属性。
//             */
//            _segmentList.Padding = new Padding(10);

//            split.Panel1.Controls.Add(_segmentList);


//            //--------------------------------------------------
//            // 右侧Segment编辑器
//            //--------------------------------------------------

//            _editor =
//                new CodeSegmentEditorControl();

//            _editor.Dock = DockStyle.Fill;

//            split.Panel2.Controls.Add(_editor);


//            //--------------------------------------------------
//            // 底部预览
//            //--------------------------------------------------

//            _preview = new CodePreviewControl();

//            _preview.Dock = DockStyle.Bottom;

//            _preview.Height = 120;

//            _preview.BindRule(_rule);


//            //--------------------------------------------------
//            // 底部操作区域
//            //--------------------------------------------------

//            FlowLayoutPanel buttons = new FlowLayoutPanel();

//            buttons.Dock = DockStyle.Bottom;

//            buttons.Height = 50;

//            buttons.Padding = new Padding(8);

//            buttons.FlowDirection = FlowDirection.LeftToRight;


//            //--------------------------------------------------
//            // 测试按钮
//            //--------------------------------------------------

//            _testButton = CreateButton("测试生成");


//            //--------------------------------------------------
//            // 保存按钮
//            //--------------------------------------------------

//            _saveButton = CreateButton("保存");


//            //--------------------------------------------------
//            // 取消按钮
//            //--------------------------------------------------

//            _cancelButton = CreateButton("取消");


//            buttons.Controls.Add(_testButton);

//            buttons.Controls.Add(_saveButton);

//            buttons.Controls.Add(_cancelButton);


//            //--------------------------------------------------
//            // 添加控件
//            //--------------------------------------------------

//            Controls.Add(split);

//            Controls.Add(_preview);

//            Controls.Add(buttons);

//            Controls.Add(_ruleInfo);


//            //--------------------------------------------------
//            // 绑定规则
//            //--------------------------------------------------

//            _segmentList.BindRule(_rule);
//        }


//        /// <summary>
//        /// 创建统一样式的操作按钮。
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
//                new Padding(5);

//            return button;
//        }

//        #endregion


//        #region 事件绑定

//        /// <summary>
//        /// 绑定编辑页面事件。
//        /// </summary>
//        private void BindEvents()
//        {
//            /*
//             * 保存当前编辑结果。
//             */
//            _saveButton.Click +=
//                SaveButton_Click;


//            /*
//             * 取消编辑。
//             */
//            _cancelButton.Click +=
//                CancelButton_Click;


//            /*
//             * 测试生成。
//             */
//            _testButton.Click +=
//                TestButton_Click;


//            /*
//             * Segment选择。
//             */
//            _segmentList.SegmentSelected +=
//                SegmentSelected;


//            /*
//             * Segment集合发生变化。
//             *
//             * 包括：
//             *
//             * 1. 新增
//             * 2. 删除
//             * 3. 上移
//             * 4. 下移
//             */
//            _segmentList.SegmentsChanged +=
//                SegmentList_SegmentsChanged;


//            /*
//             * Segment配置发生变化。
//             */
//            _editor.SegmentChanged +=
//                Editor_ConfigChanged;
//        }

//        #endregion


//        #region Segment操作

//        /// <summary>
//        /// 用户选择一个Segment。
//        ///
//        /// 将当前Segment交给右侧编辑器。
//        /// </summary>
//        private void SegmentSelected(
//            object sender,
//            CodeRuleDetail detail)
//        {
//            if (detail == null)
//            {
//                return;
//            }

//            _editor.BindSegment(
//                detail);
//        }


//        /// <summary>
//        /// Segment集合发生变化。
//        ///
//        /// Segment本身的增删排序由
//        /// CodeSegmentListControl完成。
//        ///
//        /// 当前窗口只负责刷新预览。
//        /// </summary>
//        private void SegmentList_SegmentsChanged(
//            object sender,
//            EventArgs e)
//        {
//            RefreshPreview();
//        }

//        #endregion


//        #region 编辑

//        /// <summary>
//        /// Segment配置发生变化。
//        ///
//        /// 编辑器负责修改具体Segment配置。
//        ///
//        /// 当前窗口收到通知后刷新产品码预览。
//        /// </summary>
//        private void Editor_ConfigChanged(
//            object sender,
//            object config)
//        {
//            RefreshPreview();
//        }


//        /// <summary>
//        /// 刷新产品码预览。
//        ///
//        /// 统一通过这个方法刷新，
//        /// 避免不同事件中重复写刷新代码。
//        /// </summary>
//        private void RefreshPreview()
//        {
//            if (_preview == null)
//            {
//                return;
//            }

//            _preview.RefreshPreview();
//        }

//        #endregion


//        #region 保存

//        /// <summary>
//        /// 保存当前规则。
//        ///
//        /// 当前阶段不直接写JSON，
//        /// 也不访问SQL。
//        ///
//        /// 保存操作的职责是：
//        ///
//        /// 1. 同步规则基本信息
//        /// 2. 将DialogResult设置为OK
//        /// 3. 将当前规则交还给调用窗口
//        ///
//        /// ProductCodeRuleManageForm收到OK后，
//        /// 再决定如何保存整个规则集合。
//        /// </summary>
//        private void SaveButton_Click(
//            object sender,
//            EventArgs e)
//        {
//            /*
//             * 先同步规则基本信息。
//             */
//            _ruleInfo.UpdateRule();


//            /*
//             * 保存前进行最基本的规则检查。
//             *
//             * 当前只检查规则名称。
//             *
//             * Segment具体校验后续可以继续增加。
//             */
//            if (string.IsNullOrWhiteSpace(
//                _rule.RuleName))
//            {
//                MessageBox.Show(
//                    "请输入规则名称。",
//                    "提示",
//                    MessageBoxButtons.OK,
//                    MessageBoxIcon.Warning);

//                return;
//            }


//            /*
//             * 返回给调用者。
//             *
//             * EditedRule就是当前编辑完成的规则。
//             */
//            DialogResult =
//                DialogResult.OK;

//            Close();
//        }

//        #endregion


//        #region 取消

//        /// <summary>
//        /// 取消当前编辑。
//        ///
//        /// 因为编辑已有规则时使用的是副本，
//        /// 所以这里直接关闭即可。
//        ///
//        /// 原始规则不会受到影响。
//        /// </summary>
//        private void CancelButton_Click(
//            object sender,
//            EventArgs e)
//        {
//            DialogResult =
//                DialogResult.Cancel;

//            Close();
//        }

//        #endregion


//        #region 测试

//        /// <summary>
//        /// 测试当前规则。
//        ///
//        /// 当前只刷新内存预览。
//        ///
//        /// 不访问数据库。
//        /// </summary>
//        private void TestButton_Click(
//            object sender,
//            EventArgs e)
//        {
//            /*
//             * 先同步规则基本信息，
//             * 保证测试使用的是当前页面数据。
//             */
//            _ruleInfo.UpdateRule();


//            /*
//             * 刷新产品码预览。
//             */
//            RefreshPreview();


//            MessageBox.Show(
//                "测试生成完成。",
//                "测试",
//                MessageBoxButtons.OK,
//                MessageBoxIcon.Information);
//        }

//        #endregion
//    }
//}

using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Common.Models.ProductModel;
using GT.Factory.ProductCode.UI.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.ProductCode.UI.Forms
{
    /// <summary>
    /// 产品码规则编辑窗口。
    ///
    /// 本窗口只负责编辑“一条产品码规则”。
    ///
    /// 当前规则的业务归属由外部管理窗口确定：
    ///
    /// 产品型号
    ///     ↓
    /// 零部件
    ///     ↓
    /// 当前规则
    ///
    /// 新增规则时：
    ///
    /// ProductCodeRuleManageForm
    ///     ↓
    /// ProductCodeRuleForm(ProductModel, Part)
    ///
    /// 编辑已有规则时：
    ///
    /// ProductCodeRuleManageForm
    ///     ↓
    /// ProductCodeRuleForm(CodeRule)
    ///
    /// 本窗口不负责：
    ///
    /// 1. 查询产品型号
    /// 2. 查询零部件
    /// 3. 查询规则集合
    /// 4. SQL
    /// 5. Repository
    ///
    /// 本窗口负责：
    ///
    /// 1. 编辑规则名称
    /// 2. 编辑规则描述
    /// 3. 编辑规则启用状态
    /// 4. 编辑 Segment
    /// 5. 预览产品码
    /// 6. 将编辑结果返回给管理窗口
    /// </summary>
    public class ProductCodeRuleForm : Form
    {
#region 当前归属

    /// <summary>
    /// 当前产品型号。
    ///
    /// 新增时由管理窗口传入。
    /// 编辑时由 CodeRule 查询结果携带。
    /// </summary>
    private GTProductModel _productModel;

        /// <summary>
        /// 当前产品零部件。
        ///
        /// Rule 的真正数据库归属是 PartId。
        /// </summary>
        private GTProductPart _part;

        #endregion

        #region 当前规则

        /// <summary>
        /// 当前正在编辑的规则副本。
        ///
        /// 编辑已有规则时不会直接修改外部对象。
        /// </summary>
        private CodeRule _rule;

        #endregion

        #region 顶部归属信息

        /// <summary>
        /// 产品型号显示。
        /// </summary>
        private Label _productModelValue;

        /// <summary>
        /// 产品零部件显示。
        /// </summary>
        private Label _partValue;

        #endregion

        #region 规则编辑控件

        /// <summary>
        /// 规则基本信息控件。
        ///
        /// 继续使用现有成熟控件。
        /// </summary>
        private CodeRuleInfoControl _ruleInfo;

        /// <summary>
        /// Segment列表。
        /// </summary>
        private CodeSegmentListControl _segmentList;

        /// <summary>
        /// Segment编辑器。
        /// </summary>
        private CodeSegmentEditorControl _editor;

        /// <summary>
        /// 产品码预览。
        /// </summary>
        private CodePreviewControl _preview;

        #endregion

        #region 操作按钮

        /// <summary>
        /// 保存按钮。
        /// </summary>
        private Button _saveButton;

        /// <summary>
        /// 取消按钮。
        /// </summary>
        private Button _cancelButton;

        /// <summary>
        /// 测试按钮。
        /// </summary>
        private Button _testButton;

        #endregion

        #region 返回结果

        /// <summary>
        /// 编辑完成后返回给管理窗口的规则。
        ///
        /// 注意：
        ///
        /// 这里返回的是完整 CodeRule，
        /// 其中真正写入数据库的核心归属字段是 PartId。
        /// </summary>
        public CodeRule EditedRule
        {
            get
            {
                return _rule;
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建新的产品码规则。
        ///
        /// 新规则必须同时指定：
        ///
        /// 产品型号
        /// +
        /// 产品零部件
        ///
        /// 其中真正保存到 GTCodeRule 的是：
        ///
        /// PartId
        /// </summary>
        public ProductCodeRuleForm(
            GTProductModel productModel,
            GTProductPart part)
        {
            if (productModel == null)
            {
                throw new ArgumentNullException(
                    "productModel");
            }

            if (part == null)
            {
                throw new ArgumentNullException(
                    "part");
            }

            _productModel =
                productModel;

            _part =
                part;

            InitializeNewRule();

            InitializeUI();

            BindEvents();
        }

        /// <summary>
        /// 编辑已有产品码规则。
        ///
        /// 这里使用规则副本，
        /// 避免取消编辑时修改外部对象。
        ///
        /// ProductModel / Part 的显示信息优先使用
        /// CodeRule 查询时带回来的字段。
        /// </summary>
        public ProductCodeRuleForm(
            CodeRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(
                    "rule");
            }

            string json =
                JsonConvert.SerializeObject(
                    rule);

            _rule =
                JsonConvert.DeserializeObject<CodeRule>(
                    json);

            if (_rule == null)
            {
                throw new InvalidOperationException(
                    "产品码规则复制失败。");
            }

            if (_rule.Details == null)
            {
                _rule.Details =
                    new List<CodeRuleDetail>();
            }

            /*
             * 编辑已有规则时，
             * CodeRule 中的这些字段属于查询/显示字段。
             *
             * 它们不是 GTCodeRule 的冗余数据库字段。
             */
            _productModel =
                CreateDisplayProductModel(
                    _rule);

            _part =
                CreateDisplayPart(
                    _rule);

            InitializeUI();

            BindEvents();
        }

        #endregion

        #region 初始化数据

        /// <summary>
        /// 初始化新规则。
        /// </summary>
        private void InitializeNewRule()
        {
            _rule =
                new CodeRule();

            /*
             * 新规则的真正数据库归属。
             */
            _rule.PartId =
                _part.PartId;

            _rule.RuleName =
                "新产品码规则";

            _rule.Description =
                "产品码规则";

            _rule.IsActive =
                true;

            _rule.Details =
                new List<CodeRuleDetail>();
        }

        /// <summary>
        /// 根据已有 CodeRule 创建只用于显示的型号对象。
        ///
        /// 如果当前 CodeRule 已经通过 JOIN 查询出 ProductModel，
        /// 这里可以直接用于顶部显示。
        /// </summary>
        private GTProductModel CreateDisplayProductModel(
            CodeRule rule)
        {
            GTProductModel model =
                new GTProductModel();

            model.ProductModel =
                rule.ProductModel;

            return model;
        }

        /// <summary>
        /// 根据已有 CodeRule 创建只用于显示的零部件对象。
        /// </summary>
        private GTProductPart CreateDisplayPart(
            CodeRule rule)
        {
            GTProductPart part =
                new GTProductPart();

            part.PartId =
                rule.PartId;

            part.PartCode =
                rule.PartCode;

            part.PartName =
                rule.PartName;

            return part;
        }

        #endregion

        #region UI

        /// <summary>
        /// 初始化规则编辑页面。
        /// </summary>
        private void InitializeUI()
        {
            Text =
                "产品码规则编辑";

            Width =
                1150;

            Height =
                760;

            MinimumSize =
                new Size(
                    950,
                    650);

            StartPosition =
                FormStartPosition.CenterParent;

            BackColor =
                Color.White;

            Font =
                new Font(
                    "Microsoft YaHei",
                    9F);

            //--------------------------------------------------
            // 顶部归属信息
            //--------------------------------------------------

            Panel ownerPanel =
                CreateOwnerPanel();

            ownerPanel.Dock =
                DockStyle.Top;

            //--------------------------------------------------
            // 规则基本信息
            //--------------------------------------------------

            _ruleInfo =
                new CodeRuleInfoControl();

            _ruleInfo.Dock =
                DockStyle.Top;

            _ruleInfo.Height =
                125;

            _ruleInfo.Padding =
                new Padding(10);

            _ruleInfo.BindRule(
                _rule);

            //--------------------------------------------------
            // 中间 Segment 编辑区域
            //--------------------------------------------------

            SplitContainer split =
                new SplitContainer();

            split.Dock =
                DockStyle.Fill;

            split.Orientation =
                Orientation.Vertical;

            split.Panel1MinSize =
                280;

            //split.Panel2MinSize =
            //    450;

            /*
//             * SplitterDistance在窗体创建阶段
//             * 可能受到ClientSize影响。
//             *
//             * 因此继续在Load事件中设置。
//             */
            this.Load += (s, e) =>
            {
                split.SplitterDistance = 340;
            };

            _segmentList =
                new CodeSegmentListControl();

            _segmentList.Dock =
                DockStyle.Fill;

            _segmentList.Padding =
                new Padding(8);

            split.Panel1.Controls.Add(
                _segmentList);

            //--------------------------------------------------
            // 右侧 Segment 编辑器
            //--------------------------------------------------

            _editor =
                new CodeSegmentEditorControl();

            _editor.Dock =
                DockStyle.Fill;

            _editor.Padding =
                new Padding(8);

            split.Panel2.Controls.Add(
                _editor);

            //--------------------------------------------------
            // 产品码预览
            //--------------------------------------------------

            _preview =
                new CodePreviewControl();

            _preview.Dock =
                DockStyle.Bottom;

            _preview.Height =
                115;

            _preview.BindRule(
                _rule);

            //--------------------------------------------------
            // 底部按钮
            //--------------------------------------------------

            Panel buttonPanel =
                CreateButtonPanel();

            buttonPanel.Dock =
                DockStyle.Bottom;

            //--------------------------------------------------
            // 添加控件
            //
            // Dock 顺序非常重要。
            //--------------------------------------------------

            Controls.Add(
                split);

            Controls.Add(
                _preview);

            Controls.Add(
                buttonPanel);

            Controls.Add(
                _ruleInfo);

            Controls.Add(
                ownerPanel);

            //--------------------------------------------------
            // 绑定规则到 Segment 列表
            //--------------------------------------------------

            _segmentList.BindRule(
                _rule);

            //--------------------------------------------------
            // 初始 Splitter 位置
            //--------------------------------------------------

            Shown += delegate
            {
                int width =
                    split.ClientSize.Width;

                int distance =
                    (int)(width * 0.30);

                if (distance <
                    split.Panel1MinSize)
                {
                    distance =
                        split.Panel1MinSize;
                }

                if (distance >
                    width - split.Panel2MinSize)
                {
                    distance =
                        width - split.Panel2MinSize;
                }

                if (distance > 0)
                {
                    split.SplitterDistance =
                        distance;
                }
            };
        }

        /// <summary>
        /// 创建顶部产品型号 / 零部件只读区域。
        /// </summary>
        private Panel CreateOwnerPanel()
        {
            Panel panel =
                new Panel();

            panel.Height =
                72;

            panel.Padding =
                new Padding(
                    10,
                    8,
                    10,
                    6);

            panel.BackColor =
                Color.FromArgb(
                    238,
                    246,
                    252);

            TableLayoutPanel table =
                new TableLayoutPanel();

            table.Dock =
                DockStyle.Fill;

            table.ColumnCount =
                4;

            table.RowCount =
                1;

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    75));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    75));

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    50));

            //--------------------------------------------------
            // 产品型号
            //--------------------------------------------------

            Label modelTitle =
                CreateOwnerTitle(
                    "产品型号");

            _productModelValue =
                CreateOwnerValue();

            _productModelValue.Text =
                GetProductModelText();

            //--------------------------------------------------
            // 零部件
            //--------------------------------------------------

            Label partTitle =
                CreateOwnerTitle(
                    "零部件");

            _partValue =
                CreateOwnerValue();

            _partValue.Text =
                GetPartText();

            table.Controls.Add(
                modelTitle,
                0,
                0);

            table.Controls.Add(
                _productModelValue,
                1,
                0);

            table.Controls.Add(
                partTitle,
                2,
                0);

            table.Controls.Add(
                _partValue,
                3,
                0);

            panel.Controls.Add(
                table);

            return panel;
        }

        /// <summary>
        /// 创建归属信息标题。
        /// </summary>
        private Label CreateOwnerTitle(
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
                    70,
                    90,
                    105);

            return label;
        }

        /// <summary>
        /// 创建归属信息值。
        /// </summary>
        private Label CreateOwnerValue()
        {
            Label label =
                new Label();

            label.Dock =
                DockStyle.Fill;

            label.TextAlign =
                ContentAlignment.MiddleLeft;

            label.AutoEllipsis =
                true;

            label.ForeColor =
                Color.FromArgb(
                    35,
                    50,
                    60);

            return label;
        }

        /// <summary>
        /// 获取顶部型号显示文本。
        /// </summary>
        private string GetProductModelText()
        {
            if (_productModel == null)
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(
                _productModel.ProductModel))
            {
                return "未指定";
            }

            if (string.IsNullOrWhiteSpace(
                _productModel.ProductName))
            {
                return _productModel.ProductModel;
            }

            return _productModel.ProductModel
                + " - "
                + _productModel.ProductName;
        }

        /// <summary>
        /// 获取顶部部件显示文本。
        /// </summary>
        private string GetPartText()
        {
            if (_part == null)
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(
                _part.PartCode))
            {
                return string.IsNullOrWhiteSpace(
                    _part.PartName)
                    ? "未指定"
                    : _part.PartName;
            }

            if (string.IsNullOrWhiteSpace(
                _part.PartName))
            {
                return _part.PartCode;
            }

            return _part.PartCode
                + " - "
                + _part.PartName;
        }

        /// <summary>
        /// 创建底部按钮区域。
        /// </summary>
        private Panel CreateButtonPanel()
        {
            Panel panel =
                new Panel();

            panel.Height =
                52;

            panel.Padding =
                new Padding(
                    8);

            FlowLayoutPanel buttons =
                new FlowLayoutPanel();

            buttons.Dock =
                DockStyle.Fill;

            buttons.FlowDirection =
                FlowDirection.RightToLeft;

            buttons.WrapContents =
                false;

            //--------------------------------------------------
            // 保存
            //--------------------------------------------------

            _saveButton =
                CreateButton(
                    "保存",
                    90);

            //--------------------------------------------------
            // 取消
            //--------------------------------------------------

            _cancelButton =
                CreateButton(
                    "取消",
                    90);

            //--------------------------------------------------
            // 测试
            //--------------------------------------------------

            _testButton =
                CreateButton(
                    "测试生成",
                    100);

            buttons.Controls.Add(
                _saveButton);

            buttons.Controls.Add(
                _cancelButton);

            buttons.Controls.Add(
                _testButton);

            panel.Controls.Add(
                buttons);

            return panel;
        }

        /// <summary>
        /// 创建标准按钮。
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
                    4);

            button.Font =
                new Font(
                    "Microsoft YaHei",
                    9F);

            return button;
        }

        #endregion

        #region 事件绑定

        /// <summary>
        /// 绑定窗口事件。
        /// </summary>
        private void BindEvents()
        {
            _saveButton.Click +=
                SaveButton_Click;

            _cancelButton.Click +=
                CancelButton_Click;

            _testButton.Click +=
                TestButton_Click;

            //--------------------------------------------------
            // Segment 选择
            //--------------------------------------------------

            _segmentList.SegmentSelected +=
                SegmentSelected;

            //--------------------------------------------------
            // Segment 集合变化
            //--------------------------------------------------

            _segmentList.SegmentsChanged +=
                SegmentList_SegmentsChanged;

            //--------------------------------------------------
            // Segment 配置变化
            //--------------------------------------------------

            _editor.SegmentChanged +=
                Editor_ConfigChanged;
        }

        #endregion

        #region Segment操作

        /// <summary>
        /// Segment 被选择。
        ///
        /// 将选中的 Segment 交给右侧编辑器。
        /// </summary>
        private void SegmentSelected(
            object sender,
            CodeRuleDetail detail)
        {
            if (detail == null)
            {
                return;
            }

            _editor.BindSegment(
                detail);
        }

        /// <summary>
        /// Segment 集合发生变化。
        ///
        /// 包括：
        ///
        /// 1. 新增
        /// 2. 删除
        /// 3. 上移
        /// 4. 下移
        /// </summary>
        private void SegmentList_SegmentsChanged(
            object sender,
            EventArgs e)
        {
            RefreshPreview();
        }

        /// <summary>
        /// Segment 配置发生变化。
        /// </summary>
        private void Editor_ConfigChanged(
            object sender,
            object config)
        {
            RefreshPreview();
        }

        /// <summary>
        /// 刷新产品码预览。
        /// </summary>
        private void RefreshPreview()
        {
            if (_preview == null)
            {
                return;
            }

            _preview.RefreshPreview();
        }

        #endregion

        #region 保存

        /// <summary>
        /// 保存当前规则。
        ///
        /// 注意：
        ///
        /// 这里不访问数据库。
        ///
        /// 这里只负责：
        ///
        /// 1. 从规则信息控件同步数据
        /// 2. 确保 PartId 正确
        /// 3. 检查基本数据
        /// 4. 返回 DialogResult.OK
        ///
        /// 真正的数据库新增/修改由管理窗口调用 Service 完成。
        /// </summary>
        private void SaveButton_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                //--------------------------------------------------
                // 同步规则基本信息
                //--------------------------------------------------

                _ruleInfo.UpdateRule();

                //--------------------------------------------------
                // 强制恢复当前部件归属
                //
                // 防止以后 CodeRuleInfoControl 增加字段后，
                // 不小心把 PartId 修改掉。
                //--------------------------------------------------

                if (_part != null)
                {
                    _rule.PartId =
                        _part.PartId;
                }

                //--------------------------------------------------
                // 基本校验
                //--------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    _rule.RuleName))
                {
                    MessageBox.Show(
                        "请输入规则名称。",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                //--------------------------------------------------
                // Segment 校验
                //--------------------------------------------------

                if (_rule.Details == null)
                {
                    _rule.Details =
                        new List<CodeRuleDetail>();
                }

                //--------------------------------------------------
                // 返回
                //--------------------------------------------------

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "保存规则时发生错误。\r\n\r\n"
                    + ex.Message,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 取消

        /// <summary>
        /// 取消编辑。
        ///
        /// 因为编辑已有规则时使用的是副本，
        /// 所以直接关闭窗口即可。
        /// </summary>
        private void CancelButton_Click(
            object sender,
            EventArgs e)
        {
            DialogResult =
                DialogResult.Cancel;

            Close();
        }

        #endregion

        #region 测试

        /// <summary>
        /// 测试当前产品码规则。
        ///
        /// 不写数据库。
        /// </summary>
        private void TestButton_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                /*
                 * 先同步基本信息，
                 * 确保预览使用当前页面的数据。
                 */
                _ruleInfo.UpdateRule();

                /*
                 * 强制保持当前部件归属。
                 */
                if (_part != null)
                {
                    _rule.PartId =
                        _part.PartId;
                }

                /*
                 * 刷新已有 Preview 控件。
                 */
                RefreshPreview();

                MessageBox.Show(
                    "测试生成完成。",
                    "测试",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "测试产品码失败。\r\n\r\n"
                    + ex.Message,
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        #endregion
    }

}
