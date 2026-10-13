using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Services;
using GT.Factory.ProductCode.UI.Factories;
using System;
using System.Windows.Forms;


namespace GT.Factory.ProductCode.UI.Controls
{

    /// <summary>
    /// 产品码段编辑器。
    ///
    /// 根据SegmentType动态显示不同配置界面。
    ///
    /// 例如：
    ///
    /// FixedText
    ///     ->
    /// FixedTextConfigControl
    ///
    /// Date
    ///     ->
    /// DateConfigControl
    ///
    /// SerialNumber
    ///     ->
    /// SerialConfigControl
    ///
    /// 注意：
    ///
    /// 本控件不直接操作数据库。
    ///
    /// 最终只修改：
    ///
    /// CodeRuleDetail.ConfigJson
    ///
    /// </summary>
    public class CodeSegmentEditorControl : UserControl
    {


        private Panel _contentPanel;


        private Label _titleLabel;


        private CodeRuleDetail _detail;


        private CodeConfigSerializer _serializer;



        /// <summary>
        /// 当前段修改事件。
        ///
        /// 用于通知预览刷新。
        /// </summary>
        public event EventHandler SegmentChanged;



        public CodeSegmentEditorControl()
        {

            _serializer =
                new CodeConfigSerializer();


            InitializeUI();

        }




        private void InitializeUI()
        {

            Dock =
                DockStyle.Fill;


            _titleLabel =
                new Label();


            _titleLabel.Dock =
                DockStyle.Top;


            _titleLabel.Height =
                40;


            _titleLabel.Font =
                new System.Drawing.Font(
                    "Microsoft YaHei",
                    12);



            _contentPanel =
                new Panel();


            _contentPanel.Dock =
                DockStyle.Fill;



            Controls.Add(
                _contentPanel);


            Controls.Add(
                _titleLabel);

        }





        /// <summary>
        /// 加载当前段。
        ///
        /// 当左侧列表选择改变时调用。
        /// </summary>
        public void BindSegment(
            CodeRuleDetail detail)
        {

            _detail =
                detail;


            RefreshEditor();

        }




        /// <summary>
        /// 根据SegmentType切换编辑器。
        /// </summary>
        private void RefreshEditor()
        {

            _contentPanel.Controls.Clear();



            if (_detail == null)
            {
                return;
            }



            _titleLabel.Text =
                GetTitle(
                    _detail.SegmentType);


            Control editor =
    SegmentConfigFactory.Create(
        _detail.SegmentType);

         

            if (editor == null)
            {
                return;
            }



            editor.Dock =
                DockStyle.Fill;



            _contentPanel.Controls.Add(
                editor);


            BindEditorEvent(
                editor);

        }





        /// <summary>
        /// 绑定配置变化事件。
        ///
        /// 配置控件修改后：
        ///
        /// Config对象
        ///       |
        ///       |
        ///       ▼
        /// ConfigJson
        ///
        /// </summary>
        private void BindEditorEvent(
            Control editor)
        {


            ICodeConfigEditor configEditor =
                editor as ICodeConfigEditor;



            if (configEditor == null)
            {
                return;
            }



            configEditor.ConfigChanged +=
                ConfigEditor_ConfigChanged;



            configEditor.LoadConfig(
                _detail.ConfigJson);

        }




        private void ConfigEditor_ConfigChanged(
            object sender,
            object config)
        {


            _detail.ConfigJson =
                _serializer.Serialize(
                    config);



            if (SegmentChanged != null)
            {
                SegmentChanged(
                    this,
                    EventArgs.Empty);
            }

        }





        private string GetTitle(
            CodeSegmentType type)
        {

            switch (type)
            {

                case CodeSegmentType.FixedText:

                    return "固定文本";


                case CodeSegmentType.Date:

                    return "日期配置";


                case CodeSegmentType.SerialNumber:

                    return "流水号配置";




                case CodeSegmentType.Shift:

                    return "班次";




                default:

                    return "";

            }

        }




        private Control CreateInfoControl(
            string text)
        {

            Label label =
                new Label();


            label.Text =
                text;


            label.Dock =
                DockStyle.Fill;


            label.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;


            return label;

        }


    }

}
