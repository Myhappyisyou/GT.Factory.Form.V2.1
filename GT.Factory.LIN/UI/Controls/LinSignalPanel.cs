using GT.Factory.LIN.Model;
using GT.Factory.LIN.ProjectDebug;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;



namespace GT.Factory.LIN.UI.Controls
{

    /// <summary>
    /// LIN Signal监控面板。
    ///
    /// Signal
    ///     ↓
    /// LinSignalInfo
    ///     ↓
    /// LinSignal.ValueType
    ///     ↓
    /// UI
    ///
    /// Raw
    ///     → TextBox
    ///
    /// Physical
    ///     → TextBox
    ///
    /// Logical
    ///     → ComboBox
    ///
    /// IsUserEditable
    ///     ↓
    /// 是否允许用户修改
    ///
    /// 本控件不访问：
    ///
    /// LinRuntime
    /// LinController
    /// LinSignalRuntime
    ///
    /// 用户修改：
    ///
    /// UI
    ///  ↓
    /// SignalValueChanged
    ///  ↓
    /// LinDebugSession
    /// </summary>
    public class LinSignalPanel : UserControl
    {

        #region 字段

        private DataGridView _grid;

        private readonly List<LinSignalInfo> _signals;

        /// <summary>
        /// 防止初始化和Runtime刷新时触发用户修改事件。
        /// </summary>
        private bool _initializing;

        #endregion


        #region Event

        /// <summary>
        /// 用户修改Signal值。
        ///
        /// Logical:
        ///     int
        ///
        /// Physical:
        ///     double
        ///
        /// Raw:
        ///     int / double
        /// </summary>
        public event Action<string, object>
            SignalValueChanged;

        #endregion


        #region Constructor

        public LinSignalPanel()
        {

            _signals =
                new List<LinSignalInfo>();

            Initialize();

        }

        #endregion


        #region Initialize

        private void Initialize()
        {
            Dock =
                DockStyle.Fill;

            BackColor =
                UITheme.BgBase;

            _grid =
                new DataGridView
                {
                    Dock = DockStyle.Fill,

                    AllowUserToAddRows = false,

                    AllowUserToDeleteRows = false,

                    AllowUserToResizeRows = false,

                    RowHeadersVisible = false,

                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,

                    MultiSelect = false,

                    EditMode =
                        DataGridViewEditMode.EditOnEnter,

                    ReadOnly = false
                };

            UIGrid.BuildDataGrid(
                _grid);

            BuildColumns();

            //
            // 统一处理：
            //
            // TextBox
            // ComboBox
            //
            _grid.CellValueChanged +=
                Grid_CellValueChanged;

            //
            // 防止ComboBox非法值弹默认异常
            //
            _grid.DataError +=
                Grid_DataError;

            Controls.Add(
                _grid);
        }
        #endregion


        #region Columns

        private void BuildColumns()
        {

            _grid.Columns.Clear();


            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Signal",

                    HeaderText = "Signal",

                    Width = 300,

                    ReadOnly = true
                });


            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Frame",

                    HeaderText = "Frame",

                    Width = 160,

