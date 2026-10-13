using System.Windows.Forms;

namespace CodeRule
{
    partial class CodeRuleForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ruleListBox = new System.Windows.Forms.ListBox();
            this.ruleNameTextBox = new System.Windows.Forms.TextBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.isActiveCheckBox = new System.Windows.Forms.CheckBox();
            this.segmentsDataGridView = new System.Windows.Forms.DataGridView();
            this.generatedCodeTextBox = new System.Windows.Forms.TextBox();
            this.newRuleButton = new System.Windows.Forms.Button();
            this.saveRuleButton = new System.Windows.Forms.Button();
            this.addSegmentButton = new System.Windows.Forms.Button();
            this.editSegmentButton = new System.Windows.Forms.Button();
            this.deleteSegmentButton = new System.Windows.Forms.Button();
            this.generateCodeButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.codeRegexTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.colOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFormat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPaddingChar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPaddingSide = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.segmentsDataGridView)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // ruleListBox
            // 
            this.ruleListBox.FormattingEnabled = true;
            this.ruleListBox.ItemHeight = 12;
            this.ruleListBox.Location = new System.Drawing.Point(15, 30);
            this.ruleListBox.Name = "ruleListBox";
            this.ruleListBox.Size = new System.Drawing.Size(133, 88);
            this.ruleListBox.TabIndex = 0;
            this.ruleListBox.SelectedIndexChanged += new System.EventHandler(this.ruleListBox_SelectedIndexChanged);
            // 
            // ruleNameTextBox
            // 
            this.ruleNameTextBox.Location = new System.Drawing.Point(179, 30);
            this.ruleNameTextBox.Name = "ruleNameTextBox";
            this.ruleNameTextBox.Size = new System.Drawing.Size(157, 21);
            this.ruleNameTextBox.TabIndex = 1;
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Location = new System.Drawing.Point(179, 75);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(157, 21);
            this.descriptionTextBox.TabIndex = 2;
            // 
            // isActiveCheckBox
            // 
            this.isActiveCheckBox.AutoSize = true;
            this.isActiveCheckBox.ForeColor = System.Drawing.SystemColors.Control;
            this.isActiveCheckBox.Location = new System.Drawing.Point(179, 102);
            this.isActiveCheckBox.Name = "isActiveCheckBox";
            this.isActiveCheckBox.Size = new System.Drawing.Size(48, 16);
            this.isActiveCheckBox.TabIndex = 3;
            this.isActiveCheckBox.Text = "启用";
            this.isActiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // segmentsDataGridView
            // 
            this.segmentsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.segmentsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colOrder,
            this.colType,
            this.colValue,
            this.colFormat,
            this.colLength,
            this.colPaddingChar,
            this.colPaddingSide});
            this.segmentsDataGridView.Location = new System.Drawing.Point(10, 3);
            this.segmentsDataGridView.Name = "segmentsDataGridView";
            this.segmentsDataGridView.RowTemplate.Height = 25;
            this.segmentsDataGridView.Size = new System.Drawing.Size(644, 199);
            this.segmentsDataGridView.TabIndex = 4;
            // 
            // generatedCodeTextBox
            // 
            this.generatedCodeTextBox.Location = new System.Drawing.Point(366, 75);
            this.generatedCodeTextBox.Name = "generatedCodeTextBox";
            this.generatedCodeTextBox.Size = new System.Drawing.Size(275, 21);
            this.generatedCodeTextBox.TabIndex = 5;
            // 
            // newRuleButton
            // 
            this.newRuleButton.Location = new System.Drawing.Point(13, 6);
            this.newRuleButton.Name = "newRuleButton";
            this.newRuleButton.Size = new System.Drawing.Size(87, 34);
            this.newRuleButton.TabIndex = 6;
            this.newRuleButton.Text = "新增规则";
            this.newRuleButton.UseVisualStyleBackColor = true;
            this.newRuleButton.Click += new System.EventHandler(this.newRuleButton_Click);
            // 
            // saveRuleButton
            // 
            this.saveRuleButton.Location = new System.Drawing.Point(121, 6);
            this.saveRuleButton.Name = "saveRuleButton";
            this.saveRuleButton.Size = new System.Drawing.Size(87, 34);
            this.saveRuleButton.TabIndex = 6;
            this.saveRuleButton.Text = "保存规则";
            this.saveRuleButton.UseVisualStyleBackColor = true;
            this.saveRuleButton.Click += new System.EventHandler(this.saveRuleButton_Click);
            // 
            // addSegmentButton
            // 
            this.addSegmentButton.Location = new System.Drawing.Point(229, 6);
            this.addSegmentButton.Name = "addSegmentButton";
            this.addSegmentButton.Size = new System.Drawing.Size(87, 34);
            this.addSegmentButton.TabIndex = 6;
            this.addSegmentButton.Text = "新增段";
            this.addSegmentButton.UseVisualStyleBackColor = true;
            this.addSegmentButton.Click += new System.EventHandler(this.addSegmentButton_Click);
            // 
            // editSegmentButton
            // 
            this.editSegmentButton.Location = new System.Drawing.Point(337, 6);
            this.editSegmentButton.Name = "editSegmentButton";
            this.editSegmentButton.Size = new System.Drawing.Size(87, 34);
            this.editSegmentButton.TabIndex = 6;
            this.editSegmentButton.Text = "编辑段";
            this.editSegmentButton.UseVisualStyleBackColor = true;
            this.editSegmentButton.Click += new System.EventHandler(this.editSegmentButton_Click);
            // 
            // deleteSegmentButton
            // 
            this.deleteSegmentButton.Location = new System.Drawing.Point(445, 6);
            this.deleteSegmentButton.Name = "deleteSegmentButton";
            this.deleteSegmentButton.Size = new System.Drawing.Size(87, 34);
            this.deleteSegmentButton.TabIndex = 6;
            this.deleteSegmentButton.Text = "删除段";
            this.deleteSegmentButton.UseVisualStyleBackColor = true;
            this.deleteSegmentButton.Click += new System.EventHandler(this.deleteSegmentButton_Click);
            // 
            // generateCodeButton
            // 
            this.generateCodeButton.Location = new System.Drawing.Point(553, 6);
            this.generateCodeButton.Name = "generateCodeButton";
            this.generateCodeButton.Size = new System.Drawing.Size(87, 34);
            this.generateCodeButton.TabIndex = 6;
            this.generateCodeButton.Text = "生成产品码";
            this.generateCodeButton.UseVisualStyleBackColor = true;
            this.generateCodeButton.Click += new System.EventHandler(this.generateCodeButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(15, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "编码规则列表";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(179, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "规则名";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(179, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "编码规则详情";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.Control;
            this.label5.Location = new System.Drawing.Point(366, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "编码";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.generateCodeButton);
            this.panel1.Controls.Add(this.newRuleButton);
            this.panel1.Controls.Add(this.saveRuleButton);
            this.panel1.Controls.Add(this.addSegmentButton);
            this.panel1.Controls.Add(this.editSegmentButton);
            this.panel1.Controls.Add(this.deleteSegmentButton);
            this.panel1.Location = new System.Drawing.Point(3, 360);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(656, 51);
            this.panel1.TabIndex = 9;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.segmentsDataGridView);
            this.panel2.Location = new System.Drawing.Point(3, 141);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(659, 213);
            this.panel2.TabIndex = 10;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.codeRegexTextBox);
            this.panel3.Controls.Add(this.generatedCodeTextBox);
            this.panel3.Controls.Add(this.ruleListBox);
            this.panel3.Controls.Add(this.ruleNameTextBox);
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.descriptionTextBox);
            this.panel3.Controls.Add(this.label6);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.isActiveCheckBox);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Location = new System.Drawing.Point(3, 5);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(659, 130);
            this.panel3.TabIndex = 11;
            // 
            // codeRegexTextBox
            // 
            this.codeRegexTextBox.Location = new System.Drawing.Point(366, 29);
            this.codeRegexTextBox.Name = "codeRegexTextBox";
            this.codeRegexTextBox.Size = new System.Drawing.Size(275, 21);
            this.codeRegexTextBox.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(366, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 8;
            this.label6.Text = "正则表达式";
            // 
            // colOrder
            // 
            this.colOrder.HeaderText = "顺序";
            this.colOrder.Name = "colOrder";
            this.colOrder.Width = 60;
            // 
            // colType
            // 
            this.colType.HeaderText = "类型";
            this.colType.Name = "colType";
            // 
            // colValue
            // 
            this.colValue.HeaderText = "值";
            this.colValue.Name = "colValue";
            // 
            // colFormat
            // 
            this.colFormat.HeaderText = "格式";
            this.colFormat.Name = "colFormat";
            // 
            // colLength
            // 
            this.colLength.HeaderText = "长度";
            this.colLength.Name = "colLength";
            this.colLength.Width = 60;
            // 
            // colPaddingChar
            // 
            this.colPaddingChar.HeaderText = "填充字符";
            this.colPaddingChar.Name = "colPaddingChar";
            // 
            // colPaddingSide
            // 
            this.colPaddingSide.HeaderText = "填充方向";
            this.colPaddingSide.Name = "colPaddingSide";
            this.colPaddingSide.Width = 80;
            // 
            // CodeRuleForm
            // 
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "CodeRuleForm";
            this.Size = new System.Drawing.Size(668, 418);
            ((System.ComponentModel.ISupportInitialize)(this.segmentsDataGridView)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox ruleListBox;
        private TextBox ruleNameTextBox;
        private TextBox descriptionTextBox;
        private CheckBox isActiveCheckBox;
        private DataGridView segmentsDataGridView;
        private TextBox generatedCodeTextBox;
        private Button newRuleButton;
        private Button saveRuleButton;
        private Button addSegmentButton;
        private Button editSegmentButton;
        private Button deleteSegmentButton;
        private Button generateCodeButton;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label5;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private TextBox codeRegexTextBox;
        private Label label6;
        private DataGridViewTextBoxColumn colOrder;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colValue;
        private DataGridViewTextBoxColumn colFormat;
        private DataGridViewTextBoxColumn colLength;
        private DataGridViewTextBoxColumn colPaddingChar;
        private DataGridViewTextBoxColumn colPaddingSide;
    }
}