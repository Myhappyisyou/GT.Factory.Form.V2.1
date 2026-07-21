using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.InputDialog
{
    public class InputDialogForm : Form
    {
        private TextBox txt;
        private Button btnOk;
        private Button btnCancel;

        public string InputText => txt.Text;

        public InputDialogForm(string title, string prompt)
        {
            this.Text = title;
            this.Width = 350;
            this.Height = 150;
            this.StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label
            {
                Text = prompt,
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(5),

            };

            txt = new TextBox
            {
                Dock = DockStyle.Top,
                Padding = new Padding(5),
                TabIndex = 0  // 设置为第一个 Tab 顺序

            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            btnOk = new Button { Text = "确定" };
            btnCancel = new Button { Text = "取消" };

            btnOk.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            panel.Controls.Add(btnOk);
            panel.Controls.Add(btnCancel);

            this.Controls.Add(panel);
            this.Controls.Add(txt);
            this.Controls.Add(lbl);

            // 设置窗体加载时自动聚焦到第一个控件
            this.Load += (s, e) =>
            {
                this.ActiveControl = txt;
            };
        }
    }
}
