using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.ProductCode.Services;


namespace GT.Factory.Management.UI.ProductCode.Controls.ConfigEditors
{

    /// <summary>
    /// 产品码实时预览控件。
    ///
    /// 注意：
    ///
    /// 这里只是UI模拟。
    ///
    /// 不调用数据库流水。
    ///
    /// 默认模拟:
    ///
    /// Serial=1
    ///
    /// </summary>
    public class CodePreviewControl : UserControl
    {

        private Label _label;

        private CodePreviewGenerator _generator;

        private CodeRule _rule;

        public CodePreviewControl()
        {

            _generator =
                new CodePreviewGenerator();

            InitializeUI();

        }


        private void InitializeUI()
        {

            Height = 60;

            Dock = DockStyle.Bottom;

            _label = new Label();

            _label.Dock = DockStyle.Fill;

            _label.Text = "预览";

            _label.Font =
                new System.Drawing.Font(
                    "Microsoft YaHei",
                    16);

            _label.TextAlign =
                System.Drawing.ContentAlignment.MiddleCenter;

            Controls.Add(_label);

        }


        public void BindRule(CodeRule rule)
        {

            _rule = rule;

            RefreshPreview();

        }

        /// <summary>
        /// 刷新预览。
        /// </summary>
        public void RefreshPreview()
        {
            string code;
            string errorMessage;

            if (TryGeneratePreview(
                _rule,
                out code,
                out errorMessage))
            {
                _label.Text = code;
                return;
            }

            _label.Text =
                "暂无预览：" + errorMessage;
        }


        /// <summary>
        /// 尝试生成预览产品码。
        ///
        /// 该方法专门用于UI预览。
        ///
        /// 与正式产品码生成不同，
        /// 预览允许当前规则处于未完成配置状态。
        /// </summary>
        private bool TryGeneratePreview(
            CodeRule rule,
            out string code,
            out string errorMessage)
        {
            code = null;
            errorMessage = null;

            if (rule == null)
            {
                errorMessage = "尚未绑定规则。";
                return false;
            }

            if (rule.Details == null ||
                rule.Details.Count == 0)
            {
                errorMessage = "请先添加产品码Segment。";
                return false;
            }

            try
            {
                code =
                    _generator.GeneratePreview(
                        rule);

                if (string.IsNullOrEmpty(code))
                {
                    errorMessage = "当前规则无法生成产品码。";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                /*
                 * 这里把生成器异常转化成预览错误。
                 *
                 * 预览是辅助功能，
                 * 不应该因为规则编辑中的临时状态
                 * 导致整个编辑窗口崩溃。
                 */
                errorMessage = ex.Message;
                return false;
            }
        }

    }

}
