using System;
using System.Drawing;
using System.Windows.Forms;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;

namespace GT.Factory.Server
{
    /// <summary>
    /// Server 默认首页：生产总览。
    /// 该控件只负责展示当前生产运行状态，不负责切换工单或执行生产操作。
    /// </summary>
    public class ProductionDashboardControl : UserControl
    {
        private readonly TableLayoutPanel _root;
        private readonly TableLayoutPanel _topArea;
        private readonly TableLayoutPanel _statisticsArea;
        private readonly DataGridView _stationGrid;
        private readonly TableLayoutPanel _serviceArea;

        public ProductionDashboardControl()
        {
            BackColor = UITheme.Background;
            Dock = DockStyle.Fill;
            Padding = new Padding(0, 0, 0, 0);

            _root = CreateRootLayout();
            _topArea = CreateTopArea();
            _statisticsArea = CreateStatisticsArea();
            _stationGrid = CreateStationGrid();
            _serviceArea = CreateServiceArea();

            BuildLayout();
            LoadDemoData();
        }

        /// <summary>
        /// 创建首页总布局。
        /// </summary>
        private TableLayoutPanel CreateRootLayout()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 1;
            panel.RowCount = 3;
            panel.Padding = new Padding(0, 0, 0, 0);
            panel.BackColor = UITheme.Background;

            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            //panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));

            return panel;
        }

