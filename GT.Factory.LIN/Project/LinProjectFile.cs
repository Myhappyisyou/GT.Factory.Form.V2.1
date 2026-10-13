using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Project
{

    /// <summary>
    /// LIN工程文件。
    ///
    /// 对应:
    ///
    /// *.linproj
    ///
    /// 工程目录:
    ///
    /// xxx.linproj
    /// xxx.ldf
    /// SimulationConfig.json
    ///
    /// 本对象负责:
    ///
    /// 1. 描述工程配置
    /// 2. 解析工程内部相对路径
    ///
    /// 不负责:
    ///
    /// LIN解析
    /// LIN通信
    /// UI
    ///
    /// </summary>
    public class LinProjectFile
    {

        /// <summary>
        /// 版本
        /// </summary>
        public string FileVersion
        {
            get;
            set;
        }

        /// <summary>
        /// 工程基本信息
        /// </summary>
        public LinProjectInfo Project
        {
            get;
            set;
        }



        /// <summary>
        /// LIN配置
        /// </summary>
        public LinProjectLinConfig Lin
        {
            get;
            set;
        }



        /// <summary>
        /// 运行配置
        /// </summary>
        public LinProjectRuntimeConfig Runtime
        {
            get;
            set;
        }

        /// <summary>
        /// 测试配置。
        /// </summary>
        public LinProjectTestConfig Test
        {
            get;
            set;
        }

        /// <summary>
        /// 当前linproj文件完整路径。
        ///
        /// Loader加载后自动赋值。
        ///
        /// 不保存JSON。
        /// </summary>
        [JsonIgnore]
        public string FilePath
        {
            get;
            internal set;
        }

        /// <summary>
        /// 工程目录。
        ///
        /// 例如:
        ///
        /// Config\BYD_300W\LIN
        ///
        /// </summary>
        [JsonIgnore]
        public string ProjectDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FilePath))
                    return null;


                return Path.GetDirectoryName(
                    FilePath);
            }
        }


        /// <summary>
        /// 根据相对路径获取完整路径。
        ///
        /// 示例:
        ///
        /// xxx.ldf
        ///
        /// 返回:
        ///
        /// Config\Model\LIN\xxx.ldf
        ///
        /// </summary>
        public string GetFullPath(
            string relativePath)
        {


            if (string.IsNullOrWhiteSpace(
                relativePath))
            {
                return null;
            }



            if (string.IsNullOrWhiteSpace(
                ProjectDirectory))
            {
                throw new InvalidOperationException(
                    "LIN工程路径尚未初始化。");
            }



            return Path.GetFullPath(
                Path.Combine(
                    ProjectDirectory,
                    relativePath));

        }

        /// <summary>
        /// 获取LDF文件路径。
        ///
        /// </summary>
        public string GetLdfPath()
        {

            if (Lin == null)
                return null;


            return GetFullPath(
                Lin.Ldf);

        }

        /// <summary>
        /// 获取LIN运行配置路径。
        ///
        /// </summary>
        public string GetSimulationPath()
        {

            if (Lin == null)
                return null;


            return GetFullPath(
                Lin.Simulation);

        }

        /// <summary>
        /// 获取LIN测试工程文件完整路径。
        ///
        /// 测试工程路径来源于:
        ///
        /// Test.ProjectTestCasePath
        ///
        /// 路径相对于当前 .linproj 文件所在目录。
        /// </summary>
        public string GetTestProjectPath()
        {
            if (Test == null)
                return null;

            return GetFullPath(
                Test.ProjectTestCasePath);
        }

        /// <summary>
        /// 获取工程名称。
        ///
        /// </summary>
        public string Name
        {
            get
            {
                return Project?.Name;
            }
        }


    }

}