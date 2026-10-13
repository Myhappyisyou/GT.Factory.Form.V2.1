using System;
using System.Drawing;
using System.Windows.Forms;
using GT.Factory.Station.ProcessConfig.InstrumentHandlers.Configuration;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using GT.Factory.UI.Component;

namespace GT.Factory.Station.DriverForm.Instrument
{
    /// <summary>
    /// 单台仪器配置编辑窗口。
    ///
    /// 负责编辑：
    /// 1. 仪器名称。
    /// 2. 驱动标识。
    /// 3. 仪器型号。
    /// 4. 序列号。
    /// 5. 通讯类型。
    /// 6. IP地址。
    /// 7. 端口。
    /// 8. 发送超时。
    /// 9. 接收超时。
    ///
    /// 本窗口只负责编辑配置对象，
    /// 不负责保存 JSON 文件。
    /// </summary>
    public class InstrumentConfigEditForm : Form
    {
        /// <summary>
        /// 是否处于编辑模式。
        /// </summary>
        private readonly bool _editMode;

        /// <summary>
        /// 原始仪器配置。
        /// </summary>
        private readonly InstrumentDeviceConfig _source;

        /// <summary>
        /// 编辑完成以后返回的配置对象。
        /// </summary>
        public InstrumentDeviceConfig Result { get; private set; }

        /// <summary>
        /// 仪器名称输入行。
        /// </summary>
        private UIFormRow<TextBox> _nameRow;

        /// <summary>
        /// 驱动标识输入行。
        /// </summary>
        private UIFormRow<TextBox> _driverKeyRow;

        /// <summary>
        /// 仪器型号输入行。
        /// </summary>
        private UIFormRow<TextBox> _modelRow;

        /// <summary>
        /// 序列号输入行。
        /// </summary>
        private UIFormRow<TextBox> _serialNumberRow;

        /// <summary>
        /// 通讯类型输入行。
        /// </summary>
        private UIFormRow<ComboBox> _typeRow;

        /// <summary>
        /// IP地址输入行。
        /// </summary>
        private UIFormRow<TextBox> _ipRow;

        /// <summary>
        /// 端口输入行。
        /// </summary>
        private UIFormRow<TextBox> _portRow;

        /// <summary>
        /// 发送超时输入行。
        /// </summary>
        private UIFormRow<TextBox> _sendTimeoutRow;

        /// <summary>
        /// 接收超时输入行。
        /// </summary>
        private UIFormRow<TextBox> _receiveTimeoutRow;

        /// <summary>
        /// 确定按钮。
        /// </summary>
        private Button _btnOk;

        /// <summary>
        /// 取消按钮。
        /// </summary>
        private Button _btnCancel;

        /// <summary>
        /// 创建仪器编辑窗口。
        /// </summary>
        /// <param name="source">
        /// 原始仪器配置。
        /// 新增时传 null。
        /// </param>
        /// <param name="editMode">
        /// true 表示编辑，false 表示新增。
        /// </param>
        public InstrumentConfigEditForm(
            InstrumentDeviceConfig source,
            bool editMode)
        {
            _source = source;
            _editMode = editMode;

            InitializeForm();
            InitializeControls();

            if (_editMode)
            {
                LoadSource();
            }
            else
            {
                LoadDefault();
            }
        }

        /// <summary>
        /// 初始化窗体。
        /// </summary>
        private void InitializeForm()
        {
            Text = _editMode
                ? "编辑仪器"
                : "新增仪器";

            StartPosition =
                FormStartPosition.CenterParent;

            Width = 560;
            Height = 710;

            MinimumSize =
                new Size(520, 550);

            MaximumSize =
                new Size(700, 800);

            BackColor = UITheme.BgBase;
            Font = UITheme.BodyFont;

            FormBorderStyle =
                FormBorderStyle.FixedDialog;

            MaximizeBox = false;
            MinimizeBox = false;

            ShowInTaskbar = false;
        }