        /// <summary>
        /// 创建当前生产任务和生产统计区域。
        /// </summary>
        private TableLayoutPanel CreateTopArea()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 2;
            panel.RowCount = 1;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            panel.Padding = new Padding(0, 0, 0, 8);
            return panel;
        }

        /// <summary>
        /// 创建生产统计卡片区域。
        /// </summary>
        private TableLayoutPanel CreateStatisticsArea()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 4;
            panel.RowCount = 1;
            panel.Padding = new Padding(0, 0, 0, 8);

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            return panel;
        }

        /// <summary>
        /// 创建 Station 状态表格。
        /// </summary>
        private DataGridView CreateStationGrid()
        {
            DataGridView grid = new DataGridView();
            UIGrid.Management(grid);
            grid.Dock = DockStyle.Fill;
            grid.AutoGenerateColumns = false;
            grid.RowTemplate.Height = 34;
            grid.ColumnHeadersHeight = 36;

            AddGridColumn(grid, "Station", "Station", 110);
            AddGridColumn(grid, "Connection", "连接状态", 120);
            AddGridColumn(grid, "RunState", "运行状态", 120);
            AddGridColumn(grid, "Output", "产出数量", 120);
            AddGridColumn(grid, "Alarm", "当前状态 / 报警", 180);
            AddGridColumn(grid, "UpdateTime", "更新时间", 150);

            return grid;
        }

        /// <summary>
        /// 创建 Server 服务状态区域。
        /// </summary>
        private TableLayoutPanel CreateServiceArea()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.ColumnCount = 4;
            panel.RowCount = 1;
            panel.Padding = new Padding(0, 8, 0, 0);

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            return panel;
        }

        /// <summary>
        /// 组装首页布局。
        /// </summary>
        private void BuildLayout()
        {
            Controls.Add(_root);

            _root.Controls.Add(_topArea, 0, 0);
            //_root.Controls.Add(_statisticsArea, 0, 1);
            _root.Controls.Add(CreateStationPanel(), 0, 1);
            _root.Controls.Add(_serviceArea, 0, 2);

            _topArea.Controls.Add(CreateProductionTaskPanel(), 0, 0);
            _topArea.Controls.Add(CreateStatisticsTitlePanel(), 1, 0);
        }

        /// <summary>
        /// 创建当前生产任务面板。
        /// </summary>
        private Control CreateProductionTaskPanel()
        {
            Panel panel = CreateCardPanel();

            Label title = UILabel.Title("当前生产任务");
            title.Location = new Point(18, 12);
            title.Width = 180;
            title.Height = 30;
            panel.Controls.Add(title);

            Label detail = new Label();
            detail.Text =
                "工单号：WO20260924001\r\n" +
                "型号：GTB2609\r\n" +
                "状态：生产中\r\n" +
                "开始时间：08:32:15";
            detail.Location = new Point(22, 45);
            detail.Size = new Size(430, 85);
            detail.Font = UITheme.FontNormal;
            detail.ForeColor = UITheme.Text;
            detail.AutoSize = false;
            panel.Controls.Add(detail);

            Label state = new Label();
            state.Text = "生产中";
            state.BackColor = Color.FromArgb(220, 247, 232);
            state.ForeColor = Color.FromArgb(20, 130, 75);
            state.TextAlign = ContentAlignment.MiddleCenter;
            state.Width = 70;
            state.Height = 28;
            state.Location = new Point(500, 48);
            panel.Controls.Add(state);

            return panel;
        }

        /// <summary>
        /// 创建统计区域标题面板。
        /// </summary>
        private Control CreateStatisticsTitlePanel()
        {
            Panel panel = CreateCardPanel();

            Label title = UILabel.Title("生产统计");
            title.Dock = DockStyle.Top;
            title.Width = 150;
            title.Height = 30;

            TableLayoutPanel cards = new TableLayoutPanel();
            cards.ColumnCount = 4;
            cards.RowCount = 1;
            cards.Dock = DockStyle.Fill;
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            cards.Controls.Add(CreateStatisticCard("计划数量", "5000", UITheme.Primary), 0, 0);
            cards.Controls.Add(CreateStatisticCard("已生产", "1286", UITheme.Success), 1, 0);
            cards.Controls.Add(CreateStatisticCard("OK", "1268", UITheme.Success), 2, 0);
            cards.Controls.Add(CreateStatisticCard("NG", "18", UITheme.Danger), 3, 0);

            panel.Controls.Add(cards);

            panel.Controls.Add(title);

            return panel;
        }

        /// <summary>
        /// 创建单个统计卡片。
        /// </summary>
        private Control CreateStatisticCard(string title, string value, Color valueColor)
        {
            Panel outPanel = new Panel();
            outPanel.Dock = DockStyle.Fill;
            outPanel.Padding = new Padding(1);
            outPanel.BackColor = UITheme.TextMuted;


            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = UITheme.Card;
            var titleLabel = UILabel.ValueLabel(title);
         
            titleLabel.Dock = DockStyle.Top;
          

            titleLabel.ForeColor = UITheme.Text;
            //titleLabel.Font = UITheme.FontNormal;

            Label valueLabel = new Label();
            valueLabel.Text = value;
            valueLabel.Dock = DockStyle.Bottom;
         
            valueLabel.ForeColor = valueColor;
            valueLabel.Font = new Font(UITheme.FontNormal.FontFamily, 15, FontStyle.Bold);
            panel.Controls.Add(valueLabel);
            panel.Controls.Add(titleLabel);
            outPanel.Controls.Add(panel);
            return outPanel;
        }

        /// <summary>
        /// 创建 Station 状态区域。
        /// </summary>
        private Control CreateStationPanel()
        {
            Panel panel = CreateCardPanel();
            panel.Padding = new Padding(12, 10, 12, 10);

            Label title = UILabel.Title("Station 状态");
            title.Dock = DockStyle.Top;
            title.Height = 32;

            _stationGrid.Dock = DockStyle.Fill;
            panel.Controls.Add(_stationGrid);
            panel.Controls.Add(title);

            return panel;
        }

        /// <summary>
        /// 创建 Server 服务状态卡片。
        /// </summary>
        private void BuildServiceCards()
        {
            _serviceArea.Controls.Add(CreateServiceCard("API", "正常"), 0, 0);
            _serviceArea.Controls.Add(CreateServiceCard("数据库", "正常"), 1, 0);
            _serviceArea.Controls.Add(CreateServiceCard("MES", "正常"), 2, 0);
            _serviceArea.Controls.Add(CreateServiceCard("TaskQueue", "正常"), 3, 0);
        }

        /// <summary>
        /// 创建服务状态卡片。
        /// </summary>
        private Control CreateServiceCard(string serviceName, string status)
        {
            Panel panel = CreateCardPanel();
            panel.Margin = new Padding(3, 3, 3, 3);

            Label name = UILabel.Title(serviceName);
            name.Location = new Point(16, 10);
            name.Width = 120;
            name.Height = 28;
            panel.Controls.Add(name);

            Label state = new Label();
            state.Text = "● " + status;
            state.Location = new Point(16, 42);
            state.Width = 120;
            state.Height = 25;
            state.ForeColor = Color.FromArgb(20, 140, 80);
            state.Font = UITheme.FontNormal;
            panel.Controls.Add(state);

            return panel;
        }

        /// <summary>
        /// 创建统一信息卡片容器。
        /// </summary>
        private Panel CreateCardPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = UITheme.Card;
            panel.BorderStyle = BorderStyle.FixedSingle;
            return panel;
        }

        /// <summary>
        /// 添加 DataGridView 文本列。
        /// </summary>
        private void AddGridColumn(DataGridView grid, string name, string header, int width)
        {
            DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = header;
            column.Width = width;
            column.ReadOnly = true;
            column.SortMode = DataGridViewColumnSortMode.NotSortable;
            grid.Columns.Add(column);
        }

        /// <summary>
        /// 当前阶段加载模拟数据。
        /// 后续由 ServerRuntime / StationRuntime 提供真实数据。
        /// </summary>
        private void LoadDemoData()
        {
            _stationGrid.Rows.Clear();

            AddStationRow("OP010", "● 在线", "运行", "1286", "正常", "08:32:10");
            AddStationRow("OP020", "● 在线", "运行", "1286", "正常", "08:32:08");
            AddStationRow("OP030", "● 在线", "待机", "1285", "正常", "08:32:05");
            AddStationRow("OP040", "● 在线", "报警", "1284", "气密 NG", "08:32:02");
            AddStationRow("OP050", "● 离线", "---", "---", "通信异常", "08:31:50");

            BuildServiceCards();
        }

        /// <summary>
        /// 添加 Station 示例行。
        /// </summary>
        private void AddStationRow(string station, string connection, string runState,
            string output, string alarm, string updateTime)
        {
            _stationGrid.Rows.Add(station, connection, runState, output, alarm, updateTime);
        }

        /// <summary>
        /// 刷新当前生产任务信息。
        /// 后续可以由 ServerRuntime 定时或事件驱动调用。
        /// </summary>
        public void UpdateProductionTask(string workOrderNo, string productModel,
            string status, DateTime startTime)
        {
            // 这里保留扩展入口，实际项目中建议把当前生产上下文作为独立 ViewModel 注入。
            // 当前版本先保持首页 UI 与运行时服务解耦。
        }
    }
}
