using GT.Factory.LIN.ProjectDebug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.UI.Controls
{
    /// <summary>
    /// 所有依赖 LinDebugSession 的调试子面板统一实现该接口。
    ///
    /// 目的：
    /// LinDebugForm 不再逐个面板手写 Bind/Unbind/Clear，
    /// 而是维护一个 ILinDebugSessionView 列表，统一驱动。
    ///
    /// 新增一个面板（比如以后的 LinTestRunPanel）时，
    /// Form 代码不需要任何改动，只需要把面板加入列表。
    /// </summary>
    public interface ILinDebugSessionView
    {
        /// <summary>绑定新 Session，订阅事件、加载数据。</summary>
        void BindSession(LinDebugSession session);

        /// <summary>解除旧 Session 的事件订阅（切换/关闭工程时调用）。</summary>
        void UnbindSession(LinDebugSession session);

        /// <summary>清空显示内容（关闭工程 / 无 Session 时调用）。</summary>
        void ClearSession();
    }
}
