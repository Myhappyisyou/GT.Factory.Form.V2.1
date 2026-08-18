using GT_Common.MyEnum;
using GT_Common.ProcessConfig;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace EditJson
{
    /// <summary>
    /// 单个PLC通道编辑器。
    ///
    /// 用于编辑：
    ///
    /// PlcChannel
    ///     ├── Address
    ///     ├── Length
    ///     ├── DataType
    ///     └── ScalingFactor
    ///
    /// empty channel：
    ///     表示该测点没有配置此通道。
    /// </summary>
    public class PlcChannelEditor : UserControl
    {
        private readonly string _title;
        private readonly bool _allowEmpty;

        private NumericUpDown _numAddress;
        private NumericUpDown _numLength;
        private ComboBox _cboDataType;
        private TextBox _txtScalingFactor;

        private CheckBox _chkEnabled;

        public PlcChannelEditor(
            string title,
            bool allowEmpty)
        {
            _title = title;
            _allowEmpty = allowEmpty;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Dock = DockStyle.Top;

            Height = 100;

            Margin =
                new Padding(0, 0, 0, 8);

            BuildUI();
        }

        private void BuildUI()
        {
            var group =
                new GroupBox
                {
                    Text = _title,
                    Dock = DockStyle.Fill,
                    Padding = new Padding(8)
                };

            var panel =
                new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 8,
                    RowCount = 2
                };

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    65));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    25));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    65));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    15));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    70));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    20));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute,
                    70));

            panel.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    20));

            // =========================================================
            // 启用
            // =========================================================

            if (_allowEmpty)
            {
                _chkEnabled =
                    new CheckBox
                    {
                        Text = "启用",
                        AutoSize = true,
                        Anchor =
                            AnchorStyles.Left
                    };

                _chkEnabled.CheckedChanged +=
                    ChkEnabled_CheckedChanged;

                panel.Controls.Add(
                    _chkEnabled,
                    0,
                    0);

                panel.SetColumnSpan(
                    _chkEnabled,
                    2);
            }

            // =========================================================
            // 地址
            // =========================================================

            panel.Controls.Add(
                CreateLabel("地址"),
                2,
                0);

            _numAddress =
                CreateNumeric();

            _numAddress.Maximum =
                999999;

            panel.Controls.Add(
                _numAddress,
                3,
                0);

            // =========================================================
            // 长度
            // =========================================================

            panel.Controls.Add(
                CreateLabel("长度"),
                4,
                0);

            _numLength =
                CreateNumeric();

            _numLength.Maximum =
                999999;

            panel.Controls.Add(
                _numLength,
                5,
                0);

            // =========================================================
            // 数据类型
            // =========================================================

            panel.Controls.Add(
                CreateLabel("数据类型"),
                6,
                0);

            _cboDataType =
                new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle =
                        ComboBoxStyle.DropDownList
                };

            // 替换原有的 _cboDataType.Items.AddRange 行
            _cboDataType.Items.AddRange(
                Enum.GetValues(typeof(PlcDataType)).Cast<object>().ToArray());

            if (_cboDataType.Items.Count > 0)
            {
                _cboDataType.SelectedIndex = 0;
            }


            panel.Controls.Add(
                _cboDataType,
                7,
                0);

            // =========================================================
            // 缩放系数
            // =========================================================

            panel.Controls.Add(
                CreateLabel("缩放系数"),
                0,
                1);

            _txtScalingFactor =
                new TextBox
                {
                    Dock = DockStyle.Fill,
                    Text = "1.0"
                };

            panel.Controls.Add(
                _txtScalingFactor,
                1,
                1);

            panel.SetColumnSpan(
                _txtScalingFactor,
                3);

            group.Controls.Add(panel);

            Controls.Add(group);
        }

        private NumericUpDown CreateNumeric()
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 999999,
                DecimalPlaces = 0,
                ThousandsSeparator = false
            };
        }

        private Label CreateLabel(
            string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        #region Load

        public void LoadChannel(
            PlcChannel channel)
        {
            if (channel == null)
            {
                SetEmpty();
                return;
            }

            bool configured =
                channel.IsConfigured();

            if (_chkEnabled != null)
            {
                _chkEnabled.Checked =
                    configured;
            }

            _numAddress.Value =
                Clamp(
                    channel.Address,
                    _numAddress.Minimum,
                    _numAddress.Maximum);

            _numLength.Value =
                Clamp(
                    channel.Length,
                    _numLength.Minimum,
                    _numLength.Maximum);

            SelectDataType(
                channel.DataType);

            _txtScalingFactor.Text =
                channel.ScalingFactor
                    .ToString(
                        "0.####",
                        CultureInfo.InvariantCulture);

            UpdateEnabledState();
        }

        private void SetEmpty()
        {
            if (_chkEnabled != null)
            {
                _chkEnabled.Checked = false;
            }

            _numAddress.Value = 0;
            _numLength.Value = 0;

            SelectDataType(
                GetNoneDataType());

            _txtScalingFactor.Text =
                "1.0";

            UpdateEnabledState();
        }

        private decimal Clamp(
            int value,
            decimal minimum,
            decimal maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }

        private void SelectDataType(
    PlcDataType dataType)
        {
            if (_cboDataType == null)
                return;

            if (_cboDataType.Items.Count == 0)
                return;

            int index =
                _cboDataType.Items.IndexOf(dataType);

            if (index >= 0)
            {
                _cboDataType.SelectedIndex = index;
            }
            else
            {
                _cboDataType.SelectedIndex = 0;
            }
        }

        private PlcDataType GetNoneDataType()
        {
            foreach (PlcDataType item in _cboDataType.Items)
            {
                if (Convert.ToInt32(item) == 0)
                    return item;
            }

            if (_cboDataType.Items.Count > 0)
            {
                return (PlcDataType)_cboDataType.Items[0];
            }

            return default(PlcDataType);
        }

        #endregion

        #region Enable

        private void ChkEnabled_CheckedChanged(
            object sender,
            EventArgs e)
        {
            UpdateEnabledState();
        }

        private void UpdateEnabledState()
        {
            bool enabled =
                _chkEnabled == null ||
                _chkEnabled.Checked;

            _numAddress.Enabled =
                enabled;

            _numLength.Enabled =
                enabled;

            _cboDataType.Enabled =
                enabled;

            _txtScalingFactor.Enabled =
                enabled;
        }

        #endregion

        #region Get

        public PlcChannel GetChannel()
        {
            if (_allowEmpty &&
                _chkEnabled != null &&
                !_chkEnabled.Checked)
            {
                return null;
            }

            var channel =
                new PlcChannel();

            channel.Address =
                (int)_numAddress.Value;

            channel.Length =
                (int)_numLength.Value;

            if (_cboDataType.SelectedItem != null)
            {
                channel.DataType =
                    (PlcDataType)
                        _cboDataType.SelectedItem;
            }

            if (double.TryParse(
                _txtScalingFactor.Text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double scaling))
            {
                channel.ScalingFactor =
                    scaling;
            }
            else
            {
                channel.ScalingFactor =
                    1.0;
            }

            return channel;
        }

        public bool IsConfigured()
        {
            var channel =
                GetChannel();

            return channel != null &&
                   channel.IsConfigured();
        }

        #endregion

        #region Validate

        public bool ValidateChannel(
            out string error)
        {
            error = null;

            // 没有启用，则认为没有配置
            if (_allowEmpty &&
                _chkEnabled != null &&
                !_chkEnabled.Checked)
            {
                return true;
            }

            if (_numAddress.Value <= 0)
            {
                error =
                    $"{_title}PLC地址必须大于0。";

                return false;
            }

            if (_numLength.Value <= 0)
            {
                error =
                    $"{_title}读取长度必须大于0。";

                return false;
            }

            if (_cboDataType.SelectedItem == null)
            {
                error =
                    $"{_title}数据类型不能为空。";

                return false;
            }

            var dataType =
                (PlcDataType)
                    _cboDataType.SelectedItem;

            if (Convert.ToInt32(dataType) <= 0)
            {
                error =
                    $"{_title}数据类型不能为空。";

                return false;
            }

            if (!double.TryParse(
                _txtScalingFactor.Text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double scaling))
            {
                error =
                    $"{_title}缩放系数格式不正确。";

                return false;
            }

            if (scaling <= 0)
            {
                error =
                    $"{_title}缩放系数必须大于0。";

                return false;
            }

            return true;
        }

        #endregion
    }
}