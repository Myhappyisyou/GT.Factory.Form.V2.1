using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace GT_Common.Helper
{
    #region 数据模型

    public class ProductionStats
    {
        public int Input { get; set; }

        public int FirstPass { get; set; }
        public int FirstFail { get; set; }

        public int ReworkPass { get; set; }
        public int ReworkFail { get; set; }

        public int CompletedCount => FirstPass + FirstFail + ReworkPass + ReworkFail;

        public int FinalPass => FirstPass + ReworkPass;
        public int FinalFail => FirstFail + ReworkFail;

        public double FPY => Input == 0 ? 0 : (double)FirstPass / Input;

        public double Yield => CompletedCount == 0 ? 0 : (double)FinalPass / CompletedCount;

        public double CompletionRate => Input == 0 ? 0 : (double)CompletedCount / Input;

        public string FPYText => $"{FPY * 100:0.0}%";
        public string YieldText => $"{Yield * 100:0.0}%";
        public string CompletionText => $"{CompletionRate * 100:0.0}%";

        public void Reset()
        {
            Input = 0;
            FirstPass = 0;
            FirstFail = 0;
            ReworkPass = 0;
            ReworkFail = 0;
        }
    }

    public class WorkOrderStats
    {
        public string WorkOrderNo { get; set; } = "";
        public ProductionStats Stats { get; set; } = new ProductionStats();
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class ProductInfo
    {
        public string WorkOrderNo { get; set; }
        public bool IsRework { get; set; }
    }

    public class ProductionEvent
    {
        public string WorkOrderNo { get; set; }
        public string SerialNumber { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class SnapshotData
    {
        public Dictionary<string, WorkOrderStats> WorkOrders { get; set; } = new Dictionary<string, WorkOrderStats>();
        public Dictionary<string, ProductInfo> PendingSNs { get; set; } = new Dictionary<string, ProductInfo>();
    }

    #endregion

    public class ProductionMonitor
    {
        private readonly Dictionary<string, WorkOrderStats> _workOrders = new Dictionary<string, WorkOrderStats>();
        private readonly Dictionary<string, ProductInfo> _snMap = new Dictionary<string, ProductInfo>();

        private readonly ConcurrentQueue<ProductionEvent> _eventQueue = new ConcurrentQueue<ProductionEvent>();

        private readonly Timer _flushTimer;
        private readonly Timer _snapshotTimer;

        private readonly object _fileLock = new object();

        private readonly string _baseDir;
        private readonly string _logDir;
        private readonly string _snapshotDir;

        public ProductionMonitor()
        {
            _baseDir = PathCenter.HistoryFile("ProductionMonitor");
            _logDir = Path.Combine(_baseDir, "EventLog");
            _snapshotDir = Path.Combine(_baseDir, "Snapshot");

            Directory.CreateDirectory(_logDir);
            Directory.CreateDirectory(_snapshotDir);

            LoadSnapshot();

            // 每5秒落盘日志
            _flushTimer = new Timer(5000);
            _flushTimer.Elapsed += FlushEvents;
            _flushTimer.Start();

            // 每30秒保存快照
            _snapshotTimer = new Timer(30000);
            _snapshotTimer.Elapsed += (s, e) => SaveSnapshot();
            _snapshotTimer.Start();
        }

        #region 投料

        public void OnProductInput(string sn, string workOrderNo, bool isRework = false)
        {
            if (!_workOrders.TryGetValue(workOrderNo, out var wo))
            {
                wo = new WorkOrderStats { WorkOrderNo = workOrderNo };
                _workOrders[workOrderNo] = wo;
            }

            wo.LastUpdated = DateTime.Now;

            if (!isRework)
                wo.Stats.Input++;

            _snMap[sn] = new ProductInfo
            {
                WorkOrderNo = workOrderNo,
                IsRework = isRework
            };

            Enqueue(workOrderNo, sn, isRework ? "ReworkInput" : "Input");
        }

        #endregion

        #region 下料

        public void OnProductDetected(string sn, bool isPass)
        {
            if (!_snMap.TryGetValue(sn, out var info))
                return;

            if (!_workOrders.TryGetValue(info.WorkOrderNo, out var wo))
                return;

            wo.LastUpdated = DateTime.Now;

            if (info.IsRework)
            {
                if (isPass)
                    wo.Stats.ReworkPass++;
                else
                    wo.Stats.ReworkFail++;
            }
            else
            {
                if (isPass)
                    wo.Stats.FirstPass++;
                else
                    wo.Stats.FirstFail++;
            }

            Enqueue(info.WorkOrderNo, sn, isPass ? "Pass" : "Fail");

            _snMap.Remove(sn);
        }

        #endregion

        #region 日志队列

        private void Enqueue(string wo, string sn, string type)
        {
            _eventQueue.Enqueue(new ProductionEvent
            {
                WorkOrderNo = wo,
                SerialNumber = sn,
                EventType = type
            });
        }

        private void FlushEvents(object sender, ElapsedEventArgs e)
        {
            try
            {
                var list = new List<string>();

                while (_eventQueue.TryDequeue(out var evt))
                {
                    list.Add(JsonConvert.SerializeObject(evt));
                }

                if (list.Count == 0) return;

                var file = Path.Combine(_logDir, DateTime.Now.ToString("yyyy-MM-dd") + ".log");

                lock (_fileLock)
                {
                    File.AppendAllLines(file, list);
                }
            }
            catch { }
        }

        #endregion

        #region 快照

        private void SaveSnapshot()
        {
            try
            {
                var data = new SnapshotData
                {
                    WorkOrders = _workOrders,
                    PendingSNs = _snMap
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                File.WriteAllText(
                    Path.Combine(_snapshotDir, "current.json"),
                    json);
            }
            catch { }
        }

        private void LoadSnapshot()
        {
            try
            {
                var file = Path.Combine(_snapshotDir, "current.json");
                if (!File.Exists(file)) return;

                var json = File.ReadAllText(file);
                var data = JsonConvert.DeserializeObject<SnapshotData>(json);

                if (data == null) return;

                foreach (var kv in data.WorkOrders)
                    _workOrders[kv.Key] = kv.Value;

                foreach (var kv in data.PendingSNs)
                    _snMap[kv.Key] = kv.Value;
            }
            catch { }
        }

        #endregion

        #region 查询

        public ProductionStats GetStats(string workOrderNo)
        {
            return _workOrders.TryGetValue(workOrderNo, out var wo)
                ? wo.Stats
                : new ProductionStats();
        }

        public ProductionStats GetTotalStats()
        {
            var total = new ProductionStats();

            foreach (var wo in _workOrders.Values)
            {
                total.Input += wo.Stats.Input;
                total.FirstPass += wo.Stats.FirstPass;
                total.FirstFail += wo.Stats.FirstFail;
                total.ReworkPass += wo.Stats.ReworkPass;
                total.ReworkFail += wo.Stats.ReworkFail;
            }

            return total;
        }

        public IEnumerable<WorkOrderStats> GetAllWorkOrders()
            => _workOrders.Values;

        #endregion

        public void Stop()
        {
            _flushTimer.Stop();
            _snapshotTimer.Stop();
            SaveSnapshot();
        }
    }
}