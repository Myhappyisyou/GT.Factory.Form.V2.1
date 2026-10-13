using GT.Factory.LIN.Test.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Test.Loader
{
    /// <summary>
    /// LIN测试工程加载器。
    ///
    /// 负责:
    ///
    /// JSON
    ///     ↓
    /// LinTestProject
    ///
    /// 不负责:
    ///
    /// LIN通信
    /// Runtime
    /// Controller
    /// Test执行
    /// UI
    ///
    /// </summary>
    public static class LinTestProjectLoader
    {
        /// <summary>
        /// 加载LIN测试工程。
        /// </summary>
        public static LinTestProject Load(
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
                    "LIN测试工程文件不存在。",
                    file);
            }

            var fullPath =
                Path.GetFullPath(file);

            var json =
                File.ReadAllText(fullPath);

            var settings =
                new JsonSerializerSettings();

            settings.Converters.Add(
                new Newtonsoft.Json.Converters.StringEnumConverter());

            var project =
                JsonConvert.DeserializeObject<LinTestProject>(
                    json,
                    settings);

            if (project == null)
            {
                throw new InvalidOperationException(
                    "LIN测试工程解析失败。");
            }

            Validate(project);

            return project;
        }

        /// <summary>
        /// 验证测试工程配置。
        /// </summary>
        private static void Validate(
            LinTestProject project)
        {
            if (string.IsNullOrWhiteSpace(
                project.Name))
            {
                throw new Exception(
                    "测试工程名称不能为空。");
            }

            if (project.Cases == null ||
                project.Cases.Count == 0)
            {
                throw new Exception(
                    "测试工程中没有TestCase。");
            }

            foreach (var testCase in project.Cases)
            {
                if (testCase == null)
                {
                    throw new Exception(
                        "测试工程包含空TestCase。");
                }

                if (string.IsNullOrWhiteSpace(
                    testCase.Name))
                {
                    throw new Exception(
                        "TestCase名称不能为空。");
                }

                if (testCase.Steps == null ||
                    testCase.Steps.Count == 0)
                {
                    throw new Exception(
                        $"TestCase[{testCase.Name}]没有测试步骤。");
                }
            }
        }
    }
}
