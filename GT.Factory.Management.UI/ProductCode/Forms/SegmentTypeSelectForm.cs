using System;
using System.Windows.Forms;
using GT.Factory.Common.Enums.ProductCodeEnums;


namespace GT.Factory.Management.UI.ProductCode.Forms
{

    /// <summary>
    /// 产品码段类型选择窗口。
    ///
    /// 新增Segment时使用。
    /// </summary>
    public class SegmentTypeSelectForm : Form
    {

        private ComboBox _combo;

        private Button _okButton;

        /// <summary>
        /// 用户选择结果。
        /// </summary>
        public CodeSegmentType SelectedType
        {
            get;
            private set;
        }


        public SegmentTypeSelectForm()
        {

            InitializeUI();
        }

        private void InitializeUI()
        {

            Text =
                "选择段类型";

            Width =
                350;

            Height =
                160;

            StartPosition = FormStartPosition.CenterScreen;

            _combo =
                new ComboBox();

            _combo.Dock =
                DockStyle.Top;

            _combo.DropDownStyle =
                ComboBoxStyle.DropDownList;

            foreach (CodeSegmentType type
                in Enum.GetValues(
                    typeof(CodeSegmentType)))
            {
                _combo.Items.Add(type);
            }

            _combo.SelectedIndex = 0;

            _okButton =
                new Button();

            _okButton.Text =
                "确定";

            _okButton.Dock =
                DockStyle.Bottom;

            _okButton.Click +=
                OkButton_Click;

            Controls.Add(
                _combo);

            Controls.Add(
                _okButton);

        }

        private void OkButton_Click(
            object sender,
            EventArgs e)
        {

            SelectedType =
                (CodeSegmentType)
                _combo.SelectedItem;


            DialogResult =
                DialogResult.OK;

        }
    }
}
