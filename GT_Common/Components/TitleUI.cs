using GT_Common.ProcessConfig;
using GT_Common.Helper.UIHelp;
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
    public partial class TitleUI : UserControl
    {
        private readonly Font _customFont = new Font("Segoe UI", 10f);
        private readonly string _title;
        private readonly string _vison;

        ConnectionStatusUI csuiPLC;
        ConnectionStatusUI csuiServer;
        ConnectionStatusUI csuiSQL;
        ConnectionStatusUI csuiAccess;


        public TitleUI(string processName, string vison)
        {
            _title = processName;
            _vison = vison;
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
                csuiPLC.Status = (IndicatorStatus)Shared.plcSatus;
                csuiServer.Status = (IndicatorStatus)Shared.serverSatus;
                csuiSQL.Status = (IndicatorStatus)Shared.sqlSatus;
                csuiAccess.Status = (IndicatorStatus)Shared.accessSatus;
            }
            catch
            {
                // 忽略异常，防止窗体关闭时报错
            }
        }

        private void InitFormLayout()
        {
            var MainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 243, 248),

                BorderStyle = BorderStyle.FixedSingle,
            };
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                Padding = new Padding(10, 5, 10, 5)

            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));   // Data grid

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));   // TITLE

            //// ===== 左部状态 status =====
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ColorTranslator.FromHtml("#F5F5F5"),
                Padding = new Padding(5, 2, 5, 2)
            };

            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 4,
            };

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent,25));   // Data grid

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent,25));   // TITLE

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));   // TITLE

            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));   // TITLE

            csuiPLC = new ConnectionStatusUI()
            {
                AutoSize = false,
                LabelText = "PLC",
                Font = _customFont,
            };

            csuiServer = new ConnectionStatusUI()
            {
                AutoSize = false,
                LabelText = "后台",
                Font = _customFont,
            };

            csuiSQL = new ConnectionStatusUI()
            {
                AutoSize = false,
                LabelText = "SQL",
                Font = _customFont,
            };

            csuiAccess = new ConnectionStatusUI()
            {
                AutoSize = false,
                LabelText = "Access",
                Font = _customFont,
            };

            leftLayout.Controls.Add(csuiPLC, 0, 0);

            leftLayout.Controls.Add(csuiServer, 1, 0);

            leftLayout.Controls.Add(csuiSQL, 2, 0);

            leftLayout.Controls.Add(csuiAccess, 3, 0);

            leftPanel.Controls.Add(leftLayout);

            // ===== 右部工序名 processName =====

            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = ColorTranslator.FromHtml("#F5F5F5"),

            };

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
            };

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent,70));   // Data grid

            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent,30));   // TITLE

            var lbTtileName = new Label()
            {
                AutoSize = false,
                Text = _title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#333333"),
            };
            var lbVison = new Label()
            {
                Text = "版本号："+_vison,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(0,0,10,0),
                TextAlign = ContentAlignment.MiddleRight,
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = ColorTranslator.FromHtml("#777777"),
            };

            rightLayout.Controls.Add(lbTtileName, 0, 0);

            rightLayout.Controls.Add(lbVison, 0, 1);

            rightPanel.Controls.Add(rightLayout);

            mainLayout.Controls.Add(leftPanel, 0, 0);

            mainLayout.Controls.Add(rightPanel, 1, 0);

            MainPanel.Controls.Add(mainLayout);

            this.Controls.Add(MainPanel);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    
                    UiRefreshCenter.OnRefresh -= RefreshUI; // ⭐防内存泄漏

                    components?.Dispose();
                    csuiPLC?.Dispose();
                    csuiServer?.Dispose();
                    csuiSQL?.Dispose();
                    csuiAccess?.Dispose();
                }
                catch { }
            }
            base.Dispose(disposing);
        }
    }
}
