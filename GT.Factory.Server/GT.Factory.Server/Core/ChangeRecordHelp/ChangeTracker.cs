
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GT.Factory.Server.Help;
using GT.Factory.Common.Helpers;

namespace GT.Factory.Server.Core.ChangeRecordHelp
{
    /// <summary>
    /// 控件变更跟踪器
    /// </summary>
    public class ChangeTracker
    {
        #region 事件定义
        /// <summary>
        /// 当值变更被记录时触发
        /// </summary>
        public event EventHandler<ChangeRecord> ValueChanged;

        /// <summary>
        /// 当跟踪器状态重置时触发
        /// </summary>
        public event EventHandler TrackerReset;
        #endregion

        #region 私有字段
        private readonly Dictionary<string, object> _initialValues = new Dictionary<string, object>();
        private readonly BindingList<ChangeRecord> _changeHistory = new BindingList<ChangeRecord>();
        private readonly Dictionary<Control, EventHandler> _eventHandlers = new Dictionary<Control, EventHandler>();
        #endregion

        #region 公共属性
        /// <summary>
        /// 获取变更历史记录
        /// </summary>
        public BindingList<ChangeRecord> ChangeHistory => _changeHistory;

        /// <summary>
        /// 获取或设置是否自动将变更记录到文件
        /// </summary>
        public bool AutoLogToFile { get; set; } = false;

        /// <summary>
        /// 获取或设置日志文件路径
        /// </summary>
        public string LogFilePath { get; set; } = "change_log.txt";

        public string LogRootPath { get; set; } = Path.Combine(PathCenter.LogFile("ChangeTracker"));

        /// <summary>
        /// 获取当前跟踪的控件数量
        /// </summary>
        public int TrackedControlsCount => _initialValues.Count;

        /// <summary>
        /// 获取记录的变更数量
        /// </summary>
        public int ChangeCount => _changeHistory.Count;
        #endregion

        #region 公共方法
        /// <summary>
        /// 添加控件到跟踪列表
        /// </summary>
        /// <param name="control">要跟踪的控件</param>
        public void AddControl(Control control)
        {
            if (control == null) return;

            string key = control.Name;

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("控件必须具有有效的Name属性");
            }

            if (_initialValues.ContainsKey(key))
            {
                throw new ArgumentException($"已存在名为 '{key}' 的控件");
            }

            // 保存初始值
            _initialValues[key] = GetControlValue(control);

            // 注册变更事件
            var handler = GetEventHandlerForControl(control);
            _eventHandlers[control] = handler;
            SubscribeToControlEvents(control, handler);
        }

        /// <summary>
        /// 批量添加控件
        /// </summary>
        /// <param name="controls">控件集合</param>
        public void AddControls(IEnumerable<Control> controls)
        {
            foreach (var control in controls)
            {
                AddControl(control);
            }
        }

        /// <summary>
        /// 移除跟踪的控件
        /// </summary>
        /// <param name="control">要移除的控件</param>
        public void RemoveControl(Control control)
        {
            if (control == null) return;

            string key = control.Name;

            if (_initialValues.ContainsKey(key))
            {
                _initialValues.Remove(key);

                if (_eventHandlers.TryGetValue(control, out var handler))
                {
                    UnsubscribeFromControlEvents(control, handler);
                    _eventHandlers.Remove(control);
                }
            }
        }

