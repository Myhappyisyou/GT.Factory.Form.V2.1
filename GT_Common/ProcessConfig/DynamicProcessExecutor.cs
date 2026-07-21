using GT_Common.Helper;
using GT_Common.Helper.ClientTask;
using GT_Common.Helper.PlcComm;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
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
