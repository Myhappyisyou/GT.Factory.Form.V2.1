using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Component
{
    /// <summary>
    /// 表单行（自适应宽度版）
    /// </summary>
    public class UIFormRow<T> : UserControl where T : Control
    {
        public T Input { get; private set; }

        public UIFormRow(string labelText, T input)
        {
            this.Height = UITheme.ControlHeight + 6;
            this.Width = 300;
            this.Margin = new Padding(10, 5, 10, 5);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lbl = UILabel.FormLabel(labelText, 90);

            input.Dock = DockStyle.Fill;

            table.Controls.Add(lbl, 0, 0);
            table.Controls.Add(input, 1, 0);

            this.Controls.Add(table);

            Input = input;
        }

        /// <summary>
        /// 设置下拉数据（仅适用于 ComboBox）
        /// </summary>
        public void SetOptions(params string[] items)
        {
            if (Input is ComboBox cb)
            {
                cb.Items.Clear();
                cb.Items.AddRange(items);
            }
        }

        public void SetOptions(Dictionary<string, string> items)
        {
            if (Input is ComboBox cb)
            {
                cb.DisplayMember = "Value";
                cb.ValueMember = "Key";

                cb.DataSource = new BindingSource(items, null);
            }
        }
    }
}
