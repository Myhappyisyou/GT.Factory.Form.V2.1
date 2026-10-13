using GT.Factory.Server.Core;
using GT.Factory.Server.Core.ChangeRecordHelp;
using GT.Factory.Server.Core.TreeEdit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TreeView = System.Windows.Forms.TreeView;
using TextBox = System.Windows.Forms.TextBox;
using GT.Factory.Server.Help;
using TaskContracts.Models;
using GT.Factory.Logging.Logging;
using GT.Factory.Common.Helpers;

namespace GT.Factory.Server
{
    public partial class UC_UserEdit : UserControl
    {
        private TreeView treeView;

        private AccessMdbHelper db;

        List<User> lsUsers;
        User currentUser;

        //string dbPath = @"\\192.168.1.3\Access\Users_Data.mdb"; // 目标电脑 IP 和共享名
        string dbPath = @"D:\BYD_Users\Users_Data.mdb"; // 目标电脑 IP 和共享名

        private readonly ChangeTracker _changeTracker = new ChangeTracker();

        public UC_UserEdit()
        {
            InitializeComponent();

            db = new AccessMdbHelper(dbPath);

            //  初始化菜单栏
            InitializeMenuStrip();

            //  初始化用户工号
            InitializeLoadUsers();

            //  初始化记录
            InitializeChangeTracker();

            // 初始化TreeView
            treeView = new TreeView
            {
                BackColor = Color.FromArgb(46, 51, 73),
                Dock = DockStyle.Fill,
                ForeColor = SystemColors.ControlDark, // 视觉上模拟禁用状态
                CheckBoxes = true // 启用勾选框
            };
          
            treeView.AfterCheck += TreeView_AfterCheck; // 勾选事件处理
            //Controls.Add(treeView);

            //  加载tree
            TreeNodeFileConfiguration.LoadTreeFromJson(treeView, PathCenter.UserRoleFile("treeview.json"));

            // 展开所有节点
            treeView.ExpandAll();
            LayoutPanel_edit.Controls.Add(treeView,0,0);

            // 拦截所有用户操作事件
            treeView.BeforeSelect += (s, e) => e.Cancel = true;
            treeView.BeforeCheck += (s, e) => e.Cancel = true;

            //  初始化角色
            cmbRoles.Items.Clear();
            // 绑定枚举值到 ComboBox
            cmbRoles.DataSource = Enum.GetValues(typeof(Role));
            cmbRoles.Text = "请选择权限";
        }

        //  初始化用户信息
        private void InitializeLoadUsers()
        {
            cmbJobNubs.Items.Clear ();
            lsUsers = UploadSql.SelectUsersInfors(db);
            cmbJobNubs.Items.AddRange(lsUsers.Select(u => u.JobNub).ToArray());
        }

        //  菜单栏初始化
        private void InitializeMenuStrip()
        {
            //  新增操作
            ToolStripMenuItem addMenuItem = new ToolStripMenuItem();
            addMenuItem.Text = "新增";
            addMenuItem.Click += AddMenuItem_Click;

            //  更新操作
            ToolStripMenuItem updateMenuItem = new ToolStripMenuItem();
            updateMenuItem.Text = "更新";
            updateMenuItem.Click += ApdateMenuItem_Click;

            //  删除操作
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem();
            deleteMenuItem.Text = "删除";
            deleteMenuItem.Click += DeleteMenuItem_Click;

            MenuStrip menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.FromArgb(225, 227, 228);
            menuStrip.Items.Add(addMenuItem);
            menuStrip.Items.Add(updateMenuItem);
            menuStrip.Items.Add(deleteMenuItem);

            this.Controls.Add(menuStrip);
        }

        private void InitializeChangeTracker()
        {
            // 配置自动记录到文件
            _changeTracker.AutoLogToFile = true;

            //// 只跟踪重要的控件
            //_changeTracker.AddControls(this.Controls.OfType<TextBox>()
            //                         .Where(t => t.Tag?.ToString() == "track"));

            // 只跟踪重要的控件
            _changeTracker.AddControls(panel_userInfor.Controls.OfType<TextBox>());
            // 添加要跟踪的控件
            _changeTracker.AddControl(cmbJobNubs);

            // 订阅变更事件
            _changeTracker.ValueChanged += (s, record) =>
            {
                DisplayLog.Info($"变更记录: {record.ControlName} 从 '{record.OldValue}' 改为 '{record.NewValue}'");
            };

            _changeTracker.TrackerReset += (s, e) =>
            {
                DisplayLog.Info("变更跟踪器已重置");
            };
        }

