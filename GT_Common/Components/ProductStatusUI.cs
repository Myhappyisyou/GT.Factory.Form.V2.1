using GT_Common.MyEnum;
using GT_Common.Helper.UIHelp;
using GT_Common.Util.LableStatus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Components
{
    public partial class ProductStatusUI : UserControl
    {
        StatusLabel _statusLabel = new StatusLabel();
        Label lbSn = new Label();

        private readonly Font _customFont = new Font("Microsoft YaHei", 24f, FontStyle.Bold);

        public ProductStatusUI()
        {
            this.AutoSize = true;
            InitializeComponent();
            InitFormLayout();
            UiRefreshCenter.OnRefresh += RefreshUI;

        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }
        private void RefreshUI()
        {
            if (!IsHandleCreated || IsDisposed) return;

            if (!Visible || ParentForm?.WindowState == FormWindowState.Minimized)
                return;

            try
            {
                _statusLabel.CurrentStatus = Shared.currentProductStatus;
                lbSn.Text = Shared.currentBarNo;

            }
            catch
            {
                // 忽略异常，防止窗体关闭时报错
            }
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

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BorderStyle = BorderStyle.FixedSingle,

            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));   // Data grid

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));   // TITLE


            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));   // Data grid

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // TITLE

            var lbTtileName = new Label()
            {
                AutoSize = false,
                Text = "产品\n状态",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(230, 235, 240),
                ForeColor = Color.FromArgb(40, 40, 40),
            };
            _statusLabel = new StatusLabel()
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                Font = _customFont,
                CurrentStatus = Shared.currentProductStatus,
                //BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(0,10,0,0)
            };
            //Shared.currentBarNo
            lbSn = new Label()
            {
                Text = Shared.currentBarNo,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(0),
                TextAlign = ContentAlignment.TopCenter,
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = ColorTranslator.FromHtml("#777777"),
            };

            rightLayout.Controls.Add(_statusLabel, 0, 0);
            rightLayout.Controls.Add(lbSn, 0, 1);

            mainLayout.Controls.Add(lbTtileName, 0, 0);
            mainLayout.Controls.Add(rightLayout, 1, 0);

            mainPanle.Controls.Add(mainLayout);

            this.Controls.Add(mainPanle);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
              
                UiRefreshCenter.OnRefresh -= RefreshUI;

                components?.Dispose();
                _statusLabel?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
