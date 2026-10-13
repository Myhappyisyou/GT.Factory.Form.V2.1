using GT.Factory.Logging.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT.Factory.Station.ProcessConfig
{
    /// <summary>
    /// 仪器处理器接口
    /// 每种仪器实现此接口，返回需要写回PLC的数据
    /// </summary>
    public interface IInstrumentHandler
    {
        /// <summary>
        /// 与仪器通讯，返回测量数据列表
        /// </summary>
        /// <param name="sn">当前产品SN码</param>
        /// <returns>需要写回PLC的测量数据</returns>
        Task<List<IMeasurement>> ExecuteAsync(string sn);
    }


    // ==================== 仪器分派器接口 ====================
    /// <summary>
    /// 仪器分派器接口
    /// 管理所有注册的仪器处理器
    /// </summary>
    public interface IInstrumentDispatcher
    {
        /// <summary>
        /// 注册仪器处理器
        /// </summary>
        void Register(string name, IInstrumentHandler handler, IDisposable resource);

        /// <summary>
        /// 获取特定仪器的处理器
        /// </summary>
        IInstrumentHandler GetHandler(string name);

        /// <summary>
        /// 获取所有已注册的处理器
        /// </summary>
        Dictionary<string, IInstrumentHandler> GetAllHandlers();

        /// <summary>
        /// 释放所有仪器Handler资源。
        /// </summary>
        void Shutdown();
    }

    // ==================== 仪器分派器默认实现 ====================
    /// <summary>
    /// 仪器分派器默认实现
    /// </summary>
    public class InstrumentDispatcher : IInstrumentDispatcher
    {
        private readonly Dictionary<string, IInstrumentHandler> _handlers
            = new Dictionary<string, IInstrumentHandler>();

        /// <summary>
        /// 仪器资源列表。
        ///
        /// 保存需要释放的底层通讯对象。
        /// </summary>
        private readonly List<IDisposable> _resources
            =
            new List<IDisposable>();

        public void Register(string key, IInstrumentHandler handler)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("仪器标识不能为空", nameof(key));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers[key] = handler;
            DisplayLog.Info($"✅ 注册仪器处理器: {key}");
        }

        /// <summary>
        /// 注册仪器处理器，同时管理底层资源。
        /// </summary>
        public void Register(
            string key,
            IInstrumentHandler handler,
            IDisposable resource)
        {
            Register(
                key,
                handler);


            if (resource != null)
            {
                _resources.Add(resource);
            }
        }

        public IInstrumentHandler GetHandler(string key)
        {
            _handlers.TryGetValue(key ?? string.Empty, out var handler);
            return handler;
        }

        public Dictionary<string, IInstrumentHandler> GetAllHandlers()
        {
            return new Dictionary<string, IInstrumentHandler>(_handlers);
        }

        /// <summary>
        /// 释放所有注册的仪器。
        /// </summary>
        public void Shutdown()
        {

            foreach (IDisposable resource in _resources)
            {

                try
                {
                    resource.Dispose();
                }
                catch (Exception ex)
                {
                    DisplayLog.Error(
                        "释放仪器失败",
                        ex);
                }

            }


            _resources.Clear();

            _handlers.Clear();
        }
    }
}