        //  递归处理勾选状态（可选：勾选父节点时自动勾选子节点）
        private void TreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            UpdateChildNodes(e.Node);
        }

        //  递归处理勾选
        private void UpdateChildNodes(TreeNode parentNode)
        {
            foreach (TreeNode child in parentNode.Nodes)
            {
                child.Checked = parentNode.Checked; // 同步子节点状态
                UpdateChildNodes(child); // 递归更新
            }
        }

        //  新增点击事件
        private void AddMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (lsUsers.Select(u => u.JobNub).Contains(txtJobNub.Text) || lsUsers.Select(u => u.UID).Contains(txtUID.Text))
                {
                    DisplayLog.Info($"请重新填写员工号或UID卡号",true);
                    return;
                }

                UploadSql.InsertNewUserInfor(db, GetCurrentUser());
                //  初始化用户工号
                InitializeLoadUsers();
                DisplayLog.Info($"新增用户成功",true);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"新增用户失败",ex,true);
            }
        }

        //  更新点击事件
        private void ApdateMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                User user =  GetCurrentUser();
                user.ID = UploadSql.SelectUsersInforByJobNub(db, cmbJobNubs.Text).ID;
                UploadSql.UpdateUsersInfor(db, user);
                //  初始化用户工号
                InitializeLoadUsers();
                DisplayLog.Info($"更新用户成功",true);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"更新用户失败", ex, true);
            }
        }

        //  删除点击事件
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UploadSql.DeleteUsersInfor(db, GetCurrentUser());
                //  初始化用户工号
                InitializeLoadUsers();
                DisplayLog.Info($"删除用户成功",true);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"删除用户失败",ex,true);
            }
        }

        //  用户工号选择
        private void cmbJobNubs_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentUser = lsUsers.Where(t => t.JobNub == cmbJobNubs.Text).FirstOrDefault();
            txtNewUser.Text=currentUser.UserName;
            txtPassword.Text = currentUser.UserPassword;
            cmbRoles.SelectedText = currentUser.UserRole;
            txtUID.Text = currentUser.UID;
            txtJobNub.Text = currentUser.JobNub;
            SetSelectedRole(currentUser.UserRole);
            txtMesAccount.Text = currentUser.MesAccount;
            txtMesPassword.Text = currentUser.MesPassword;
            TreeNodeFileConfiguration.LoadTreeFromJson(treeView, PathCenter.UserRoleFile($"{currentUser.UserRole}_Treeview.json"));
            // 展开所有节点
            treeView.ExpandAll();
        }

        // 根据字符串设置选中项
        private void SetSelectedRole(string roleString)
        {
            // 尝试将字符串转换为枚举值
            if (Enum.TryParse(roleString, true, out Role selectedRole))
            {
                // 查找匹配的枚举项
                foreach (Role item in cmbRoles.Items)
                {
                    if (item == selectedRole)
                    {
                        cmbRoles.SelectedItem = item;
                        return;
                    }
                }
            }

            // 未找到匹配项时设置默认值
            cmbRoles.SelectedIndex = 0;
        }

        //  获取当前用户
        private User GetCurrentUser()
        {
            return new User
            {
                UserName = txtNewUser.Text,
                UserPassword = txtPassword.Text,
                UserRole = cmbRoles.Text,
                UID = txtUID.Text,
                JobNub = txtJobNub.Text,
                MesAccount = txtMesAccount.Text,
                MesPassword = txtMesPassword.Text,
            };
        }

        //  权限变化
        private void cmbRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
            TreeNodeFileConfiguration.LoadTreeFromJson(treeView, PathCenter.UserRoleFile($"{cmbRoles.Text}_Treeview.json"));
            // 展开所有节点
            treeView.ExpandAll();
        }

    }
}
