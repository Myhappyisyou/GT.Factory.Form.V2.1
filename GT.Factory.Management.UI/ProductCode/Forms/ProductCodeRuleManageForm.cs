using GT.Factory.Common.Enums.ProductCodeEnums;
using GT.Factory.Common.Models.ProductCode;
using GT.Factory.Common.Models.ProductModel;
using GT.Factory.GTUser.Repository;
using GT.Factory.Management.UI.Product;
using GT.Factory.Product.Service;
using GT.Factory.ProductCode.Service;
using GT.Factory.ProductCode.Services;
using GT.Factory.UI.Apply;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GT.Factory.Management.UI.ProductCode.Forms
{
    /// <summary>
    /// 用户管理独立窗口。
    ///
    /// 本窗体只负责窗口宿主功能，
    /// 用户管理业务由 UserManageControl 实现。
    /// </summary>
    public partial class ProductCodeRuleManageForm : Form
    {
        private readonly ProductCodeRuleManageControl _control;

        /// <summary>
        /// 创建用户管理窗口。
        /// </summary>
        public ProductCodeRuleManageForm(
             ProductModelService productModelService,
            ProductPartService productPartService,
            ProductCodeRuleService ruleService)
        {
            Text = "产品码配置管理";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            _control =
                new ProductCodeRuleManageControl(
                    productModelService, productPartService, ruleService);

            _control.Dock = DockStyle.Fill;

            Controls.Add(_control);

            //UIStyle.ApplyWidget(this);
        }
    }
}

