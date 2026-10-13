using GT.Factory.Common.Models;
using GT.Factory.GTUser.Repository;
using GT.Factory.UI.Apply;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GT.Factory.Management.UI.GTUser
{
    /// <summary>
    /// 用户管理独立窗口。
    ///
    /// 本窗体只负责窗口宿主功能，
    /// 用户管理业务由 UserManageControl 实现。
    /// </summary>
    public partial class UserManageForm : Form
    {
        private readonly UserManageControl _control;

        /// <summary>
        /// 创建用户管理窗口。
        /// </summary>
        public UserManageForm(
            LocalUserService localUserService)
        {
            Text = "用户管理";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            _control =
                new UserManageControl(
                    localUserService);

            _control.Dock = DockStyle.Fill;

            Controls.Add(_control);

            UIStyle.ApplyWidget(this);
        }
    }
}

