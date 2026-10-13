using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TaskContracts;
using Formatting = Newtonsoft.Json.Formatting;

namespace GT.Factory.Server.Queue
{

    public class DbTaskQueue
    {
        private readonly BlockingCollection<DbTask> _queue = new BlockingCollection<DbTask>();

        public void Enqueue(DbTask task) => _queue.Add(task);

        public DbTask Take(CancellationToken token) => _queue.Take(token);

        public int Count => _queue.Count;
    }

    // 全局队列实例
    public static class DbTaskQueueInstance
    {
        public static readonly DbTaskQueue Instance = new DbTaskQueue();
    }

}