                    ReadOnly = true
                });


            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Publisher",

                    HeaderText = "Publisher",

                    Width = 120,

                    ReadOnly = true
                });


            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Value",

                    HeaderText = "Value",

                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill,

                    ReadOnly = false
                });

        }

        #endregion


        #region Public API

        /// <summary>
        /// 加载全部Signal。
        /// </summary>
        public void LoadSignals(
            IEnumerable<LinSignalInfo> signals)
        {

            if (signals == null)
                return;


            _signals.Clear();


            _signals.AddRange(
                signals);


            RefreshGrid();

        }


        /// <summary>
        /// 显示指定Frame中的Signal。
        /// </summary>
        public void ShowFrame(
            LinFrameInfo frame)
        {

            if (frame == null)
                return;


            ShowSignals(
                frame.Signals);

        }


        /// <summary>
        /// 显示Signal。
        /// </summary>
        public void ShowSignals(
            IEnumerable<LinSignalInfo> signals)
        {

            if (signals == null)
                return;


            _signals.Clear();


            _signals.AddRange(
                signals);


            RefreshGrid();

        }


        /// <summary>
        /// 刷新Signal运行值。
        ///
        /// 不重建Row。
        ///
        /// 不重建Cell。
        /// </summary>
        public void RefreshValues()
        {

            if (InvokeRequired)
            {

                BeginInvoke(
                    new Action(
                        RefreshValues));

                return;

            }


            _initializing = true;


            try
            {

                for (int i = 0;
                     i < _signals.Count;
                     i++)
                {

                    if (i >=
                        _grid.Rows.Count)
                        continue;


                    LinSignalInfo signalInfo =
                        _signals[i];


                    DataGridViewCell cell =
                        _grid.Rows[i]
                            .Cells["Value"];


                    SetCellValue(
                        cell,
                        signalInfo);

                }

            }
            finally
            {

                _initializing = false;

            }

        }

        #endregion


        #region Grid

        private void RefreshGrid()
        {

            if (InvokeRequired)
            {

                BeginInvoke(
                    new Action(
                        RefreshGrid));

                return;

            }


            _initializing = true;


            _grid.SuspendLayout();


            try
            {

                _grid.Rows.Clear();


                foreach (LinSignalInfo signalInfo
                    in _signals)
                {

                    if (signalInfo == null)
                        continue;


                    int rowIndex =
                        _grid.Rows.Add();


                    DataGridViewRow row =
                        _grid.Rows[rowIndex];


                    row.Cells["Signal"].Value =
                        signalInfo.Name;


                    row.Cells["Frame"].Value =
                        signalInfo.FrameName;


                    row.Cells["Publisher"].Value =
                        signalInfo.Publisher;


                    CreateValueCell(
                        row,
                        signalInfo);

                }


                _grid.ClearSelection();

            }
            finally
            {

                _grid.ResumeLayout();


                _initializing = false;

            }

        }

        #endregion


        #region Value Cell

        private void CreateValueCell(
            DataGridViewRow row,
            LinSignalInfo signalInfo)
        {

            if (row == null ||
                signalInfo == null)
                return;


            LinSignal signal =
                signalInfo.Source;


            if (signal == null)
                return;


            DataGridViewCell cell;


            switch (signal.ValueType)
            {

                case LinSignalValueType.Logical:

                    cell =
                        CreateComboBoxCell(
                            signal);

                    break;


                case LinSignalValueType.Physical:

                    cell =
                        new DataGridViewTextBoxCell();

                    break;


                case LinSignalValueType.Raw:

                default:

                    cell =
                        new DataGridViewTextBoxCell();

                    break;

            }


            //
            // 用户是否允许编辑
            //
            cell.ReadOnly =
                !signal.IsUserEditable;


            //
            // 替换Value Cell
            //
            row.Cells["Value"] =
                cell;


            //
            // 设置初始值
            //
            SetCellValue(
                cell,
                signalInfo);

        }

        #endregion


        #region ComboBox

        /// <summary>
        /// 创建Logical Signal ComboBox。
        ///
        /// 关键点：
        ///
        /// Items中的对象：
        ///
        /// LinSignalComboItem
        ///
        /// 但是：
        ///
        /// Cell.Value
        ///
        /// 不再保存LinSignalComboItem对象。
        ///
        /// Cell.Value直接保存：
        ///
        /// int rawValue
        ///
        /// 这样DataGridView在结束编辑时
        /// 可以稳定匹配Items。
        /// </summary>
        private DataGridViewComboBoxCell CreateComboBoxCell(
    LinSignal signal)
        {
            DataGridViewComboBoxCell cell =
                new DataGridViewComboBoxCell();

            cell.DisplayStyle =
                DataGridViewComboBoxDisplayStyle.DropDownButton;

            cell.FlatStyle =
                FlatStyle.Flat;

            if (signal == null ||
                signal.Encoding == null ||
                signal.Encoding.LogicalValues == null)
            {
                return cell;
            }

            var items =
                signal.Encoding.LogicalValues
                .Select(x =>
                    new LinSignalComboItem
                    {
                        Value = x.Key,
                        Text = x.Value
                    })
                .ToList();

            cell.DataSource =
                items;

            cell.DisplayMember =
                nameof(LinSignalComboItem.Text);

            cell.ValueMember =
                nameof(LinSignalComboItem.Value);

            cell.ValueType =
                typeof(int);

            return cell;
        }

        #endregion


        #region Set Cell Value

        /// <summary>
        /// 设置Cell当前值。
        ///
        /// Logical：
        ///
        /// Runtime Value
        ///      ↓
        /// int raw
        ///      ↓
        /// cell.Value
        ///
        /// DataGridViewComboBoxCell
        /// 会根据Value在Items中寻找对应项，
        /// 然后显示Text。
        /// </summary>
        private void SetCellValue(
    DataGridViewCell cell,
    LinSignalInfo signalInfo)
        {
            if (cell == null ||
                signalInfo == null)
                return;

            LinSignal signal =
                signalInfo.Source;

            if (signal == null)
                return;

            DataGridViewComboBoxCell comboCell =
                cell as DataGridViewComboBoxCell;

            if (comboCell != null)
            {
                int rawValue;

                if (!TryConvertToInt(
                    signalInfo.Value,
                    out rawValue))
                {
                    comboCell.Value = null;
                    return;
                }

                //
                // 只有当LogicalValues中存在
                // 这个Raw值时才设置。
                //
                bool exists =
                    false;

                if (comboCell.DataSource != null)
                {
                    var items =
                        comboCell.DataSource
                        as IEnumerable<LinSignalComboItem>;

                    if (items != null)
                    {
                        exists =
                            items.Any(
                                x => x.Value == rawValue);
                    }
                }

                if (exists)
                {
                    comboCell.Value =
                        rawValue;
                }
                else
                {
                    //
                    // Runtime可能出现：
                    //
                    // 2
                    //
                    // 但是LDF只有：
                    //
                    // 0 = Stop
                    // 1 = Normal
                    //
                    // 此时显示空。
                    //
                    comboCell.Value =
                        null;
                }

                return;
            }

            //
            // Physical / Raw
            //
            cell.Value =
                signalInfo.Value;
        }

        #endregion


        #region Editing Control

        private void Grid_EditingControlShowing(
            object sender,
            DataGridViewEditingControlShowingEventArgs e)
        {

            ComboBox combo =
                e.Control as ComboBox;


            if (combo == null)
                return;


            combo.SelectionChangeCommitted -=
                Combo_SelectionChangeCommitted;


            combo.SelectionChangeCommitted +=
                Combo_SelectionChangeCommitted;

        }


        /// <summary>
        /// ComboBox用户真正选择了一个项目。
        ///
        /// SelectionChangeCommitted：
        ///
        /// 只会在用户操作时触发。
        ///
        /// Runtime刷新不会进入这里。
        /// </summary>
        private void Combo_SelectionChangeCommitted(
            object sender,
            EventArgs e)
        {

            if (_initializing)
                return;


            ComboBox combo =
                sender as ComboBox;


            if (combo == null)
                return;


            DataGridViewCell cell =
                _grid.CurrentCell;


            if (cell == null)
                return;


            if (cell.OwningColumn == null)
                return;


            if (cell.OwningColumn.Name !=
                "Value")
                return;


            int rowIndex =
                cell.RowIndex;


            if (rowIndex < 0 ||
                rowIndex >= _signals.Count)
                return;


            LinSignalInfo signalInfo =
                _signals[rowIndex];


            if (signalInfo == null ||
                signalInfo.Source == null)
                return;


            LinSignal signal =
                signalInfo.Source;


            if (!signal.IsUserEditable)
                return;


            //
            // 获取ComboBox当前选中的对象
            //
            LinSignalComboItem item =
                combo.SelectedItem
                as LinSignalComboItem;


            if (item == null)
                return;


            //
            // 直接使用Raw Value
            //
            int rawValue =
                item.Value;


            //
            // 更新UI Model
            //
            signalInfo.Value =
                rawValue;


            //
            // 通知外部
            //
            SignalValueChanged?.Invoke(
                signal.Name,
                rawValue);

        }

        #endregion


        #region Cell Value Changed

        private void Grid_CellValueChanged(
    object sender,
    DataGridViewCellEventArgs e)
        {
            if (_initializing)
                return;

            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex < 0)
                return;

            DataGridViewColumn valueColumn =
                _grid.Columns["Value"];

            if (valueColumn == null)
                return;

            if (e.ColumnIndex !=
                valueColumn.Index)
                return;

            if (e.RowIndex >=
                _signals.Count)
                return;

            LinSignalInfo signalInfo =
                _signals[e.RowIndex];

            if (signalInfo == null)
                return;

            LinSignal signal =
                signalInfo.Source;

            if (signal == null)
                return;

            //
            // ReadOnly
            //
            if (!signal.IsUserEditable)
                return;

            DataGridViewCell cell =
                _grid.Rows[e.RowIndex]
                .Cells[e.ColumnIndex];

            object value =
                GetCellValue(
                    cell,
                    signal);

            if (value == null)
                return;

            //
            // 更新UI Model
            //
            signalInfo.Value =
                value;

            //
            // 唯一对外通知出口
            //
            SignalValueChanged?.Invoke(
                signal.Name,
                value);
        }

        #endregion


        #region DataError

        /// <summary>
        /// ComboBox非法Value不弹默认异常框。
        ///
        /// 例如：
        ///
        /// Logical：
        ///
        /// 0 = Stop Mode
        /// 1 = Normal Operation
        ///
        /// Runtime：
        ///
        /// 2
        ///
        /// 2虽然是合法Raw值，
        /// 但没有Logical文本。
        ///
        /// UI显示空即可。
        /// </summary>
        private void Grid_DataError(
            object sender,
            DataGridViewDataErrorEventArgs e)
        {

            e.ThrowException =
                false;


            e.Cancel =
                true;

        }

        #endregion


        #region Get Cell Value

        private object GetCellValue(
            DataGridViewCell cell,
            LinSignal signal)
        {

            if (cell == null)
                return null;


            //
            // Logical
            //
            DataGridViewComboBoxCell comboCell =
                cell as DataGridViewComboBoxCell;


            if (comboCell != null)
            {

                object value =
                    comboCell.Value;


                if (value == null)
                    return null;


                int rawValue;


                if (TryConvertToInt(
                    value,
                    out rawValue))
                {

                    return rawValue;

                }


                return null;

            }


            //
            // TextBox
            //
            object raw =
                cell.Value;


            if (raw == null)
                return null;


            string text =
                raw.ToString();


            if (string.IsNullOrWhiteSpace(text))
                return null;


            //
            // Physical
            //
            if (signal != null &&
                signal.ValueType ==
                LinSignalValueType.Physical)
            {

                double physicalValue;


                if (double.TryParse(
                    text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out physicalValue))
                {

                    return physicalValue;

                }


                if (double.TryParse(
                    text,
                    out physicalValue))
                {

                    return physicalValue;

                }


                return null;

            }


            //
            // Raw
            //
            int intValue;


            if (int.TryParse(
                text,
                out intValue))
            {

                return intValue;

            }


            double doubleValue;


            if (double.TryParse(
                text,
                out doubleValue))
            {

                return doubleValue;

            }


            return text;

        }

        #endregion


        #region Conversion

        private bool TryConvertToInt(
            object value,
            out int result)
        {

            result = 0;


            if (value == null)
                return false;


            if (value is int)
            {

                result =
                    (int)value;

                return true;

            }


            if (value is byte)
            {

                result =
                    (byte)value;

                return true;

            }


            if (value is sbyte)
            {

                result =
                    (sbyte)value;

                return true;

            }


            if (value is short)
            {

                result =
                    (short)value;

                return true;

            }


            if (value is ushort)
            {

                result =
                    (ushort)value;

                return true;

            }


            if (value is long)
            {

                long valueLong =
                    (long)value;


                if (valueLong <
                    int.MinValue ||
                    valueLong >
                    int.MaxValue)
                {
                    return false;
                }


                result =
                    (int)valueLong;

                return true;

            }


            if (value is uint)
            {

                uint valueUInt =
                    (uint)value;


                if (valueUInt >
                    int.MaxValue)
                {
                    return false;
                }


                result =
                    (int)valueUInt;

                return true;

            }


            if (value is double)
            {

                double valueDouble =
                    (double)value;


                if (valueDouble <
                    int.MinValue ||
                    valueDouble >
                    int.MaxValue)
                {
                    return false;
                }


                result =
                    Convert.ToInt32(
                        valueDouble);

                return true;

            }


            return int.TryParse(
                Convert.ToString(
                    value,
                    CultureInfo.InvariantCulture),
                out result);

        }

        #endregion

    }


    /// <summary>
    /// DataGridView ComboBox项目。
    ///
    /// Value：
    ///     LDF Raw值
    ///
    /// Text：
    ///     LDF Logical文本
    /// </summary>
    internal sealed class LinSignalComboItem
    {

        public int Value
        {
            get;
            set;
        }


        public string Text
        {
            get;
            set;
        }


        public override string ToString()
        {
            return Text;
        }

    }

}




