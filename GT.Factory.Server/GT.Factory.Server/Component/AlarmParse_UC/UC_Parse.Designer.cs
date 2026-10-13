
namespace GT.Factory.Server
{
    partial class UC_Parse
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
            this.dgvAlarmData = new System.Windows.Forms.DataGridView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbInsert = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbOpenFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbReadFromData = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarmData)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvAlarmData
            // 
            this.dgvAlarmData.ColumnHeadersHeight = 80;
            this.dgvAlarmData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAlarmData.Location = new System.Drawing.Point(0, 0);
            this.dgvAlarmData.Name = "dgvAlarmData";
            this.dgvAlarmData.RowHeadersWidth = 51;
            this.dgvAlarmData.RowTemplate.Height = 27;
            this.dgvAlarmData.Size = new System.Drawing.Size(704, 661);
            this.dgvAlarmData.TabIndex = 5;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbInsert,
            this.toolStripSeparator1,
            this.tsbOpenFile,
            this.toolStripSeparator2,
            this.tsbReadFromData});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(704, 27);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbInsert
            // 
            this.tsbInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbInsert.Image = global::GT.Factory.Server.Properties.Resources.导入表单_资料_操作_jurassic;
            this.tsbInsert.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbInsert.Name = "tsbInsert";
            this.tsbInsert.Size = new System.Drawing.Size(24, 24);
            this.tsbInsert.Text = "导入";
            this.tsbInsert.Click += new System.EventHandler(this.tsbInsert_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbOpenFile
            // 
            this.tsbOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpenFile.Image = global::GT.Factory.Server.Properties.Resources.打开文件;
            this.tsbOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpenFile.Name = "tsbOpenFile";
            this.tsbOpenFile.Size = new System.Drawing.Size(24, 24);
            this.tsbOpenFile.Text = "toolStripButton1";
            this.tsbOpenFile.Click += new System.EventHandler(this.tsbOpenFile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // tsbReadFromData
            // 
            this.tsbReadFromData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbReadFromData.Image = global::GT.Factory.Server.Properties.Resources.数据库ECQL过滤读取;
            this.tsbReadFromData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbReadFromData.Name = "tsbReadFromData";
            this.tsbReadFromData.Size = new System.Drawing.Size(24, 24);
            this.tsbReadFromData.Text = "toolStripButton1";
            this.tsbReadFromData.Click += new System.EventHandler(this.tsbReadFromData_Click);
            // 
            // UC_Parse
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.dgvAlarmData);
            this.Name = "UC_Parse";
            this.Size = new System.Drawing.Size(704, 661);
            this.Load += new System.EventHandler(this.UC_Parse_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlarmData)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvAlarmData;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbInsert;
        private System.Windows.Forms.ToolStripButton tsbOpenFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbReadFromData;
    }
}