        /// <summary>
        /// 创建编辑界面。
        /// </summary>
        private void InitializeControls()
        {
            Panel header = CreateHeaderPanel();
            header.Dock = DockStyle.Top;
            header.Height = 70;

            Controls.Add(header);

            Panel bottom = CreateBottomPanel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 70;

            Controls.Add(bottom);

            Panel content = CreateContentPanel();
            content.Dock = DockStyle.Fill;

            Controls.Add(content);

            // 保证内容区域位于顶部和底部区域之间。
            content.BringToFront();
        }

        /// <summary>
        /// 创建顶部标题区域。
        /// </summary>
        private Panel CreateHeaderPanel()
        {
            Panel panel = new Panel();

            panel.BackColor = UITheme.BgCard;
            panel.Padding =
                new Padding(20, 15, 20, 10);

            Label title = new Label();

            title.Text = _editMode
                ? "编辑仪器"
                : "新增仪器";

            title.AutoSize = true;
            title.Font = UITheme.HeaderFont;
            title.ForeColor = UITheme.TextMain;
            title.Location =
                new Point(20, 15);

            panel.Controls.Add(title);

            return panel;
        }

        /// <summary>
        /// 创建参数编辑区域。
        ///
        /// 使用 UIFormRow<T> 统一处理：
        /// 标签 + 输入控件。
        ///
        /// 这样 InstrumentConfigEditForm
        /// 不再重复实现表单行布局。
        /// </summary>
        private Panel CreateContentPanel()
        {
            Panel panel = new Panel();

            panel.BackColor = UITheme.BgBase;
            panel.Padding =
                new Padding(25, 15, 25, 15);

            TableLayoutPanel table =
                new TableLayoutPanel();

            table.Dock = DockStyle.Fill;
            table.ColumnCount = 1;
            table.RowCount = 10;

            table.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            // 基本信息四行。
            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            // 通讯配置标题。
            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    38));

