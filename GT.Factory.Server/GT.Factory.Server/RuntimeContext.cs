using GT.Factory.Server.Help;
using GT.Factory.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskContracts.Models;

namespace GT.Factory.Server
{
    public static class RuntimeContext
    {
        public static AccessMdbHelper UserDb { get; set; }

        public static AccessMdbHelper ProductionDb { get; set; }

        public static User CurrentUser { get; set; }

        public static PluginManager PluginManager { get; set; }

        public static TaskQueueService TaskService { get; set; }
    }
}
