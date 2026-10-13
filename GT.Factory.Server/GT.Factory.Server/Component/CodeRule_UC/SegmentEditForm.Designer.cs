using System.Windows.Forms;

namespace CodeRule
{
    partial class SegmentEditForm
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
            this.typeComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.paddingSideComboBox = new System.Windows.Forms.ComboBox();
            this.paddingSideLabel = new System.Windows.Forms.Label();
            this.orderNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.formatTextBox = new System.Windows.Forms.TextBox();
            this.lengthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.paddingCharTextBox = new System.Windows.Forms.TextBox();
            this.valueLabel = new System.Windows.Forms.Label();
            this.formatLabel = new System.Windows.Forms.Label();
            this.lengthLabel = new System.Windows.Forms.Label();
            this.paddingCharLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.orderNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lengthNumericUpDown)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // typeComboBox
            // 
            this.typeComboBox.FormattingEnabled = true;
            this.typeComboBox.Location = new System.Drawing.Point(18, 27);
            this.typeComboBox.Name = "typeComboBox";
            this.typeComboBox.Size = new System.Drawing.Size(120, 25);
            this.typeComboBox.TabIndex = 0;
            this.typeComboBox.SelectedIndexChanged += new System.EventHandler(this.typeComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "初始化段类型下拉框";
            // 
            // paddingSideComboBox
            // 
            this.paddingSideComboBox.FormattingEnabled = true;
            this.paddingSideComboBox.Location = new System.Drawing.Point(18, 91);
            this.paddingSideComboBox.Name = "paddingSideComboBox";
            this.paddingSideComboBox.Size = new System.Drawing.Size(120, 25);
            this.paddingSideComboBox.TabIndex = 2;
            // 
            // paddingSideLabel
            // 
            this.paddingSideLabel.AutoSize = true;
            this.paddingSideLabel.Location = new System.Drawing.Point(18, 71);
            this.paddingSideLabel.Name = "paddingSideLabel";
            this.paddingSideLabel.Size = new System.Drawing.Size(128, 17);
            this.paddingSideLabel.TabIndex = 3;
            this.paddingSideLabel.Text = "初始化填充方向下拉框";
            // 
            // orderNumericUpDown
            // 
            this.orderNumericUpDown.Location = new System.Drawing.Point(18, 155);
            this.orderNumericUpDown.Name = "orderNumericUpDown";
            this.orderNumericUpDown.Size = new System.Drawing.Size(120, 23);
            this.orderNumericUpDown.TabIndex = 4;
            // 
            // valueTextBox
            // 
            this.valueTextBox.Location = new System.Drawing.Point(18, 217);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(121, 23);
            this.valueTextBox.TabIndex = 5;
            // 
            // formatTextBox
            // 
            this.formatTextBox.Location = new System.Drawing.Point(18, 279);
            this.formatTextBox.Name = "formatTextBox";
            this.formatTextBox.Size = new System.Drawing.Size(120, 23);
            this.formatTextBox.TabIndex = 6;
            // 
            // lengthNumericUpDown
            // 
            this.lengthNumericUpDown.Location = new System.Drawing.Point(18, 341);
            this.lengthNumericUpDown.Name = "lengthNumericUpDown";
            this.lengthNumericUpDown.Size = new System.Drawing.Size(120, 23);
            this.lengthNumericUpDown.TabIndex = 4;
            // 
            // paddingCharTextBox
            // 
            this.paddingCharTextBox.Location = new System.Drawing.Point(18, 403);
            this.paddingCharTextBox.Name = "paddingCharTextBox";
            this.paddingCharTextBox.Size = new System.Drawing.Size(120, 23);
            this.paddingCharTextBox.TabIndex = 6;
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(18, 197);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(20, 17);
            this.valueLabel.TabIndex = 7;
            this.valueLabel.Text = "值";
            // 
            // formatLabel
            // 
            this.formatLabel.AutoSize = true;
            this.formatLabel.Location = new System.Drawing.Point(18, 259);
            this.formatLabel.Name = "formatLabel";
            this.formatLabel.Size = new System.Drawing.Size(80, 17);
            this.formatLabel.TabIndex = 7;
            this.formatLabel.Text = "格式化字符串";
            // 
            // lengthLabel
            // 
            this.lengthLabel.AutoSize = true;
            this.lengthLabel.Location = new System.Drawing.Point(18, 321);
            this.lengthLabel.Name = "lengthLabel";
            this.lengthLabel.Size = new System.Drawing.Size(44, 17);
            this.lengthLabel.TabIndex = 7;
            this.lengthLabel.Text = "段长度";
            // 
            // paddingCharLabel
            // 
            this.paddingCharLabel.AutoSize = true;
            this.paddingCharLabel.Location = new System.Drawing.Point(18, 383);
            this.paddingCharLabel.Name = "paddingCharLabel";
            this.paddingCharLabel.Size = new System.Drawing.Size(56, 17);
            this.paddingCharLabel.TabIndex = 8;
            this.paddingCharLabel.Text = "填充字符";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 135);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "段顺序";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(46, 58);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(146, 43);
            this.okButton.TabIndex = 9;
            this.okButton.Text = "确定";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(46, 208);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(146, 43);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "取消";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.paddingCharTextBox);
            this.panel1.Controls.Add(this.typeComboBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.paddingCharLabel);
            this.panel1.Controls.Add(this.paddingSideComboBox);
            this.panel1.Controls.Add(this.lengthLabel);
            this.panel1.Controls.Add(this.paddingSideLabel);
            this.panel1.Controls.Add(this.formatLabel);
            this.panel1.Controls.Add(this.orderNumericUpDown);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lengthNumericUpDown);
            this.panel1.Controls.Add(this.valueLabel);
            this.panel1.Controls.Add(this.valueTextBox);
            this.panel1.Controls.Add(this.formatTextBox);
            this.panel1.Location = new System.Drawing.Point(39, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(177, 444);
            this.panel1.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.okButton);
            this.panel2.Controls.Add(this.cancelButton);
            this.panel2.Location = new System.Drawing.Point(289, 125);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(225, 289);
            this.panel2.TabIndex = 11;
            // 
            // SegmentEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 523);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "SegmentEditForm";
            this.Text = "SegmentEditForm";
            this.Load += new System.EventHandler(this.SegmentEditForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.orderNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lengthNumericUpDown)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComboBox typeComboBox;
        private Label label1;
        private ComboBox paddingSideComboBox;
        private Label paddingSideLabel;
        private NumericUpDown orderNumericUpDown;
        private TextBox valueTextBox;
        private TextBox formatTextBox;
        private NumericUpDown lengthNumericUpDown;
        private TextBox paddingCharTextBox;
        private Label valueLabel;
        private Label formatLabel;
        private Label lengthLabel;
        private Label paddingCharLabel;
        private Label label2;
        private Button okButton;
        private Button cancelButton;
        private Panel panel1;
        private Panel panel2;
    }
}