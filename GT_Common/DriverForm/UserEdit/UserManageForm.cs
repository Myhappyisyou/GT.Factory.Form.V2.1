using GT.Factory.Common.Models;
using GT.Factory.Data.Core;
using GT.Factory.UI.Apply;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using GT_Common;
using GT_Common.DriverForm.Login;
using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.UserEdit
{
    public partial class UserManageForm : Form
    {
        private UIToolPage page;

        private DataGridView dgvUsers;

        private readonly DataRuntime _dataRuntime;

        public UserManageForm(DataRuntime dataRuntime)
        {
            InitializeComponent();

            Init();

            _dataRuntime = dataRuntime;

            LoadUsers();

            UIStyle.ApplyWidget(this);

        }

        private void Init()
        {
            Text = "用户管理";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            page = new UIToolPage();

            var btnAdd = UIButton.Primary("新增");
            var btnEdit = UIButton.Primary("编辑");
            var btnDelete = UIButton.Primary("删除");
            var btnRefresh = UIButton.Primary("刷新");

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (sender, e) => LoadUsers();

            page.AddButton(btnAdd);
            page.AddButton(btnEdit);
            page.AddButton(btnDelete);
            page.AddButton(btnRefresh);

            page.Grid.DoubleClick += DgvUsers_DoubleClick;

            Controls.Add(dgvUsers);

            Controls.Add(page);
        }

        private async void LoadUsers()
        {
            try
            {
                List<User> users = await _dataRuntime.Repositories.Access.GetAllAsync();
                page.Grid.DataSource = null;
                page.Grid.DataSource = users;

                HideSensitiveColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用户失败：{ex.Message}");
            }
        }

        private void HideSensitiveColumns()
        {
            if (page.Grid.Columns["UserPassword"] != null)
                page.Grid.Columns["UserPassword"].Visible = false;

            if (page.Grid.Columns["MesPassword"] != null)
                page.Grid.Columns["MesPassword"].Visible = false;
        }

        private async void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var frm = new UserEditForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    await _dataRuntime.Repositories.Access.InsertNewUserAsync(frm.UserData);
                     LoadUsers();
                }
            }
        }

        private async void BtnEdit_Click(object sender, EventArgs e)
        {
            await EditCurrentUser();
        }

        private async void DgvUsers_DoubleClick(object sender, EventArgs e)
        {
             await EditCurrentUser();
        }

        private async Task EditCurrentUser()
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                MessageBox.Show("请选择用户");
                return;
            }

            using (var frm = new UserEditForm(user))
            {
                if (UIHelper.ShowDialogWithMask(frm, this.FindForm()) == DialogResult.OK)

                {
                    await _dataRuntime.Repositories.Access.UpdateAsync(frm.UserData);
                    LoadUsers();
                }
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                MessageBox.Show("请选择用户");
                return;
            }

            if (MessageBox.Show(
                $"确定删除用户 {user.UserName} ?",
                "确认",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                await _dataRuntime.Repositories.Access.DeleteAsync(user.JobNub);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}");
            }
        }

        private User GetCurrentUser()
        {
            if (page.Grid.CurrentRow == null)
                return null;

            return page.Grid.CurrentRow.DataBoundItem as User;
        }
    }
}
