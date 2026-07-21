using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    public interface IProcessStageHandler
    {
        Task BeforeLoadCalibrationOrPeriodInspection(ProcessContext context);
        Task AfterLoadCalibrationOrPeriodInspection(ProcessContext context);
        Task BeforeSnRead(ProcessContext context);
        Task AfterSnRead(ProcessContext context);
        Task BeforeResultRead(ProcessContext context);
        Task AfterResultRead(ProcessContext context);
        Task BeforeDataRead(ProcessContext context);
        Task AfterDataRead(ProcessContext context);
        Task BeforeFileProcess(ProcessContext context);
        Task AfterFileProcess(ProcessContext context);
        Task BeforeUpdateProcess(ProcessContext context);
        Task AfterUpdateProcess(ProcessContext context);
        Task BeforePlcWrite(ProcessContext context);
        Task OnError(ProcessContext context, Exception ex);
    }
}
