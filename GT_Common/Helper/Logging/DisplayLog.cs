using GT_Common;
using GT_Common.ProcessConfig;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Logging
{
    public static class DisplayLog
    {
        private static ILogWriter _writer;
        private static ILogDisplay _display;

        public static void SetDisplay(ILogDisplay display)
        {
            _display = display;
        }

        public static void Init(ILogWriter writer, ILogDisplay display)
        {
            _writer = writer;
            _display = display;
        }
        public static void Info(string message, bool ui = true) => Log(LogLevel.Info, message, null, ui);
        public static void Info(string message) => Log(LogLevel.Info, message, null,false);

        public static void Warn(string message, bool ui = true) => Log(LogLevel.Warning, message, null, ui);
        public static void Warn(string message) => Log(LogLevel.Warning, message, null, false);

        public static void Error(string message, Exception ex = null, bool ui = true) => Log(LogLevel.Error, message, ex, ui);
        public static void Error(string message, Exception ex = null) => Log(LogLevel.Error, message, ex, false);

        private static void Log(LogLevel level, string message, Exception ex, bool ui)
        {
            _writer?.Log(level, message, ex);

            if (ui)
            {
                string fullMessage = ex != null ? $"{message} - {ex.Message}" : message;
                _display?.Show(level, fullMessage);
            }
        }

        #region 业务日志方法

        //判断是否返回给PLC成功
      
        public static void LogProcessStart(string processName)
        {
            var msg = $"流程 【{processName}】 开始执行";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessEnd(string processName,string duration)
        {
            var msg = $"流程 【{processName}】 执行完毕，用时 {duration} ms";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadSn(string processName,string bar_no)
        {
            var msg = $"流程 【{processName}】 收到PLC发的SN码为{bar_no}";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessGetNgCode(string processName,string bar_no, string NgCode)
        {
            var msg = $"流程 【[{processName}】 查询{bar_no}工艺代码为{NgCode}";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadResult(string processName, string resultCode)
        {
            var msg = $"流程 【{processName}】 收到PLC反馈工艺代码为{resultCode}";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadTaktTime(string processName, string TaktTime)
        {
            var msg = $"流程 【{processName}】 收到PLC反馈节拍为{TaktTime}";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadCalibrationFlag(string processName, string TaktTime)
        {
            var msg = $"流程 【{processName}】 收到PLC反馈点检项为{TaktTime}";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadPlcData(string processName,string bar_no,string data)
        {
            var msg = $"流程 【{processName}】 收到PLC反馈{bar_no}数据{data}";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessStartBuildTestData(string processName, string bar_no)
        {
            var msg = $"流程 【{processName}】 开始处理{bar_no}数据";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessFinishedBuildTestData(string processName, string bar_no)
        {
            var msg = $"流程 【{processName}】 处理{bar_no}数据完成";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessUploadData(string processName)
        {
            var msg = $"流程 【{processName}】 数据上传成功";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadBindBar(string processName)
        {
            var msg = $"流程 【{processName}】 开始读取绑定码";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessReadBindBar(string processName,string sn)
        {
            var msg = $"流程 【{processName}】 读取绑定码【{sn}】";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void WriteIsSucess(OperateResult operateResult, string processName,int value)
        {
            if (!operateResult.IsSuccess)
            {
                var msg = $"流程 【{processName}】 返回PLC结果{value}失败";
                Log(LogLevel.Error, msg, null, true);
            }
            else
            {
                var msg = $"流程 【{processName}】 返回PLC结果{value}成功";
                Log(LogLevel.Info, msg, null, true);
            }
        }

        public static void IsSucess(OperateResult operateResult, string processName)
        {
            if (!operateResult.IsSuccess)
            {
                var msg = $"流程 【{processName}】 返回给PLC失败";
                Log(LogLevel.Error, msg, null, true);
            }
            else
            {
                var msg = $"流程 【{processName}】 返回给PLC成功";
                Log(LogLevel.Info, msg, null, true);
            }
        }
      
        public static void LogProcessInfo(string processName, string message, string sn = null)
        {
            var msg = sn != null ? $"流程 【{processName}】 - SN:【{sn}】- {message}": $"流程 【{processName}】 ";
            Log(LogLevel.Info, msg, null, true);
        }

        public static void LogProcessError(string processName, Exception ex)
        {
            var msg = $"流程 【{processName}】 出现异常: {ex.Message}";
            Log(LogLevel.Error, msg, ex, true);
        }

        public static void LogProcessError(string processName, Exception ex, string sn = null)
        {
            var message = sn != null ? $"流程 【{processName}】 - SN:【{sn}】 处理失败" : $"{processName}流程处理失败";
            Log(LogLevel.Error, message, ex, true);
        }

        public static void LogGenerateProductCode(string processName, string bar_no, Exception ex = null)
        {
            if (ex != null)
            {
                var msg = $"流程 【{processName}】 生成产品码:【{bar_no}】失败";
                Log(LogLevel.Error, msg, null, true);
            }
            else
            {
                var msg = $"流程 【{processName}】 生成产品码:【{bar_no}】成功";
                Log(LogLevel.Info, msg, ex, true);
            }
        }

        public static void LogProcessFileUpload(string processName, string bar_no, string fileName,Exception ex = null)
        {
            if (ex != null)
            {
                var msg = $"流程 【{processName}】 【{bar_no}】上传文件{fileName}失败";
                Log(LogLevel.Error, msg, null, true);
            }
            else
            {
                var msg = $"流程 【{processName}】 【{bar_no}】上传文件{fileName}成功";
                Log(LogLevel.Info, msg, ex, true);
            }
        }

        //  过站检测
        //public static void LogProcessCheckResult(string processName, string bar_no, string fileName, Exception ex = null)
        //{
        //    DisplayLog.Info($"流程 [{checkResultProcess.Name}] --查询{checkResultProcess.CheckResultInfo.Step}结果为{dataItem.Result}");

        //    if (ex != null)
        //    {
        //        var msg = $"流程 [{processName}] {bar_no}查询{fileName}结果为";
        //        Log(LogLevel.Error, msg, null, true);
        //    }
        //    else
        //    {
        //        var msg = $"流程 [{processName}] {bar_no}上传文件{fileName}结果为";
        //        Log(LogLevel.Info, msg, ex, true);
        //    }
        //}

        public static void LogSqlNoSn(string bar_no)
        {
            Info($"三花数据上传失败产品码为空——{bar_no}");
        }

        public static void LogSqlBasicData(string bar_no)
        {
            Info($"产品码:{bar_no}数据上传成功");
        }

        public static void LogSqlBasicDataErr(string bar_no, Exception ex)
        {
            Error($"产品码:{bar_no}数据上传失败", ex, true);
        }

        public static void LogSqlFile(string bar_no)
        {
            Info($"产品码文件:{bar_no}数据上传成功");
        }

        public static void LogSqlFileErr(string bar_no, Exception ex)
        {
            Error($"产品码文件:{bar_no}上传失败", ex, true);
        }

        public static void LogSqlCalibrationFinish(string test_item, string bar_no)
        {
            Info($"点检项---{test_item}---SN码:{bar_no}数据上传成功");
        }

        public static void LogSqlCalibrationErr(string test_item, string bar_no, Exception ex)
        {
            Error($"点检项---{test_item}---SN码:{bar_no}数据上传失败", ex, true);
        }

        public static void LogSqlSelectCalibrationItem(string inspection, string station_id)
        {
            Info($"点检配置:{inspection}--{station_id}--查询到配置项");
        }

        public static void LogSqlSelectCalibrationItemWarn(string inspection, string station_id)
        {
            Warn($"点检配置:{inspection}--{station_id}--不存在配置项");
        }

        public static void LogSqlSelectCalibrationItemErr(string inspection, string station_id, Exception ex)
        {
            Error($"点检配置:{inspection}--{station_id}--未查询到配置项", ex, true);
        }

        public static void LogSqlPeriodFinish(string test_item, string bar_no)
        {
            Info($"抽检项---{test_item}---SN码:{bar_no}数据上传成功");
        }

        public static void LogSqlPeriodErr(string test_item, string bar_no, Exception ex)
        {
            Error($"抽检项---{test_item}---SN码:{bar_no}数据上传失败", ex, true);
        }

        public static void LogSqlSelectNgCode(string bar_no)
        {
            Warn($"产品码:{bar_no}--未查询到NG代码");
        }

        public static void LogSqlSelectNgCodeFinish(string bar_no, string NgCode)
        {
            Info($"产品码:{bar_no}--查询到NG代码--{NgCode}");
        }

        public static void LogSqlSelectNgCodeFinishErr(string bar_no, Exception ex)
        {
            Error($"产品码:{bar_no}--未查询到NG代码", ex, true);
        }

        public static void LogSqlGenerateProductCode(string bar_no)
        {
            Info($"生成产品码成功:{bar_no}");
        }

        public static void LogSqlGenerateProductCodeErr(Exception ex)
        {
            Error("生成产品码异常:", ex, true);
        }

        #endregion
    }
}
