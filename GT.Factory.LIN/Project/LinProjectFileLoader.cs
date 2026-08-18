using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace GT.Factory.LIN.Project
{

    /// <summary>
    /// LIN工程文件加载器。
    ///
    /// 负责:
    ///
    /// *.linproj
    ///     ↓
    /// LinProjectFile
    ///
    /// 不负责:
    ///
    /// Path定位
    /// LDF解析
    /// LIN通信
    /// UI
    /// 
    /// </summary>
    public static class LinProjectFileLoader
    {

        public static LinProjectFile Load(
            string file)
        {

            if (string.IsNullOrWhiteSpace(file))
            {
                throw new ArgumentNullException(
                    nameof(file));
            }

            if (!File.Exists(file))
            {
                throw new FileNotFoundException(
                    "LIN工程文件不存在。",
                    file);
            }

            var fullPath =
                Path.GetFullPath(file);

            var json =
                File.ReadAllText(
                    fullPath);

            var settings = new JsonSerializerSettings();

            settings.Converters.Add(
                new Newtonsoft.Json.Converters.StringEnumConverter());

            var project =
            JsonConvert.DeserializeObject<LinProjectFile>(
                json,
                settings);

            if (project == null)
            {
                throw new InvalidOperationException(
                    "LIN工程文件解析失败。");
            }

            //
            // 保存自身路径
            //
            project.FilePath =
                fullPath;

            Validate(
                project);

            return project;

        }

        public static void Save(
            string file,
            LinProjectFile project)
        {

            if (project == null)
                throw new ArgumentNullException(
                    nameof(project));

            var fullPath =
                Path.GetFullPath(file);

            var dir =
                Path.GetDirectoryName(
                    fullPath);

            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json =
                JsonConvert.SerializeObject(
                    project,
                    Formatting.Indented);

            File.WriteAllText(
                fullPath,
                json);

            project.FilePath =
                fullPath;

        }


        private static void Validate(
            LinProjectFile project)
        {

            if (project.Project == null)
            {
                throw new Exception(
                    "缺少Project配置");
            }

            if (project.Lin == null)
            {
                throw new Exception(
                    "缺少LIN配置");
            }

            if (string.IsNullOrWhiteSpace(
                project.Lin.Ldf))
            {
                throw new Exception(
                    "未配置LDF");
            }


            //
            // 相对路径转换
            //
            var root =
                Path.GetDirectoryName(
                    project.FilePath);

            var ldf =
                Path.Combine(
                    root,
                    project.Lin.Ldf);

            var simulationConfig =
                Path.Combine(
                    root,
                    project.Lin.Simulation);

            if (!File.Exists(ldf))
            {
                throw new FileNotFoundException(
                    "LDF不存在",
                    ldf);
            }

            if (!File.Exists(simulationConfig))
            {
                throw new FileNotFoundException(
                    "SimulationConfig不存在",
                    simulationConfig);
            }

        }

    }

}