using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.ProcessConfig
{
    public class StageStep
    {
        public string Name { get; }
        public Func<ProcessContext, Task> Action { get; }

        public StageStep(string name, Func<ProcessContext, Task> action)
        {
            Name = name;
            Action = action;
        }
    }

}
