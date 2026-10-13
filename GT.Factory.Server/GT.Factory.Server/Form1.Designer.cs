
using System.Windows.Forms;

namespace GT.Factory.Server
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnConsumableEdit = new System.Windows.Forms.Button();
            this.btnUserEdit = new System.Windows.Forms.Button();
            this.btnMaximized = new System.Windows.Forms.Button();
            this.btnMinimized = new System.Windows.Forms.Button();
            this.pnlNav = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnParameter = new System.Windows.Forms.Button();
            this.btnDashBoard = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.rtboxLog = new System.Windows.Forms.RichTextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.btnConsumableEdit);
            this.panel1.Controls.Add(this.btnUserEdit);
            this.panel1.Controls.Add(this.btnMaximized);
            this.panel1.Controls.Add(this.btnMinimized);
            this.panel1.Controls.Add(this.pnlNav);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.btnParameter);
            this.panel1.Controls.Add(this.btnDashBoard);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(199, 544);
            this.panel1.TabIndex = 2;
            // 
            // btnConsumableEdit
            // 
            this.btnConsumableEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnConsumableEdit.FlatAppearance.BorderColor = System.Drawing.SystemColors.GrayText;
            this.btnConsumableEdit.FlatAppearance.BorderSize = 0;
            this.btnConsumableEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConsumableEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnConsumableEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnConsumableEdit.Image = global::GT.Factory.Server.Properties.Resources.易损件管理__3_;
            this.btnConsumableEdit.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConsumableEdit.Location = new System.Drawing.Point(0, 254);
            this.btnConsumableEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnConsumableEdit.Name = "btnConsumableEdit";
            this.btnConsumableEdit.Size = new System.Drawing.Size(197, 58);
            this.btnConsumableEdit.TabIndex = 7;
            this.btnConsumableEdit.Tag = "Nav";
            this.btnConsumableEdit.Text = "易损件编辑";
            this.btnConsumableEdit.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnConsumableEdit.UseVisualStyleBackColor = true;
            this.btnConsumableEdit.Click += new System.EventHandler(this.NavButton_Click);
            // 
            // btnUserEdit
            // 
            this.btnUserEdit.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnUserEdit.FlatAppearance.BorderColor = System.Drawing.SystemColors.GrayText;
            this.btnUserEdit.FlatAppearance.BorderSize = 0;
            this.btnUserEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUserEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnUserEdit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnUserEdit.Image = global::GT.Factory.Server.Properties.Resources.User_Settings;
            this.btnUserEdit.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnUserEdit.Location = new System.Drawing.Point(0, 196);
            this.btnUserEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnUserEdit.Name = "btnUserEdit";
            this.btnUserEdit.Size = new System.Drawing.Size(197, 58);
            this.btnUserEdit.TabIndex = 6;
            this.btnUserEdit.Tag = "Nav";
            this.btnUserEdit.Text = "用户编辑";
            this.btnUserEdit.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnUserEdit.UseVisualStyleBackColor = true;
            this.btnUserEdit.Click += new System.EventHandler(this.NavButton_Click);
            // 
            // btnMaximized
            // 
            this.btnMaximized.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnMaximized.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnMaximized.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMaximized.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnMaximized.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnMaximized.Image = global::GT.Factory.Server.Properties.Resources.Maximize1;
            this.btnMaximized.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMaximized.Location = new System.Drawing.Point(0, 430);
            this.btnMaximized.Name = "btnMaximized";
            this.btnMaximized.Size = new System.Drawing.Size(197, 27);
            this.btnMaximized.TabIndex = 5;
            this.btnMaximized.Text = "最大化";
            this.btnMaximized.UseVisualStyleBackColor = false;
            this.btnMaximized.Click += new System.EventHandler(this.btnMaximized_Click);
            // 
            // btnMinimized
            // 
            this.btnMinimized.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.btnMinimized.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnMinimized.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimized.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnMinimized.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnMinimized.Image = global::GT.Factory.Server.Properties.Resources.Minimize2;
            this.btnMinimized.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnMinimized.Location = new System.Drawing.Point(0, 457);
            this.btnMinimized.Name = "btnMinimized";
            this.btnMinimized.Size = new System.Drawing.Size(197, 27);
            this.btnMinimized.TabIndex = 4;
            this.btnMinimized.Text = "最小化";
            this.btnMinimized.UseVisualStyleBackColor = false;
            this.btnMinimized.Click += new System.EventHandler(this.btnMinimized_Click);
            // 
            // pnlNav
            // 
            this.pnlNav.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.pnlNav.Location = new System.Drawing.Point(0, 128);
            this.pnlNav.Margin = new System.Windows.Forms.Padding(2);
            this.pnlNav.Name = "pnlNav";
            this.pnlNav.Size = new System.Drawing.Size(8, 80);
            this.pnlNav.TabIndex = 2;
            // 
            // btnExit
            // 
            this.btnExit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.WhiteSmoke;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnExit.Image = global::GT.Factory.Server.Properties.Resources.关闭;
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExit.Location = new System.Drawing.Point(0, 484);
            this.btnExit.Margin = new System.Windows.Forms.Padding(2);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(197, 58);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "Exit";
            this.btnExit.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            this.btnExit.Leave += new System.EventHandler(this.btnExit_Leave);
            // 
            // btnParameter
            // 
            this.btnParameter.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnParameter.FlatAppearance.BorderColor = System.Drawing.SystemColors.GrayText;
            this.btnParameter.FlatAppearance.BorderSize = 0;
            this.btnParameter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParameter.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnParameter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnParameter.Image = global::GT.Factory.Server.Properties.Resources.产品参数;
            this.btnParameter.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnParameter.Location = new System.Drawing.Point(0, 138);
            this.btnParameter.Margin = new System.Windows.Forms.Padding(2);
            this.btnParameter.Name = "btnParameter";
            this.btnParameter.Size = new System.Drawing.Size(197, 58);
            this.btnParameter.TabIndex = 1;
            this.btnParameter.Tag = "Nav";
            this.btnParameter.Text = "型号设定";
            this.btnParameter.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnParameter.UseVisualStyleBackColor = true;
            this.btnParameter.Click += new System.EventHandler(this.NavButton_Click);
            // 
            // btnDashBoard
            // 
            this.btnDashBoard.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnDashBoard.FlatAppearance.BorderColor = System.Drawing.SystemColors.GrayText;
            this.btnDashBoard.FlatAppearance.BorderSize = 0;
            this.btnDashBoard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDashBoard.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.btnDashBoard.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.btnDashBoard.Image = global::GT.Factory.Server.Properties.Resources.主页;
            this.btnDashBoard.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDashBoard.Location = new System.Drawing.Point(0, 80);
            this.btnDashBoard.Margin = new System.Windows.Forms.Padding(2);
            this.btnDashBoard.Name = "btnDashBoard";
            this.btnDashBoard.Size = new System.Drawing.Size(197, 58);
            this.btnDashBoard.TabIndex = 1;
            this.btnDashBoard.Tag = "Nav";
            this.btnDashBoard.Text = "切换型号";
            this.btnDashBoard.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.btnDashBoard.UseVisualStyleBackColor = true;
            this.btnDashBoard.Click += new System.EventHandler(this.NavButton_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(30)))), ((int)(((byte)(54)))));
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(197, 80);
            this.panel2.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GT.Factory.Server.Properties.Resources.BYD;
            this.pictureBox1.Location = new System.Drawing.Point(63, 10);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(75, 40);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(126)))), ((int)(((byte)(249)))));
            this.label1.Location = new System.Drawing.Point(81, 52);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "BYD";
            // 
            // pnlControls
            // 
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlControls.Location = new System.Drawing.Point(199, 0);
            this.pnlControls.Margin = new System.Windows.Forms.Padding(2);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(721, 544);
            this.pnlControls.TabIndex = 3;
            // 
            // rtboxLog
            // 
            this.rtboxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.rtboxLog.Location = new System.Drawing.Point(199, 424);
            this.rtboxLog.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtboxLog.Name = "rtboxLog";
            this.rtboxLog.Size = new System.Drawing.Size(721, 120);
            this.rtboxLog.TabIndex = 4;
            this.rtboxLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.ClientSize = new System.Drawing.Size(920, 544);
            this.Controls.Add(this.rtboxLog);
            this.Controls.Add(this.pnlControls);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Server";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDashBoard;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnParameter;
        private System.Windows.Forms.Panel pnlNav;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button btnMaximized;
        private System.Windows.Forms.Button btnMinimized;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private RichTextBox rtboxLog;
        private Button btnUserEdit;
        private Button btnConsumableEdit;
    }
}

