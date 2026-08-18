using GT.Factory.Station.ProcessConfig;
using GT.Factory.Station.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.ProcessConfig
{
    //数据管道
    public interface ITestDataProcessor
    {
        Task HandleData(ProcessContext context);

        Task CalibrationHandleData(ProcessContext context);
    }

}
