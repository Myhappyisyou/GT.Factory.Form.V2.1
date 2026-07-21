using GT_Common.MyEnum;
using GT_Common.Model;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT_Common.DriverForm.UserEdit
{
    public partial class UserEditForm : Form
    {
        #region 控件

        private TextBox txtUserName;
        private TextBox txtPassword;
        private ComboBox cmbLevel;
        private TextBox txtUID;
        private TextBox txtJobNo;
        private TextBox txtMesAccount;
        private TextBox txtMesPassword;

        private Button btnOk;
        private Button btnCancel;

        #endregion

        #region 属性

        public User UserData { get; private set; }

        #endregion

        #region 构造

      

        public UserEditForm(User user = null)
        {
            UserData = user ?? new User();

            InitializeComponent();

            Init();

            LoadUser();
        }

        #endregion

        #region 初始化UI
        private void Init()
        {
            Text = "用户编辑";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(520, 480);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("微软雅黑", 10F);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(15),
                AutoSize = false
            };

            // ⭐ 左右结构
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ⭐ 每行用 Percent（关键：不要固定 50）
            for (int i = 0; i < 7; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28f));
            }

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

            txtUserName = CreateTextBox();
            txtPassword = CreateTextBox();
            txtPassword.PasswordChar = '*';

            cmbLevel = new ComboBox
            {
                Dock = DockStyle.None,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Height = 28,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(3, 6, 3, 6)
            };

            cmbLevel.Items.AddRange(Enum.GetNames(typeof(UserLevel)));

            txtUID = CreateTextBox();
            txtJobNo = CreateTextBox();
            txtMesAccount = CreateTextBox();
            txtMesPassword = CreateTextBox();
            txtMesPassword.PasswordChar = '*';

            AddRow(table, 0, "用户名", txtUserName);
            AddRow(table, 1, "用户密码", txtPassword);
            AddRow(table, 2, "用户权限", cmbLevel);
            AddRow(table, 3, "厂牌UID", txtUID);
            AddRow(table, 4, "工号", txtJobNo);
            AddRow(table, 5, "MES账号", txtMesAccount);
            AddRow(table, 6, "MES密码", txtMesPassword);

            // ======================
            // 按钮区（居中关键）
            // ======================
            var panelButtons = new Panel
            {
                Dock = DockStyle.Fill
            };

            btnOk = new Button
            {
                Text = "确定",
                Size = new Size(100, 35)
            };

            btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(100, 35)
            };

            btnOk.Click += BtnOk_Click;
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            var btnLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 10, 0),
                WrapContents = false
            };

            btnLayout.Controls.Add(btnCancel);
            btnLayout.Controls.Add(btnOk);

            panelButtons.Controls.Add(btnLayout);

            table.Controls.Add(panelButtons, 0, 7);
            table.SetColumnSpan(panelButtons, 2);

            Controls.Add(table);
        }
        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.None,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Height = 28,
                Margin = new Padding(3, 6, 3, 6)
            };
        }

        private void AddRow(TableLayoutPanel table, int row, string title, Control control)
        {
            var lbl = new Label
            {
                Text = title,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill
            };

            table.Controls.Add(lbl, 0, row);
            table.Controls.Add(control, 1, row);
        }

        #endregion

        #region 数据加载

        private void LoadUser()
        {
            txtUserName.Text = UserData.UserName;
            txtPassword.Text = UserData.UserPassword;
            txtUID.Text = UserData.UID;
            txtJobNo.Text = UserData.JobNub;
            txtMesAccount.Text = UserData.MesAccount;
            txtMesPassword.Text = UserData.MesPassword;

            cmbLevel.SelectedItem = UserData.LevelEnum.ToString();

            if (cmbLevel.SelectedIndex < 0)
            {
                cmbLevel.SelectedIndex = 0;
            }
        }

        #endregion

        #region 保存

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            UserData.UserName = txtUserName.Text.Trim();
            UserData.UserPassword = txtPassword.Text.Trim();
            UserData.UID = txtUID.Text.Trim();
            UserData.JobNub = txtJobNo.Text.Trim();
            UserData.MesAccount = txtMesAccount.Text.Trim();
            UserData.MesPassword = txtMesPassword.Text.Trim();

            UserData.LevelEnum =
                (UserLevel)Enum.Parse(
                    typeof(UserLevel),
                    cmbLevel.SelectedItem.ToString());

            DialogResult = DialogResult.OK;
        }

        #endregion

        #region 验证

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("请输入用户名");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("请输入密码");
                return false;
            }

            if (cmbLevel.SelectedItem == null)
            {
                MessageBox.Show("请选择权限等级");
                return false;
            }

            return true;
        }

        #endregion
    }
}
