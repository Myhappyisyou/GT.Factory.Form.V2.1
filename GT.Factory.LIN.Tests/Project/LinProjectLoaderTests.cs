using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Tests.Project
{
    /// <summary>
    /// LIN工程加载器测试。
    ///
    /// 测试目标：
    ///
    /// .linproj
    ///     ↓
    /// LinProjectLoader
    ///     ↓
    /// LinProject
    ///
    /// 验证LinProjectLoader能够正确组织：
    ///
    /// 1. LDF
    /// 2. Runtime
    /// 3. SimulationConfig
    /// 4. TestProject
    ///
    /// 本测试不负责：
    ///
    /// DebugSession
    /// DebugForm
    /// 测试执行
    /// </summary>
    [TestClass]
    public class LinProjectLoaderTests
    {
        /// <summary>
        /// 测试LIN工程能够被完整加载。
        /// </summary>
        [TestMethod]
        public void Test_Load_LinProject()
        {
            //
            // LIN工程文件。
            //
            string path =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\BYD_300W.linproj";

            //
            // 确认LIN工程文件存在。
            //
            Assert.IsTrue(
                File.Exists(path),
                "LIN工程文件不存在");

            //
            // 创建Simulation使用的虚拟Adapter。
            //
            // 注意：
            // 这里使用你当前项目中已经存在的
            // Simulation Adapter，不要新建新的Adapter。
            //
            var adapter =
                new MockLinAdapter();

            //
            // 通过LinProjectLoader加载完整LIN工程。
            //
            var project =
                LinProjectLoader.Load(
                    path,
                    adapter);

            //
            // ==========================
            // LinProject验证
            // ==========================
            Assert.IsNotNull(
                project,
                "LinProject加载失败");

            //
            // ==========================
            // Network验证
            // ==========================
            Assert.IsNotNull(
                project.Network,
                "Network为空");

            //
            // 至少应该存在一个Frame。
            //
            Assert.IsTrue(
                project.Network.Frames.Count > 0,
                "Network中没有Frame");

            //
            // ==========================
            // Runtime验证
            // ==========================
            Assert.IsNotNull(
                project.Runtime,
                "Runtime为空");

            //
            // ==========================
            // SimulationConfig验证
            // ==========================
            Assert.IsNotNull(
                project.SimulationConfig,
                "SimulationConfig为空");

            //
            // ==========================
            // Mode验证
            // ==========================
            Assert.AreEqual(
                LinRunMode.Simulation,
                project.Mode);

            //
            // ==========================
            // Controller验证
            // ==========================
            Assert.IsNotNull(
                project.Controller,
                "Controller为空");

            //
            // ==========================
            // TestProject验证
            // ==========================
            Assert.IsNotNull(
                project.TestProject,
                "TestProject没有加载");

            //
            // 验证测试工程名称。
            //
            Assert.AreEqual(
                "BYD_300W_Motor_Test",
                project.TestProject.Name);

            //
            // ==========================
            // Test Cases验证
            // ==========================
            Assert.IsNotNull(
                project.TestProject.Cases,
                "TestProject.Cases为空");

            //
            // 至少存在一个测试案例。
            //
            Assert.IsTrue(
                project.TestProject.Cases.Count > 0,
                "TestProject没有测试案例");
        }
    }
}