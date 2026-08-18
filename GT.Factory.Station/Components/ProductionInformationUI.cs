using GT.Factory.UI;
using GT.Factory.UI.Apply;
using GT.Factory.Station.DriverForm.DataDetail;
using GT.Factory.Station.Helper;
using GT.Factory.Station.Helper.UIHelp;
using GT.Factory.Station.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Station.Components
{
    public partial class ProductionInformationUI : UserControl
    {
        private readonly Font _customFont = new Font("Segoe UI", 10f);

        public  MergedDataGridView gridView;


        public ProductionInformationUI()
        {
            InitializeComponent();
            InitFormLayout();

            //UIStyle.ApplyWidget(this);

            // 监听值变化
            Shared.productInformationDataList[0].PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Col1")
                {
                    Shared.productInformationDataList[6].Col1 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            };

            //gridView.CellForeColorProvider = (row, col) =>
            //{
            //    if (row == 4 && col == 1) // Col3
            //    {
            //        return Color.Green;
            //    }

            //    return null;
            //};

            gridView.MouseDoubleClick += GridView_MouseDoubleClick;

            UiRefreshCenter.OnRefresh += RefreshUI;



            gridView.DataBindingComplete += (sender, e) => gridView.ClearSelection();
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
                // 安全访问 Shared 数据并同步到 DataGridView
                if (Shared.productInformationDataList[0].Col1 != Shared.totalNub)
                    Shared.productInformationDataList[0].Col1 = Shared.totalNub;

                if (Shared.productInformationDataList[1].Col1 != Shared.workOrder)
                    Shared.productInformationDataList[1].Col1 = Shared.workOrder;

                if (Shared.productInformationDataList[2].Col1 != Shared.orderNub)
                    Shared.productInformationDataList[2].Col1 = Shared.orderNub;

                if (Shared.productInformationDataList[3].Col1 != Shared.finishNub)
                    Shared.productInformationDataList[3].Col1 = Shared.finishNub;

                if (Shared.productInformationDataList[4].Col1 != Shared.okNub)
                    Shared.productInformationDataList[4].Col1 = Shared.okNub;

                if (Shared.productInformationDataList[5].Col1 != Shared.rate)
                    Shared.productInformationDataList[5].Col1 = Shared.rate;
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

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));   // TITLE

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // Data grid

            var lbTitleName = new Label()
            {
                AutoSize = false,
                Text = "生产\n信息",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                //Font = _customFont,
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(230, 235, 240),
                ForeColor = Color.FromArgb(40, 40, 40),
            };

            //var lbTtileName = new UIVerticalTitle("生产信息");


            gridView = new MergedDataGridView()
            {
                Dock = DockStyle.Fill,
                DataSource = Shared.productInformationDataList,
                MaxRowsToStretch = 8,
                BorderStyle = BorderStyle.Fixed3D,
                BackgroundColor = Color.White,
            };

            // 禁止用户选中行
            gridView.DefaultCellStyle.SelectionBackColor = gridView.DefaultCellStyle.BackColor;
            gridView.DefaultCellStyle.SelectionForeColor = gridView.DefaultCellStyle.ForeColor;

            //UIGrid.ApplyGrid(gridView);

            mainLayout.Controls.Add(lbTitleName, 0, 0);

            mainLayout.Controls.Add(gridView, 1, 0);

            mainPanle.Controls.Add(mainLayout);

            this.Controls.Add(mainPanle);
        }

        private void GridView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var wo = Shared.workOrder;

            var form = new ProductionDetailForm(
                Shared.monitor,
                wo);

            form.Show(this); // 非模态
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                UiRefreshCenter.OnRefresh -= RefreshUI;

                components?.Dispose();
                gridView?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
