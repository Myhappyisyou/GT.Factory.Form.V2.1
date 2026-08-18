using GT.Factory.LIN.Hardware;
using System;

namespace GT.Factory.LIN.Project
{
    /// <summary>
    /// LIN工程加载器。
    ///
    /// 负责:
    ///
    /// ProjectFile
    ///     ↓
    /// LinProject
    ///
    /// 这是ProjectJson与现有LIN Core之间的桥梁。
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
            // 调用现有LinProject
            //
            return LinProject.Load(
                ldfFile,
                simulationFile,
                adapter,
                mode);
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
