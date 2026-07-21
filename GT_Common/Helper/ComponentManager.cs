using GT_Common;
using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common
{
    [Serializable]
    public class ComponentItem
    {
        /// <summary>部件码</summary>
        public string ComponentCode;
        /// <summary>总成码</summary>
        public string AssemblyCode;
        /// <summary>时间戳，用于过期判断</summary>
        public DateTime Timestamp;
    }

    [Serializable]
    public class ComponentBuffer
    {
        public List<ComponentItem> Items;

        public ComponentBuffer()
        {
            Items = new List<ComponentItem>();
        }

        public void Add(ComponentItem item)
        {
            // 如果已有相同部件码，则更新
            var existing = Items.FirstOrDefault(x => x.ComponentCode == item.ComponentCode);
            if (existing != null)
            {
                existing.AssemblyCode = item.AssemblyCode;
                existing.Timestamp = DateTime.Now;
            }
            else
            {
                item.Timestamp = DateTime.Now;
                Items.Add(item);
            }
        }

        public ComponentItem Get(string componentCode)
        {
            return Items.FirstOrDefault(x => x.ComponentCode == componentCode);
        }

        public ComponentItem GetComponentCode(string assemblyCode)
        {
            return Items.FirstOrDefault(x => x.AssemblyCode == assemblyCode);
        }

        public List<ComponentItem> Query(string code, bool isAssemblyCode = false)
        {
            if (isAssemblyCode)
                return Items.Where(x => x.AssemblyCode == code).ToList();
            else
                return Items.Where(x => x.ComponentCode == code).ToList();
        }

        public void Remove(string componentCode)
        {
            Items.RemoveAll(x => x.ComponentCode == componentCode);
        }

        public void RemoveExpired(TimeSpan expireTime)
        {
            var now = DateTime.Now;
            Items.RemoveAll(x => now - x.Timestamp > expireTime);
        }
    }

    public class ComponentManager
    {
        private readonly string _cacheFile;
        private readonly object _lock = new object();
        public ComponentBuffer _buffer;
        private Timer _cleanupTimer;

        public event EventHandler<ComponentItem> AssemblyCodeReady;

        public ComponentManager(string stationName = "ComponentCache")
        {
            _cacheFile = PathCenter.HistoryFile(Path.Combine("ComponentCache", $"{stationName}.cache"));

            _buffer = new ComponentBuffer();

            // 每小时清理一次过期72小时缓存数据 启动程序后，等待1小时，然后每1小时自动删除超过72小时的过期数据
            _cleanupTimer = new Timer(_ =>
            {
                RemoveExpired(TimeSpan.FromHours(72));
            }, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));

            Load();
        }

        /// <summary>加载缓存文件</summary>
        public void Load()
        {
            lock (_lock)
            {
                if (File.Exists(_cacheFile))
                {
                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(_cacheFile, FileMode.Open, FileAccess.Read);
                        BinaryFormatter bf = new BinaryFormatter();
                        object obj = bf.Deserialize(fs);
                        if (obj != null && obj is ComponentBuffer)
                        {
                            _buffer = (ComponentBuffer)obj;
                        }
                        else
                        {
                            _buffer = new ComponentBuffer();
                        }
                    }
                    catch
                    {
                        _buffer = new ComponentBuffer();
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                        }
                    }
                }
                else
                {
                    _buffer = new ComponentBuffer();
                }
            }
        }

        /// <summary>保存缓存文件</summary>
        private void Save()
        {
            lock (_lock)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(_cacheFile, FileMode.Create, FileAccess.Write);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, _buffer);
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
            }
        }

        /// <summary>添加或更新部件码 → 总成码映射</summary>
        public void AddOrUpdate(string componentCode, string assemblyCode)
        {
            lock (_lock)
            {
                var item = new ComponentItem
                {
                    ComponentCode = componentCode,
                    AssemblyCode = assemblyCode
                };
                _buffer.Add(item);
                Save();

                // 触发事件
                AssemblyCodeReady?.Invoke(this, item);
            }
        }

        /// <summary>获取总成码，如果不存在返回 null</summary>
        public string GetAssemblyCode(string componentCode)
        {
            lock (_lock)
            {
                var item = _buffer.Get(componentCode);
                return item?.AssemblyCode;
            }
        }

        /// <summary>获取部件码，如果不存在返回 null</summary>
        public bool TryGetComponentCode(string assemblyCode, out string componentCode)
        {
            lock (_lock)
            {
                var item = _buffer.GetComponentCode(assemblyCode);
                componentCode = item?.ComponentCode;
                return item != null;
            }
        }

        public bool TryGetAssemblyCode(string componentCode, out string assemblyCode)
        {
            lock (_lock)
            {
                var item = _buffer.Get(componentCode);
                assemblyCode = item?.AssemblyCode;
                return item != null;
            }
        }

        /// <summary>获取总成码，如果不存在返回 null</summary>
        public List<ComponentItem> GetQueryCode(string componentCode)
        {
            lock (_lock)
            {
                var item = _buffer.Query(componentCode);
                return item;
            }
        }

        /// <summary>删除指定部件码</summary>
        public void Remove(string componentCode)
        {
            lock (_lock)
            {
                _buffer.Remove(componentCode);
                Save();
            }
        }

        public void Remove(string code, bool isAssemblyCode = false)
        {
            lock (_lock)
            {
                if (isAssemblyCode)
                    _buffer.Items.RemoveAll(x => x.AssemblyCode == code);
                else
                    _buffer.Remove(code); // 现有按部件码删除逻辑

                Save();
            }
        }

        public void Remove(Predicate<ComponentItem> predicate)
        {
            lock (_lock)
            {
                _buffer.Items.RemoveAll(predicate);
                Save();
            }
        }

        /// <summary>删除过期缓存</summary>
        /// <param name="expireTime">超过多久未更新视为过期</param>
        public void RemoveExpired(TimeSpan expireTime)
        {
            lock (_lock)
            {
                _buffer.RemoveExpired(expireTime);
                Save();
            }
        }

        /// <summary>获取所有缓存，方便调试或导出</summary>
        public List<ComponentItem> GetAll()
        {
            lock (_lock)
            {
                return _buffer.Items.ToList();
            }
        }
    }
}
