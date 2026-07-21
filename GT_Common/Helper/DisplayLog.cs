using GTJPN2503006.Model;
using HslCommunication;
using HslCommunication.Profinet.OpenProtocol;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GT_Common.Helper
{
    public static class DisplayLog
    {
        private static RichTextBox _logBox;

        private static bool _isInitialized = false;

        private static readonly object _rtbLock = new object();

        private static void InitLog()
        {
            if (_isInitialized) return;

            Log.Logger = new LoggerConfiguration()
                                  .MinimumLevel.Debug()
                                  .Enrich.FromLogContext()
                                  .WriteTo.File(
                                      path: "D:\\MESLog\\log-.txt",   // 注意 - 是为了占位日期
                                      rollingInterval: RollingInterval.Day,
                                      rollOnFileSizeLimit: true,
                                      retainedFileCountLimit: 30 // 保留最近30天
                                  )
                                  .CreateLogger();

            _isInitialized = true;
        }

        #region 公共方法
        public static void CloseLog()
        {
            lock (_rtbLock)
            {
                if (Log.Logger != null)
                {
                    (Log.Logger as IDisposable)?.Dispose();
                    Log.CloseAndFlush();
                    _isInitialized = false;
                }
            }
            
        }

        public static void BindLogBox(RichTextBox textBox) => _logBox = textBox;

        #endregion

        #region 核心日志方法
        public static void ShowLogInfo(string str, bool _enableUILog = true)
        {
            LogInternal(str, LogLevel.Info, _enableUILog);
        }

        public static void ShowLogWarn(string str, bool _enableUILog = true)
        {
            LogInternal(str, LogLevel.Warning, _enableUILog);
        }

        public static void ShowLogError(string str, bool _enableUILog = true, Exception ex = null)
        {
            if (ex != null)
            {
                str += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }

            LogInternal(str, LogLevel.Error, _enableUILog);
        }

        #endregion

        #region 私有实现

        private enum LogLevel { Info, Warning, Error }

        private static void LogInternal(string message, LogLevel level, bool _enableUILog, Exception ex = null)
        {
            // 确保已初始化
            if (!_isInitialized) InitLog();
            // 文件日志

            switch (level)
            {
                case LogLevel.Info:
                    Log.Information(message);
                    break;
                case LogLevel.Warning:
                    Log.Warning(message);
                    break;
                case LogLevel.Error:
                    if (ex != null)
                        Log.Error(ex, message);
                    else
                        Log.Error(message);
                    break;
            }

            // UI日志
            if (_enableUILog)
            {
                Color color;
                switch (level)
                {
                    case LogLevel.Info:
                        color = Color.Black;
                        break;
                    case LogLevel.Warning:
                        color = Color.Blue;
                        break;
                    case LogLevel.Error:
                        color = Color.Red;
                        break;
                    default:
                        color = Color.Black;
                        break;
                }


                string fullMessage = ex != null
                    ? $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}"
                    : message;

                DispColorText(_logBox, fullMessage, color);
            }
        }

        private static void DispColorText(RichTextBox rtbox, string dispText, Color color, int maxLines = 5000, int linesToKeep = 4900)
        {
            if (rtbox == null || rtbox.IsDisposed) return;

            lock (_rtbLock)
            {
                try
                {
                    if (rtbox.InvokeRequired)
                    {
                        rtbox.BeginInvoke((Action)(() => SafeAppendText(rtbox, dispText, color, maxLines, linesToKeep)));
                    }
                    else
                    {
                        SafeAppendText(rtbox, dispText, color, maxLines, linesToKeep);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"日志显示错误: {ex.Message}");
                }
            }
        }

        private static void SafeAppendText(RichTextBox rtbox, string text, Color color, int maxLines, int linesToKeep)
        {
            // 检查是否在查看最新内容
            bool wasAtBottom = IsScrollAtBottom(rtbox);

            // 清理旧日志
            if (rtbox.Lines.Length > maxLines)
            {
                rtbox.SuspendLayout();
                try
                {
                    int removeCount = rtbox.Lines.Length - linesToKeep;
                    int charsToRemove = rtbox.GetFirstCharIndexFromLine(removeCount);
                    rtbox.Select(0, charsToRemove);
                    rtbox.SelectedText = string.Empty;
                }
                finally
                {
                    rtbox.ResumeLayout();
                }
            }

            // 添加新日志
            rtbox.SelectionColor = color;
            rtbox.AppendText($"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} : {text}\r\n");

            // 自动滚动到底部（如果之前已经在底部）
            if (wasAtBottom)
            {
                rtbox.SelectionStart = rtbox.Text.Length;
                rtbox.ScrollToCaret();
            }
        }

        private static bool IsScrollAtBottom(RichTextBox rtbox)
        {
            if (rtbox.TextLength == 0) return true;

            // 获取最后可见字符的位置
            Point bottomPoint = new Point(1, rtbox.ClientSize.Height - 1);
            int lastVisibleCharIndex = rtbox.GetCharIndexFromPosition(bottomPoint);

            // 判断是否已经显示到最后
            return lastVisibleCharIndex >= rtbox.TextLength - 1;
        }


        #endregion

        #region 业务日志方法

        //判断是否返回给PLC成功
        public static void IsSucess(OperateResult operateResult, string logMsg)
        {
            if (!operateResult.IsSuccess)
            {
                ShowLogError(logMsg + ",返回给PLC失败");
            }
            else
            {
                ShowLogInfo(logMsg + ",返回给PLC成功");
            }
        }

        public static void LogProcessStart(string processName)
        {
            ShowLogInfo($"{processName}流程开始");
        }

        public static void LogProcessSuccess(string processName, string sn = null)
        {
            var message = sn != null ? $"{processName}流程 - SN:{sn} 处理成功" : $"{processName}流程处理成功";
            ShowLogInfo(message);
        }

        public static void LogProcessError(string processName, Exception ex, string sn = null)
        {
            var message = sn != null ? $"{processName}流程 - SN:{sn} 处理失败" : $"{processName}流程处理失败";
            ShowLogError(message, true, ex);
        }

        public static void LogSqlNoSn(string bar_no)
        {
            ShowLogWarn($"三花数据上传失败产品码为空——{bar_no}");
        }

        public static void LogSqlBasicData(string bar_no)
        {
            ShowLogInfo($"产品码:{bar_no}数据上传成功");
        }

        public static void LogSqlBasicDataErr(string bar_no, Exception ex)
        {
            ShowLogError($"产品码:{bar_no}数据上传失败", true, ex);
        }

        public static void LogSqlFile(string bar_no)
        {
            ShowLogInfo($"产品码文件:{bar_no}数据上传成功");
        }

        public static void LogSqlFileErr(string bar_no, Exception ex)
        {
            ShowLogError($"产品码文件:{bar_no}上传失败",true, ex);
        }
        public static void LogSqlCalibrationFinish(string test_item,string bar_no)
        {
            ShowLogInfo($"点检项---{test_item}---SN码:{bar_no}数据上传成功");
        }

        public static void LogSqlCalibrationErr(string test_item, string bar_no, Exception ex)
        {
           ShowLogError($"点检项---{test_item}---SN码:{bar_no}数据上传失败",true, ex);
        }
        public static void LogSqlSelectCalibrationItem(string inspection, string station_id)
        {
            ShowLogError($"点检配置:{inspection}--{station_id}--查询到配置项");
        }

        public static void LogSqlSelectCalibrationItemWarn(string inspection, string station_id)
        {
            ShowLogWarn($"点检配置:{inspection}--{station_id}--不存在配置项");
        }

        public static void LogSqlSelectCalibrationItemErr(string inspection, string station_id, Exception ex)
        {
            ShowLogError($"点检配置:{inspection}--{station_id}--未查询到配置项", true, ex);
        }

        public static void LogSqlPeriodFinish(string test_item, string bar_no)
        {
            ShowLogInfo($"抽检项---{test_item}---SN码:{bar_no}数据上传成功");
        }

        public static void LogSqlPeriodErr(string test_item, string bar_no, Exception ex)
        {
            ShowLogError($"抽检项---{test_item}---SN码:{bar_no}数据上传失败", true, ex);
        }

        public static void LogSqlSelectNgCode(string bar_no)
        {
            ShowLogError($"产品码:{bar_no}--未查询到NG代码");
        }

        public static void LogSqlSelectNgCodeFinish( string bar_no, string NgCode)
        {
            ShowLogInfo($"产品码:{bar_no}--查询到NG代码--{NgCode}");
        }

        public static void LogSqlSelectNgCodeFinishErr(string bar_no, Exception ex)
        {
            ShowLogError($"产品码:{bar_no}--未查询到NG代码", true, ex);
        }

        public static void LogSqlGenerateProductCode(string bar_no)
        {
            ShowLogInfo($"生成产品码成功:{bar_no}");
        }

        public static void LogSqlGenerateProductCodeErr(Exception ex)
        {
            ShowLogError("生成产品码异常:", true, ex);
        }
        #endregion
    }

}
