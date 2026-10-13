using GT.Factory.LIN.Project;

namespace GT.Factory.LIN.Tests.Project
{
    /// <summary>
    /// LIN工程文件加载器测试。
    ///
    /// 测试目标：
    ///
    /// .linproj
    ///     ↓
    /// LinProjectFileLoader
    ///     ↓
    /// LinProjectFile
    ///
    /// 本测试只验证：
    ///
    /// 1. LIN工程文件能够加载
    /// 2. Project配置能够读取
    /// 3. LIN配置能够读取
    /// 4. Runtime配置能够读取
    /// 5. Test配置能够读取
    /// 6. LDF文件能够通过工程相对路径找到
    /// 7. SimulationConfig能够通过工程相对路径找到
    /// 8. Test工程能够通过工程相对路径找到
    ///
    /// 本测试不负责：
    ///
    /// LinProject创建
    /// LinRuntime
    /// LinController
    /// 测试执行
    /// </summary>
    [TestClass]
    public class LinProjectFileLoaderTests
    {
        /// <summary>
        /// 测试LIN工程文件能够正确加载。
        /// </summary>
        [TestMethod]
        public void Test_Load_LinProjectFile()
        {
            string path =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\BYD_300W.linproj";

            //
            // 确认LIN工程文件存在。
            //
            Assert.IsTrue(
                File.Exists(path),
                "LIN工程文件不存在");

            //
            // 加载LIN工程文件。
            //
            var project =
                LinProjectFileLoader.Load(
                    path);

            //
            // 确认工程对象加载成功。
            //
            Assert.IsNotNull(
                project,
                "LinProjectFile加载失败");

            //
            // ==========================
            // Project配置验证
            // ==========================
            Assert.IsNotNull(
                project.Project,
                "Project配置为空");

            Assert.AreEqual(
                "BYD_300W",
                project.Project.Name);

            //
            // ==========================
            // LIN配置验证
            // ==========================
            Assert.IsNotNull(
                project.Lin,
                "LIN配置为空");

            Assert.AreEqual(
                "BYD_300W_HEV_ELOP_V1.2.ldf",
                project.Lin.Ldf);

            Assert.AreEqual(
                "SimulationConfig.json",
                project.Lin.Simulation);

            //
            // ==========================
            // Runtime配置验证
            // ==========================
            Assert.IsNotNull(
                project.Runtime,
                "Runtime配置为空");

            Assert.AreEqual(
                LinRunMode.Simulation,
                project.Runtime.Mode);

            Assert.IsFalse(
                project.Runtime.AutoStart);

            //
            // ==========================
            // Test配置验证
            // ==========================
            Assert.IsNotNull(
                project.Test,
                "Test配置为空");

            Assert.AreEqual(
                "Motor_Test.json",
                project.Test.ProjectTestCasePath);

            //
            // ==========================
            // 工程自身路径验证
            // ==========================
            Assert.AreEqual(
                Path.GetFullPath(path),
                project.FilePath);

            //
            // ==========================
            // LDF完整路径验证
            // ==========================
            var ldfPath =
                project.GetLdfPath();

            Assert.IsTrue(
                File.Exists(ldfPath),
                "LDF文件不存在");

            //
            // ==========================
            // Simulation完整路径验证
            // ==========================
            var simulationPath =
                project.GetSimulationPath();

            Assert.IsTrue(
                File.Exists(simulationPath),
                "SimulationConfig文件不存在");

            //
            // ==========================
            // Test工程完整路径验证
            // ==========================
            var testProjectPath =
                project.GetTestProjectPath();

            Assert.IsTrue(
                File.Exists(testProjectPath),
                "LIN测试工程文件不存在");

            //
            // 确认Test工程路径确实位于
            // 当前.linproj工程目录下。
            //
            var expectedTestPath =
                Path.Combine(
                    Path.GetDirectoryName(path),
                    "Motor_Test.json");

            Assert.AreEqual(
                Path.GetFullPath(expectedTestPath),
                testProjectPath);
        }
    }
}