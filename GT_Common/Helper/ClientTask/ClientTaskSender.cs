using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaskContracts;

namespace GT_Common.Helper.ClientTask
{
    public sealed class ClientTaskSender : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;

        private readonly string _pendingFile = PathCenter.HistoryFile(Path.Combine("PendingTasks", "PendingTasks.json")); // 本地缓存

        //private readonly string _pendingFile = "PendingTasks.json"; // 本地缓存

        private readonly object _fileLock = new object();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _retryWorker;

      
        private const int MaxRetryCount = 5;
        private const int RetryIntervalMs = 500;
        private const int BaseRetryMs = 1000;
        private const int SlowRetryMs = 5 * 60 * 1000; // 5分钟


        /// <summary>日志回调</summary>
        public Action<string> LogCallback { get; set; }

        private void EnsureDirectory()
        {
            var dir = Path.GetDirectoryName(_pendingFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        //指数退避算法
        //private TimeSpan GetRetryDelay(PendingTask task)
        //{
        //    if (task.RetryCount < MaxRetryCount)
        //    {
        //        // 指数退避：1s,2s,4s,8s,16s
        //        int ms = (int)Math.Pow(2, task.RetryCount) * BaseRetryMs;
        //        return TimeSpan.FromMilliseconds(ms);
        //    }

        //    // 超过最大次数 → 慢速重试
        //    return TimeSpan.FromMilliseconds(SlowRetryMs);
        //}
        private TimeSpan GetRetryDelay(PendingTask task)
        {
            int[] retryTable =
            {
                1000,   // 1s
                2000,   // 2s
                4000,   // 4s
                8000,   // 8s
                16000,  // 16s
                30000,  // 30s
                60000,  // 1min
                120000, // 2min
                300000  // 5min
            };

            int index = Math.Min(task.RetryCount, retryTable.Length - 1);

            return TimeSpan.FromMilliseconds(retryTable[index]);
        }

        public ClientTaskSender(string serverUrl)
        {
            if (serverUrl == null) throw new ArgumentNullException(nameof(serverUrl));
            _serverUrl = serverUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            _retryWorker = Task.Run(RetryPendingTasks, _cts.Token);
        }

        #region 对外发送接口

        /// <summary>
        /// 发送任务（失败自动进入 Pending）
        /// </summary>
        public async Task<bool> SendTaskAsync(DbTask task)
        {
            return await SendInternalAsync(task, fromRetry: false);
        }

        /// <summary>
        /// 快速发送任务（带 Payload）
        /// </summary>
        public Task SendTaskAsync<T>(string taskType, T payload)
        {
            var task = new DbTask
            {
                TaskType = taskType,
                PayloadJson = JsonConvert.SerializeObject(payload),
                CreateTime = DateTime.Now
            };
            return SendTaskAsync(task);
        }

        #endregion

        #region 核心发送逻辑

        private async Task<bool> SendInternalAsync(DbTask task, bool fromRetry)
        {
            try
            {
                string json = JsonConvert.SerializeObject(task);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_serverUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Log($"【ClientTaskSender】发送成功: {task.TaskType}");
                    return true;
                }

                Log($"【ClientTaskSender】发送失败: {task.TaskType} {response.StatusCode}");
                if (!fromRetry) SavePending(task, response.StatusCode.ToString());
                return false;
            }
            catch (Exception ex)
            {
                Log($"【ClientTaskSender】服务器不可达: {ex.Message}");
                if (!fromRetry) SavePending(task, ex.Message);
                return false;
            }
        }

        #endregion

        #region Pending 持久化

        private void SavePending(DbTask task, string error)
        {
            lock (_fileLock)
            {
                var list = LoadPendingInternal();

                // 生成全局唯一 TaskId
                var taskId = Guid.NewGuid().ToString("N");

                list.Add(new PendingTask
                {
                    TaskId = taskId,
                    Task = task,
                    RetryCount = 0,
                    LastError = error,
                    LastTryTime = DateTime.Now
                });

                WritePending(list);
            }
        }

        private List<PendingTask> LoadPendingInternal()
        {
            EnsureDirectory();

            if (!File.Exists(_pendingFile)) return new List<PendingTask>();
            try
            {
                string json = File.ReadAllText(_pendingFile);
                return JsonConvert.DeserializeObject<List<PendingTask>>(json) ?? new List<PendingTask>();
            }
            catch
            {
                // 文件损坏自动备份
                var bak = _pendingFile + ".bak";
                File.Copy(_pendingFile, bak, true);
                File.Delete(_pendingFile);

                return new List<PendingTask>();
            }
        }

        private void WritePending(List<PendingTask> list)
        {
            EnsureDirectory();
            
            File.WriteAllText(_pendingFile, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        #endregion

        #region 后台重试任务

        //private async Task RetryPendingTasks()
        //{
        //    while (!_cts.Token.IsCancellationRequested)
        //    {
        //        List<PendingTask> list;
        //        lock (_fileLock)
        //        {
        //            list = LoadPendingInternal();
        //        }

        //        if (list.Count == 0)
        //        {
        //            await Task.Delay(RetryIntervalMs, _cts.Token);
        //            continue;
        //        }

        //        bool changed = false;

        //        for (int i = list.Count - 1; i >= 0; i--)
        //        {
        //            var pending = list[i];

        //            if (pending.RetryCount >= MaxRetryCount)
        //            {
        //                Log($"任务丢弃（超过最大重试）：{pending.Task.TaskType}");
        //                list.RemoveAt(i);
        //                changed = true;
        //                continue;
        //            }

        //            bool ok = await SendInternalAsync(pending.Task, fromRetry: true);

        //            if (ok)
        //            {
        //                list.RemoveAt(i);
        //                changed = true;
        //            }
        //            else
        //            {
        //                pending.RetryCount++;
        //                pending.LastTryTime = DateTime.Now;
        //                changed = true;
        //            }
        //        }

        //        if (changed)
        //        {
        //            lock (_fileLock)
        //            {
        //                WritePending(list);
        //            }
        //        }

        //        await Task.Delay(RetryIntervalMs, _cts.Token);
        //    }
        //}

        private async Task RetryPendingTasks()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                List<PendingTask> list;
                lock (_fileLock)
                {
                    list = LoadPendingInternal();
                }

                if (list.Count == 0)
                {
                    await Task.Delay(BaseRetryMs, _cts.Token);
                    continue;
                }

                bool changed = false;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var pending = list[i];

                    // 计算等待时间
                    var delay = GetRetryDelay(pending);

                    // 未到重试时间跳过
                    if (DateTime.Now - pending.LastTryTime < delay)
                        continue;

                    bool ok = await SendInternalAsync(pending.Task, fromRetry: true);

                    if (ok)
                    {
                        Log($"【ClientTaskSender】重试成功: {pending.Task.TaskType}");
                        list.RemoveAt(i);
                        changed = true;
                    }
                    else
                    {
                        pending.RetryCount++;
                        pending.LastTryTime = DateTime.Now;

                        //if (pending.RetryCount >= MaxRetryCount)
                        //{
                        //    Log($"【ClientTaskSender】进入慢速重试模式: {pending.Task.TaskType}");
                        //}
                        // 如果处于慢速模式，周期性恢复快速模式
                        if (pending.RetryCount > MaxRetryCount + 10)
                        {
                            pending.RetryCount = MaxRetryCount - 1;
                        }
                        changed = true;
                    }
                }

                if (changed)
                {
                    lock (_fileLock)
                    {
                        WritePending(list);
                    }
                }

                await Task.Delay(BaseRetryMs, _cts.Token);
            }
        }


        #endregion

        #region 工具 & Dispose

        private void Log(string msg)
        {
            LogCallback?.Invoke($"{msg}");
            Console.WriteLine(msg);
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _retryWorker.Wait(); } catch { }
            _httpClient.Dispose();
            _cts.Dispose();
        }

        #endregion

        #region 内部模型

        private sealed class PendingTask
        {
            public string TaskId { get; set; }           // 全局唯一键
            public DbTask Task { get; set; }
            public int RetryCount { get; set; }
            public string LastError { get; set; }
            public DateTime LastTryTime { get; set; }
        }

        #endregion
    }
}
