using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskContracts
{
    public interface IDbTaskPlugin
    {
        string TaskType { get; } // 插件处理任务类型

        // db 可以是 AccessMdbHelper（由服务端传入）
        void Execute(DbTask task, object db, Action<string, string> log);
    }
}
