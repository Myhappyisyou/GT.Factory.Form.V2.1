using GT.Factory.Common.Helpers;
using GT.Factory.Station.EditJson;
using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GT.Factory.Station.DriverForm.Instrument
{
    /// <summary>
    /// 工站仪器配置主界面。
    ///
    /// 负责：
    /// 1. 显示当前工站所有仪器配置。
    /// 2. 新增仪器。
    /// 3. 编辑仪器。
    /// 4. 删除仪器。
    /// 5. 重新从 JSON 加载配置。
    /// 6. 将当前配置保存到 JSON 文件。
    ///
    /// 注意：
    /// 本窗体不修改 BaseParaFileConfiguration。
    /// 配置文件保存由 InstrumentConfigFileStore 负责。
    /// </summary>
    public class InstrumentConfigForm : Form
    {
        /// <summary>
        /// 仪器配置文件路径。
        /// 例如：
        /// OP070\Config\InstrumentConfig.json
        /// </summary>
        private readonly string _configPath = PathCenter.ConfigFile("InstrumentConfig.json");

        /// <summary>
        /// 仪器配置文件存储器。
        /// </summary>
        private readonly InstrumentConfigFileStore _fileStore;

        /// <summary>
        /// 仪器配置列表。
        /// </summary>
        private DataGridView _grid;

        /// <summary>
        /// 新增按钮。
        /// </summary>
        private Button _btnAdd;

        /// <summary>
        /// 编辑按钮。
        /// </summary>
        private Button _btnEdit;

        /// <summary>
        /// 删除按钮。
        /// </summary>
        private Button _btnRemove;

        /// <summary>
        /// 重新加载按钮。
        /// </summary>
        private Button _btnReload;

        /// <summary>
        /// 保存按钮。
        /// </summary>
        private Button _btnSave;

        /// <summary>
        /// 当前配置文件路径显示标签。
        /// </summary>
        private Label _lblPath;

        /// <summary>
        /// 状态信息标签。
        /// </summary>
        private Label _lblStatus;


        /// <summary>
        /// 创建仪器配置主界面。
        /// </summary>
        /// <param name="configPath">
        /// 当前工站的 InstrumentConfig.json 路径。
        /// </param>
        public InstrumentConfigForm()
        {
            if (string.IsNullOrWhiteSpace(_configPath))
            {
                throw new ArgumentException(
                    "仪器配置文件路径不能为空。",
                    nameof(_configPath));
            }

            _fileStore = new InstrumentConfigFileStore(_configPath);

            InitializeForm();
            InitializeControls();
            RefreshGrid();
        }

        /// <summary>
        /// 初始化窗体基础属性。
        /// </summary>
        private void InitializeForm()
        {
            Text = "仪器配置";
            StartPosition = FormStartPosition.CenterScreen;
            Width = 1150;
            Height = 700;
            MinimumSize = new Size(900, 550);

            // 使用统一工业主题作为窗体背景。
            BackColor = UITheme.BgBase;

            // 使用统一字体。
            Font = UITheme.BodyFont;

            // 配置窗口不需要复杂的边框效果。
            FormBorderStyle = FormBorderStyle.Sizable;
        }

        /// <summary>
        /// 创建并布局主界面控件。
        /// </summary>
        private void InitializeControls()
        {
            // 创建顶部标题区域。
            Panel headerPanel = CreateHeaderPanel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 82;
            Controls.Add(headerPanel);

            // 创建底部操作区域。
            Panel bottomPanel = CreateBottomPanel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 70;
            Controls.Add(bottomPanel);

            // 创建中央仪器列表。
            _grid = CreateGrid();
            _grid.Dock = DockStyle.Fill;

            // 使用统一表格样式。
            UIGrid.BuildDataGrid(_grid);

            Controls.Add(_grid);

            // 保证列表位于标题和底部按钮之间。
            _grid.BringToFront();
        }

        /// <summary>
        /// 创建顶部标题区域。
        /// </summary>
        private Panel CreateHeaderPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = UITheme.BgCard;
            panel.Padding = new Padding(20, 12, 20, 10);

            Label title = new Label();
            title.Text = "仪器配置";
            title.AutoSize = true;
            title.Font = UITheme.HeaderFont;
            title.ForeColor = UITheme.TextMain;
            title.Location = new Point(20, 10);

            _lblPath = new Label();
            _lblPath.Text = "配置文件：" + _configPath;
            _lblPath.AutoSize = false;
            _lblPath.Width = 900;
            _lblPath.Height = 25;
            _lblPath.Font = UITheme.SmallFont;
            _lblPath.ForeColor = UITheme.TextMuted;
            _lblPath.Location = new Point(22, 48);

            panel.Controls.Add(title);
            panel.Controls.Add(_lblPath);

            return panel;
        }

        /// <summary>
        /// 创建底部操作区域。
        /// </summary>
        /// <summary>
        /// 创建主配置界面底部操作区域。
        ///
        /// 左侧显示当前配置状态，
        /// 右侧显示新增、编辑、删除、重新加载、保存按钮。
        ///
        /// 使用 TableLayoutPanel 进行布局，
        /// 避免 Dock + FlowLayoutPanel 组合造成按钮位置异常。
        /// </summary>
        private Panel CreateBottomPanel()
        {
            Panel panel = new Panel();

            panel.BackColor = UITheme.BgCard;
            panel.Padding = new Padding(20, 12, 20, 12);

            // 创建底部两列布局。
            TableLayoutPanel layout = new TableLayoutPanel();

            layout.Dock = DockStyle.Fill;

            layout.ColumnCount = 2;
            layout.RowCount = 1;

            // 左侧状态区域自动占用剩余空间。
            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    30F));

            // 右侧固定用于放置操作按钮。
            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    70F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            layout.BackColor = UITheme.BgCard;

            // 创建状态文字。
            _lblStatus = new Label();

            _lblStatus.Text = "就绪";
            _lblStatus.Dock = DockStyle.Fill;

            _lblStatus.TextAlign =
                ContentAlignment.MiddleLeft;

            _lblStatus.Font = UITheme.SmallFont;
            _lblStatus.ForeColor = UITheme.TextMuted;

            // 创建按钮。
            _btnAdd = UIButton.Primary("新增仪器");
            _btnAdd.Width = 105;
            _btnAdd.Click += BtnAdd_Click;

            _btnEdit = UIButton.Default("编辑仪器");
            _btnEdit.Width = 105;
            _btnEdit.Click += BtnEdit_Click;

            _btnRemove = UIButton.Ghost("删除仪器");
            _btnRemove.Width = 105;
            _btnRemove.Click += BtnRemove_Click;

            _btnReload = UIButton.Default("重新加载");
            _btnReload.Width = 105;
            _btnReload.Click += BtnReload_Click;

            _btnSave = UIButton.Primary("保存配置");
            _btnSave.Width = 105;
            _btnSave.Click += BtnSave_Click;

            // 创建按钮区域。
            FlowLayoutPanel buttonPanel =
                new FlowLayoutPanel();

            buttonPanel.Dock = DockStyle.Fill;

            buttonPanel.FlowDirection =
                FlowDirection.RightToLeft;

            buttonPanel.WrapContents = false;

            buttonPanel.Margin = new Padding(0);
            buttonPanel.Padding = new Padding(0);

            buttonPanel.BackColor = UITheme.BgCard;

            // 所有按钮统一垂直居中。
            buttonPanel.Controls.Add(
                CreateButtonSpacing());

            buttonPanel.Controls.Add(_btnAdd);

            buttonPanel.Controls.Add(
                CreateButtonSpacing());

            buttonPanel.Controls.Add(_btnEdit);

            buttonPanel.Controls.Add(
                CreateButtonSpacing());

            buttonPanel.Controls.Add(_btnRemove);

            buttonPanel.Controls.Add(
                CreateButtonSpacing());

            buttonPanel.Controls.Add(_btnReload);

            buttonPanel.Controls.Add(
                CreateButtonSpacing());

            buttonPanel.Controls.Add(_btnSave);

            layout.Controls.Add(
                _lblStatus,
                0,
                0);

            layout.Controls.Add(
                buttonPanel,
                1,
                0);

            panel.Controls.Add(layout);

            return panel;
        }
        /// <summary>
        /// 创建按钮之间的统一间距。
        /// </summary>
        private Control CreateButtonSpacing()
        {
            Panel spacing = new Panel();
            spacing.Width = UITheme.ControlSpacing;
            spacing.Height = UITheme.ControlHeight;
            return spacing;
        }

        /// <summary>
        /// 创建仪器 DataGridView。
        /// </summary>
        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView();

            grid.Dock = DockStyle.Fill;

            // 配置界面不允许用户直接在表格中修改数据。
            grid.ReadOnly = true;

            // 一次选择整行，方便编辑和删除。
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.MultiSelect = false;

            // 不允许用户直接增加或删除行。
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;

            // 禁止用户修改行高。
            grid.AllowUserToResizeRows = false;

            // 使用手工列，避免自动根据对象属性产生不需要的列。
            grid.AutoGenerateColumns = false;

            // 双击一行直接进入编辑。
            grid.CellDoubleClick += Grid_CellDoubleClick;

            // 选中行以后更新按钮状态。
            grid.SelectionChanged += Grid_SelectionChanged;

            // 添加列。
            AddTextColumn(
                grid,
                "Name",
                "仪器名称",
                150);

            AddTextColumn(
                grid,
                "DriverKey",
                "驱动标识",
                130);

            AddTextColumn(
                grid,
                "Model",
                "仪器型号",
                130);

            AddTextColumn(
                grid,
                "SerialNumber",
                "序列号",
                150);

            AddTextColumn(
                grid,
                "CommunicationType",
                "通讯类型",
                90);

            AddTextColumn(
                grid,
                "Ip",
                "IP地址",
                140);

            AddTextColumn(
                grid,
                "Port",
                "端口",
                80);

            AddTextColumn(
                grid,
                "SendTimeout",
                "发送超时",
                90);

            AddTextColumn(
                grid,
                "ReceiveTimeout",
                "接收超时",
                90);

            return grid;
        }

        /// <summary>
        /// 向 DataGridView 添加普通文本列。
        /// </summary>
        private void AddTextColumn(
            DataGridView grid,
            string name,
            string headerText,
            int width)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            column.Name = name;
            column.HeaderText = headerText;
            column.DataPropertyName = name;
            column.Width = width;

            // 配置表格中的数据统一居中显示。
            column.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            grid.Columns.Add(column);
        }

        /// <summary>
        /// 刷新仪器列表。
        /// </summary>
        private void RefreshGrid()
        {
            _grid.Rows.Clear();

            InstrumentConfig.Initialize(PathCenter.ConfigFile(_configPath));
           


            InstrumentConfig config =
                InstrumentConfig.Instance;

            if (config == null ||
                config.Instruments == null)
            {
                UpdateStatus("当前没有仪器配置。");
                UpdateButtonState();
                return;
            }

            foreach (InstrumentDeviceConfig instrument
                in config.Instruments)
            {
                if (instrument == null)
                    continue;

                InstrumentCommunicationConfig communication =
                    instrument.Communication;

                string type =
                    communication == null
                        ? string.Empty
                        : communication.Type;

                string ip =
                    communication == null
                        ? string.Empty
                        : communication.Ip;

                string port =
                    communication == null
                        ? string.Empty
                        : communication.Port.ToString();

                string sendTimeout =
                    communication == null
                        ? string.Empty
                        : communication.SendTimeout.ToString();

                string receiveTimeout =
                    communication == null
                        ? string.Empty
                        : communication.ReceiveTimeout.ToString();

                _grid.Rows.Add(
                    instrument.Name,
                    instrument.DriverKey,
                    instrument.Model,
                    instrument.SerialNumber,
                    type,
                    ip,
                    port,
                    sendTimeout,
                    receiveTimeout);
            }

            UpdateStatus(
                "共 " +
                _grid.Rows.Count +
                " 台仪器。");

            UpdateButtonState();
        }

        /// <summary>
        /// 获取当前选中的仪器配置。
        /// </summary>
        private InstrumentDeviceConfig GetSelectedInstrument()
        {
            if (_grid.SelectedRows.Count == 0)
                return null;

            DataGridViewRow row =
                _grid.SelectedRows[0];

            if (row.Index < 0)
                return null;

            InstrumentConfig config =
                InstrumentConfig.Instance;

            if (config == null ||
                config.Instruments == null)
            {
                return null;
            }

            if (row.Index >= config.Instruments.Count)
                return null;

            return config.Instruments[row.Index];
        }

        /// <summary>
        /// 新增仪器。
        /// </summary>
        private void BtnAdd_Click(
            object sender,
            EventArgs e)
        {
            InstrumentConfigEditForm form =
                new InstrumentConfigEditForm(
                    null,
                    false);

            try
            {
                if (form.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                InstrumentDeviceConfig config =
                    form.Result;

                InstrumentConfig.Instance.Add(config);

                RefreshGrid();

                // 新增以后自动选中最后一行。
                if (_grid.Rows.Count > 0)
                {
                    _grid.ClearSelection();

                    _grid.Rows[
                        _grid.Rows.Count - 1]
                        .Selected = true;
                }

                UpdateStatus(
                    "已新增仪器：" +
                    config.Name +
                    "，请保存配置。");
            }
            finally
            {
                form.Dispose();
            }
        }

        /// <summary>
        /// 编辑当前选中的仪器。
        /// </summary>
        private void BtnEdit_Click(
            object sender,
            EventArgs e)
        {
            InstrumentDeviceConfig selected =
                GetSelectedInstrument();

            if (selected == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一台仪器。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            InstrumentConfigEditForm form =
                new InstrumentConfigEditForm(
                    selected,
                    true);

            try
            {
                if (form.ShowDialog(this) !=
                    DialogResult.OK)
                {
                    return;
                }

                // 编辑时保留原来的 Name，
                // 因为 InstrumentConfig.Update 是按 Name 查找原对象。
                InstrumentDeviceConfig config =
                    form.Result;

                InstrumentConfig.Instance.Update(config);

                RefreshGrid();

                // 编辑后重新选中对应仪器。
                SelectInstrument(config.Name);

                UpdateStatus(
                    "已修改仪器：" +
                    config.Name +
                    "，请保存配置。");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "修改仪器失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                form.Dispose();
            }
        }

        /// <summary>
        /// 删除当前选中的仪器。
        /// </summary>
        private void BtnRemove_Click(
            object sender,
            EventArgs e)
        {
            InstrumentDeviceConfig selected =
                GetSelectedInstrument();

            if (selected == null)
            {
                MessageBox.Show(
                    this,
                    "请先选择一台仪器。",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult result =
                MessageBox.Show(
                    this,
                    "确定删除仪器：\r\n\r\n" +
                    selected.Name +
                    "\r\n\r\n删除后需要点击“保存配置”才会写入 JSON。",
                    "确认删除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                bool removed =
                    InstrumentConfig.Instance.Remove(
                        selected.Name);

                if (!removed)
                {
                    MessageBox.Show(
                        this,
                        "删除失败，未找到仪器：" +
                        selected.Name,
                        "删除失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                RefreshGrid();

                UpdateStatus(
                    "已删除仪器：" +
                    selected.Name +
                    "，请保存配置。");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "删除仪器失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 从 JSON 文件重新加载配置。
        ///
        /// 注意：
        /// 未保存的内存修改会被丢弃。
        /// </summary>
        private void BtnReload_Click(
            object sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    this,
                    "重新加载会丢弃当前尚未保存的修改。\r\n\r\n确定重新加载吗？",
                    "重新加载",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                InstrumentConfig.Reload();

                RefreshGrid();

                UpdateStatus(
                    "配置已从 JSON 重新加载。");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "重新加载配置失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 保存当前仪器配置。
        /// </summary>
        private void BtnSave_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                InstrumentConfig config =
                    InstrumentConfig.Instance;

                // FileStore 内部会再次执行 Validate。
                _fileStore.Save(config);

                UpdateStatus(
                    "配置保存成功：" +
                    _configPath);

                MessageBox.Show(
                    this,
                    "仪器配置保存成功。",
                    "保存成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "保存仪器配置失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 双击仪器列表进入编辑。
        /// </summary>
        private void Grid_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            BtnEdit_Click(sender, e);
        }

        /// <summary>
        /// 选择发生变化时更新按钮状态。
        /// </summary>
        private void Grid_SelectionChanged(
            object sender,
            EventArgs e)
        {
            UpdateButtonState();
        }

        /// <summary>
        /// 更新编辑和删除按钮状态。
        /// </summary>
        private void UpdateButtonState()
        {
            bool hasSelection =
                _grid != null &&
                _grid.SelectedRows.Count > 0;

            if (_btnEdit != null)
                _btnEdit.Enabled = hasSelection;

            if (_btnRemove != null)
                _btnRemove.Enabled = hasSelection;
        }

        /// <summary>
        /// 根据仪器名称选中对应行。
        /// </summary>
        private void SelectInstrument(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            for (int i = 0; i < _grid.Rows.Count; i++)
            {
                object value =
                    _grid.Rows[i].Cells["Name"].Value;

                if (value == null)
                    continue;

                if (string.Equals(
                    value.ToString(),
                    name,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _grid.ClearSelection();
                    _grid.Rows[i].Selected = true;

                    if (_grid.Columns.Count > 0)
                    {
                        _grid.CurrentCell =
                            _grid.Rows[i].Cells[0];
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// 更新底部状态文字。
        /// </summary>
        private void UpdateStatus(string text)
        {
            if (_lblStatus != null)
                _lblStatus.Text = text;
        }

        /// <summary>
        /// 关闭窗口时释放表格等资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_grid != null)
                {
                    _grid.Dispose();
                    _grid = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
