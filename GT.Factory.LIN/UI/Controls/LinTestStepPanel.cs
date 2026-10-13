using GT.Factory.LIN.ProjectDebug;
using GT.Factory.LIN.Test.Model;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.LIN.UI.Controls
{
    /// <summary>
    /// LIN测试流程显示面板。
    ///
    /// 主要职责：
    ///
    /// 1. 显示当前LIN测试工程。
    /// 2. 显示测试Case列表。
    /// 3. 显示当前Case的Step列表。
    /// 4. 显示Case / Step执行状态。
    /// 5. 提供运行Case、运行全部、暂停、停止等测试操作入口。
    ///
    /// 注意：
    ///
    /// 本控件属于UI层。
    ///
    /// 当前阶段保留模拟Step执行逻辑，用于验证UI流程。
    /// 后续接入真实测试执行器时，只需要替换ExecuteSingleStepAsync()
    /// 的具体执行内容，不应将LIN Runtime逻辑直接写入本控件。
    /// </summary>
    public class LinTestStepPanel : UserControl, ILinDebugSessionView
    {
        #region 私有字段

        /// <summary>
        /// 当前测试工程中的Case集合。
        ///
        /// UI使用自己的集合保存当前显示数据，
        /// 避免直接依赖ListBox内部的数据结构。
        /// </summary>
        private readonly List<LinTestCase> _cases =new List<LinTestCase>();

        /// <summary>
        /// Case执行结果。
        ///
        /// Key为当前Case在工程中的索引。
        /// 当前阶段保持与现有测试模型兼容。
        /// </summary>
        private readonly Dictionary<int, CaseResult> _caseResults =new Dictionary<int, CaseResult>();

        /// <summary>
        /// 当前测试执行使用的取消令牌源。
        ///
        /// Stop按钮通过该对象通知测试流程停止。
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 当前正在执行的Case索引。
        /// </summary>
        private int? _currentRunningCase;

        /// <summary>
        /// 当前正在执行的Step编号。
        /// </summary>
        private int? _currentRunningStep;

        /// <summary>
        /// 当前是否处于测试执行状态。
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// 当前绑定的LIN测试工程。
        /// </summary>
        private LinTestProject _project;

        #endregion


        #region UI控件

        /// <summary>
        /// 当前工程名称显示。
        /// </summary>
        private Label _projectLabel;

        /// <summary>
        /// 运行当前Case按钮。
        /// </summary>
        private Button _runCaseButton;

        /// <summary>
        /// 运行全部Case按钮。
        /// </summary>
        private Button _runAllButton;

        /// <summary>
        /// 暂停按钮。
        /// </summary>
        private Button _pauseButton;

        /// <summary>
        /// 停止按钮。
        /// </summary>
        private Button _stopButton;

        /// <summary>
        /// 总体执行进度条。
        /// </summary>
        private ProgressBar _progressBar;

        /// <summary>
        /// Case统计信息显示。
        /// </summary>
        private Label _statisticLabel;

        /// <summary>
        /// Case列表。
        /// </summary>
        private ListBox _caseList;

        /// <summary>
        /// 当前Case标题。
        /// </summary>
        private Label _caseTitle;

        /// <summary>
        /// 当前Case的Step表格。
        /// </summary>
        private DataGridView _stepGrid;

        #endregion


        #region 构造函数

        /// <summary>
        /// 初始化LIN测试流程面板。
        /// </summary>
        public LinTestStepPanel()
        {
            Initialize();
        }

        #endregion


        #region 初始化

        /// <summary>
        /// 初始化整个测试流程UI。
        ///
        /// UI控件统一使用GT.Factory.UI中的Style工厂创建。
        /// </summary>
        private void Initialize()
        {
            // 设置控件填充父容器。
            Dock = DockStyle.Fill;

            // 使用统一工业主题作为整个测试面板背景。
            BackColor = UITheme.BgBase;

            // 创建主布局。
            TableLayoutPanel layout = CreateMainLayout();

            // 将主布局添加到当前控件。
            Controls.Add(layout);

            // 初始刷新按钮状态。
            UpdateButtonStates();
        }

        /// <summary>
        /// 创建测试面板主布局。
        ///
        /// 布局结构：
        ///
        /// Header
        /// Progress
        /// Statistic
        /// Body
        /// </summary>
        private TableLayoutPanel CreateMainLayout()
        {
            TableLayoutPanel layout =
                new TableLayoutPanel();

            // 主布局填充整个测试面板。
            layout.Dock = DockStyle.Fill;

            // 当前页面使用四行布局。
            layout.RowCount = 4;

            // 当前页面只有一列。
            layout.ColumnCount = 1;

            // Header固定高度。
            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    60));

            // Progress固定高度。
            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    20));

            // Statistic固定高度。
            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    35));

            // Body占据剩余空间。
            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100));

            // 添加Header。
            layout.Controls.Add(
                CreateHeader(),
                0,
                0);

            // 添加进度条。
            layout.Controls.Add(
                CreateProgressBar(),
                0,
                1);

            // 添加统计信息。
            layout.Controls.Add(
                CreateStatisticLabel(),
                0,
                2);

            // 添加主体区域。
            layout.Controls.Add(
                CreateBody(),
                0,
                3);

            return layout;
        }

        /// <summary>
        /// 创建测试总体进度条。
        ///
        /// ProgressBar本身没有独立的UI工厂，
        /// 因此这里只设置布局和数据属性。
        /// </summary>
        private ProgressBar CreateProgressBar()
        {
            _progressBar =new ProgressBar();

            // 进度条填充整行。
            _progressBar.Dock =DockStyle.Fill;

            // 设置最小进度。
            _progressBar.Minimum = 0;

            // 设置最大进度。
            _progressBar.Maximum = 100;

            // 初始进度为0。
            _progressBar.Value = 0;

            return _progressBar;
        }

        /// <summary>
        /// 创建Case统计信息。
        ///
        /// Label通过GT.Factory.UI统一创建。
        /// </summary>
        private Label CreateStatisticLabel()
        {
            _statisticLabel =
                UILabel.ValueLabel(
                    "Pass: 0   Fail: 0   Pending: 0");

            // 统计信息横向填充。
            _statisticLabel.Dock =DockStyle.Fill;

            // 统计信息居中显示。
            _statisticLabel.TextAlign =ContentAlignment.MiddleCenter;

            // 使用主题正文颜色。
            _statisticLabel.ForeColor =UITheme.TextMain;

            // 使用统一正文字体。
            _statisticLabel.Font =UITheme.SmallFont;

            return _statisticLabel;
        }

        #endregion


        #region Header区域

        /// <summary>
        /// 创建测试面板Header。
        ///
        /// Header结构：
        ///
        /// 左侧：
        ///     当前工程名称
        ///
        /// 右侧：
        ///     运行Case
        ///     全部测试
        ///     暂停
        ///     停止
        /// </summary>
        private Control CreateHeader()
        {
            TableLayoutPanel headerLayout = new TableLayoutPanel();
            headerLayout.Dock = DockStyle.Top;
            headerLayout.Height = 60;
            headerLayout.ColumnCount = 2;
            headerLayout.RowCount = 1;
            headerLayout.BackColor = UITheme.BgCard;
            headerLayout.Padding = new Padding(UITheme.ControlSpacing);

            // 第一列：根据内容自动调整（工程名称）
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            // 第二列：占满剩余空间（按钮区域）
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // 工程名称 - 不设置 Dock，让宽度自适应
            _projectLabel = UILabel.Title("No Test Project");
            _projectLabel.TextAlign = ContentAlignment.MiddleLeft;
            _projectLabel.AutoSize = true;  // 根据文本自动调整宽度
            headerLayout.Controls.Add(_projectLabel, 0, 0);

            // 按钮面板 - Dock Fill 填充剩余空间
            Panel buttonsPanel = new Panel();
            buttonsPanel.Dock = DockStyle.Fill;

            FlowLayoutPanel buttonsFlow = new FlowLayoutPanel();
            buttonsFlow.Dock = DockStyle.Fill;
            buttonsFlow.WrapContents = false;
            buttonsFlow.FlowDirection = FlowDirection.RightToLeft;  // 从右向左排列
            buttonsFlow.Padding = new Padding(0);
            buttonsFlow.Margin = new Padding(0);

            // 创建按钮
            _runCaseButton = UIButton.Primary("运行Case");
            _runCaseButton.Width = 90;
            _runCaseButton.Height = 36;  // 明确高度
            _runCaseButton.Click += RunCaseButton_Click;

            _runAllButton = UIButton.Primary("全部测试");
            _runAllButton.Width = 90;
            _runAllButton.Height = 36;
            _runAllButton.Click += RunAllButton_Click;

            _pauseButton = UIButton.Default("暂停");
            _pauseButton.Width = 80;
            _pauseButton.Height = 36;
            _pauseButton.Enabled = false;
            _pauseButton.Click += PauseButton_Click;

            _stopButton = UIButton.Default("停止");
            _stopButton.Width = 80;
            _stopButton.Height = 36;
            _stopButton.Enabled = false;
            _stopButton.Click += StopButton_Click;

            // 添加按钮到 FlowLayoutPanel
            buttonsFlow.Controls.AddRange(new Control[] {
                _runCaseButton,
                _runAllButton,
                _pauseButton,
                _stopButton
            });

            buttonsPanel.Controls.Add(buttonsFlow);
            headerLayout.Controls.Add(buttonsPanel, 1, 0);

            return headerLayout;
        }
        #endregion


        #region Body区域

        /// <summary>
        /// 创建测试主体区域。
        ///
        /// 左侧：
        ///     Case列表
        ///
        /// 右侧：
        ///     Case详情 + Step列表
        /// </summary>
        private Control CreateBody()
        {
            SplitContainer split =new SplitContainer();

            // SplitContainer填充整个Body。
            split.Dock =DockStyle.Fill;

            // 使用统一主题背景。
            split.BackColor =UITheme.BgBase;

            // SplitterDistance 必须在控件有真实尺寸之后设置
            HandleCreated += (s, e) =>
            {
                if (split.Width > split.Panel1MinSize + split.Panel2MinSize)
                {
                    split.SplitterDistance = 280;
                }
            };

            // 左侧Case列表。
            Panel leftPanel =CreateCaseListPanel();

            // 添加左侧Panel。
            split.Panel1.Controls.Add(leftPanel);

            // 右侧Case详情。
            Panel rightPanel =CreateCaseDetailPanel();

            // 添加右侧Panel。
            split.Panel2.Controls.Add(rightPanel);

            // 使用SizeChanged确保SplitContainer已经获得实际尺寸后再设置分割位置。
            split.SizeChanged +=
                delegate (object sender, EventArgs e)
                {
                    SetSplitterDistance(split);
                };

            // 初始布局完成后再次设置一次分割位置。
            split.Layout +=
                delegate (object sender, LayoutEventArgs e)
                {
                    SetSplitterDistance(split);
                };

            return split;
        }


        /// <summary>
        /// 设置SplitContainer初始分割位置。
        ///
        /// 只有当控件实际宽度满足条件时才设置，
        /// 避免窗口初始化阶段设置SplitterDistance失败。
        /// </summary>
        private void SetSplitterDistance(SplitContainer split)
        {
            if (split == null)
                return;

            // 防止在尺寸尚未准备好时修改SplitterDistance。
            int availableWidth =split.Width;

            if (availableWidth <=
                split.Panel1MinSize +
                split.Panel2MinSize)
            {
                return;
            }

            // 当前测试界面固定左侧Case列表宽度约250px。
            int desiredDistance = 250;

            // 根据当前窗口宽度限制最大分割位置。
            int maximumDistance =availableWidth -split.Panel2MinSize;

            // 防止SplitterDistance超过可用范围。
            if (desiredDistance > maximumDistance)
            {
                desiredDistance =maximumDistance;
            }

            // 防止SplitterDistance小于左侧最小宽度。
            if (desiredDistance <split.Panel1MinSize)
            {
                desiredDistance =split.Panel1MinSize;
            }

            // 应用最终分割位置。
            split.SplitterDistance =desiredDistance;
        }


        /// <summary>
        /// 创建左侧Case列表。
        /// </summary>
        /// <summary>
        /// 创建左侧Case列表。
        /// </summary>
        private Panel CreateCaseListPanel()
        {
            Panel panel =
            new Panel();

// 左侧Case列表区域填充父容器。
panel.Dock =
    DockStyle.Fill;

            // 使用统一主题背景。
            panel.BackColor =
                UITheme.BgCard;

            // 使用统一区域间距。
            panel.Padding =
                new Padding(
                    UITheme.ControlSpacing);

            // ================================
            // 1. 创建Case列表。
            // ================================

            _caseList =
                UIListBox.Create();

            // 使用自绘模式显示Case状态。
            _caseList.DrawMode =
                DrawMode.OwnerDrawFixed;

            // 绑定Case选择事件。
            _caseList.SelectedIndexChanged +=
                CaseList_SelectedIndexChanged;

            // 绑定Case绘制事件。
            _caseList.DrawItem +=
                CaseList_DrawItem;

            // Case列表填充标题区域以下的剩余空间。
            _caseList.Dock =
                DockStyle.Fill;

            // 先添加Fill控件。
            //
            // 后续添加Top控件后，
            // Fill控件自动占用剩余区域。
            panel.Controls.Add(
                _caseList);

            // ================================
            // 2. 创建标题区域。
            // ================================

            Panel titlePanel =
                new Panel();

            // 标题区域停靠在顶部。
            titlePanel.Dock =
                DockStyle.Top;

            // 标题区域包含标题高度和底部间距。
            titlePanel.Height =
                UITheme.ControlHeight + 6 + 12;

            // 使用与父容器一致的背景。
            titlePanel.BackColor =
                UITheme.BgCard;

            // 创建标题。
            Label titleLabel =
                UILabel.Title(
                    "测试用例列表");

            // 标题填充标题区域。
            titleLabel.Dock =
                DockStyle.Fill;

            // 标题文本左对齐。
            titleLabel.TextAlign =
                ContentAlignment.MiddleLeft;

            // 添加标题到标题区域。
            titlePanel.Controls.Add(
                titleLabel);

            // 最后添加顶部标题区域。
            panel.Controls.Add(
                titlePanel);

            return panel;

}


        /// <summary>
        /// 自定义绘制Case列表项。
        ///
        /// UI状态：
        ///
        /// Idle：
        ///     普通文本
        ///
        /// Running：
        ///     Warning / Primary
        ///
        /// Success：
        ///     Success
        ///
        /// Failed：
        ///     Danger
        ///
        /// Disabled：
        ///     TextMuted
        /// </summary>
        private void CaseList_DrawItem(
            object sender,
            DrawItemEventArgs e)
        {
            // 无效索引直接返回。
            if (e.Index < 0 ||
                e.Index >= _cases.Count)
            {
                return;
            }

            // 获取当前Case。
            LinTestCase testCase =_cases[e.Index];

            // 判断当前Case是否被选中。
            bool selected =
                (e.State &
                 DrawItemState.Selected) != 0;

            // 根据是否选中设置背景。
            Color backgroundColor =
                selected
                    ? UITheme.BgInput
                    : UITheme.BgBase;

            // 使用主题颜色绘制背景。
            using (Brush backgroundBrush =
                   new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(
                    backgroundBrush,
                    e.Bounds);
            }

            // 默认使用主题主文本。
            Color textColor =UITheme.TextMain;

            // 默认没有状态文本。
            string statusText =string.Empty;

            // 根据Case执行结果设置状态。
            CaseResult result;

            if (_caseResults.TryGetValue(
                    e.Index,
                    out result))
            {
                switch (result.Status)
                {
                    case TestStatus.Running:

                        // Running使用主题主操作色。
                        textColor =
                            UITheme.Primary;

                        statusText =
                            "  ●";

                        break;

                    case TestStatus.Success:

                        // Success使用统一成功色。
                        textColor =
                            UITheme.Success;

                        statusText =
                            "  ✓";

                        break;

                    case TestStatus.Failed:

                        // Failed使用统一危险色。
                        textColor =
                            UITheme.Danger;

                        statusText =
                            "  ✗";

                        break;

                    default:

                        // Idle保持默认主题颜色。
                        textColor =
                            UITheme.TextMain;

                        break;
                }
            }

            // Disabled Case使用统一弱化文本颜色。
            if (!testCase.Enabled)
            {
                textColor =
                    UITheme.TextMuted;
            }

            // 组合显示文本。
            string displayText =
                testCase.Name +
                statusText;

            // 创建文本绘制区域。
            Rectangle textBounds =
                new Rectangle(
                    e.Bounds.Left + 10,
                    e.Bounds.Top,
                    e.Bounds.Width - 20,
                    e.Bounds.Height);

            // 使用主题字体绘制Case名称。
            using (Brush textBrush =
                   new SolidBrush(textColor))
            {
                e.Graphics.DrawString(
                    displayText,
                    UITheme.BodyFont,
                    textBrush,
                    textBounds);
            }

            // 选中状态使用主题高亮边框提示。
            if (selected)
            {
                using (Pen focusPen =
                       new Pen(UITheme.CyanGlow))
                {
                    Rectangle focusBounds =
                        new Rectangle(
                            e.Bounds.Left + 1,
                            e.Bounds.Top + 1,
                            e.Bounds.Width - 2,
                            e.Bounds.Height - 2);

                    e.Graphics.DrawRectangle(
                        focusPen,
                        focusBounds);
                }
            }
        }


        /// <summary>
        /// 创建右侧Case详情区域。
        /// </summary>
        /// <summary>
        /// 创建右侧Case详情区域。
        /// </summary>
        private Panel CreateCaseDetailPanel()
        {
            Panel panel =
            new Panel();

// 右侧Panel填充整个区域。
panel.Dock =
    DockStyle.Fill;

            // 使用统一主题背景。
            panel.BackColor =
                UITheme.BgBase;

            // 使用统一间距。
            panel.Padding =
                new Padding(
                    UITheme.ControlSpacing);

            // 创建Step Grid。
            _stepGrid =
                CreateStepGrid();

            // 创建Grid列。
            BuildColumns();

            // 先添加Fill控件。
            //
            // WinForms Dock布局建议先添加Fill，
            // 再添加Top等边缘Dock控件，
            // 以确保Grid正确填充剩余区域。
            panel.Controls.Add(
                _stepGrid);

            // 创建Case标题区域。
            Panel titlePanel =
                new Panel();

            // 标题区域固定高度。
            titlePanel.Dock =
                DockStyle.Top;

            titlePanel.Height =
                UITheme.ControlHeight + 8;

            // 创建Case标题Label。
            _caseTitle =
                UILabel.Title(
                    "请选择一个测试用例");

            // 标题填充区域。
            _caseTitle.Dock =
                DockStyle.Fill;

            // 标题左对齐。
            _caseTitle.TextAlign =
                ContentAlignment.MiddleLeft;

            // 添加Case标题。
            titlePanel.Controls.Add(
                _caseTitle);

            // 最后添加顶部标题区域。
            panel.Controls.Add(
                titlePanel);

            return panel;

}


        /// <summary>
        /// 创建Step DataGridView。
        ///
        /// DataGridView基础视觉全部交给UIGrid处理。
        /// </summary>
        private DataGridView CreateStepGrid()
        {
            DataGridView grid =
                new DataGridView();

            // Grid填充Case详情区域剩余空间。
            grid.Dock =
                DockStyle.Fill;

            // 测试流程Grid只用于显示。
            grid.ReadOnly =
                true;

            // 禁止用户新增行。
            grid.AllowUserToAddRows =
                false;

            // 禁止用户删除行。
            grid.AllowUserToDeleteRows =
                false;

            // 禁止用户调整行高。
            grid.AllowUserToResizeRows =
                false;

            // 隐藏行头。
            grid.RowHeadersVisible =
                false;

            // 禁止自动生成列。
            grid.AutoGenerateColumns =
                false;

            // 禁止多行选择。
            grid.MultiSelect =
                false;

            // 使用整行选择。
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            // 使用GT.Factory.UI统一Grid样式。
            UIGrid.BuildDataGrid(
                grid);

            return grid;
        }


        /// <summary>
        /// 创建Step Grid列。
        ///
        /// 注意：
        ///
        /// UIGrid.BuildDataGrid已经默认使用Fill模式。
        /// 因此这里通过FillWeight控制各列相对比例，
        /// 不再同时设置Width和Fill产生冲突。
        /// </summary>
        private void BuildColumns()
        {
            // 清空旧列，避免重复创建。
            _stepGrid.Columns.Clear();

            // 序号列保持较小宽度。
            AddColumn(
                "Index",
                "序号",
                35);

            // Step名称占主要空间。
            AddColumn(
                "Name",
                "步骤名称",
                160);

            // Action列保持中等宽度。
            AddColumn(
                "Action",
                "动作",
                80);

            // 参数列占主要空间。
            AddColumn(
                "Parameter",
                "参数",
                140);

            // 状态列保持固定比例。
            AddColumn(
                "Status",
                "状态",
                70);

            // 结果列保持较小宽度。
            AddColumn(
                "Result",
                "结果",
                50);

            // 耗时列保持较小宽度。
            AddColumn(
                "Duration",
                "耗时",
                70);
        }


        /// <summary>
        /// 添加一个Step Grid文本列。
        /// </summary>
        private void AddColumn(
            string name,
            string header,
            int fillWeight)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            // 设置内部列名称。
            column.Name =
                name;

            // 设置显示标题。
            column.HeaderText =
                header;

            // 使用FillWeight控制列比例。
            column.FillWeight =
                fillWeight;

            // 禁止用户修改列宽。
            column.Resizable =
                DataGridViewTriState.False;

            column.AutoSizeMode =
    DataGridViewAutoSizeColumnMode.Fill;

            // 添加到Step Grid。
            _stepGrid.Columns.Add(
                column);

        }

        #endregion


        #region 数据加载

        /// <summary>
        /// 加载LIN测试工程。
        ///
        /// 该方法只负责：
        ///
        /// Project
        ///     ↓
        /// Case
        ///     ↓
        /// UI
        ///
        /// 不负责Runtime初始化。
        /// </summary>
        public void LoadProject(
            LinTestProject project)
        {
            // 保存当前测试工程。
            _project =
                project;

            // 清空当前Case集合。
            _cases.Clear();

            // 清空Case列表。
            if (_caseList != null)
            {
                UIListBox.Clear(
                    _caseList);
            }

            // 清空Step显示。
            if (_stepGrid != null)
            {
                _stepGrid.Rows.Clear();
            }

            // 清空历史Case结果。
            _caseResults.Clear();

            // 清空当前运行状态。
            _currentRunningCase =
                null;

            _currentRunningStep =
                null;

            // 没有工程时恢复默认UI。
            if (project == null)
            {
                _projectLabel.Text =
                    "No Test Project";

                _caseTitle.Text =
                    "请选择一个测试用例";

                UpdateStatistic();
                UpdateButtonStates();

                return;
            }

            // 显示工程名称。
            _projectLabel.Text =
                string.IsNullOrEmpty(project.Name)
                    ? "Unnamed Test Project"
                    : project.Name;

            // 加载Case。
            if (project.Cases != null)
            {
                // 将工程Case复制到当前UI集合。
                _cases.AddRange(
                    project.Cases);

                // ListBox使用索引作为内部项，
                // 具体名称和状态由DrawItem负责绘制。
                for (int i = 0;
                     i < _cases.Count;
                     i++)
                {
                    _caseList.Items.Add(i);
                }
            }

            // 如果存在Case，则默认选择第一个。
            if (_cases.Count > 0)
            {
                _caseList.SelectedIndex = 0;
            }
            else
            {
                _caseTitle.Text =
                    "当前工程没有测试用例";
            }

            // 更新统计信息。
            UpdateStatistic();

            // 更新按钮状态。
            UpdateButtonStates();
        }

        #endregion


        #region Step加载

        /// <summary>
        /// Case选择变化事件。
        /// </summary>
        private void CaseList_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            // 获取当前选中索引。
            int index =
                _caseList.SelectedIndex;

            // 没有选中Case时恢复默认显示。
            if (index < 0 ||
                index >= _cases.Count)
            {
                _caseTitle.Text =
                    "请选择一个测试用例";

                _stepGrid.Rows.Clear();

                UpdateButtonStates();

                return;
            }

            // 获取当前Case。
            LinTestCase testCase =
                _cases[index];

            // 创建Case标题。
            string title =
                testCase.Name;

            // 如果存在描述，则一起显示。
            if (!string.IsNullOrEmpty(
                    testCase.Description))
            {
                title +=
                    " - " +
                    testCase.Description;
            }

            // 更新Case标题。
            _caseTitle.Text =
                title;

            // 加载Step。
            LoadSteps(
                testCase,
                index);

            // 更新按钮状态。
            UpdateButtonStates();
        }


        /// <summary>
        /// 加载指定Case的Step。
        /// </summary>
        private void LoadSteps(
            LinTestCase testCase,
            int caseIndex)
        {
            // 清空旧Step。
            _stepGrid.Rows.Clear();

            // 没有Case或Step时直接返回。
            if (testCase == null ||
                testCase.Steps == null)
            {
                return;
            }

            // UI序号从1开始显示。
            int stepIndex = 1;

            // 遍历当前Case所有Step。
            foreach (LinTestStep step
                     in testCase.Steps)
            {
                // 创建Grid行数据。
                object[] rowData =
                    CreateStepRowData(
                        step,
                        caseIndex,
                        stepIndex);

                // 添加Grid行。
                _stepGrid.Rows.Add(
                    rowData);

                // 增加显示序号。
                stepIndex++;
            }

            // 根据Step执行状态更新颜色。
            UpdateStepColors(
                caseIndex);
        }


        /// <summary>
        /// 创建Step Grid行数据。
        /// </summary>
        private object[] CreateStepRowData(
            LinTestStep step,
            int caseIndex,
            int index)
        {
            // 默认状态为待执行。
            string status =
                "待执行";

            // 默认结果为空。
            string result =
                string.Empty;

            // 默认耗时为空。
            string duration =
                string.Empty;

            // 如果Case已经有执行结果，则读取对应Step结果。
            CaseResult caseResult;

            if (_caseResults.TryGetValue(
                    caseIndex,
                    out caseResult))
            {
                // 根据StepNo查找执行结果。
                StepResult stepResult;

                if (caseResult.StepResults.TryGetValue(
                        step.StepNo,
                        out stepResult))
                {
                    // 转换状态文本。
                    status =
                        GetStatusText(
                            stepResult.Status);

                    // 转换结果文本。
                    result =
                        GetResultText(
                            stepResult.Status);

                    // 如果有耗时则显示毫秒。
                    if (stepResult.Duration.HasValue)
                    {
                        duration =
                            stepResult.Duration.Value +
                            "ms";
                    }
                }
            }

            // 返回Grid行数据。
            return new object[]
            {
                index,
                step.Name,
                step.Action,
                GetStepParameter(step),
                status,
                result,
                duration
            };
        }


        /// <summary>
        /// 获取Step参数显示文本。
        ///
        /// 当前阶段暂不假设LinTestStep内部字段结构，
        /// 因此保持空字符串。
        ///
        /// 后续确认LinTestStep实际Action参数模型后，
        /// 再在这里做UI格式化。
        /// </summary>
        private string GetStepParameter(
            LinTestStep step)
        {
            // 当前阶段不直接猜测Step参数模型。
            return string.Empty;
        }


        /// <summary>
        /// 将测试状态转换成中文显示文本。
        /// </summary>
        private string GetStatusText(
            TestStatus status)
        {
            switch (status)
            {
                case TestStatus.Idle:

                    return "待执行";

                case TestStatus.Running:

                    return "执行中";

                case TestStatus.Success:

                    return "通过";

                case TestStatus.Failed:

                    return "失败";

                default:

                    return "待执行";
            }
        }


        /// <summary>
        /// 将测试状态转换成结果符号。
        /// </summary>
        private string GetResultText(
            TestStatus status)
        {
            switch (status)
            {
                case TestStatus.Success:

                    return "✓";

                case TestStatus.Failed:

                    return "✗";

                case TestStatus.Running:

                    return "...";

                default:

                    return string.Empty;
            }
        }


        /// <summary>
        /// 更新Step行背景颜色。
        ///
        /// 状态颜色统一来自UITheme。
        /// </summary>
        private void UpdateStepColors(
            int caseIndex)
        {
            // 没有Case结果时不需要更新。
            CaseResult caseResult;

            if (!_caseResults.TryGetValue(
                    caseIndex,
                    out caseResult))
            {
                return;
            }

            // 防止Case索引越界。
            if (caseIndex < 0 ||
                caseIndex >= _cases.Count)
            {
                return;
            }

            // 获取当前Case。
            LinTestCase testCase =
                _cases[caseIndex];

            // 没有Step时直接返回。
            if (testCase.Steps == null)
            {
                return;
            }

            // 遍历Step。
            for (int i = 0;
                 i < testCase.Steps.Count &&
                 i < _stepGrid.Rows.Count;
                 i++)
            {
                // 获取当前Step。
                LinTestStep step =
                    testCase.Steps[i];

                // 查找Step执行结果。
                StepResult stepResult;

                if (!caseResult.StepResults.TryGetValue(
                        step.StepNo,
                        out stepResult))
                {
                    continue;
                }

                // 获取Grid行。
                DataGridViewRow row =
                    _stepGrid.Rows[i];

                // 使用主题颜色更新Step状态。
                row.DefaultCellStyle.BackColor =
                    GetStepColor(stepResult);

                // 根据状态使用对应文本颜色。
                row.DefaultCellStyle.ForeColor =
                    GetStepTextColor(stepResult);
            }
        }


        /// <summary>
        /// 获取Step状态背景颜色。
        /// </summary>
        private Color GetStepColor(
            StepResult stepResult)
        {
            // 当前正在执行的Step使用主题输入背景，
            // 避免与成功/失败颜色混淆。
            if (_currentRunningStep.HasValue &&
                _currentRunningStep.Value ==
                stepResult.StepNo)
            {
                return UITheme.BgInput;
            }

            // 根据Step结果使用统一语义色。
            switch (stepResult.Status)
            {
                case TestStatus.Success:

                    return UITheme.BgCard;

                case TestStatus.Failed:

                    return UITheme.BgCard;

                case TestStatus.Running:

                    return UITheme.BgInput;

                default:

                    return UITheme.BgBase;
            }
        }


        /// <summary>
        /// 获取Step状态文字颜色。
        /// </summary>
        private Color GetStepTextColor(
            StepResult stepResult)
        {
            // Running使用主操作色。
            if (_currentRunningStep.HasValue &&
                _currentRunningStep.Value ==
                stepResult.StepNo)
            {
                return UITheme.Primary;
            }

            // Success使用统一成功色。
            switch (stepResult.Status)
            {
                case TestStatus.Success:

                    return UITheme.Success;

                case TestStatus.Failed:

                    return UITheme.Danger;

                case TestStatus.Running:

                    return UITheme.Primary;

                default:

                    return UITheme.TextMain;
            }
        }

        #endregion


        #region 按钮事件

        /// <summary>
        /// 运行当前选中的Case。
        /// </summary>
        private async void RunCaseButton_Click(
            object sender,
            EventArgs e)
        {
            // 获取当前Case索引。
            int index =
                _caseList.SelectedIndex;

            // 没有选择Case时提示用户。
            if (index < 0 ||
                index >= _cases.Count)
            {
                MessageBox.Show(
                    "请先选择一个测试用例",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // 获取当前Case。
            LinTestCase testCase =
                _cases[index];

            // Disabled Case禁止执行。
            if (!testCase.Enabled)
            {
                MessageBox.Show(
                    "该测试用例已禁用",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // 执行指定Case。
            await RunCase(
                testCase,
                index);
        }


        /// <summary>
        /// 运行全部启用Case。
        /// </summary>
        private async void RunAllButton_Click(
            object sender,
            EventArgs e)
        {
            // 没有Case时提示用户。
            if (_cases.Count == 0)
            {
                MessageBox.Show(
                    "没有可执行的测试用例",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            // 执行全部启用Case。
            await RunAll();
        }


        /// <summary>
        /// 暂停当前测试。
        ///
        /// 当前阶段暂不真正暂停Step执行，
        /// 只保留UI入口。
        ///
        /// 后续接入真实测试执行器后，
        /// 再增加可取消/可恢复的执行控制。
        /// </summary>
        private void PauseButton_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "暂停功能将在接入正式测试执行器后实现。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }


        /// <summary>
        /// 停止当前测试。
        ///
        /// CancellationToken会在当前异步Step完成后立即终止后续执行。
        /// </summary>
        private void StopButton_Click(
            object sender,
            EventArgs e)
        {
            // 没有正在执行的测试时不处理。
            if (_cancellationTokenSource == null)
            {
                return;
            }

            // 请求取消当前测试。
            _cancellationTokenSource.Cancel();
        }

        #endregion


        #region 测试执行

        /// <summary>
        /// 执行指定Case。
        /// </summary>
        private async Task RunCase(
            LinTestCase testCase,
            int caseIndex)
        {
            await ExecuteTest(
                async delegate (CancellationToken token)
                {
                    // 设置当前执行Case。
                    _currentRunningCase =
                        caseIndex;

                    // 执行Case。
                    await ExecuteCase(
                        testCase,
                        caseIndex,
                        token);

                    // Case执行完成后清除运行Case。
                    _currentRunningCase =
                        null;

                    // 单Case完成后进度为100%。
                    _progressBar.Value =
                        100;
                });
        }


        /// <summary>
        /// 执行全部启用Case。
        /// </summary>
        private async Task RunAll()
        {
            await ExecuteTest(
                async delegate (CancellationToken token)
                {
                    // 获取所有启用Case。
                    List<LinTestCase> enabledCases =
                        _cases
                            .Where(c => c.Enabled)
                            .ToList();

                    // 没有启用Case时直接返回。
                    if (enabledCases.Count == 0)
                    {
                        return;
                    }

                    // 统计总Case数量。
                    int totalCases =
                        enabledCases.Count;

                    // 已完成Case数量。
                    int completedCases = 0;

                    // 遍历工程中的Case。
                    for (int i = 0;
                         i < _cases.Count;
                         i++)
                    {
                        // 获取当前Case。
                        LinTestCase testCase =
                            _cases[i];

                        // Disabled Case跳过。
                        if (!testCase.Enabled)
                        {
                            continue;
                        }

                        // 检查用户是否请求停止。
                        token.ThrowIfCancellationRequested();

                        // 设置当前执行Case。
                        _currentRunningCase =
                            i;

                        // 执行Case。
                        await ExecuteCase(
                            testCase,
                            i,
                            token);

                        // 当前Case执行完成。
                        _currentRunningCase =
                            null;

                        // 增加完成数量。
                        completedCases++;

                        // 更新总体进度。
                        int progress =
                            (int)Math.Round(
                                (double)completedCases /
                                totalCases *
                                100);

                        // 限制进度范围。
                        progress =
                            Math.Max(
                                0,
                                Math.Min(
                                    100,
                                    progress));

                        // 更新进度条。
                        _progressBar.Value =
                            progress;

                        // 更新统计。
                        UpdateStatistic();
                    }
                });
        }


        /// <summary>
        /// 执行一次测试任务。
        ///
        /// 这里负责：
        ///
        /// 1. 设置运行状态。
        /// 2. 创建CancellationTokenSource。
        /// 3. 执行测试。
        /// 4. 处理用户取消。
        /// 5. 恢复UI状态。
        /// </summary>
        private async Task ExecuteTest(
            Func<CancellationToken, Task> testAction)
        {
            // 标记正在执行。
            _isRunning = true;

            // 更新按钮。
            UpdateButtonStates();

            // 重置进度。
            _progressBar.Value = 0;

            // 创建当前测试的取消令牌。
            using (CancellationTokenSource cts =
                   new CancellationTokenSource())
            {
                // 保存CancellationTokenSource。
                _cancellationTokenSource =
                    cts;

                try
                {
                    // 执行实际测试。
                    await testAction(
                        cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // 用户点击停止时会进入这里。
                    _currentRunningCase =
                        null;

                    _currentRunningStep =
                        null;
                }
                finally
                {
                    // 恢复非运行状态。
                    _isRunning =
                        false;

                    // 清理当前运行Case。
                    _currentRunningCase =
                        null;

                    // 清理当前运行Step。
                    _currentRunningStep =
                        null;

                    // 清空取消令牌引用。
                    _cancellationTokenSource =
                        null;

                    // 刷新UI。
                    UpdateButtonStates();

                    // 刷新Case列表状态。
                    _caseList.Invalidate();

                    // 刷新当前Step显示。
                    RefreshCurrentCaseDisplay();
                }
            }
        }


        /// <summary>
        /// 执行一个Case。
        /// </summary>
        private async Task ExecuteCase(
            LinTestCase testCase,
            int caseIndex,
            CancellationToken cancellationToken)
        {
            // 初始化Case结果。
            CaseResult caseResult =
                InitializeCaseResult(
                    caseIndex);

            try
            {
                // 如果Case存在Step，则依次执行。
                if (testCase.Steps != null)
                {
                    foreach (LinTestStep step
                             in testCase.Steps)
                    {
                        // Disabled Step直接跳过。
                        if (!step.Enabled)
                        {
                            continue;
                        }

                        // 检查取消状态。
                        cancellationToken
                            .ThrowIfCancellationRequested();

                        // 设置当前执行Step。
                        _currentRunningStep =
                            step.StepNo;

                        // 刷新当前Step显示。
                        RefreshStepDisplay(
                            caseIndex,
                            testCase);

                        // 异步执行Step。
                        await ExecuteSingleStep(
                            step,
                            caseIndex,
                            testCase,
                            caseResult,
                            cancellationToken);

                        // Step执行完成后清除当前Step。
                        _currentRunningStep =
                            null;
                    }
                }

                // 完成Case结果。
                CompleteCaseResult(
                    caseResult);

                // 保存Case结果。
                _caseResults[caseIndex] =
                    caseResult;

                // 更新Case列表。
                UpdateCaseListItem(
                    caseIndex);

                // 更新统计。
                UpdateStatistic();
            }
            catch (OperationCanceledException)
            {
                // Stop操作时将当前Case保持为失败/中断前状态，
                // 不强制伪造一个Success结果。
                _currentRunningStep =
                    null;

                // 将当前结果保存下来，
                // 便于UI保留已经完成的Step状态。
                _caseResults[caseIndex] =
                    caseResult;

                // 刷新UI。
                UpdateCaseListItem(
                    caseIndex);

                UpdateStatistic();

                // 继续向上抛出取消异常，
                // 由ExecuteTest统一处理。
                throw;
            }
        }


        /// <summary>
        /// 初始化Case执行结果。
        /// </summary>
        private CaseResult InitializeCaseResult(
            int caseIndex)
        {
            CaseResult caseResult =
                new CaseResult();

            // Case初始状态为Running。
            caseResult.Status =
                TestStatus.Running;

            // 记录开始时间。
            caseResult.StartTime =
                DateTime.Now;

            // 初始化通过Step数量。
            caseResult.PassedSteps =
                0;

            // 初始化失败Step数量。
            caseResult.FailedSteps =
                0;

            // 初始化Step结果字典。
            caseResult.StepResults =
                new Dictionary<int, StepResult>();

            // 保存到当前Case结果集合。
            _caseResults[caseIndex] =
                caseResult;

            // 刷新Case列表。
            UpdateCaseListItem(
                caseIndex);

            // 刷新统计。
            UpdateStatistic();

            return caseResult;
        }


        /// <summary>
        /// 异步执行单个Step。
        ///
        /// 当前阶段仍然是模拟执行。
        ///
        /// 与旧版本最大的区别：
        ///
        /// 不再使用Thread.Sleep。
        ///
        /// 使用Task.Delay后WinForms UI线程不会被阻塞，
        /// 因此Running状态、进度、Stop按钮都可以正常响应。
        /// </summary>
        private async Task ExecuteSingleStep(
            LinTestStep step,
            int caseIndex,
            LinTestCase testCase,
            CaseResult caseResult,
            CancellationToken cancellationToken)
        {
            // 创建Step执行结果。
            StepResult stepResult =
                new StepResult();

            // 设置Step为Running。
            stepResult.Status =
                TestStatus.Running;

            // 记录Step开始时间。
            stepResult.StartTime =
                DateTime.Now;

            // 保存Step编号。
            stepResult.StepNo =
                step.StepNo;

            // 保存当前Step结果。
            caseResult.StepResults[
                step.StepNo] =
                stepResult;

            // 刷新Step Running状态。
            RefreshStepDisplay(
                caseIndex,
                testCase);

            // 模拟Step执行时间。
            Random random =
                new Random();

            int executionTime =
                random.Next(
                    100,
                    500);

            // 使用异步Delay替代Thread.Sleep。
            //
            // 这样不会阻塞WinForms UI线程。
            await Task.Delay(
                executionTime,
                cancellationToken);

            // 记录Step结束时间。
            DateTime endTime =
                DateTime.Now;

            stepResult.EndTime =
                endTime;

            // 计算Step耗时。
            if (stepResult.StartTime.HasValue)
            {
                stepResult.Duration =
                    (int)(
                        endTime -
                        stepResult.StartTime.Value)
                        .TotalMilliseconds;
            }

            // 当前阶段模拟85%成功率。
            bool isSuccess =
                random.NextDouble() > 0.15;

            // 设置最终Step状态。
            stepResult.Status =
                isSuccess
                    ? TestStatus.Success
                    : TestStatus.Failed;

            // 保存Step最终结果。
            caseResult.StepResults[
                step.StepNo] =
                stepResult;

            // 更新Case统计。
            if (stepResult.Status ==
                TestStatus.Success)
            {
                caseResult.PassedSteps++;
            }
            else
            {
                caseResult.FailedSteps++;
            }

            // 刷新Step最终状态。
            RefreshStepDisplay(
                caseIndex,
                testCase);
        }


        /// <summary>
        /// 刷新当前选中Case的Step显示。
        /// </summary>
        private void RefreshStepDisplay(
            int caseIndex,
            LinTestCase testCase)
        {
            // 只有当前选中Case才刷新右侧Step Grid。
            if (_caseList.SelectedIndex ==
                caseIndex)
            {
                LoadSteps(
                    testCase,
                    caseIndex);

                // 强制刷新Grid。
                _stepGrid.Invalidate();

                // 强制刷新Case列表状态。
                _caseList.Invalidate();
            }
        }


        /// <summary>
        /// 刷新当前选中Case。
        ///
        /// 用于测试完成、停止等状态切换后，
        /// 确保当前UI与结果集合保持一致。
        /// </summary>
        private void RefreshCurrentCaseDisplay()
        {
            int index =
                _caseList.SelectedIndex;

            if (index < 0 ||
                index >= _cases.Count)
            {
                return;
            }

            LoadSteps(
                _cases[index],
                index);

            _caseList.Invalidate();
            _stepGrid.Invalidate();
        }


        /// <summary>
        /// 完成Case结果。
        /// </summary>
        private void CompleteCaseResult(
            CaseResult caseResult)
        {
            // 记录Case结束时间。
            caseResult.EndTime =
                DateTime.Now;

            // 计算Case总耗时。
            if (caseResult.StartTime.HasValue &&
                caseResult.EndTime.HasValue)
            {
                caseResult.Duration =
                    (int)(
                        caseResult.EndTime.Value -
                        caseResult.StartTime.Value)
                        .TotalMilliseconds;
            }

            // 只要存在失败Step，Case就是Failed。
            caseResult.Status =
                caseResult.FailedSteps > 0
                    ? TestStatus.Failed
                    : TestStatus.Success;
        }

        #endregion


        #region UI更新

        /// <summary>
        /// 更新顶部操作按钮状态。
        ///
        /// 非运行状态：
        ///
        /// RunCase：
        ///     有选中Case即可
        ///
        /// RunAll：
        ///     有Case即可
        ///
        /// Pause / Stop：
        ///     禁用
        ///
        /// 运行状态：
        ///
        /// RunCase / RunAll：
        ///     禁用
        ///
        /// Pause / Stop：
        ///     启用
        /// </summary>
        private void UpdateButtonStates()
        {
            // 防止控件尚未初始化时访问。
            if (_runCaseButton == null)
            {
                return;
            }

            // 当前是否存在选中Case。
            bool hasSelectedCase =
                _caseList != null &&
                _caseList.SelectedIndex >= 0 &&
                _caseList.SelectedIndex < _cases.Count;

            // RunCase只有在非运行状态且存在Case时可用。
            _runCaseButton.Enabled =
                !_isRunning &&
                hasSelectedCase;

            // RunAll只有在非运行状态且存在Case时可用。
            _runAllButton.Enabled =
                !_isRunning &&
                _cases.Count > 0;

            // Pause只有运行期间可用。
            _pauseButton.Enabled =
                _isRunning;

            // Stop只有运行期间可用。
            _stopButton.Enabled =
                _isRunning;
        }


        /// <summary>
        /// 更新Case统计信息。
        /// </summary>
        private void UpdateStatistic()
        {
            // Pass数量。
            int pass = 0;

            // Fail数量。
            int fail = 0;

            // Pending初始为Case总数量。
            int pending =
                _cases.Count;

            // 遍历所有Case结果。
            foreach (CaseResult result
                     in _caseResults.Values)
            {
                // Success表示完成并通过。
                if (result.Status ==
                    TestStatus.Success)
                {
                    pass++;
                    pending--;
                }
                // Failed表示完成但失败。
                else if (result.Status ==
                         TestStatus.Failed)
                {
                    fail++;
                    pending--;
                }
            }

            // 防止Pending出现负数。
            pending =
                Math.Max(
                    0,
                    pending);

            // 更新Label。
            if (_statisticLabel != null)
            {
                _statisticLabel.Text =
                    string.Format(
                        "Pass: {0}   Fail: {1}   Pending: {2}",
                        pass,
                        fail,
                        pending);
            }
        }


        /// <summary>
        /// 更新Case列表显示。
        /// </summary>
        private void UpdateCaseListItem(
            int caseIndex)
        {
            // 刷新整个ListBox自绘区域。
            if (_caseList != null)
            {
                _caseList.Invalidate();
            }

            // 当前选中的Case才需要刷新右侧Step。
            if (_caseList != null &&
                _caseList.SelectedIndex ==
                caseIndex &&
                caseIndex >= 0 &&
                caseIndex < _cases.Count)
            {
                LoadSteps(
                    _cases[caseIndex],
                    caseIndex);
            }
        }

        #endregion


        #region ILinDebugSessionView实现

        /// <summary>
        /// 将当前测试面板绑定到DebugSession。
        ///
        /// Session
        ///     ↓
        /// Project
        ///     ↓
        /// TestProject
        ///     ↓
        /// LinTestStepPanel
        /// </summary>
        public void BindSession(
            LinDebugSession session)
        {
            // Session为空时清空当前UI。
            if (session == null)
            {
                ClearSession();
                return;
            }

            // Session存在时加载对应测试工程。
            if (session.Project == null)
            {
                ClearSession();
                return;
            }

            // 加载测试工程。
            LoadProject(
                session.Project.TestProject);
        }


        /// <summary>
        /// 解除Session绑定。
        ///
        /// 当前UI没有持有Session事件订阅，
        /// 因此不需要额外解绑操作。
        /// </summary>
        public void UnbindSession(
            LinDebugSession session)
        {
            // 当前阶段不需要执行解绑逻辑。
        }


        /// <summary>
        /// 清空当前Session和测试工程。
        /// </summary>
        public void ClearSession()
        {
            // 如果当前仍在执行测试，
            // 先请求停止。
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }

            // 清空工程。
            LoadProject(
                null);

            // 恢复Case标题。
            _caseTitle.Text =
                "请选择一个测试用例";

            // 清空进度。
            _progressBar.Value =
                0;

            // 清空统计。
            _statisticLabel.Text =
                "Pass: 0   Fail: 0   Pending: 0";

            // 更新按钮。
            UpdateButtonStates();
        }

        #endregion
    }


    #region 辅助类型

    /// <summary>
    /// 测试状态枚举。
    ///
    /// 用于描述Case和Step当前执行状态。
    /// </summary>
    public enum TestStatus
    {
        /// <summary>
        /// 尚未执行。
        /// </summary>
        Idle,

        /// <summary>
        /// 正在执行。
        /// </summary>
        Running,

        /// <summary>
        /// 执行成功。
        /// </summary>
        Success,

        /// <summary>
        /// 执行失败。
        /// </summary>
        Failed
    }


    /// <summary>
    /// Case执行结果。
    ///
    /// 保存一个测试Case整体执行过程中的统计信息。
    /// </summary>
    public class CaseResult
    {
        /// <summary>
        /// Case最终状态。
        /// </summary>
        public TestStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Case开始执行时间。
        /// </summary>
        public DateTime? StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Case结束执行时间。
        /// </summary>
        public DateTime? EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Case总耗时，单位毫秒。
        /// </summary>
        public int? Duration
        {
            get;
            set;
        }

        /// <summary>
        /// 成功Step数量。
        /// </summary>
        public int PassedSteps
        {
            get;
            set;
        }

        /// <summary>
        /// 失败Step数量。
        /// </summary>
        public int FailedSteps
        {
            get;
            set;
        }

        /// <summary>
        /// 当前Case中每个Step的执行结果。
        ///
        /// Key为StepNo。
        /// </summary>
        public Dictionary<int, StepResult> StepResults
        {
            get;
            set;
        }


        /// <summary>
        /// 初始化Case执行结果。
        /// </summary>
        public CaseResult()
        {
            StepResults =
                new Dictionary<int, StepResult>();
        }
    }


    /// <summary>
    /// Step执行结果。
    ///
    /// 保存单个Step的执行状态、时间和耗时。
    /// </summary>
    public class StepResult
    {
        /// <summary>
        /// Step编号。
        /// </summary>
        public int StepNo
        {
            get;
            set;
        }

        /// <summary>
        /// Step执行状态。
        /// </summary>
        public TestStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Step开始时间。
        /// </summary>
        public DateTime? StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Step结束时间。
        /// </summary>
        public DateTime? EndTime
        {
            get;
            set;
        }

        /// <summary>
        /// Step执行耗时，单位毫秒。
        /// </summary>
        public int? Duration
        {
            get;
            set;
        }

        /// <summary>
        /// 实际值。
        ///
        /// 当前阶段保留字段，
        /// 后续接入实际Signal测试时使用。
        /// </summary>
        public int? ActualValue
        {
            get;
            set;
        }

        /// <summary>
        /// 期望值。
        ///
        /// 当前阶段保留字段，
        /// 后续接入实际Signal测试时使用。
        /// </summary>
        public int? ExpectedValue
        {
            get;
            set;
        }
    }

    #endregion
}