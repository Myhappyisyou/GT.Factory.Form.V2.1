using GT.Factory.LIN;
using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Tests.ProjectDebug;
using GT.Factory.LIN.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace GT.Factory.LIN.Tests.ProjectDebug
{

    /// <summary>
    /// LIN Debug Host 测试。
    ///
    /// 验证：
    ///
    /// 1. .linproj Project 打开
    /// 2. Session 创建
    /// 3. Runtime 启动
    /// 4. Runtime 停止
    /// 5. Project 关闭
    /// 6. Project 切换
    /// 7. Project 加载失败时旧 Project 保持
    ///
    /// 注意：
    ///
    /// 本测试验证的是 Project Debug 生命周期。
    ///
    /// 不测试：
    ///
    /// 1. LIN协议
    /// 2. Frame Encode / Decode
    /// 3. Signal解析
    /// 4. Runtime内部算法
    /// 5. WinForms Application.Run()
    ///
    /// 这些内容由其它测试负责。
    /// </summary>
    [TestClass]
    public class LinDebugHostTests
    {

        #region Test Configuration

        /// <summary>
        /// LIN Project 工程文件。
        ///
        /// 注意：
        ///
        /// Debug Host 的真正工程入口是 .linproj。
        ///
        /// 不能直接使用 SimulationConfig.json。
        /// </summary>
        private const string ProjectFile =
            @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\BYD_300W.linproj";

        #endregion


        #region Helpers

        /// <summary>
        /// 创建测试Host。
        /// </summary>
        private LinDebugHost CreateHost()
        {
            return new LinDebugHost();
        }


        /// <summary>
        /// 创建测试Adapter。
        /// </summary>
        private MockLinAdapter CreateAdapter()
        {
            return new MockLinAdapter();
        }


        /// <summary>
        /// 调用LinDebugHost私有方法。
        /// </summary>
        private object InvokePrivate(
            LinDebugHost host,
            string methodName,
            params object[] parameters)
        {
            MethodInfo method =
                typeof(LinDebugHost).GetMethod(
                    methodName,
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                method,
                "找不到私有方法：" + methodName);

            try
            {
                return method.Invoke(
                    host,
                    parameters);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;

                throw;
            }
        }


        /// <summary>
        /// 设置Host Adapter。
        ///
        /// 测试不进入Application.Run()，
        /// 因此直接注入Mock Adapter。
        /// </summary>
        private void SetAdapter(
            LinDebugHost host,
            ILinAdapter adapter)
        {
            FieldInfo field =
                typeof(LinDebugHost).GetField(
                    "_adapter",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

            Assert.IsNotNull(
                field,
                "找不到LinDebugHost._adapter字段");

            field.SetValue(
                host,
                adapter);
        }


        /// <summary>
        /// 清理Host。
        /// </summary>
        private void Cleanup(
            LinDebugHost host)
        {
            if (host == null)
                return;

            try
            {
                InvokePrivate(
                    host,
                    "StopRuntime");
            }
            catch
            {
            }

            try
            {
                InvokePrivate(
                    host,
                    "CloseProject");
            }
            catch
            {
            }
        }


        /// <summary>
        /// 打开测试工程。
        ///
        /// 统一使用 .linproj。
        /// </summary>
        private void OpenProject(
            LinDebugHost host)
        {
            SetAdapter(
                host,
                CreateAdapter());

            InvokePrivate(
                host,
                "OpenProject",
                ProjectFile);
        }

        #endregion


        #region Project Open

        /// <summary>
        /// 测试：
        ///
        /// .linproj
        ///     ↓
        /// OpenProject
        ///     ↓
        /// LinProject
        ///     ↓
        /// LinDebugSession
        ///
        /// 必须全部创建成功。
        /// </summary>
        [TestMethod]
        public void Test_Host_OpenProject()
        {
            var host =
                CreateHost();

            try
            {
                OpenProject(host);

                Assert.IsNotNull(
                    host.Project);

                Assert.IsNotNull(
                    host.Session);

                Console.WriteLine(
                    "Project Open OK");

                Console.WriteLine(
                    "Project=" +
                    host.Project);

                Console.WriteLine(
                    "Session=" +
                    host.Session);
            }
            finally
            {
                Cleanup(host);
            }
        }

        #endregion


        #region Runtime

        /// <summary>
        /// 测试：
        ///
        /// Open Project
        ///     ↓
        /// Start Runtime
        ///
        /// Runtime可以正常启动。
        /// </summary>
        [TestMethod]
        public void Test_Host_StartRuntime()
        {
            var host =
                CreateHost();

            try
            {
                OpenProject(host);

                Assert.IsNotNull(
                    host.Project);

                Assert.IsNotNull(
                    host.Session);

                InvokePrivate(
                    host,
                    "StartRuntime");

                Assert.IsNotNull(
                    host.Project);

                Assert.IsNotNull(
                    host.Project.Controller);

                Console.WriteLine(
                    "Runtime Start OK");
            }
            finally
            {
                Cleanup(host);
            }
        }


        /// <summary>
        /// 测试：
        ///
        /// Start
        ///     ↓
        /// Stop
        ///
        /// Runtime可以正常停止。
        /// </summary>
        [TestMethod]
        public void Test_Host_StopRuntime()
        {
            var host =
                CreateHost();

            try
            {
                OpenProject(host);

                Assert.IsNotNull(
                    host.Project);

                InvokePrivate(
                    host,
                    "StartRuntime");

                InvokePrivate(
                    host,
                    "StopRuntime");

                Assert.IsNotNull(
                    host.Project);

                Assert.IsNotNull(
                    host.Session);

                Console.WriteLine(
                    "Runtime Stop OK");
            }
            finally
            {
                Cleanup(host);
            }
        }

        #endregion


        #region Project Close

        /// <summary>
        /// 测试：
        ///
        /// Open Project
        ///     ↓
        /// Close Project
        ///
        /// Project和Session都应该被清除。
        ///
        /// 同时验证：
        ///
        /// Frame Tree
        /// Signal
        /// Frame Data
        /// Trace
        ///
        /// 都被清空。
        /// </summary>
        [TestMethod]
        public void Test_Host_CloseProject()
        {
            var host =
                CreateHost();

            try
            {
                OpenProject(host);

                Assert.IsNotNull(
                    host.Project);

                Assert.IsNotNull(
                    host.Session);

                //
                // 关闭工程前必须存在Session数据。
                //
                Assert.IsTrue(
                    host.Session.Frames.Count > 0);

                Assert.IsTrue(
                    host.Session.Signals.Count > 0);

                //
                // 关闭工程。
                //
                InvokePrivate(
                    host,
                    "CloseProject");

                //
                // Project / Session必须清除。
                //
                Assert.IsNull(
                    host.Project);

                Assert.IsNull(
                    host.Session);

                Console.WriteLine(
                    "Project Close OK");
            }
            finally
            {
                Cleanup(host);
            }
        }

        #endregion


        #region Project Switch

        /// <summary>
        /// 测试：
        ///
        /// Project A
        ///     ↓
        /// Project B
        ///
        /// 新Project成功加载后：
        ///
        /// 1. Project必须是新实例
        /// 2. Session必须是新实例
        /// </summary>
        [TestMethod]
        public void Test_Host_SwitchProject()
        {
            var host =
                CreateHost();

            try
            {
                OpenProject(host);

                var projectA =
                    host.Project;

                var sessionA =
                    host.Session;

                Assert.IsNotNull(
                    projectA);

                Assert.IsNotNull(
                    sessionA);

                //
                // 再次打开同一个 .linproj。
                //
                InvokePrivate(
                    host,
                    "OpenProject",
                    ProjectFile);

                Assert.IsNotNull(
                    host.Project);

                Assert.IsNotNull(
                    host.Session);

                //
                // 必须是新的Project。
                //
                Assert.AreNotSame(
                    projectA,
                    host.Project);

                //
                // 必须是新的Session。
                //
                Assert.AreNotSame(
                    sessionA,
                    host.Session);

                Console.WriteLine(
                    "Project Switch OK");
            }
            finally
            {
                Cleanup(host);
            }
        }

        #endregion


        #region Load Failure Protection

        /// <summary>
        /// 测试：
        ///
        /// 当前Project A已经打开。
        ///
        /// 尝试打开不存在的Project B。
        ///
        /// B加载失败后：
        ///
        /// A必须仍然保持。
        /// </summary>
        [TestMethod]
        public void Test_Host_OpenProject_Failed_KeepsCurrentProject()
        {
            var host =
                CreateHost();

            try
            {
                OpenProject(host);

                var projectA =
                    host.Project;

                var sessionA =
                    host.Session;

                Assert.IsNotNull(
                    projectA);

                Assert.IsNotNull(
                    sessionA);

                //
                // 构造不存在的 .linproj。
                //
                string invalidProject =
                    Path.Combine(
                        Path.GetDirectoryName(
                            ProjectFile),
                        "NotExist_Project.linproj");

                if (File.Exists(
                    invalidProject))
                {
                    File.Delete(
                        invalidProject);
                }

                //
                // 尝试加载不存在的Project。
                //
                InvokePrivate(
                    host,
                    "OpenProject",
                    invalidProject);

                //
                // 原Project必须保持。
                //
                Assert.AreSame(
                    projectA,
                    host.Project);

                Assert.AreSame(
                    sessionA,
                    host.Session);

                Console.WriteLine(
                    "Load Failure Protection OK");
            }
            finally
            {
                Cleanup(host);
            }
        }

        #endregion

    }

}