using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Batch
{
    public static class BatchLog
    {
        private static readonly string LogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BatchOperations.log");

        // UI回调（可选）
        public static Action<string> OnLog { get; set; }

        public static void Write(string partName, string action, int oldValue, int newValue, string operatorName = "System")
        {
            string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {operatorName} | {partName} | {action} | {oldValue} => {newValue}";

            try
            {
                File.AppendAllText(LogFile, msg + Environment.NewLine);
            }
            catch { /* 忽略写入异常 */ }

            OnLog?.Invoke(msg);
        }

        // 扫码日志
        public static void Scan(string partName, int totalQty, int usedQty, string operatorName = "System")
        {
            string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {operatorName} | {partName} | Scan | Total={totalQty}, Used={usedQty}";

            try { File.AppendAllText(LogFile, msg + Environment.NewLine); } catch { }

            OnLog?.Invoke(msg);
        }
    }
}
