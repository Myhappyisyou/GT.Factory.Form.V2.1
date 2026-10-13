using GT.Factory.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace GT.Factory.Station.Helper
{
    #region 数据模型

    /// <summary>
    /// 工单生产统计。
    ///
    /// 统计原则：
    ///
    /// FirstInput：
    /// 实际进入设备的新产品数量。
    /// 一个新的 SN 只计一次。
    ///
    /// ReInput：
    /// 已经 NG 后重新进入设备的次数。
    /// 不代表新增产品数量。
    ///
    /// FirstPass / FirstFail：
    /// 产品第一次测试的结果。
    ///
    /// ReworkPass / ReworkFail：
    /// 产品返工测试的结果。
    ///
    /// FinalPass：
    /// 最终测试 OK 的实际产品数量。
    /// </summary>
    public class ProductionStats
    {
        /// <summary>
        /// 首次投入数量。
        ///
        /// 代表实际进入设备的新产品数量。
        /// </summary>
        public int FirstInput { get; set; }

        /// <summary>
        /// 返工投入次数。
        ///
        /// 同一个产品 NG 后再次进入设备时增加。
        /// </summary>
        public int ReInput { get; set; }

        /// <summary>
        /// 实际产品总数量。
        ///
        /// 一个产品只在首次进入设备时计算一次，
        /// 因此不包含 ReInput。
        /// </summary>
        public int Input => FirstInput;

        /// <summary>
        /// 首次测试 OK 数量。
        /// </summary>
        public int FirstPass { get; set; }

        /// <summary>
        /// 首次测试 NG 数量。
        /// </summary>
        public int FirstFail { get; set; }

        /// <summary>
        /// 返工测试 OK 次数。
        /// </summary>
        public int ReworkPass { get; set; }

        /// <summary>
        /// 返工测试 NG 次数。
        /// </summary>
        public int ReworkFail { get; set; }

        /// <summary>
        /// 当前 NG 待返工产品数量。
        ///
        /// 一个 SN 无论返工 NG 多少次，
        /// 始终只计算一个实际产品。
        ///
        /// 返工成功后，
        /// 该产品会从待返工数量中移除。
        /// </summary>
        public int FailedPending { get; set; }

        /// <summary>
        /// 最终 OK 产品数量。
        ///
        /// 计算方式：
        ///
        /// 首次 OK
        /// +
        /// 返工 OK
        /// </summary>
        public int FinalPass => FirstPass + ReworkPass;

        /// <summary>
        /// 已完成产品数量。
        ///
        /// 当前定义：
        /// 最终测试 OK 的产品数量。
        ///
        /// NG 产品仍然可以返工，
        /// 因此不会立即视为最终完成。
        /// </summary>
        public int CompletedCount => FinalPass + FailedPending;

        /// <summary>
        /// 一次通过率。
        ///
        /// FirstPass / FirstInput
        /// </summary>
        public double FPY =>
            FirstInput == 0
                ? 0
                : (double)FirstPass / FirstInput;

        /// <summary>
        /// 当前最终通过率。
        ///
        /// FinalPass / FirstInput
        ///
        /// 待返工产品会暂时降低该比例，
        /// 返工成功后比例自动恢复。
        /// </summary>
        public double Yield =>
            CompletedCount == 0
                ? 0
                : (double)FinalPass / CompletedCount;

        /// <summary>
        /// 产品完成率。
        ///
        /// 当前已最终 OK 的产品
        /// /
        /// 实际进入设备的产品。
        /// </summary>
        public double CompletionRate =>
            FirstInput == 0
                ? 0
                : (double)CompletedCount / FirstInput;

        /// <summary>
        /// 一次通过率文本。
        /// </summary>
        public string FPYText =>
            $"{FPY * 100:0.0}%";

        /// <summary>
        /// 最终通过率文本。
        /// </summary>
        public string YieldText =>
            $"{Yield * 100:0.0}%";

        /// <summary>
        /// 完成率文本。
        /// </summary>
        public string CompletionText =>
            $"{CompletionRate * 100:0.0}%";

        /// <summary>
        /// 重置统计数据。
        /// </summary>
        public void Reset()
        {
            FirstInput = 0;
            ReInput = 0;

            FirstPass = 0;
            FirstFail = 0;

            ReworkPass = 0;
            ReworkFail = 0;

            FailedPending = 0;
        }

        /// <summary>
        /// 创建统计数据副本。
        ///
        /// 避免 UI 直接持有内部统计对象。
        /// </summary>
        public ProductionStats Clone()
        {
            return new ProductionStats
            {
                FirstInput = FirstInput,
                ReInput = ReInput,

                FirstPass = FirstPass,
                FirstFail = FirstFail,

                ReworkPass = ReworkPass,
                ReworkFail = ReworkFail,

                FailedPending = FailedPending
            };
        }
    }

    /// <summary>
    /// 工单统计信息。
    /// </summary>
    public class WorkOrderStats
    {
        /// <summary>
        /// 工单号。
        /// </summary>
        public string WorkOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 工单统计数据。
        /// </summary>
        public ProductionStats Stats { get; set; }
            = new ProductionStats();

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime LastUpdated { get; set; }
            = DateTime.Now;
    }

    /// <summary>
    /// 当前正在设备中的产品信息。
    /// </summary>
    public class ProductInfo
    {
        /// <summary>
        /// 产品所属工单。
        /// </summary>
        public string WorkOrderNo { get; set; }
            = string.Empty;

        /// <summary>
        /// 是否属于返工流程。
        ///
        /// 由 ProductionMonitor 自动判断。
        /// </summary>
        public bool IsRework { get; set; }

        /// <summary>
        /// 产品进入设备的时间。
        /// </summary>
        public DateTime InputTime { get; set; }
            = DateTime.Now;
    }

    /// <summary>
    /// NG 待返工产品信息。
    ///
    /// 同一个 SN 在这里存在时，
    /// 下次 OnProductInput 自动识别为返工件。
    /// </summary>
    public class FailedProductInfo
    {
        /// <summary>
        /// 产品所属工单。
        /// </summary>
        public string WorkOrderNo { get; set; }
            = string.Empty;

        /// <summary>
        /// NG 时间。
        /// </summary>
        public DateTime FailedTime { get; set; }
            = DateTime.Now;

        /// <summary>
        /// 已经返工的次数。
        /// </summary>
        public int ReworkCount { get; set; }
    }

    /// <summary>
    /// 生产事件。
    ///
    /// 用于记录事件日志。
    /// </summary>
    public class ProductionEvent
    {
        /// <summary>
        /// 工单号。
        /// </summary>
        public string WorkOrderNo { get; set; }
            = string.Empty;

        /// <summary>
        /// 产品 SN。
        /// </summary>
        public string SerialNumber { get; set; }
            = string.Empty;

        /// <summary>
        /// 事件类型。
        /// </summary>
        public string EventType { get; set; }
            = string.Empty;

        /// <summary>
        /// 事件发生时间。
        /// </summary>
        public DateTime Timestamp { get; set; }
            = DateTime.Now;
    }

    /// <summary>
    /// 生产监控快照。
    ///
    /// 注意：
    ///
    /// 快照保存的是当前内存状态，
    /// 每次保存都会覆盖 current.json。
    ///
    /// 不保存事件日志历史，
    /// 因此不会因为快照而无限增长。
    /// </summary>
    public class SnapshotData
    {
        /// <summary>
        /// 当前所有工单统计。
        /// </summary>
        public Dictionary<string, WorkOrderStats> WorkOrders
        {
            get;
            set;
        }
        = new Dictionary<string, WorkOrderStats>();

        /// <summary>
        /// 当前正在设备中的产品。
        /// </summary>
        public Dictionary<string, ProductInfo> ActiveSNs
        {
            get;
            set;
        }
        = new Dictionary<string, ProductInfo>();

        /// <summary>
        /// 当前 NG 待返工产品。
        ///
        /// 这里只保存当前未完成的 NG 产品，
        /// 产品返工成功后会立即删除。
        ///
        /// 因此该集合不会无限增长。
        /// </summary>
        public Dictionary<string, FailedProductInfo> FailedSNs
        {
            get;
            set;
        }
        = new Dictionary<string, FailedProductInfo>();
    }

    #endregion

    /// <summary>
    /// 生产数量监控器。
    ///
    /// 核心职责：
    ///
    /// 1. 统计设备实际产品数量。
    /// 2. 自动识别返工件。
    /// 3. 管理当前设备中的产品。
    /// 4. 管理 NG 待返工产品。
    /// 5. 定期保存当前状态快照。
    /// 6. 记录生产事件日志。
    /// </summary>
    public class ProductionMonitor
    {
        #region 字段

        /// <summary>
        /// 所有工单统计。
        /// </summary>
        private readonly Dictionary<string, WorkOrderStats>
            _workOrders =
                new Dictionary<string, WorkOrderStats>();

        /// <summary>
        /// 当前正在设备中的 SN。
        ///
        /// 产品进入设备时加入，
        /// 检测完成后删除。
        /// </summary>
        private readonly Dictionary<string, ProductInfo>
            _activeSNs =
                new Dictionary<string, ProductInfo>();

        /// <summary>
        /// 当前 NG 待返工 SN。
        ///
        /// 自动识别返工的核心集合。
        /// </summary>
        private readonly Dictionary<string, FailedProductInfo>
            _failedSNs =
                new Dictionary<string, FailedProductInfo>();

        /// <summary>
        /// 生产事件队列。
        /// </summary>
        private readonly ConcurrentQueue<ProductionEvent>
            _eventQueue =
                new ConcurrentQueue<ProductionEvent>();

        /// <summary>
        /// 事件日志刷新定时器。
        /// </summary>
        private readonly Timer _flushTimer;

        /// <summary>
        /// 快照保存定时器。
        /// </summary>
        private readonly Timer _snapshotTimer;


        /// <summary>
        /// 工单统计数据过期时间。
        ///
        /// 工单在指定时间内没有任何更新，
        /// 则认为已经是历史数据，
        /// 可以从当前快照中清除。
        /// </summary>
        private readonly TimeSpan _workOrderExpiration =
            TimeSpan.FromDays(30);

        /// <summary>
        /// 当前设备中产品的最大停留时间。
        ///
        /// 超过该时间仍未收到检测完成事件，
        /// 认为属于异常残留数据。
        /// </summary>
        private readonly TimeSpan _activeProductExpiration =
            TimeSpan.FromHours(24);

        /// <summary>
        /// NG 待返工产品的最大保留时间。
        ///
        /// 超过该时间仍未返工，
        /// 则从当前待返工集合中清除。
        /// </summary>
        private readonly TimeSpan _failedProductExpiration =
            TimeSpan.FromDays(30);

        /// <summary>
        /// 数据同步锁。
        ///
        /// 用于保护：
        ///
        /// _workOrders
        /// _activeSNs
        /// _failedSNs
        /// </summary>
        private readonly object _syncLock =
            new object();

        /// <summary>
        /// 文件写入锁。
        /// </summary>
        private readonly object _fileLock =
            new object();

        /// <summary>
        /// 基础目录。
        /// </summary>
        private readonly string _baseDir;

        /// <summary>
        /// 事件日志目录。
        /// </summary>
        private readonly string _logDir;

        /// <summary>
        /// 快照目录。
        /// </summary>
        private readonly string _snapshotDir;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建生产监控器。
        /// </summary>
        public ProductionMonitor()
        {
            _baseDir =
                PathCenter.HistoryFile(
                    "ProductionMonitor");

            _logDir =
                Path.Combine(
                    _baseDir,
                    "EventLog");

            _snapshotDir =
                Path.Combine(
                    _baseDir,
                    "Snapshot");

            Directory.CreateDirectory(
                _logDir);

            Directory.CreateDirectory(
                _snapshotDir);

            // 启动时恢复最后一次快照。
            LoadSnapshot();

            // 每 5 秒写入一次事件日志。
            _flushTimer =
                new Timer(5000);

            _flushTimer.Elapsed +=
                FlushEvents;

            _flushTimer.AutoReset =
                true;

            _flushTimer.Start();

            // 每 5 秒保存一次当前状态快照。
            //
            // 每次覆盖 current.json，
            // 不会产生不断增加的快照文件。
            _snapshotTimer =
                new Timer(5000);

            _snapshotTimer.Elapsed +=
                (s, e) => SaveSnapshot();

            _snapshotTimer.AutoReset =
                true;

            _snapshotTimer.Start();
        }

        #endregion

        #region 投料

        /// <summary>
        /// 产品进入设备。
        ///
        /// 自动判断是否为返工件。
        ///
        /// 判断规则：
        ///
        /// 如果 SN 存在于 _failedSNs：
        ///     自动识别为返工件。
        ///
        /// 否则：
        ///     识别为首次投入。
        /// </summary>
        /// <param name="sn">
        /// 产品序列号。
        /// </param>
        /// <param name="workOrderNo">
        /// 工单号。
        /// </param>
        public void OnProductInput(
            string sn,
            string workOrderNo)
        {
            if (string.IsNullOrWhiteSpace(sn))
                return;

            if (string.IsNullOrWhiteSpace(workOrderNo))
                return;

            sn =
                sn.Trim();

            workOrderNo =
                workOrderNo.Trim();

            string eventType;

            lock (_syncLock)
            {
                // 防止同一个 SN 在未检测完成前重复投入。
                if (_activeSNs.ContainsKey(sn))
                {
                    return;
                }

                // 获取或创建工单统计对象。
                var workOrder =
                    GetOrCreateWorkOrder(
                        workOrderNo);

                // 自动判断是否为返工件。
                var isRework =
                    _failedSNs.TryGetValue(
                        sn,
                        out var failedInfo);

                // 如果是返工件，
                // 优先使用第一次 NG 时保存的工单。
                if (isRework &&
                    !string.IsNullOrWhiteSpace(
                        failedInfo.WorkOrderNo))
                {
                    workOrderNo =
                        failedInfo.WorkOrderNo;

                    workOrder =
                        GetOrCreateWorkOrder(
                            workOrderNo);

                    // 增加返工投入次数。
                    workOrder.Stats.ReInput++;

                    // 增加返工次数记录。
                    failedInfo.ReworkCount++;

                    eventType =
                        "ReInput";
                }
                else
                {
                    // 新产品进入设备，
                    // 实际产品数量增加。
                    workOrder.Stats.FirstInput++;

                    eventType =
                        "FirstInput";
                }

                // 更新工单最后更新时间。
                workOrder.LastUpdated =
                    DateTime.Now;

                // 将产品加入当前设备状态。
                _activeSNs[sn] =
                    new ProductInfo
                    {
                        WorkOrderNo =
                            workOrderNo,

                        IsRework =
                            isRework,

                        InputTime =
                            DateTime.Now
                    };
            }

            // 记录投料事件。
            Enqueue(
                workOrderNo,
                sn,
                eventType);
        }

        #endregion

        #region 检测完成

        /// <summary>
        /// 产品检测完成。
        ///
        /// OK：
        /// 产品最终完成，
        /// 从当前设备和待返工集合中删除。
        ///
        /// NG：
        /// 产品进入待返工集合，
        /// 下次投入时自动识别为返工件。
        /// </summary>
        /// <param name="sn">
        /// 产品序列号。
        /// </param>
        /// <param name="isPass">
        /// 是否测试 OK。
        /// </param>
        public void OnProductDetected(
            string sn,
            bool isPass)
        {
            if (string.IsNullOrWhiteSpace(sn))
                return;

            sn =
                sn.Trim();

            string workOrderNo;
            string eventType;

            lock (_syncLock)
            {
                // 产品必须存在于当前设备中。
                if (!_activeSNs.TryGetValue(
                        sn,
                        out var productInfo))
                {
                    return;
                }

                workOrderNo =
                    productInfo.WorkOrderNo;

                // 获取所属工单。
                if (!_workOrders.TryGetValue(
                        workOrderNo,
                        out var workOrder))
                {
                    // 数据异常时删除当前设备状态，
                    // 防止 SN 永久卡在设备中。
                    _activeSNs.Remove(sn);

                    return;
                }

                // 更新工单时间。
                workOrder.LastUpdated =
                    DateTime.Now;

                // 根据首次 / 返工状态统计结果。
                if (productInfo.IsRework)
                {
                    if (isPass)
                    {
                        workOrder.Stats.ReworkPass++;

                        eventType =
                            "ReworkPass";
                    }
                    else
                    {
                        workOrder.Stats.ReworkFail++;

                        eventType =
                            "ReworkFail";
                    }
                }
                else
                {
                    if (isPass)
                    {
                        workOrder.Stats.FirstPass++;

                        eventType =
                            "FirstPass";
                    }
                    else
                    {
                        workOrder.Stats.FirstFail++;

                        eventType =
                            "FirstFail";
                    }
                }

                // 产品检测结束，
                // 从当前设备集合删除。
                _activeSNs.Remove(sn);

                if (isPass)
                {
                    // 产品最终 OK。
                    //
                    // 如果之前是返工件，
                    // 从待返工集合中删除。
                    _failedSNs.Remove(sn);
                }
                else
                {
                    // 产品 NG，
                    // 保存到待返工集合。
                    if (!_failedSNs.TryGetValue(
                            sn,
                            out var failedInfo))
                    {
                        failedInfo =
                            new FailedProductInfo
                            {
                                WorkOrderNo =
                                    workOrderNo,

                                FailedTime =
                                    DateTime.Now,

                                ReworkCount =
                                    0
                            };

                        _failedSNs[sn] =
                            failedInfo;
                    }
                    else
                    {
                        // 多次返工 NG 时，
                        // 更新最后 NG 时间。
                        failedInfo.FailedTime =
                            DateTime.Now;
                    }
                }
            }

            // 记录检测结果事件。
            Enqueue(
                workOrderNo,
                sn,
                eventType);
        }

        #endregion

        #region 工单管理

        /// <summary>
        /// 获取或创建工单统计对象。
        ///
        /// 调用此方法时必须已经进入 _syncLock。
        /// </summary>
        private WorkOrderStats GetOrCreateWorkOrder(
            string workOrderNo)
        {
            if (_workOrders.TryGetValue(
                    workOrderNo,
                    out var workOrder))
            {
                return workOrder;
            }

            workOrder =
                new WorkOrderStats
                {
                    WorkOrderNo =
                        workOrderNo,

                    LastUpdated =
                        DateTime.Now
                };

            _workOrders[workOrderNo] =
                workOrder;

            return workOrder;
        }

        #endregion

        #region 事件日志

        /// <summary>
        /// 添加生产事件。
        /// </summary>
        private void Enqueue(
            string workOrderNo,
            string sn,
            string eventType)
        {
            _eventQueue.Enqueue(
                new ProductionEvent
                {
                    WorkOrderNo =
                        workOrderNo,

                    SerialNumber =
                        sn,

                    EventType =
                        eventType,

                    Timestamp =
                        DateTime.Now
                });
        }

        /// <summary>
        /// 定期写入事件日志。
        ///
        /// 事件日志按日期保存，
        /// 用于历史追溯。
        /// </summary>
        private void FlushEvents(
            object sender,
            ElapsedEventArgs e)
        {
            try
            {
                var lines =
                    new List<string>();

                while (_eventQueue.TryDequeue(
                    out var productionEvent))
                {
                    lines.Add(
                        JsonConvert.SerializeObject(
                            productionEvent));
                }

                if (lines.Count == 0)
                    return;

                var file =
                    Path.Combine(
                        _logDir,
                        DateTime.Now.ToString(
                            "yyyy-MM-dd")
                        + ".log");

                lock (_fileLock)
                {
                    File.AppendAllLines(
                        file,
                        lines);
                }
            }
            catch
            {
                // 日志保存失败不影响生产程序。
            }
        }

        #endregion

        #region 快照

        /// <summary>
        /// 保存当前状态快照。
        ///
        /// 关键原则：
        ///
        /// 每次保存覆盖 current.json，
        /// 不创建新的快照文件。
        ///
        /// 因此不会出现快照文件数量持续增加的问题。
        /// </summary>
        private void SaveSnapshot()
        {
            try
            {
                SnapshotData snapshot;

                lock (_syncLock)
                {
                    // 保存快照前，
                    // 先清理已经过期的内存数据。
                    CleanupExpiredData();

                    // 创建独立快照，
                    // 避免序列化过程中集合被其他线程修改。
                    snapshot =
                        CreateSnapshot();
                }

                var json =
                    JsonConvert.SerializeObject(
                        snapshot,
                        Formatting.Indented);

                var file =
                    Path.Combine(
                        _snapshotDir,
                        "current.json");

                var tempFile =
                    file + ".tmp";

                lock (_fileLock)
                {
                    // 先写入临时文件。
                    File.WriteAllText(
                        tempFile,
                        json);

                    // 再替换正式快照。
                    //
                    // 防止程序突然关闭时
                    // current.json 只写入一半。
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    File.Move(
                        tempFile,
                        file);
                }
            }
            catch
            {
                // 快照失败不影响生产程序。
            }
        }

        /// <summary>
        /// 创建当前内存状态快照。
        ///
        /// 调用此方法时必须已经进入 _syncLock。
        /// </summary>
        private SnapshotData CreateSnapshot()
        {
            var snapshot =
                new SnapshotData();

            // 复制工单统计。
            foreach (var item in _workOrders)
            {
                snapshot.WorkOrders[item.Key] =
                    new WorkOrderStats
                    {
                        WorkOrderNo =
                            item.Value.WorkOrderNo,

                        LastUpdated =
                            item.Value.LastUpdated,

                        Stats =
                            item.Value.Stats.Clone()
                    };
            }

            // 复制当前设备中的产品。
            foreach (var item in _activeSNs)
            {
                snapshot.ActiveSNs[item.Key] =
                    new ProductInfo
                    {
                        WorkOrderNo =
                            item.Value.WorkOrderNo,

                        IsRework =
                            item.Value.IsRework,

                        InputTime =
                            item.Value.InputTime
                    };
            }

            // 复制当前 NG 待返工产品。
            foreach (var item in _failedSNs)
            {
                snapshot.FailedSNs[item.Key] =
                    new FailedProductInfo
                    {
                        WorkOrderNo =
                            item.Value.WorkOrderNo,

                        FailedTime =
                            item.Value.FailedTime,

                        ReworkCount =
                            item.Value.ReworkCount
                    };
            }

            return snapshot;
        }

        /// <summary>
        /// 启动时恢复最后一次快照。
        /// </summary>
        private void LoadSnapshot()
        {
            try
            {
                var file =
                    Path.Combine(
                        _snapshotDir,
                        "current.json");

                if (!File.Exists(file))
                    return;

                var json =
                    File.ReadAllText(
                        file);

                var snapshot =
                    JsonConvert.DeserializeObject<SnapshotData>(
                        json);

                if (snapshot == null)
                    return;

                lock (_syncLock)
                {
                    // 恢复工单统计。
                    if (snapshot.WorkOrders != null)
                    {
                        foreach (var item
                            in snapshot.WorkOrders)
                        {
                            _workOrders[item.Key] =
                                item.Value;
                        }
                    }

                    // 恢复当前设备中的产品。
                    if (snapshot.ActiveSNs != null)
                    {
                        foreach (var item
                            in snapshot.ActiveSNs)
                        {
                            _activeSNs[item.Key] =
                                item.Value;
                        }
                    }

                    // 恢复 NG 待返工产品。
                    if (snapshot.FailedSNs != null)
                    {
                        foreach (var item
                            in snapshot.FailedSNs)
                        {
                            _failedSNs[item.Key] =
                                item.Value;
                        }
                    }
                }
            }
            catch
            {
                // 快照读取失败时，
                // 直接使用空数据启动。
            }
        }


        #region 过期数据清理

        /// <summary>
        /// 清理过期的生产数据。
        ///
        /// 清理范围：
        ///
        /// 1. 长时间未更新的工单。
        /// 2. 长时间停留在设备中的异常产品。
        /// 3. 长时间未返工的 NG 产品。
        ///
        /// 注意：
        ///
        /// 此方法必须在 _syncLock 锁内调用。
        /// </summary>
        private void CleanupExpiredData()
        {
            var now =
                DateTime.Now;

            CleanupExpiredActiveProducts(
                now);

            CleanupExpiredFailedProducts(
                now);

            CleanupExpiredWorkOrders(
                now);
        }

        /// <summary>
        /// 清理长时间停留在设备中的产品。
        ///
        /// 正常情况下：
        ///
        /// OnProductInput
        ///     ↓
        /// 加入 _activeSNs
        ///     ↓
        /// OnProductDetected
        ///     ↓
        /// 删除
        ///
        /// 如果程序异常、PLC异常或者检测流程中断，
        /// 产品可能永久停留在 _activeSNs。
        ///
        /// 因此超过最大停留时间后自动清理。
        /// </summary>
        private void CleanupExpiredActiveProducts(
            DateTime now)
        {
            var expiredSNs =
                _activeSNs
                    .Where(
                        item =>
                            now - item.Value.InputTime
                            > _activeProductExpiration)
                    .Select(
                        item =>
                            item.Key)
                    .ToList();

            foreach (var sn in expiredSNs)
            {
                _activeSNs.Remove(
                    sn);

                Enqueue(
                    string.Empty,
                    sn,
                    "ActiveExpired");
            }
        }

        /// <summary>
        /// 清理过期的 NG 待返工产品。
        ///
        /// 一个产品长时间没有重新进入设备，
        /// 则认为当前生产周期已经结束。
        ///
        /// 清理后：
        ///
        /// 下次再次投入该 SN 时，
        /// 会被视为新的首次投入产品。
        /// </summary>
        private void CleanupExpiredFailedProducts(
            DateTime now)
        {
            var expiredSNs =
                _failedSNs
                    .Where(
                        item =>
                            now - item.Value.FailedTime
                            > _failedProductExpiration)
                    .Select(
                        item =>
                            item.Key)
                    .ToList();

            foreach (var sn in expiredSNs)
            {
                var failedInfo =
                    _failedSNs[sn];

                _failedSNs.Remove(
                    sn);

                Enqueue(
                    failedInfo.WorkOrderNo,
                    sn,
                    "FailedExpired");
            }
        }

        /// <summary>
        /// 清理过期工单统计。
        ///
        /// 只有满足以下条件才删除：
        ///
        /// 1. 工单长时间没有更新。
        /// 2. 当前没有产品正在设备中。
        /// 3. 当前没有 NG 产品等待返工。
        ///
        /// 这样不会误删仍然处于生产状态的工单。
        /// </summary>
        private void CleanupExpiredWorkOrders(
            DateTime now)
        {
            var activeWorkOrders =
                new HashSet<string>(
                    _activeSNs.Values
                        .Select(
                            item =>
                                item.WorkOrderNo));

            var failedWorkOrders =
                new HashSet<string>(
                    _failedSNs.Values
                        .Select(
                            item =>
                                item.WorkOrderNo));

            var expiredWorkOrders =
                _workOrders
                    .Where(
                        item =>
                        {
                            var workOrder =
                                item.Value;

                            // 工单最近仍有更新，
                            // 不允许删除。
                            if (now - workOrder.LastUpdated
                                <= _workOrderExpiration)
                            {
                                return false;
                            }

                            // 工单仍有产品在设备中，
                            // 不允许删除。
                            if (activeWorkOrders.Contains(
                                    workOrder.WorkOrderNo))
                            {
                                return false;
                            }

                            // 工单仍存在待返工产品，
                            // 不允许删除。
                            if (failedWorkOrders.Contains(
                                    workOrder.WorkOrderNo))
                            {
                                return false;
                            }

                            return true;
                        })
                    .Select(
                        item =>
                            item.Key)
                    .ToList();

            foreach (var workOrderNo
                in expiredWorkOrders)
            {
                _workOrders.Remove(
                    workOrderNo);

                Enqueue(
                    workOrderNo,
                    "SYSTEM",
                    "WorkOrderExpired");
            }
        }

        #endregion

        #endregion

        #region 查询

        /// <summary>
        /// 获取指定工单统计。
        ///
        /// 返回数据副本，
        /// 避免外部直接修改内部统计数据。
        /// </summary>
        public ProductionStats GetStats(
            string workOrderNo)
        {
            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                return new ProductionStats();
            }

            lock (_syncLock)
            {
                if (!_workOrders.TryGetValue(
                        workOrderNo,
                        out var workOrder))
                {
                    return new ProductionStats();
                }

                // 创建统计副本。
                var stats =
                    workOrder.Stats.Clone();

                // 根据当前待返工 SN 集合，
                // 实时计算当前 NG 待返工产品数量。
                //
                // 一个 SN 在集合中只存在一次，
                // 因此不会因为多次返工 NG 而重复计算。
                stats.FailedPending =
                    _failedSNs.Values.Count(
                        item =>
                            item.WorkOrderNo ==
                            workOrderNo);

                return stats;
            }
        }

        /// <summary>
        /// 获取所有工单总统计。
        ///
        /// 注意：
        ///
        /// FirstInput 表示实际产品数量。
        ///
        /// ReInput 仅表示返工次数。
        /// </summary>
        public ProductionStats GetTotalStats()
        {
            lock (_syncLock)
            {
                var total =
                    new ProductionStats();

                foreach (var workOrder
                    in _workOrders.Values)
                {
                    total.FirstInput +=
                        workOrder.Stats.FirstInput;

                    total.ReInput +=
                        workOrder.Stats.ReInput;

                    total.FirstPass +=
                        workOrder.Stats.FirstPass;

                    total.FirstFail +=
                        workOrder.Stats.FirstFail;

                    total.ReworkPass +=
                        workOrder.Stats.ReworkPass;

                    total.ReworkFail +=
                        workOrder.Stats.ReworkFail;

                    total.FailedPending =
                        _failedSNs.Count;
                }

                return total;
            }
        }

        /// <summary>
        /// 获取所有工单。
        ///
        /// 返回副本，
        /// 防止外部修改内部数据。
        /// </summary>
        public IEnumerable<WorkOrderStats>
            GetAllWorkOrders()
        {
            lock (_syncLock)
            {
                return _workOrders.Values
                    .Select(
                        item =>
                            new WorkOrderStats
                            {
                                WorkOrderNo =
                                    item.WorkOrderNo,

                                LastUpdated =
                                    item.LastUpdated,

                                Stats =
                                    item.Stats.Clone()
                            })
                    .ToList();
            }
        }

        /// <summary>
        /// 获取当前设备中的产品数量。
        /// </summary>
        public int GetPendingCount()
        {
            lock (_syncLock)
            {
                return _activeSNs.Count;
            }
        }

        /// <summary>
        /// 获取指定工单当前设备中的产品数量。
        /// </summary>
        public int GetPendingCount(
            string workOrderNo)
        {
            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                return 0;
            }

            lock (_syncLock)
            {
                return _activeSNs.Values.Count(
                    item =>
                        item.WorkOrderNo ==
                        workOrderNo);
            }
        }

        /// <summary>
        /// 获取当前 NG 待返工产品数量。
        ///
        /// 一个 SN 无论返工多少次，
        /// 在这里始终只计算一个实际产品。
        /// </summary>
        public int GetFailedProductCount()
        {
            lock (_syncLock)
            {
                return _failedSNs.Count;
            }
        }

        /// <summary>
        /// 获取指定工单当前 NG 待返工产品数量。
        /// </summary>
        public int GetFailedProductCount(
            string workOrderNo)
        {
            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                return 0;
            }

            lock (_syncLock)
            {
                return _failedSNs.Values.Count(
                    item =>
                        item.WorkOrderNo ==
                        workOrderNo);
            }
        }

        /// <summary>
        /// 判断指定 SN 是否为待返工产品。
        /// </summary>
        public bool IsFailedProduct(
            string sn)
        {
            if (string.IsNullOrWhiteSpace(sn))
                return false;

            lock (_syncLock)
            {
                return _failedSNs.ContainsKey(
                    sn.Trim());
            }
        }

        #endregion

        #region 重置

        /// <summary>
        /// 重置指定工单的统计数据。
        ///
        /// 注意：
        ///
        /// 只重置统计数字。
        ///
        /// 不删除：
        ///
        /// 1. 当前设备中的产品。
        /// 2. 当前 NG 待返工产品。
        ///
        /// 这样不会破坏正在运行的产品生命周期。
        /// </summary>
        public bool ResetWorkOrder(
            string workOrderNo)
        {
            if (string.IsNullOrWhiteSpace(
                    workOrderNo))
            {
                return false;
            }

            workOrderNo =
                workOrderNo.Trim();

            bool success;

            lock (_syncLock)
            {
                if (!_workOrders.TryGetValue(
                        workOrderNo,
                        out var workOrder))
                {
                    return false;
                }

                // 清零工单统计。
                workOrder.Stats.Reset();

                // 更新工单时间。
                workOrder.LastUpdated =
                    DateTime.Now;

                success =
                    true;
            }

            // 记录清零事件。
            Enqueue(
                workOrderNo,
                "SYSTEM_RESET",
                "Reset");

            // 立即保存快照。
            SaveSnapshot();

            return success;
        }

        #endregion

        #region 停止

        /// <summary>
        /// 停止生产监控器。
        ///
        /// 停止定时器后：
        ///
        /// 1. 刷新剩余事件日志。
        /// 2. 保存最终快照。
        /// </summary>
        public void Stop()
        {
            _flushTimer.Stop();

            _snapshotTimer.Stop();

            // 写入队列中剩余日志。
            FlushEvents(
                this,
                null);

            // 保存最终状态。
            SaveSnapshot();

            _flushTimer.Dispose();

            _snapshotTimer.Dispose();
        }

        #endregion
    }
}



