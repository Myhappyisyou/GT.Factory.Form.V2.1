using System.Windows.Forms;

namespace GT.Factory.Server
{
    partial class UC_UserEdit
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.LayoutPanel_Body = new System.Windows.Forms.TableLayoutPanel();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.cmbJobNubs = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LayoutPanel_edit = new System.Windows.Forms.TableLayoutPanel();
            this.panel_userInfor = new System.Windows.Forms.Panel();
            this.txtMesPassword = new System.Windows.Forms.TextBox();
            this.txtMesAccount = new System.Windows.Forms.TextBox();
            this.txtJobNub = new System.Windows.Forms.TextBox();
            this.txtUID = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtNewUser = new System.Windows.Forms.TextBox();
            this.cmbRoles = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.LayoutPanel_Body.SuspendLayout();
            this.titlePanel.SuspendLayout();
            this.LayoutPanel_edit.SuspendLayout();
            this.panel_userInfor.SuspendLayout();
            this.SuspendLayout();
            // 
            // LayoutPanel_Body
            // 
            this.LayoutPanel_Body.ColumnCount = 1;
            this.LayoutPanel_Body.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutPanel_Body.Controls.Add(this.titlePanel, 0, 0);
            this.LayoutPanel_Body.Controls.Add(this.LayoutPanel_edit, 0, 1);
            this.LayoutPanel_Body.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LayoutPanel_Body.Location = new System.Drawing.Point(0, 0);
            this.LayoutPanel_Body.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LayoutPanel_Body.Name = "LayoutPanel_Body";
            this.LayoutPanel_Body.RowCount = 2;
            this.LayoutPanel_Body.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.36585F));
            this.LayoutPanel_Body.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 84.63415F));
            this.LayoutPanel_Body.Size = new System.Drawing.Size(734, 410);
            this.LayoutPanel_Body.TabIndex = 0;
            // 
            // titlePanel
            // 
            this.titlePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.titlePanel.Controls.Add(this.cmbJobNubs);
            this.titlePanel.Controls.Add(this.label1);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titlePanel.Location = new System.Drawing.Point(3, 2);
            this.titlePanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(728, 59);
            this.titlePanel.TabIndex = 0;
            // 
            // cmbJobNubs
            // 
            this.cmbJobNubs.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cmbJobNubs.FormattingEnabled = true;
            this.cmbJobNubs.Location = new System.Drawing.Point(273, 16);
            this.cmbJobNubs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbJobNubs.Name = "cmbJobNubs";
            this.cmbJobNubs.Size = new System.Drawing.Size(220, 29);
            this.cmbJobNubs.TabIndex = 1;
            this.cmbJobNubs.Tag = "用户名";
            this.cmbJobNubs.Text = "请选择工号";
            this.cmbJobNubs.SelectedIndexChanged += new System.EventHandler(this.cmbJobNubs_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(199, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "工号：";
            // 
            // LayoutPanel_edit
            // 
            this.LayoutPanel_edit.ColumnCount = 2;
            this.LayoutPanel_edit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.LayoutPanel_edit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.LayoutPanel_edit.Controls.Add(this.panel_userInfor, 1, 0);
            this.LayoutPanel_edit.Dock = System.Windows.Forms.DockStyle.Top;
            this.LayoutPanel_edit.Location = new System.Drawing.Point(3, 65);
            this.LayoutPanel_edit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LayoutPanel_edit.Name = "LayoutPanel_edit";
            this.LayoutPanel_edit.RowCount = 1;
            this.LayoutPanel_edit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LayoutPanel_edit.Size = new System.Drawing.Size(728, 343);
            this.LayoutPanel_edit.TabIndex = 1;
            // 
            // panel_userInfor
            // 
            this.panel_userInfor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_userInfor.Controls.Add(this.txtMesPassword);
            this.panel_userInfor.Controls.Add(this.txtMesAccount);
            this.panel_userInfor.Controls.Add(this.txtJobNub);
            this.panel_userInfor.Controls.Add(this.txtUID);
            this.panel_userInfor.Controls.Add(this.txtPassword);
            this.panel_userInfor.Controls.Add(this.txtNewUser);
            this.panel_userInfor.Controls.Add(this.cmbRoles);
            this.panel_userInfor.Controls.Add(this.label8);
            this.panel_userInfor.Controls.Add(this.label7);
            this.panel_userInfor.Controls.Add(this.label6);
            this.panel_userInfor.Controls.Add(this.label5);
            this.panel_userInfor.Controls.Add(this.label4);
            this.panel_userInfor.Controls.Add(this.label3);
            this.panel_userInfor.Controls.Add(this.label2);
            this.panel_userInfor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_userInfor.Location = new System.Drawing.Point(221, 2);
            this.panel_userInfor.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel_userInfor.Name = "panel_userInfor";
            this.panel_userInfor.Size = new System.Drawing.Size(504, 339);
            this.panel_userInfor.TabIndex = 0;
            // 
            // txtMesPassword
            // 
            this.txtMesPassword.Location = new System.Drawing.Point(223, 261);
            this.txtMesPassword.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMesPassword.Name = "txtMesPassword";
            this.txtMesPassword.PasswordChar = '*';
            this.txtMesPassword.Size = new System.Drawing.Size(118, 21);
            this.txtMesPassword.TabIndex = 2;
            this.txtMesPassword.Tag = "MES密码";
            this.txtMesPassword.Text = "1223";
            // 
            // txtMesAccount
            // 
            this.txtMesAccount.Location = new System.Drawing.Point(223, 224);
            this.txtMesAccount.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtMesAccount.Name = "txtMesAccount";
            this.txtMesAccount.Size = new System.Drawing.Size(118, 21);
            this.txtMesAccount.TabIndex = 2;
            this.txtMesAccount.Tag = "MES账号";
            this.txtMesAccount.Text = "MES1";
            // 
            // txtJobNub
            // 
            this.txtJobNub.Location = new System.Drawing.Point(223, 186);
            this.txtJobNub.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtJobNub.Name = "txtJobNub";
            this.txtJobNub.Size = new System.Drawing.Size(118, 21);
            this.txtJobNub.TabIndex = 2;
            this.txtJobNub.Tag = "工号";
            this.txtJobNub.Text = "123456";
            // 
            // txtUID
            // 
            this.txtUID.Location = new System.Drawing.Point(223, 149);
            this.txtUID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtUID.Name = "txtUID";
            this.txtUID.Size = new System.Drawing.Size(118, 21);
            this.txtUID.TabIndex = 2;
            this.txtUID.Tag = "厂牌UID";
            this.txtUID.Text = "ABCD";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(223, 74);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(118, 21);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.Tag = "密码";
            this.txtPassword.Text = "123456";
            // 
            // txtNewUser
            // 
            this.txtNewUser.Location = new System.Drawing.Point(223, 37);
            this.txtNewUser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtNewUser.Name = "txtNewUser";
            this.txtNewUser.Size = new System.Drawing.Size(118, 21);
            this.txtNewUser.TabIndex = 2;
            this.txtNewUser.Tag = "新建用户名";
            this.txtNewUser.Text = "User1";
            // 
            // cmbRoles
            // 
            this.cmbRoles.FormattingEnabled = true;
            this.cmbRoles.Location = new System.Drawing.Point(223, 111);
            this.cmbRoles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cmbRoles.Name = "cmbRoles";
            this.cmbRoles.Size = new System.Drawing.Size(118, 20);
            this.cmbRoles.TabIndex = 1;
            this.cmbRoles.Tag = "用户权限";
            this.cmbRoles.Text = "请选择用户权限";
            this.cmbRoles.SelectedIndexChanged += new System.EventHandler(this.cmbRoles_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label8.ForeColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(117, 263);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "MES密码：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label7.ForeColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(117, 226);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "MES账号：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(117, 188);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "工号：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(117, 151);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "厂牌UID：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label4.ForeColor = System.Drawing.SystemColors.Control;
            this.label4.Location = new System.Drawing.Point(117, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "用户权限：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(117, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "密码：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(117, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "新建用户名：";
            // 
            // UC_UserEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.Controls.Add(this.LayoutPanel_Body);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UC_UserEdit";
            this.Size = new System.Drawing.Size(734, 410);
            this.LayoutPanel_Body.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.titlePanel.PerformLayout();
            this.LayoutPanel_edit.ResumeLayout(false);
            this.panel_userInfor.ResumeLayout(false);
            this.panel_userInfor.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel LayoutPanel_Body;
        private Panel titlePanel;
        private ComboBox cmbJobNubs;
        private Label label1;
        private TableLayoutPanel LayoutPanel_edit;
        private Panel panel_userInfor;
        private ComboBox cmbRoles;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label2;
        private TextBox txtMesPassword;
        private TextBox txtMesAccount;
        private TextBox txtJobNub;
        private TextBox txtUID;
        private TextBox txtPassword;
        private TextBox txtNewUser;
    }
}
