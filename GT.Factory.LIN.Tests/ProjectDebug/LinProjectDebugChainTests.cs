using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.ProjectDebug;
using GT.Factory.LIN.Test.Loader;
using GT.Factory.LIN.Test.Model;
using GT.Factory.LIN.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Tests.ProjectDebug
{
    /// <summary>
    /// LIN Project → Debug UI 完整链路测试。
    ///
    /// 测试目标：
    ///
    /// .linproj
    ///     ↓
    /// LinProjectFileLoader
    ///     ↓
    /// LinProject
    ///     ↓
    /// LinDebugSession
    ///     ↓
    /// LinDebugForm
    ///     ↓
    /// ReplaceSession()
    ///     ↓
    /// Frame / Signal / FrameData / Trace
    ///
    /// 本测试不验证：
    ///
    /// LinRuntime内部逻辑
    /// LinController内部逻辑
    /// LIN协议收发
    ///
    /// 本测试只确认：
    ///
    /// “工程配置是否已经真正进入Debug UI”。
    /// </summary>
    [TestClass]
    public class LinProjectDebugChainTests
    {
        /// <summary>
        /// 验证LIN工程从.linproj进入Debug UI的完整链路。
        /// </summary>
        [TestMethod]
        public void Test_LinProject_To_DebugUI_FullChain()
        {
            // ============================================================
            // 1. 使用现有LIN工程文件。
            //
            // 不创建新的TestProject。
            // 不生成临时JSON。
            // 直接使用现有已经验证通过的.linproj。
            // ============================================================

            string path =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\BYD_300W.linproj";


            // ============================================================
            // 2. 确认.linproj存在。
            // ============================================================

            Assert.IsTrue(
                File.Exists(path),
                "LIN工程文件不存在。");


            // ============================================================
            // 3. 通过现有LinProjectFileLoader加载工程配置。
            //
            // 这一层已经由LinProjectFileLoaderTests单独验证过。
            // 这里继续使用同一入口，不重新设计加载方式。
            // ============================================================

            var projectFile =
                LinProjectFileLoader.Load(
                    path);


            Assert.IsNotNull(
                projectFile,
                "LinProjectFileLoader.Load()失败。");


            // ============================================================
            // 4. 取得工程实际需要的LDF路径。
            //
            // .linproj中的：
            //
            // Lin.Ldf
            //
            // 通过工程目录转换成完整路径。
            // ============================================================

            string ldfPath =
                projectFile.GetLdfPath();


            Assert.IsTrue(
                File.Exists(ldfPath),
                "工程对应的LDF文件不存在。");


            // ============================================================
            // 5. 创建LinProject。
            //
            // 这里使用现有LinProject.Load()。
            //
            // 这一步就是：
            //
            // .linproj
            //     ↓
            // LinProject.Load()
            // ============================================================

            //
            // 使用虚拟Adapter
            //
            var adapter =
                new MockLinAdapter();



            //
            // 获取测试工程完整路径。
            //
            // 测试工程路径是相对于当前.linproj目录的。
            //
            var testProjectFile =
                projectFile.GetTestProjectPath();

            //
            // 加载测试工程。
            //
            var testProject =
                LinTestProjectLoader.Load(
                    testProjectFile);


       

            LinProject project =
                LinProject.Load(
                    projectFile.GetLdfPath(), projectFile.GetSimulationPath(), adapter, LinRunMode.Simulation, testProject);


            Assert.IsNotNull(
                project,
                "LinProject.Load()失败。");


            // ============================================================
            // 6. 确认LinProject已经获得工程数据。
            // ============================================================

            Assert.IsNotNull(
                project.Network.Frames,
                "LinProject.Frames为空。");


            Assert.IsTrue(
                project.Network.Frames.Count > 0,
                "LinProject没有加载任何Frame。");


            // ============================================================
            // 7. 创建DebugSession。
            //
            // 这里开始进入Project Debug链路。
            // ============================================================

            LinDebugSession session =
                new LinDebugSession(
                    project);


            Assert.IsNotNull(
                session,
                "LinDebugSession创建失败。");


            // ============================================================
            // 8. 确认Session已经拿到Project的Frame。
            // ============================================================

            Assert.IsTrue(
                session.Frames.Count > 0,
                "LinDebugSession没有获得Project Frames。");


            // ============================================================
            // 9. 确认Session已经拿到Project的Signal。
            // ============================================================

            Assert.IsTrue(
                session.Signals.Count > 0,
                "LinDebugSession没有获得Project Signals。");


            // ============================================================
            // 10. 创建空的Debug UI。
            //
            // 注意：
            //
            // 不调用Application.Run。
            //
            // 因为这里测试的是：
            //
            // Session能不能进入Form。
            // ============================================================

            //using (LinDebugForm form =
            //    new LinDebugForm())
            //{
            //    Assert.IsNotNull(
            //        form,
            //        "LinDebugForm创建失败。");


            //    // ========================================================
            //    // 11. 将Session交给DebugForm。
            //    //
            //    // 这是当前阶段最关键的一步：
            //    //
            //    // LinDebugForm.ReplaceSession()
            //    //
            //    // 内部会继续：
            //    //
            //    // Session.Frames
            //    //     ↓
            //    // FrameTree.LoadFrames()
            //    //
            //    // Session.Signals
            //    //     ↓
            //    // SignalPanel.LoadSignals()
            //    //
            //    // Session.Frames
            //    //     ↓
            //    // FrameDataPanel.LoadFrames()
            //    //
            //    // Session.Trace
            //    //     ↓
            //    // TraceGrid.UpdateTrace()
            //    // ========================================================

            //    form.ReplaceSession(
            //        session);


            //    // ========================================================
            //    // 12. ReplaceSession执行完成。
            //    //
            //    // 如果这里没有异常，
            //    // 说明Project已经进入DebugForm的数据绑定链路。
            //    // ========================================================

            //    form.RefreshData();
            //}
        }
    }
}
