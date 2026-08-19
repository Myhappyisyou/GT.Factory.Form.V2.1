using GT.Factory.Station.Helper;
using GT.Factory.Station.Helper.ClientTask;
using Consumables = GT.Factory.Common.Models.Consumables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GT.Factory.PLC.PlcComm;

namespace GT.Factory.Station.ProcessConfig
{
    // DynamicProcessExecutor.cs

    public class DynamicProcessExecutor : BaseProcessExecutor
    {
        public DynamicProcessExecutor(
            PatchReader reader,
            ComponentManager componentManager,
            UploadManager uploadManager,
            AccessMdbHelper db,
            List<Consumables> lsConsumables,
            ClientTaskSender clientTask,
                        ProductionMonitor productionMonitor,

            TestDataProcessor testDataProcessor,
            Dictionary<string, Func<ProcessContext, Task>> handlers)
            : base(reader, componentManager, uploadManager, db, lsConsumables, clientTask, productionMonitor, testDataProcessor, null, handlers)
        {
        }
    }

}
