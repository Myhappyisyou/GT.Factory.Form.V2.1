using GT_Common.ProcessConfig;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    //数据管道
    public interface ITestDataProcessor
    {
        Task HandleData(ProcessContext context);

        Task CalibrationHandleData(ProcessContext context);
    }

}
