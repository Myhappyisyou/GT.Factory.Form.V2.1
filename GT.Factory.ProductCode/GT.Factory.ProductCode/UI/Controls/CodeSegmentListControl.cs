using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Services;
using GT.Factory.ProductCode.UI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace GT.Factory.ProductCode.UI.Controls
{
    /// <summary>
    /// 产品码段列表控件。
    ///
    /// 负责：
    ///
    /// 1. 显示当前规则中的所有 Segment
    /// 2. 新增 Segment
    /// 3. 删除 Segment
    /// 4. 调整 Segment 顺序
    /// 5. 通知外部当前选择的 Segment
    /// 6. 通知外部 Segment 集合发生变化
    ///
    /// 不负责：
    ///
    /// 1. Segment 具体配置编辑
    /// 2. JSON 保存
    /// 3. 数据库操作
    ///
    /// Segment 的具体配置由 CodeSegmentEditorControl 负责。
    /// </summary>
    public class CodeSegmentListControl : UserControl
    {
        private ListBox _segmentList;

        private Button _btnAdd;
        private Button _btnDelete;
        private Button _btnUp;
        private Button _btnDown;

        private CodeRule _rule;

        /// <summary>
        /// 当前 Segment 被选中时触发。
        /// </summary>
        public event EventHandler<CodeRuleDetail> SegmentSelected;

        /// <summary>
        /// Segment 集合发生变化时触发。
        ///
        /// 包括：
        ///
        /// 1. 新增
        /// 2. 删除
        /// 3. 上移
        /// 4. 下移
        /// </summary>
        public event EventHandler SegmentsChanged;

        /// <summary>
        /// 创建 Segment 列表控件。
        /// </summary>
        public CodeSegmentListControl()
        {
            InitializeUI();
        }

        /// <summary>
        /// 初始化界面。
        /// </summary>
        private void InitializeUI()
        {
            Dock = DockStyle.Fill;

            Panel bottomPanel = new Panel();
            bottomPanel.Height = 90;
            bottomPanel.Dock = DockStyle.Bottom;

            _segmentList = new ListBox();
            _segmentList.Dock = DockStyle.Fill;
            _segmentList.Font = new Font("Microsoft YaHei", 10);
            _segmentList.SelectedIndexChanged += SegmentList_SelectedIndexChanged;

            _btnAdd = CreateButton("新增");
            _btnDelete = CreateButton("删除");
            _btnUp = CreateButton("↑");
            _btnDown = CreateButton("↓");

            _btnAdd.Click += BtnAdd_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnUp.Click += BtnUp_Click;
            _btnDown.Click += BtnDown_Click;

            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.Padding = new Padding(5);

            flow.Controls.Add(_btnAdd);
            flow.Controls.Add(_btnDelete);
            flow.Controls.Add(_btnUp);
            flow.Controls.Add(_btnDown);

            bottomPanel.Controls.Add(flow);

            Controls.Add(_segmentList);
            Controls.Add(bottomPanel);
        }

        /// <summary>
        /// 创建列表操作按钮。
        /// </summary>
        private Button CreateButton(string text)
        {
            Button button = new Button();

            button.Text = text;
            button.Width = 70;
            button.Height = 30;

            return button;
        }

        /// <summary>
        /// 绑定产品码规则。
        /// </summary>
        public void BindRule(CodeRule rule)
        {
            _rule = rule;

            RefreshList();
        }

        /// <summary>
        /// 刷新 Segment 列表。
        ///
        /// 刷新时不主动改变业务数据。
        /// </summary>
        public void RefreshList()
        {
            int selectedIndex = _segmentList.SelectedIndex;

            _segmentList.Items.Clear();

            if (_rule == null ||
                _rule.Details == null)
            {
                return;
            }

            foreach (CodeRuleDetail detail in _rule.Details)
            {
                _segmentList.Items.Add(CreateDisplayText(detail));
            }

            if (_segmentList.Items.Count == 0)
            {
                return;
            }

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            if (selectedIndex >= _segmentList.Items.Count)
            {
                selectedIndex = _segmentList.Items.Count - 1;
            }

            _segmentList.SelectedIndex = selectedIndex;
        }

        /// <summary>
        /// 创建 Segment 的显示文本。
        /// </summary>
        private string CreateDisplayText(CodeRuleDetail detail)
        {
            return detail.SegmentOrder +
                   "  " +
                   GetSegmentName(detail.SegmentType);
        }

        /// <summary>
        /// 将 SegmentType 转换成用户看到的中文名称。
        ///
        /// 注意：
        ///
        /// 这里仅负责 UI 显示。
        /// 实际保存的仍然是 CodeSegmentType 枚举。
        /// </summary>
        private string GetSegmentName(CodeSegmentType type)
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
        /// Segment 选择发生变化。
        /// </summary>
        private void SegmentList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_segmentList.SelectedIndex < 0)
            {
                return;
            }

            if (_rule == null ||
                _rule.Details == null)
            {
                return;
            }

            if (_segmentList.SelectedIndex >= _rule.Details.Count)
            {
                return;
            }

            CodeRuleDetail detail =
                _rule.Details[_segmentList.SelectedIndex];

            if (SegmentSelected != null)
            {
                SegmentSelected(this, detail);
            }
        }

        /// <summary>
        /// 新增 Segment。
        /// </summary>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_rule == null)
            {
                return;
            }

            if (_rule.Details == null)
            {
                _rule.Details = new List<CodeRuleDetail>();
            }

            SegmentTypeSelectForm form =
                new SegmentTypeSelectForm();

            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            CodeRuleDetail detail = new CodeRuleDetail();

            detail.Enabled = true;
            detail.SegmentType = form.SelectedType;
            detail.SegmentOrder = _rule.Details.Count + 1;

            detail.ConfigJson =
                SegmentDefaultConfigFactory.Create(
                    detail.SegmentType);



            _rule.Details.Add(detail);

            RebuildOrder();

            RefreshList();

            /*
             * 新增后直接选中新 Segment，
             * 方便用户立即配置。
             */
            _segmentList.SelectedIndex =
                _rule.Details.Count - 1;

            OnSegmentsChanged();
        }

        /// <summary>
        /// 删除当前 Segment。
        /// </summary>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            int index = _segmentList.SelectedIndex;

            if (index < 0)
            {
                return;
            }

            if (_rule == null ||
                _rule.Details == null)
            {
                return;
            }

            if (index >= _rule.Details.Count)
            {
                return;
            }

            _rule.Details.RemoveAt(index);

            RebuildOrder();

            RefreshList();

            /*
             * 删除后尽量选择原位置。
             */
            if (_rule.Details.Count > 0)
            {
                if (index >= _rule.Details.Count)
                {
                    index = _rule.Details.Count - 1;
                }

                _segmentList.SelectedIndex = index;
            }

            OnSegmentsChanged();
        }

        /// <summary>
        /// 当前 Segment 上移。
        /// </summary>
        private void BtnUp_Click(object sender, EventArgs e)
        {
            int index = _segmentList.SelectedIndex;

            if (index <= 0)
            {
                return;
            }

            if (_rule == null ||
                _rule.Details == null)
            {
                return;
            }

            CodeRuleDetail temp =
                _rule.Details[index];

            _rule.Details[index] =
                _rule.Details[index - 1];

            _rule.Details[index - 1] =
                temp;

            RebuildOrder();

            RefreshList();

            _segmentList.SelectedIndex =
                index - 1;

            OnSegmentsChanged();
        }

        /// <summary>
        /// 当前 Segment 下移。
        /// </summary>
        private void BtnDown_Click(object sender, EventArgs e)
        {
            int index = _segmentList.SelectedIndex;

            if (index < 0)
            {
                return;
            }

            if (_rule == null ||
                _rule.Details == null)
            {
                return;
            }

            if (index >= _rule.Details.Count - 1)
            {
                return;
            }

            CodeRuleDetail temp =
                _rule.Details[index];

            _rule.Details[index] =
                _rule.Details[index + 1];

            _rule.Details[index + 1] =
                temp;

            RebuildOrder();

            RefreshList();

            _segmentList.SelectedIndex =
                index + 1;

            OnSegmentsChanged();
        }

        /// <summary>
        /// 重新建立 Segment 顺序。
        ///
        /// Sort 永远从 1 开始连续编号。
        /// </summary>
        private void RebuildOrder()
        {
            if (_rule == null ||
                _rule.Details == null)
            {
                return;
            }

            for (int i = 0; i < _rule.Details.Count; i++)
            {
                _rule.Details[i].SegmentOrder = i + 1;
            }
        }

        /// <summary>
        /// 通知外部 Segment 集合发生变化。
        /// </summary>
        private void OnSegmentsChanged()
        {
            if (SegmentsChanged != null)
            {
                SegmentsChanged(this, EventArgs.Empty);
            }
        }
    }
}