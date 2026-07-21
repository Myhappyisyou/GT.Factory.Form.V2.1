using GT_Common;
using GT_Common.Helper;
using GT_Common.Model;
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
        private DataGridView dgvUsers;

        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;

        private AccessMdbHelper db;

        public UserManageForm()
        {
            InitializeComponent();
            Init();

            db = new AccessMdbHelper(Config.Instance.UserdbPath);

            LoadUsers();
        }

        private void Init()
        {
            Text = "用户管理";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Font = new Font("微软雅黑", 10F);

            var toolPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            btnAdd = CreateButton("新增", 10);
            btnEdit = CreateButton("编辑", 120);
            btnDelete = CreateButton("删除", 230);
            btnRefresh = CreateButton("刷新", 340);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += (s, e) => LoadUsers();

            toolPanel.Controls.Add(btnAdd);
            toolPanel.Controls.Add(btnEdit);
            toolPanel.Controls.Add(btnDelete);
            toolPanel.Controls.Add(btnRefresh);

            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            dgvUsers.ColumnHeadersHeight = 45;
            dgvUsers.EnableHeadersVisualStyles = false;

            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("微软雅黑", 10F, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvUsers.BackgroundColor = Color.White;
            dgvUsers.GridColor = Color.LightGray;

            dgvUsers.DoubleClick += DgvUsers_DoubleClick;

            Controls.Add(dgvUsers);
            Controls.Add(toolPanel);
        }

        private Button CreateButton(string text, int left)
        {
            return new Button
            {
                Text = text,
                Width = 100,
                Height = 35,
                Left = left,
                Top = 12
            };
        }

        private void LoadUsers()
        {
            try
            {

                List<User> users = UploadSql.Ac_SelectUsersInfors(db);

                dgvUsers.DataSource = null;
                dgvUsers.DataSource = users;

                HideSensitiveColumns();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载用户失败：{ex.Message}");
            }
        }

        private void HideSensitiveColumns()
        {
            if (dgvUsers.Columns["UserPassword"] != null)
                dgvUsers.Columns["UserPassword"].Visible = false;

            if (dgvUsers.Columns["MesPassword"] != null)
                dgvUsers.Columns["MesPassword"].Visible = false;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var frm = new UserEditForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    UploadSql.Ac_InsertNewUserInfor(db, frm.UserData);
                    LoadUsers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            EditCurrentUser();
        }

        private void DgvUsers_DoubleClick(object sender, EventArgs e)
        {
            EditCurrentUser();
        }

        private void EditCurrentUser()
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                MessageBox.Show("请选择用户");
                return;
            }

            using (var frm = new UserEditForm(user))
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    UploadSql.Ac_UpdateUsersInfor(db, frm.UserData);
                    LoadUsers();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
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
                UploadSql.Ac_DeleteUsersInfor(db, user);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}");
            }
        }

        private User GetCurrentUser()
        {
            if (dgvUsers.CurrentRow == null)
                return null;

            return dgvUsers.CurrentRow.DataBoundItem as User;
        }
    }
}
