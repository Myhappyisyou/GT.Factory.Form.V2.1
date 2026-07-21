using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    public class SnContextManager
    {
        private readonly ConcurrentDictionary<string, SnContext> _contexts
            = new ConcurrentDictionary<string, SnContext>();

        public SnContext GetOrCreate(string sn)
        {
            return _contexts.GetOrAdd(sn,
                s => new SnContext
                {
                    Sn = s
                });
        }

        public bool TryGet(string sn, out SnContext context)
        {
            return _contexts.TryGetValue(sn, out context);
        }

        public void Remove(string sn)
        {
            _contexts.TryRemove(sn, out _);
        }

        public void Clear()
        {
            _contexts.Clear();
        }
    }
}
