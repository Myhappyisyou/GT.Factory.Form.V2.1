using GT_Common.Helper;
using GT_Common.Helper.ClientTask;
using GT_Common.Helper.PlcComm;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    // DynamicExecutorFactory.cs
    public class DynamicExecutorFactory
    {
        private readonly PatchReader _reader;
        private readonly ComponentManager _componentManager;
        private readonly UploadManager _uploadManager;
        private readonly Dictionary<string, Func<ProcessContext, Task>> _globalHandlers;
        private readonly AccessMdbHelper _db;

        //private readonly AccessShortConnectionHelper _remoteDb;
        private readonly List<Consumables> _lsConsumables;
        protected readonly ClientTaskSender _clientTaskSender;
        protected readonly ProductionMonitor _prodMonitor;
        private readonly TestDataProcessor _testDataProcessor;
        public DynamicExecutorFactory(
            PatchReader reader,
            ComponentManager componentManager,
            UploadManager uploadManager,
            AccessMdbHelper db,
            List<Consumables> lsConsumables,
            ClientTaskSender clientTaskSender,
            ProductionMonitor productionMonitor,
            TestDataProcessor testDataProcessor)
        {
            _reader = reader;
            _componentManager = componentManager;
            _uploadManager = uploadManager;
            _db = db;
            _lsConsumables = lsConsumables;
            _clientTaskSender = clientTaskSender;
            _prodMonitor = productionMonitor;
            _testDataProcessor = testDataProcessor;
            _globalHandlers = new Dictionary<string, Func<ProcessContext, Task>>();
        }

        // 注册全局处理器（适用于所有流程）
        public void RegisterGlobalHandler(string stageName, Func<ProcessContext, Task> handler)
        {
            _globalHandlers[stageName] = handler;
        }

        // 创建动态执行器
        public BaseProcessExecutor CreateExecutor(
            StandardProcess process,
            Dictionary<string, Func<ProcessContext, Task>> specificHandlers = null)
        {
            // 合并全局和特定处理器
            var allHandlers = new Dictionary<string, Func<ProcessContext, Task>>(_globalHandlers);
            if (specificHandlers != null)
            {
                foreach (var handler in specificHandlers)
                {
                    allHandlers[handler.Key] = handler.Value;
                }
            }

            return new DynamicProcessExecutor(_reader, _componentManager, _uploadManager, _db, _lsConsumables, _clientTaskSender, _prodMonitor, _testDataProcessor, allHandlers);
        }

        // 创建动态执行器
        public BaseProcessExecutor CreateCalibrationProcessExecutor(
            CalibrationProcess CalibrationProcess,
            Dictionary<string, Func<ProcessContext, Task>> specificHandlers = null)
        {
            // 合并全局和特定处理器
            var allHandlers = new Dictionary<string, Func<ProcessContext, Task>>(_globalHandlers);
            if (specificHandlers != null)
            {
                foreach (var handler in specificHandlers)
                {
                    allHandlers[handler.Key] = handler.Value;
                }
            }

            return new DynamicProcessExecutor(_reader, _componentManager, _uploadManager, _db, _lsConsumables, _clientTaskSender, _prodMonitor, _testDataProcessor, allHandlers);
        }

        // 创建动态执行器
        public BaseProcessExecutor CreateBarBindProcessExecutor(
            BarBindProcess BarBindProcess,
            Dictionary<string, Func<ProcessContext, Task>> specificHandlers = null)
        {
            // 合并全局和特定处理器
            var allHandlers = new Dictionary<string, Func<ProcessContext, Task>>(_globalHandlers);
            if (specificHandlers != null)
            {
                foreach (var handler in specificHandlers)
                {
                    allHandlers[handler.Key] = handler.Value;
                }
            }

            return new DynamicProcessExecutor(_reader, _componentManager, _uploadManager, _db, _lsConsumables, _clientTaskSender, _prodMonitor, _testDataProcessor, allHandlers);
        }
    }
}
