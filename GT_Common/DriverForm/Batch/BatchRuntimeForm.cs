using System;
using System.Drawing;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Batch
{
    public partial class BatchRuntimeForm : Form
    {
        private FlowLayoutPanel flow;

        public BatchRuntimeForm()
        {
            InitUI();
            LoadData();
        }

        private void InitUI()
        {
            this.Text = "批次运行管理";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(232, 245, 232);
            this.Width = 300;
            this.Height = 500;

            flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = this.BackColor,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            this.Controls.Add(flow);
        }

        private void LoadData()
        {
            flow.Controls.Clear();

            foreach (var part in BatchRuntimeManager.Parts)
            {
                var card = new PartCard(part);
                flow.Controls.Add(card);
            }
        }
    }
}