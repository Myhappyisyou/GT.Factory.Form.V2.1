using GT.Factory.Server.Help;
using GT.Factory.Server.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GT.Factory.Server.Services
{
    public class TaskQueueService
    {
        private readonly DbTaskQueue _queue;
        private readonly PluginManager _pluginManager;
        private readonly object _dbObj;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Thread _workerThread;
        private Action<string,string> _log;

        public TaskQueueService(DbTaskQueue queue, PluginManager pluginManager, object dbObj, Action<string, string> log)
        {
            _queue = queue;
            _pluginManager = pluginManager;
            _dbObj = dbObj;
            _log = log;
        }

        public void Start()
        {
            _workerThread = new Thread(() => WorkerLoop(_cts.Token)) { IsBackground = true };
            _workerThread.Start();
        }

        public void Stop() => _cts.Cancel();

        private void WorkerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var task = _queue.Take(token);

                    if (_pluginManager.TryGetPlugin(task.TaskType, out var plugin))
                    {
                        DatabaseSessionManager.EnsureDatabase();

                        var db = DbContext.CurrentDb;

                        plugin.Execute(task, db, _log);
                        _log?.Invoke("TaskQueueService", $"完成任务：{task.TaskType}");
                    }
                    else
                    {
                        _log?.Invoke("TaskQueueService", $"未找到插件处理 {task.TaskType}, 保存本地");
                        LocalCache.Save(task);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _log?.Invoke("TaskQueueService", $"Worker异常: {ex.Message}");
                    Thread.Sleep(2000);
                }
            }
        }
    }

}
