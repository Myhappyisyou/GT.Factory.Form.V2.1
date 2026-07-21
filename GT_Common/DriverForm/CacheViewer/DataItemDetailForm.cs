using GT_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.CacheViewer
{
    public partial class DataItemDetailForm : Form
    {
        private TextBox txtDetail;

        public DataItemDetailForm(DataItem item)
        {
            Text = $"详细信息 - SN: {item.Sn}, Step: {item.Step}";
            Width = 800;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            txtDetail = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false
            };

            Controls.Add(txtDetail);

            txtDetail.Text = BuildDetailText(item);
        }

        private string BuildDetailText(DataItem item)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"SN: {item.Sn}");
            sb.AppendLine($"DoTime: {item.Do_time}");
            sb.AppendLine($"Step: {item.Step}");
            sb.AppendLine($"IsLastStep: {item.IsLastStep}");
            sb.AppendLine($"Result: {item.Result}");
            sb.AppendLine($"NgMsg: {item.NgMsg}");
            sb.AppendLine();

            if (item.Data_name != null)
            {
                for (int i = 0; i < item.Data_name.Length; i++)
                {
                    string name = item.Data_name.Length > i ? item.Data_name[i] : "";
                    string val = item.Data.Length > i ? item.Data[i] : "";
                    string up = item.Data_up.Length > i ? item.Data_up[i] : "";
                    string down = item.Data_down.Length > i ? item.Data_down[i] : "";
                    string result = item.Data_result.Length > i ? item.Data_result[i] : "";

                    sb.AppendLine($"{i + 1}. {name} => Value: {val}, Up: {up}, Down: {down}, Result: {result}");
                }
            }

            return sb.ToString();
        }
    }
}
