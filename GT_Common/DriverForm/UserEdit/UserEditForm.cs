using GT.Factory.Common.Enums;
using GT.Factory.Common.Models;
using GT.Factory.UI.Apply;
using GT.Factory.UI.Component;
using GT.Factory.UI.Style;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT_Common.DriverForm.UserEdit
{
    public partial class UserEditForm : Form
    {
        #region 控件

        private UIFormRow<TextBox> txtUserName;
        private UIFormRow<UIPasswordInput> txtPassword;
        private UIFormRow<ComboBox> cmbLevel;
        private UIFormRow<TextBox> txtUID;
        private UIFormRow<TextBox> txtJobNo;
        private UIFormRow<TextBox> txtMesAccount;
        private UIFormRow<UIPasswordInput> txtMesPassword;

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

            UIStyle.ApplyWidget(this);

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

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(15),
                AutoSize = false
            };

            // ⭐ 左右结构
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // ⭐ 每行用 Percent
            for (int i = 0; i < 7; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28f));
            }

            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));

            txtUserName = new UIFormRow<TextBox>("用户名", UIInput.Text());
            txtPassword = new UIFormRow<UIPasswordInput>("用户密码", UIInput.Password());
            cmbLevel = new UIFormRow<ComboBox>("用户权限", UIInput.Combo());
            txtUID = new UIFormRow<TextBox>("厂牌UID", UIInput.Text());
            txtJobNo = new UIFormRow<TextBox>("工号", UIInput.Text());
            txtMesAccount = new UIFormRow<TextBox>("MES账号", UIInput.Text());
            txtMesPassword = new UIFormRow<UIPasswordInput>("MES密码", UIInput.Password());

            AddRow(table, 0, txtUserName);
            AddRow(table, 0, txtPassword);
            AddRow(table, 0, cmbLevel);
            AddRow(table, 0, txtUID);
            AddRow(table, 0, txtJobNo);
            AddRow(table, 0, txtMesAccount);
            AddRow(table, 0, txtMesPassword);

            cmbLevel.Input.Items.AddRange(Enum.GetNames(typeof(UserLevel)));

            // ======================
            // 按钮区（居中关键）
            // ======================
            var panelButtons = new Panel
            {
                Dock = DockStyle.Fill
            };

            btnOk = UIButton.Primary("确定");
            btnCancel = UIButton.Primary("取消");

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

        private void AddRow(TableLayoutPanel table, int row, Control control)
        {

            table.Controls.Add(control, 0, row);
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

            cmbLevel.Input.SelectedItem = UserData.LevelEnum.ToString();

            if (cmbLevel.Input.SelectedIndex < 0)
            {
                cmbLevel.Input.SelectedIndex = 0;
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
                    cmbLevel.Input.SelectedItem.ToString());

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

            if (cmbLevel.Input.SelectedItem == null)
            {
                MessageBox.Show("请选择权限等级");
                return false;
            }

            return true;
        }

        #endregion
    }
}