            // 通讯参数五行。
            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            table.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    46));

            // 创建基本信息输入控件。
            TextBox txtName = UIInput.Text();
            TextBox txtDriverKey = UIInput.Text();
            TextBox txtModel = UIInput.Text();
            TextBox txtSerialNumber = UIInput.Text();

            // 创建通讯类型下拉框。
            ComboBox cmbType = UIInput.Combo();

            cmbType.Items.Add("TCP");
            cmbType.Items.Add("SerialPort");
            cmbType.SelectedIndex = 0;

            // 创建通讯参数输入控件。
            TextBox txtIp = UIInput.Text();
            TextBox txtPort = UIInput.Text();
            TextBox txtSendTimeout = UIInput.Text();
            TextBox txtReceiveTimeout = UIInput.Text();

            // 使用 UIFormRow 统一包装每一个表单字段。
            _nameRow =
                new UIFormRow<TextBox>(
                    "仪器名称",
                    txtName);

            _driverKeyRow =
                new UIFormRow<TextBox>(
                    "驱动标识",
                    txtDriverKey);

            _modelRow =
                new UIFormRow<TextBox>(
                    "仪器型号",
                    txtModel);

            _serialNumberRow =
                new UIFormRow<TextBox>(
                    "序列号",
                    txtSerialNumber);

            _typeRow =
                new UIFormRow<ComboBox>(
                    "通讯类型",
                    cmbType);

            _ipRow =
                new UIFormRow<TextBox>(
                    "IP地址",
                    txtIp);

            _portRow =
                new UIFormRow<TextBox>(
                    "端口",
                    txtPort);

            _sendTimeoutRow =
                new UIFormRow<TextBox>(
                    "发送超时",
                    txtSendTimeout);

            _receiveTimeoutRow =
                new UIFormRow<TextBox>(
                    "接收超时",
                    txtReceiveTimeout);

            // 编辑模式下不允许修改 Name。
            // 因为 InstrumentConfig.Update 当前按 Name 查找原配置。
            if (_editMode)
            {
                txtName.ReadOnly = true;
                txtName.BackColor = UITheme.BgCard;
                txtName.ForeColor = UITheme.TextMuted;
            }

            // 将表单行添加到主布局。
            AddFormRow(
                table,
                _nameRow,
                0);

            AddFormRow(
                table,
                _driverKeyRow,
                1);

            AddFormRow(
                table,
                _modelRow,
                2);

            AddFormRow(
                table,
                _serialNumberRow,
                3);

            // 添加通讯配置分组标题。
            Label sectionLabel =
                CreateSectionLabel("通讯配置");

            table.Controls.Add(
                sectionLabel,
                0,
                4);

            AddFormRow(
                table,
                _typeRow,
                5);

            AddFormRow(
                table,
                _ipRow,
                6);

            AddFormRow(
                table,
                _portRow,
                7);

            AddFormRow(
                table,
                _sendTimeoutRow,
                8);

            AddFormRow(
                table,
                _receiveTimeoutRow,
                9);

            panel.Controls.Add(table);

            return panel;
        }

        /// <summary>
        /// 将 UIFormRow 添加到主表格。
        ///
        /// UIFormRow 自身已经负责：
        /// 标签宽度、输入控件 Dock、内部布局。
        ///
        /// 这里仅负责让它适应当前编辑窗口宽度。
        /// </summary>
        private void AddFormRow(
            TableLayoutPanel table,
            Control row,
            int index)
        {
            row.Dock = DockStyle.Fill;
            row.Margin =
                new Padding(0, 2, 0, 2);

            table.Controls.Add(
                row,
                0,
                index);
        }

        /// <summary>
        /// 创建通讯配置分组标题。
        /// </summary>
        private Label CreateSectionLabel(string title)
        {
            Label label = new Label();

            label.Text = title;
            label.Dock = DockStyle.Fill;
            label.TextAlign =
                ContentAlignment.MiddleLeft;

            label.Font = UITheme.BodyFont;
            label.ForeColor = UITheme.CyanGlow;

            return label;
        }

        /// <summary>
        /// 创建编辑窗口底部操作区域。
        ///
        /// 使用 TableLayoutPanel 直接控制按钮位置，
        /// 不再嵌套 FlowLayoutPanel。
        ///
        /// 这样可以避免窗口宽度变化时按钮出现位置异常。
        /// </summary>
        private Panel CreateBottomPanel()
        {
            Panel panel = new Panel();

            panel.BackColor = UITheme.BgCard;
            panel.Padding =
                new Padding(20, 12, 20, 12);

            TableLayoutPanel layout =
                new TableLayoutPanel();

            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 3;
            layout.RowCount = 1;

            // 左侧占用全部剩余空间。
            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    100F));

            // 取消按钮。
            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    100F));

            // 确定按钮。
            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    100F));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent,
                    100F));

            layout.BackColor = UITheme.BgCard;

            // 创建取消按钮。
            _btnCancel =
                UIButton.Ghost("取消");

            _btnCancel.Dock =
                DockStyle.Fill;

            _btnCancel.Margin =
                new Padding(0, 0, UITheme.ControlSpacing, 0);

            _btnCancel.Click +=
                BtnCancel_Click;

            // 创建确定按钮。
            _btnOk =
                UIButton.Primary("确定");

            _btnOk.Dock =
                DockStyle.Fill;

            _btnOk.Margin =
                new Padding(0);

            _btnOk.Click +=
                BtnOk_Click;

            // 直接添加到固定列，
            // 不再通过 FlowLayoutPanel 控制位置。
            layout.Controls.Add(
                _btnCancel,
                1,
                0);

            layout.Controls.Add(
                _btnOk,
                2,
                0);

            panel.Controls.Add(layout);

            return panel;
        }

        /// <summary>
        /// 加载新增模式的默认参数。
        /// </summary>
        private void LoadDefault()
        {
            _nameRow.Input.Text =
                string.Empty;

            _driverKeyRow.Input.Text =
                string.Empty;

            _modelRow.Input.Text =
                string.Empty;

            _serialNumberRow.Input.Text =
                string.Empty;

            _typeRow.Input.SelectedItem =
                "TCP";

            _ipRow.Input.Text =
                string.Empty;

            // TCP 默认端口。
            _portRow.Input.Text =
                "60000";

            // 与当前 ItechPowerTcpClient 保持一致。
            _sendTimeoutRow.Input.Text =
                "1000";

            _receiveTimeoutRow.Input.Text =
                "1000";
        }

        /// <summary>
        /// 将已有配置加载到编辑控件。
        /// </summary>
        private void LoadSource()
        {
            if (_source == null)
            {
                LoadDefault();
                return;
            }

            _nameRow.Input.Text =
                _source.Name ?? string.Empty;

            _driverKeyRow.Input.Text =
                _source.DriverKey ?? string.Empty;

            _modelRow.Input.Text =
                _source.Model ?? string.Empty;

            _serialNumberRow.Input.Text =
                _source.SerialNumber ?? string.Empty;

            InstrumentCommunicationConfig communication =
                _source.Communication;

            if (communication == null)
            {
                communication =
                    new InstrumentCommunicationConfig();
            }

            _typeRow.Input.SelectedItem =
                string.IsNullOrWhiteSpace(
                    communication.Type)
                    ? "TCP"
                    : communication.Type;

            _ipRow.Input.Text =
                communication.Ip ?? string.Empty;

            _portRow.Input.Text =
                communication.Port.ToString();

            _sendTimeoutRow.Input.Text =
                communication.SendTimeout.ToString();

            _receiveTimeoutRow.Input.Text =
                communication.ReceiveTimeout.ToString();
        }

        /// <summary>
        /// 点击确定，读取输入并生成配置对象。
        /// </summary>
        private void BtnOk_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                InstrumentDeviceConfig config =
                    BuildConfig();

                // 在返回主界面以前就完成一次完整校验。
                config.Validate();

                Result = config;

                DialogResult =
                    DialogResult.OK;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    "配置校验失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 根据界面输入创建 InstrumentDeviceConfig。
        /// </summary>
        private InstrumentDeviceConfig BuildConfig()
        {
            string name =
                _nameRow.Input.Text.Trim();

            string driverKey =
                _driverKeyRow.Input.Text.Trim();

            string model =
                _modelRow.Input.Text.Trim();

            string serialNumber =
                _serialNumberRow.Input.Text.Trim();

            string type =
                _typeRow.Input.SelectedItem == null
                    ? string.Empty
                    : _typeRow.Input.SelectedItem.ToString();

            string ip =
                _ipRow.Input.Text.Trim();

            int port =
                ParseInt(
                    _portRow.Input.Text,
                    "端口");

            int sendTimeout =
                ParseInt(
                    _sendTimeoutRow.Input.Text,
                    "发送超时");

            int receiveTimeout =
                ParseInt(
                    _receiveTimeoutRow.Input.Text,
                    "接收超时");

            // 创建通讯配置对象。
            InstrumentCommunicationConfig communication =
                new InstrumentCommunicationConfig();

            communication.Type = type;
            communication.Ip = ip;
            communication.Port = port;
            communication.SendTimeout = sendTimeout;
            communication.ReceiveTimeout = receiveTimeout;

            // 创建仪器设备配置对象。
            InstrumentDeviceConfig config =
                new InstrumentDeviceConfig();

            config.Name = name;
            config.DriverKey = driverKey;
            config.Model = model;
            config.SerialNumber = serialNumber;
            config.Communication = communication;

            return config;
        }

        /// <summary>
        /// 将文本转换为整数。
        /// </summary>
        private int ParseInt(
            string text,
            string fieldName)
        {
            int value;

            if (!int.TryParse(
                text.Trim(),
                out value))
            {
                throw new FormatException(
                    fieldName +
                    "必须是有效的整数。");
            }

            return value;
        }

        /// <summary>
        /// 点击取消。
        /// </summary>
        private void BtnCancel_Click(
            object sender,
            EventArgs e)
        {
            DialogResult =
                DialogResult.Cancel;

            Close();
        }
    }
}