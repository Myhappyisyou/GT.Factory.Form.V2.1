using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Test.Loader;
using System;

namespace GT.Factory.LIN.Project
{
    /// <summary>
    /// LIN工程加载器。
    ///
    /// 加载链：
    ///
    /// *.linproj
    ///     ↓
    /// LinProjectFile
    ///     ↓
    /// ┌───────────────┬────────────────┐
    /// ↓               ↓                ↓
    /// LDF             Runtime          Test
    /// ↓               ↓                ↓
    /// LinNetwork      LinController    LinTestProject
    ///
    /// 本类只负责工程资源的组织与组装。
    ///
    /// 不负责：
    ///
    /// LIN协议解析实现
    /// Runtime内部逻辑
    /// Test执行
    /// UI
    /// </summary>
    public static class LinProjectLoader
    {
        /// <summary>
        /// 根据LIN工程文件创建LinProject。
        /// </summary>
        public static LinProject Load(
            string projectFile,
            ILinAdapter adapter)
        {
            if (string.IsNullOrWhiteSpace(
                projectFile))
            {
                throw new ArgumentNullException(
                    nameof(projectFile));
            }


            if (adapter == null)
            {
                throw new ArgumentNullException(
                    nameof(adapter));
            }

            //
            // 读取ProjectJson
            //
            var projectConfig =
                LinProjectFileLoader.Load(
                    projectFile);

            //
            // 运行模式
            //
            var mode =
                ParseRunMode(
                    projectConfig);


            //
            // 获取LDF完整路径
            //
            var ldfFile =
                projectConfig.GetLdfPath();

            //
            // 获取LIN配置完整路径
            //
            //
            // 4.
            // Simulation配置
            //
            string simulationFile = null;


            if (mode == LinRunMode.Simulation)
            {

                simulationFile =
                    projectConfig.GetSimulationPath();


                if (string.IsNullOrWhiteSpace(
                    simulationFile))
                {
                    throw new InvalidOperationException(
                        "Simulation模式未配置Simulation文件");
                }

            }


            //
            // 获取测试工程完整路径。
            //
            // 测试工程路径是相对于当前.linproj目录的。
            //
            var testProjectFile =
                projectConfig.GetTestProjectPath();

            //
            // 加载测试工程。
            //
            var testProject =
                LinTestProjectLoader.Load(
                    testProjectFile);


            //
            // 创建完整LIN工程。
            //
            // 此时已经准备好：
            //
            // LDF
            // Simulation
            // Runtime Mode
            // TestProject
            //
            return LinProject.Load(
                ldfFile,
                simulationFile,
                adapter,
                mode,
                testProject);
        }


        /// <summary>
        /// 解析LIN运行模式。
        /// </summary>
        private static LinRunMode ParseRunMode(
            LinProjectFile project)
        {

            //
            // 默认真实硬件
            //
            if (project.Runtime == null)
            {
                return LinRunMode.Hardware;
            }

            return project.Runtime.Mode;

        }
    }
}
