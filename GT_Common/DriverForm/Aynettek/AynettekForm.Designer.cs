namespace GT_Common.DriverForm.Aynettek
{
    partial class AynettekForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tSMItem_about = new System.Windows.Forms.ToolStripMenuItem();
            this.tSMItem_close = new System.Windows.Forms.ToolStripMenuItem();
            this.TSMenuItem_prompt = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.tB_ID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.gB_productInfo = new System.Windows.Forms.GroupBox();
            this.label_readersoft = new System.Windows.Forms.Label();
            this.label_readerSn = new System.Windows.Forms.Label();
            this.label_readername = new System.Windows.Forms.Label();
            this.label_dllVer = new System.Windows.Forms.Label();
            this.label_demoVer = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.gB_tcp = new System.Windows.Forms.GroupBox();
            this.bt_connectIP = new System.Windows.Forms.Button();
            this.tB_ipPort = new System.Windows.Forms.TextBox();
            this.tB_ip = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.pB_tagSingle = new System.Windows.Forms.PictureBox();
            this.label21 = new System.Windows.Forms.Label();
            this.tB_cacheTime = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.tB_UID = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.bt_exec = new System.Windows.Forms.Button();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.tB_tagMemdatas = new System.Windows.Forms.TextBox();
            this.tB_tagMemCnt = new System.Windows.Forms.TextBox();
            this.tB_tagMemAddr = new System.Windows.Forms.TextBox();
            this.label24 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.radioBt_setCacheTime = new System.Windows.Forms.RadioButton();
            this.radioBt_getcachetime = new System.Windows.Forms.RadioButton();
            this.radioBt_tagsingle = new System.Windows.Forms.RadioButton();
            this.radioBt_writeMem = new System.Windows.Forms.RadioButton();
            this.radioBt_readMem = new System.Windows.Forms.RadioButton();
            this.radioBt_UID = new System.Windows.Forms.RadioButton();
            this.richTB_status = new System.Windows.Forms.RichTextBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.gB_productInfo.SuspendLayout();
            this.gB_tcp.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pB_tagSingle)).BeginInit();
            this.groupBox14.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSMItem_about,
            this.tSMItem_close,
            this.TSMenuItem_prompt});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(695, 25);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tSMItem_about
            // 
            this.tSMItem_about.Name = "tSMItem_about";
            this.tSMItem_about.Size = new System.Drawing.Size(44, 21);
            this.tSMItem_about.Text = "关于";
            // 
            // tSMItem_close
            // 
            this.tSMItem_close.Name = "tSMItem_close";
            this.tSMItem_close.Size = new System.Drawing.Size(44, 21);
            this.tSMItem_close.Text = "退出";
            this.tSMItem_close.Click += new System.EventHandler(this.tSMItem_close_Click);
            // 
            // TSMenuItem_prompt
            // 
            this.TSMenuItem_prompt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.TSMenuItem_prompt.ForeColor = System.Drawing.Color.Red;
            this.TSMenuItem_prompt.Name = "TSMenuItem_prompt";
            this.TSMenuItem_prompt.Size = new System.Drawing.Size(188, 21);
            this.TSMenuItem_prompt.Text = "注：Dec:十进制--Hex:十六进制";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox6);
            this.splitContainer1.Panel2.Controls.Add(this.richTB_status);
            this.splitContainer1.Size = new System.Drawing.Size(695, 560);
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.gB_productInfo);
            this.groupBox1.Controls.Add(this.gB_tcp);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(222, 558);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "读写器通信";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.tB_ID);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Location = new System.Drawing.Point(6, 19);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(212, 55);
            this.groupBox5.TabIndex = 6;
            this.groupBox5.TabStop = false;
            // 
            // tB_ID
            // 
            this.tB_ID.Location = new System.Drawing.Point(137, 20);
            this.tB_ID.Name = "tB_ID";
            this.tB_ID.Size = new System.Drawing.Size(43, 26);
            this.tB_ID.TabIndex = 1;
            this.tB_ID.Text = "1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 23);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 20);
            this.label10.TabIndex = 0;
            this.label10.Text = "设备站点号:";
            // 
            // gB_productInfo
            // 
            this.gB_productInfo.Controls.Add(this.label_readersoft);
            this.gB_productInfo.Controls.Add(this.label_readerSn);
            this.gB_productInfo.Controls.Add(this.label_readername);
            this.gB_productInfo.Controls.Add(this.label_dllVer);
            this.gB_productInfo.Controls.Add(this.label_demoVer);
            this.gB_productInfo.Controls.Add(this.label9);
            this.gB_productInfo.Controls.Add(this.label8);
            this.gB_productInfo.Controls.Add(this.label7);
            this.gB_productInfo.Controls.Add(this.label6);
            this.gB_productInfo.Controls.Add(this.label5);
            this.gB_productInfo.Enabled = false;
            this.gB_productInfo.Location = new System.Drawing.Point(4, 208);
            this.gB_productInfo.Name = "gB_productInfo";
            this.gB_productInfo.Size = new System.Drawing.Size(212, 191);
            this.gB_productInfo.TabIndex = 5;
            this.gB_productInfo.TabStop = false;
            this.gB_productInfo.Text = "产品信息";
            // 
            // label_readersoft
            // 
            this.label_readersoft.AutoSize = true;
            this.label_readersoft.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_readersoft.Location = new System.Drawing.Point(84, 159);
            this.label_readersoft.Name = "label_readersoft";
            this.label_readersoft.Size = new System.Drawing.Size(18, 17);
            this.label_readersoft.TabIndex = 9;
            this.label_readersoft.Text = "--";
            // 
            // label_readerSn
            // 
            this.label_readerSn.AutoSize = true;
            this.label_readerSn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_readerSn.Location = new System.Drawing.Point(84, 127);
            this.label_readerSn.Name = "label_readerSn";
            this.label_readerSn.Size = new System.Drawing.Size(18, 17);
            this.label_readerSn.TabIndex = 8;
            this.label_readerSn.Text = "--";
            // 
            // label_readername
            // 
            this.label_readername.AutoSize = true;
            this.label_readername.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_readername.Location = new System.Drawing.Point(84, 95);
            this.label_readername.Name = "label_readername";
            this.label_readername.Size = new System.Drawing.Size(18, 17);
            this.label_readername.TabIndex = 7;
            this.label_readername.Text = "--";
            // 
            // label_dllVer
            // 
            this.label_dllVer.AutoSize = true;
            this.label_dllVer.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_dllVer.Location = new System.Drawing.Point(84, 60);
            this.label_dllVer.Name = "label_dllVer";
            this.label_dllVer.Size = new System.Drawing.Size(18, 17);
            this.label_dllVer.TabIndex = 6;
            this.label_dllVer.Text = "--";
            // 
            // label_demoVer
            // 
            this.label_demoVer.AutoSize = true;
            this.label_demoVer.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_demoVer.Location = new System.Drawing.Point(84, 29);
            this.label_demoVer.Name = "label_demoVer";
            this.label_demoVer.Size = new System.Drawing.Size(18, 17);
            this.label_demoVer.TabIndex = 5;
            this.label_demoVer.Text = "--";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(9, 159);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 17);
            this.label9.TabIndex = 4;
            this.label9.Text = "软件版本号:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(9, 127);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 17);
            this.label8.TabIndex = 3;
            this.label8.Text = "序列号:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(9, 95);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(71, 17);
            this.label7.TabIndex = 2;
            this.label7.Text = "读写器型号:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(9, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(68, 17);
            this.label6.TabIndex = 1;
            this.label6.Text = "DLL版本号:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(9, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(47, 17);
            this.label5.TabIndex = 0;
            this.label5.Text = "版本号:";
            // 
            // gB_tcp
            // 
            this.gB_tcp.Controls.Add(this.bt_connectIP);
            this.gB_tcp.Controls.Add(this.tB_ipPort);
            this.gB_tcp.Controls.Add(this.tB_ip);
            this.gB_tcp.Controls.Add(this.label2);
            this.gB_tcp.Controls.Add(this.label1);
            this.gB_tcp.Location = new System.Drawing.Point(4, 80);
            this.gB_tcp.Name = "gB_tcp";
            this.gB_tcp.Size = new System.Drawing.Size(212, 122);
            this.gB_tcp.TabIndex = 0;
            this.gB_tcp.TabStop = false;
            this.gB_tcp.Text = "TCP/IP";
            // 
            // bt_connectIP
            // 
            this.bt_connectIP.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_connectIP.Location = new System.Drawing.Point(64, 80);
            this.bt_connectIP.Name = "bt_connectIP";
            this.bt_connectIP.Size = new System.Drawing.Size(118, 37);
            this.bt_connectIP.TabIndex = 0;
            this.bt_connectIP.Text = "连接";
            this.bt_connectIP.UseVisualStyleBackColor = true;
            this.bt_connectIP.Click += new System.EventHandler(this.bt_connectIP_Click);
            // 
            // tB_ipPort
            // 
            this.tB_ipPort.Location = new System.Drawing.Point(62, 48);
            this.tB_ipPort.Name = "tB_ipPort";
            this.tB_ipPort.Size = new System.Drawing.Size(120, 26);
            this.tB_ipPort.TabIndex = 3;
            this.tB_ipPort.Text = "1030";
            // 
            // tB_ip
            // 
            this.tB_ip.Location = new System.Drawing.Point(61, 19);
            this.tB_ip.Name = "tB_ip";
            this.tB_ip.Size = new System.Drawing.Size(121, 26);
            this.tB_ip.TabIndex = 2;
            this.tB_ip.Text = "192.168.1.253";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "端口:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP:";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.tabControl1);
            this.groupBox6.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox6.Location = new System.Drawing.Point(3, 0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(458, 386);
            this.groupBox6.TabIndex = 1;
            this.groupBox6.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Enabled = false;
            this.tabControl1.Location = new System.Drawing.Point(6, 17);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(446, 361);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.groupBox15);
            this.tabPage1.Controls.Add(this.bt_exec);
            this.tabPage1.Controls.Add(this.groupBox14);
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(438, 328);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "标签内存寄存器";
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.pB_tagSingle);
            this.groupBox15.Controls.Add(this.label21);
            this.groupBox15.Controls.Add(this.tB_cacheTime);
            this.groupBox15.Controls.Add(this.label23);
            this.groupBox15.Controls.Add(this.tB_UID);
            this.groupBox15.Controls.Add(this.label20);
            this.groupBox15.Location = new System.Drawing.Point(3, 88);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Size = new System.Drawing.Size(429, 74);
            this.groupBox15.TabIndex = 4;
            this.groupBox15.TabStop = false;
            // 
            // pB_tagSingle
            // 
            this.pB_tagSingle.Location = new System.Drawing.Point(80, 13);
            this.pB_tagSingle.Name = "pB_tagSingle";
            this.pB_tagSingle.Size = new System.Drawing.Size(35, 26);
            this.pB_tagSingle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pB_tagSingle.TabIndex = 8;
            this.pB_tagSingle.TabStop = false;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(267, 45);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(113, 20);
            this.label21.TabIndex = 2;
            this.label21.Text = "标签缓存(10ms):";
            // 
            // tB_cacheTime
            // 
            this.tB_cacheTime.Location = new System.Drawing.Point(386, 42);
            this.tB_cacheTime.Name = "tB_cacheTime";
            this.tB_cacheTime.Size = new System.Drawing.Size(37, 26);
            this.tB_cacheTime.TabIndex = 7;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(6, 13);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(68, 20);
            this.label23.TabIndex = 5;
            this.label23.Text = "标签信号:";
            // 
            // tB_UID
            // 
            this.tB_UID.Location = new System.Drawing.Point(53, 42);
            this.tB_UID.Name = "tB_UID";
            this.tB_UID.Size = new System.Drawing.Size(204, 26);
            this.tB_UID.TabIndex = 1;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(6, 42);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(37, 20);
            this.label20.TabIndex = 0;
            this.label20.Text = "UID:";
            // 
            // bt_exec
            // 
            this.bt_exec.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_exec.Location = new System.Drawing.Point(384, 280);
            this.bt_exec.Name = "bt_exec";
            this.bt_exec.Size = new System.Drawing.Size(54, 34);
            this.bt_exec.TabIndex = 3;
            this.bt_exec.Text = "执行";
            this.bt_exec.UseVisualStyleBackColor = true;
            this.bt_exec.Click += new System.EventHandler(this.bt_exec_Click);
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.label26);
            this.groupBox14.Controls.Add(this.label25);
            this.groupBox14.Controls.Add(this.tB_tagMemdatas);
            this.groupBox14.Controls.Add(this.tB_tagMemCnt);
            this.groupBox14.Controls.Add(this.tB_tagMemAddr);
            this.groupBox14.Controls.Add(this.label24);
            this.groupBox14.Controls.Add(this.label22);
            this.groupBox14.Location = new System.Drawing.Point(3, 157);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Size = new System.Drawing.Size(378, 160);
            this.groupBox14.TabIndex = 2;
            this.groupBox14.TabStop = false;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(3, 69);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(45, 20);
            this.label26.TabIndex = 6;
            this.label26.Text = "(Hex)";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(3, 49);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(40, 20);
            this.label25.TabIndex = 5;
            this.label25.Text = "数据:";
            // 
            // tB_tagMemdatas
            // 
            this.tB_tagMemdatas.Location = new System.Drawing.Point(53, 46);
            this.tB_tagMemdatas.Multiline = true;
            this.tB_tagMemdatas.Name = "tB_tagMemdatas";
            this.tB_tagMemdatas.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tB_tagMemdatas.Size = new System.Drawing.Size(319, 108);
            this.tB_tagMemdatas.TabIndex = 4;
            // 
            // tB_tagMemCnt
            // 
            this.tB_tagMemCnt.Location = new System.Drawing.Point(281, 16);
            this.tB_tagMemCnt.Name = "tB_tagMemCnt";
            this.tB_tagMemCnt.Size = new System.Drawing.Size(45, 26);
            this.tB_tagMemCnt.TabIndex = 3;
            // 
            // tB_tagMemAddr
            // 
            this.tB_tagMemAddr.Location = new System.Drawing.Point(89, 16);
            this.tB_tagMemAddr.Name = "tB_tagMemAddr";
            this.tB_tagMemAddr.Size = new System.Drawing.Size(62, 26);
            this.tB_tagMemAddr.TabIndex = 2;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(157, 19);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(118, 20);
            this.label24.TabIndex = 1;
            this.label24.Text = "寄存器数量(Dec):";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(6, 19);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(76, 20);
            this.label22.TabIndex = 0;
            this.label22.Text = "地址(Dec):";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.tableLayoutPanel1);
            this.groupBox7.Location = new System.Drawing.Point(3, 0);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(429, 88);
            this.groupBox7.TabIndex = 1;
            this.groupBox7.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.16229F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.56086F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.27685F));
            this.tableLayoutPanel1.Controls.Add(this.radioBt_setCacheTime, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.radioBt_getcachetime, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.radioBt_tagsingle, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.radioBt_writeMem, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioBt_readMem, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioBt_UID, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 11);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45.83333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 54.16667F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(420, 70);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // radioBt_setCacheTime
            // 
            this.radioBt_setCacheTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioBt_setCacheTime.AutoSize = true;
            this.radioBt_setCacheTime.Location = new System.Drawing.Point(270, 38);
            this.radioBt_setCacheTime.Name = "radioBt_setCacheTime";
            this.radioBt_setCacheTime.Size = new System.Drawing.Size(139, 24);
            this.radioBt_setCacheTime.TabIndex = 6;
            this.radioBt_setCacheTime.TabStop = true;
            this.radioBt_setCacheTime.Text = "设置标签缓存时间";
            this.radioBt_setCacheTime.UseVisualStyleBackColor = true;
            // 
            // radioBt_getcachetime
            // 
            this.radioBt_getcachetime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioBt_getcachetime.AutoSize = true;
            this.radioBt_getcachetime.Location = new System.Drawing.Point(122, 38);
            this.radioBt_getcachetime.Name = "radioBt_getcachetime";
            this.radioBt_getcachetime.Size = new System.Drawing.Size(139, 24);
            this.radioBt_getcachetime.TabIndex = 5;
            this.radioBt_getcachetime.TabStop = true;
            this.radioBt_getcachetime.Text = "获取标签缓存时间";
            this.radioBt_getcachetime.UseVisualStyleBackColor = true;
            // 
            // radioBt_tagsingle
            // 
            this.radioBt_tagsingle.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioBt_tagsingle.AutoSize = true;
            this.radioBt_tagsingle.Location = new System.Drawing.Point(4, 38);
            this.radioBt_tagsingle.Name = "radioBt_tagsingle";
            this.radioBt_tagsingle.Size = new System.Drawing.Size(83, 24);
            this.radioBt_tagsingle.TabIndex = 4;
            this.radioBt_tagsingle.TabStop = true;
            this.radioBt_tagsingle.Text = "标签信号";
            this.radioBt_tagsingle.UseVisualStyleBackColor = true;
            // 
            // radioBt_writeMem
            // 
            this.radioBt_writeMem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioBt_writeMem.AutoSize = true;
            this.radioBt_writeMem.Location = new System.Drawing.Point(270, 4);
            this.radioBt_writeMem.Name = "radioBt_writeMem";
            this.radioBt_writeMem.Size = new System.Drawing.Size(69, 24);
            this.radioBt_writeMem.TabIndex = 2;
            this.radioBt_writeMem.TabStop = true;
            this.radioBt_writeMem.Text = "写内存";
            this.radioBt_writeMem.UseVisualStyleBackColor = true;
            // 
            // radioBt_readMem
            // 
            this.radioBt_readMem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioBt_readMem.AutoSize = true;
            this.radioBt_readMem.Location = new System.Drawing.Point(122, 4);
            this.radioBt_readMem.Name = "radioBt_readMem";
            this.radioBt_readMem.Size = new System.Drawing.Size(69, 24);
            this.radioBt_readMem.TabIndex = 1;
            this.radioBt_readMem.TabStop = true;
            this.radioBt_readMem.Text = "读内存";
            this.radioBt_readMem.UseVisualStyleBackColor = true;
            // 
            // radioBt_UID
            // 
            this.radioBt_UID.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.radioBt_UID.AutoSize = true;
            this.radioBt_UID.Checked = true;
            this.radioBt_UID.Location = new System.Drawing.Point(4, 4);
            this.radioBt_UID.Name = "radioBt_UID";
            this.radioBt_UID.Size = new System.Drawing.Size(52, 24);
            this.radioBt_UID.TabIndex = 0;
            this.radioBt_UID.TabStop = true;
            this.radioBt_UID.Text = "UID";
            this.radioBt_UID.UseVisualStyleBackColor = true;
            // 
            // richTB_status
            // 
            this.richTB_status.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTB_status.Location = new System.Drawing.Point(3, 386);
            this.richTB_status.Name = "richTB_status";
            this.richTB_status.Size = new System.Drawing.Size(458, 160);
            this.richTB_status.TabIndex = 0;
            this.richTB_status.Text = "";
            // 
            // AynettekForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(695, 585);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "AynettekForm";
            this.Text = "AynettekForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.gB_productInfo.ResumeLayout(false);
            this.gB_productInfo.PerformLayout();
            this.gB_tcp.ResumeLayout(false);
            this.gB_tcp.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pB_tagSingle)).EndInit();
            this.groupBox14.ResumeLayout(false);
            this.groupBox14.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tSMItem_about;
        private System.Windows.Forms.ToolStripMenuItem tSMItem_close;
        private System.Windows.Forms.ToolStripMenuItem TSMenuItem_prompt;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox tB_ID;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox gB_productInfo;
        private System.Windows.Forms.Label label_readersoft;
        private System.Windows.Forms.Label label_readerSn;
        private System.Windows.Forms.Label label_readername;
        private System.Windows.Forms.Label label_dllVer;
        private System.Windows.Forms.Label label_demoVer;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox gB_tcp;
        private System.Windows.Forms.Button bt_connectIP;
        private System.Windows.Forms.TextBox tB_ipPort;
        private System.Windows.Forms.TextBox tB_ip;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RichTextBox richTB_status;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox15;
        private System.Windows.Forms.PictureBox pB_tagSingle;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox tB_cacheTime;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox tB_UID;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button bt_exec;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox tB_tagMemdatas;
        private System.Windows.Forms.TextBox tB_tagMemCnt;
        private System.Windows.Forms.TextBox tB_tagMemAddr;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RadioButton radioBt_setCacheTime;
        private System.Windows.Forms.RadioButton radioBt_getcachetime;
        private System.Windows.Forms.RadioButton radioBt_tagsingle;
        private System.Windows.Forms.RadioButton radioBt_writeMem;
        private System.Windows.Forms.RadioButton radioBt_readMem;
        private System.Windows.Forms.RadioButton radioBt_UID;
    }
}