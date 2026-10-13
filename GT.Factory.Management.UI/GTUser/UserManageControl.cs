using GT.Factory.Common.Models;
using GT.Factory.GTUser.Repository;
using GT.Factory.Management.UI.GTUser;
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
    /// 用户管理业务页面。
    ///
    /// 本控件不依赖 Form，可以：
    /// 1. 由 UserManageForm 作为独立窗口承载；
    /// 2. 被 Server 等管理平台直接嵌入；
    ///
    /// 用户数据由外部传入 LocalUserService，
    /// 本页面不负责创建用户服务。
    /// </summary>
    public class UserManageControl : UserControl
    {
        private UIToolPage _page;

        private readonly ILocalUserService _localUserService;

        /// <summary>
        /// 创建用户管理页面。
        /// </summary>
        /// <param name="localUserService">
        /// 用户服务，由外部应用负责创建和传入。
        /// </param>
        public UserManageControl(
            ILocalUserService localUserService)
        {
            if (localUserService == null)
                throw new ArgumentNullException("localUserService");

            _localUserService = localUserService;

            InitializeUI();
            // 不能在构造函数中直接 await，需要改为同步调用或在外部初始化后调用 LoadUsers
            // await LoadUsers();
            LoadUsers().ConfigureAwait(false);

            UIStyle.ApplyWidget(this);
        }

        /// <summary>
        /// 初始化用户管理页面。
        /// </summary>
        private void InitializeUI()
        {
            _page = new UIToolPage();

            var btnAdd = UIButton.Primary("新增");
            var btnEdit = UIButton.Primary("编辑");
            var btnDelete = UIButton.Primary("删除");
            var btnRefresh = UIButton.Primary("刷新");

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;

            _page.AddButton(btnAdd);
            _page.AddButton(btnEdit);
            _page.AddButton(btnDelete);
            _page.AddButton(btnRefresh);

            _page.Grid.DoubleClick += DgvUsers_DoubleClick;

            _page.Dock = DockStyle.Fill;

            Controls.Add(_page);
        }

        /// <summary>
        /// 加载用户数据。
        /// </summary>
        private async Task LoadUsers()
        {
            try
            {
                List<User> users = await _localUserService.GetAllAsync();

                _page.Grid.DataSource = null;
                _page.Grid.DataSource = users;

                HideSensitiveColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "加载用户失败：" + ex.Message,
                    "用户管理",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 隐藏用户密码等敏感字段。
        /// </summary>
        private void HideSensitiveColumns()
        {
            if (_page.Grid.Columns["UserPassword"] != null)
            {
                _page.Grid.Columns["UserPassword"].Visible = false;
            }

            if (_page.Grid.Columns["MesPassword"] != null)
            {
                _page.Grid.Columns["MesPassword"].Visible = false;
            }
        }

        /// <summary>
        /// 新增用户。
        /// </summary>
        private async void BtnAdd_Click(
            object sender,
            EventArgs e)
        {
            using (var frm = new UserEditForm())
            {
                if (frm.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    await _localUserService.InsertNewUsersInforAsync(
                        frm.UserData);

                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "新增用户失败：" + ex.Message,
                        "用户管理",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 编辑当前用户。
        /// </summary>
        private async void BtnEdit_Click(
            object sender,
            EventArgs e)
        {
            await EditCurrentUser();
        }

        /// <summary>
        /// 刷新用户列表。
        /// </summary>
        private void BtnRefresh_Click(
            object sender,
            EventArgs e)
        {
            LoadUsers().ConfigureAwait(false);
        }

        /// <summary>
        /// 双击用户列表时编辑用户。
        /// </summary>
        private async void DgvUsers_DoubleClick(
            object sender,
            EventArgs e)
        {
            await EditCurrentUser();
        }

        /// <summary>
        /// 编辑当前选中的用户。
        /// </summary>
        private async Task EditCurrentUser()
        {
            User user = GetCurrentUser();

            if (user == null)
            {
                MessageBox.Show(
                    "请选择用户",
                    "用户管理",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (var frm = new UserEditForm(user))
            {
                Form hostForm = FindForm();

                DialogResult result;

                if (hostForm != null)
                {
                    result =
                        DialogWithMaskStyle.ShowDialogWithMask(
                            frm,
                            hostForm);
                }
                else
                {
                    result = frm.ShowDialog();
                }

                if (result != DialogResult.OK)
                    return;

                try
                {
                    await _localUserService.UpdateAsync(
                        frm.UserData);

                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "更新用户失败：" + ex.Message,
                        "用户管理",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 删除当前选中的用户。
        /// </summary>
        private async void BtnDelete_Click(
            object sender,
            EventArgs e)
        {
            User user = GetCurrentUser();

            if (user == null)
            {
                MessageBox.Show(
                    "请选择用户",
                    "用户管理",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    "确定删除用户 " + user.UserName + "？",
                    "确认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                await _localUserService.DeleteAsync(
                    user.JobNum);

                await LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "删除失败：" + ex.Message,
                    "用户管理",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取当前选中的用户。
        /// </summary>
        private User GetCurrentUser()
        {
            if (_page.Grid.CurrentRow == null)
                return null;

            return _page.Grid.CurrentRow.DataBoundItem as User;
        }
    }
}