        /// <summary>
        /// 重置跟踪器（保存当前值为初始值）
        /// </summary>
        public void ResetTracker()
        {
            foreach (var control in _eventHandlers.Keys.ToList())
            {
                _initialValues[control.Name] = GetControlValue(control);
            }

            _changeHistory.Clear();
            TrackerReset?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 导出变更历史到CSV文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void ExportToCsv(string filePath)
        {
            var lines = new List<string> { "Timestamp,ControlName,ControlType,OldValue,NewValue" };

            foreach (var record in _changeHistory)
            {
                lines.Add($"\"{record.Timestamp:yyyy-MM-dd HH:mm:ss}\"," +
                         $"\"{record.ControlName}\"," +
                         $"\"{record.ControlType}\"," +
                         $"\"{EscapeCsvValue(record.OldValue)}\"," +
                         $"\"{EscapeCsvValue(record.NewValue)}\"");
            }

            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// 获取指定控件的变更记录
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <returns>变更记录列表</returns>
        public IEnumerable<ChangeRecord> GetChangesForControl(string controlName)
        {
            return _changeHistory.Where(r => r.ControlName == controlName);
        }

        /// <summary>
        /// 清除所有变更历史
        /// </summary>
        public void ClearHistory()
        {
            _changeHistory.Clear();
        }
        #endregion

        #region 私有方法

        private string GetTodayLogFilePath()
        {
            string folder = Path.Combine(
                LogRootPath,
                DateTime.Now.ToString("yyyy"),
                DateTime.Now.ToString("MM")
            );

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return Path.Combine(
                folder,
                $"change_{DateTime.Now:yyyyMMdd}.log"
            );
        }

        private EventHandler GetEventHandlerForControl(Control control)
        {
            return (sender, e) => TrackChange(control);
        }

        private void SubscribeToControlEvents(Control control, EventHandler handler)
        {
            switch (control)
            {
                case TextBox txt:
                    txt.Leave += handler;
                    break;
                case NumericUpDown num:
                    num.ValueChanged += handler;
                    break;
                case CheckBox chk:
                    chk.CheckedChanged += handler;
                    break;
                case ComboBox cmb:
                    cmb.SelectedIndexChanged += handler;
                    break;
                case DateTimePicker dtp:
                    dtp.ValueChanged += handler;
                    break;
                case RadioButton rdo:
                    rdo.CheckedChanged += handler;
                    break;
                case ListBox lb:
                    lb.SelectedIndexChanged += handler;
                    break;
                default:
                    control.TextChanged += handler;
                    break;
            }
        }

        private void UnsubscribeFromControlEvents(Control control, EventHandler handler)
        {
            switch (control)
            {
                case TextBox txt:
                    txt.TextChanged -= handler;
                    break;
                case NumericUpDown num:
                    num.ValueChanged -= handler;
                    break;
                case CheckBox chk:
                    chk.CheckedChanged -= handler;
                    break;
                case ComboBox cmb:
                    cmb.SelectedIndexChanged -= handler;
                    break;
                case DateTimePicker dtp:
                    dtp.ValueChanged -= handler;
                    break;
                case RadioButton rdo:
                    rdo.CheckedChanged -= handler;
                    break;
                case ListBox lb:
                    lb.SelectedIndexChanged -= handler;
                    break;
                default:
                    control.TextChanged -= handler;
                    break;
            }
        }

        private void TrackChange(Control control)
        {
            string controlName = control.Name;

            if (!_initialValues.TryGetValue(controlName, out object oldValue))
                return;

            object newValue = GetControlValue(control);

            // 仅当值实际发生变化时记录
            if (!Equals(oldValue, newValue))
            {
                // 创建变更记录
                var record = new ChangeRecord
                {
                    Timestamp = DateTime.Now,
                    ControlName = controlName,
                    ControlType = control.GetType().Name,
                    OldValue = FormatValue(oldValue),
                    NewValue = FormatValue(newValue)
                };

                // 添加到历史记录
                _changeHistory.Add(record);

                // 更新初始值为当前值
                _initialValues[controlName] = newValue;

                // 触发事件
                ValueChanged?.Invoke(this, record);

                // 自动记录到文件
                if (AutoLogToFile)
                {
                    LogToFile(record);
                }
            }
        }

        //  控件值
        private object GetControlValue(Control control)
        {
            switch (control)
            {
                case TextBox txt:
                    return txt.Text;
                case NumericUpDown num:
                    return num.Value;
                case CheckBox chk:
                    return chk.Checked;
                case ComboBox cmb:
                    return cmb.SelectedItem ?? cmb.Text;
                case DateTimePicker dtp:
                    return dtp.Value;
                case RadioButton rdo:
                    return rdo.Checked;
                case ListBox lb:
                    return lb.SelectedItem;
                case RichTextBox rtb:
                    return rtb.Text;
                case MaskedTextBox mtb:
                    return mtb.Text;
                case DomainUpDown dud:
                    return dud.SelectedItem;
                case TrackBar tb:
                    return tb.Value;
                case MonthCalendar mc:
                    return mc.SelectionStart;
                // 添加更多控件类型
                default:
                    return control.Text;
            }
        }

        //  格式化
        private string FormatValue(object value)
        {
            if (value == null)
                return "[null]";

            switch (value)
            {
                case bool b:
                    return b ? "True" : "False";
                case DateTime dt:
                    return dt.ToString("yyyy-MM-dd HH:mm");
                case decimal dec:
                    return dec.ToString("N2");
                case double dbl:
                    return dbl.ToString("N2");
                case float flt:
                    return flt.ToString("N2");
                case int i:
                    return i.ToString("N0");  // 添加整数格式化
                case long l:
                    return l.ToString("N0");  // 添加长整数格式化
                case TimeSpan ts:
                    return ts.ToString(@"hh\:mm\:ss");  // 添加时间跨度格式化
                case DBNull _:  // 处理数据库空值
                    return "[DBNull]";
                default:
                    return value.ToString();
            }
        }


        //  记录
        private void LogToFile(ChangeRecord record)
        {
            try
            {
                string logFile = GetTodayLogFilePath();

                string logEntry = $"[{record.Timestamp:yyyy-MM-dd HH:mm:ss}] " +
                                $"{record.ControlName} ({record.ControlType}): " +
                                $"'{record.OldValue}' → '{record.NewValue}'";

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // 在实际应用中，应该使用日志框架记录此错误
                Console.WriteLine($"日志写入失败: {ex.Message}");
            }
        }

        //  异步记录
        //private async Task LogToFileAsync(ChangeRecord record)
        //{
        //    string logEntry = $"[{record.Timestamp:yyyy-MM-dd HH:mm:ss}] {record.ControlName}: " +
        //                     $"'{record.OldValue}' → '{record.NewValue}'";

        //    await File.AppendAllTextAsync(LogFilePath, logEntry + Environment.NewLine);
        //}

        //  处理特殊字符
        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.Contains("\"") || value.Contains(",") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
        #endregion
    }

}
