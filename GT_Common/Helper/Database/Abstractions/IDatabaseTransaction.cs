using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Abstractions
{
    /// <summary>
    /// 数据库事务接口
    /// </summary>
    public interface IDatabaseTransaction : IAsyncDisposable
    {
        Task CommitAsync();

        Task RollbackAsync();
    }
}
