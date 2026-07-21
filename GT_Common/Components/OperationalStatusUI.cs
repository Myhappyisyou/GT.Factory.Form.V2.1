using GT_Common.Helper;
using GT_Common.Helper.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayLog = GT_Common.Helper.Logging.DisplayLog;

namespace GT_Common.Components
{
    public partial class OperationalStatusUI : UserControl
    {
        private readonly Font _customFont = new Font("Segoe UI", 10f);

        public OperationalStatusUI()
        {
            this.AutoSize = true;
            InitializeComponent();
            InitFormLayout();
        }

        private void InitFormLayout()
        {
            var mainPanle = new Panel() 
            { 
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 248, 252),
                Padding = new Padding(6)
            };

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = Color.White

            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));   // TITLE

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // Data grid

            var lbTtileName = new Label()
            {
                AutoSize = false,
                Text = "运行\n状态",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(230, 235, 240),
                ForeColor = Color.FromArgb(40, 40, 40),
            };

            var rtbox = new RichTextBox()
            {
                AutoSize = false,
                ScrollBars= (RichTextBoxScrollBars)ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 30, 30),
            };

            // 只注册显示器，不再 new Writer
            DisplayLog.SetDisplay(new RichTextBoxLogDisplay(rtbox));

            ////  日志初始化
            //DisplayLog.Init(
            //        new SerilogWriter(),
            //        new RichTextBoxLogDisplay(rtbox)
            //        );

            mainLayout.Controls.Add(lbTtileName, 0, 0);

            mainLayout.Controls.Add(rtbox, 1, 0);

            mainPanle.Controls.Add(mainLayout);

            this.Controls.Add(mainPanle);
        }
    }
}
