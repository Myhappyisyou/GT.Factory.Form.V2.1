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
    /// LIN测试流程加载器
    ///
    /// 负责:
    ///
    /// json
    ///     ↓
    /// LinTestCase
    ///
    /// </summary>
    public static class LinTestCaseLoader
    {

        public static LinTestCase Load(
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
                    "测试文件不存在",
                    file);
            }

            string json =
                File.ReadAllText(file);


            var settings =
                new JsonSerializerSettings();


            settings.Converters.Add(
                new Newtonsoft.Json.Converters.StringEnumConverter());


            var testCase =
                JsonConvert.DeserializeObject<LinTestCase>(
                    json,
                    settings);


            if (testCase == null)
            {
                throw new Exception(
                    "测试流程解析失败");
            }


            Validate(testCase);


            return testCase;

        }

        private static void Validate(
            LinTestCase testCase)
        {


            if (string.IsNullOrWhiteSpace(
                testCase.Name))
            {
                throw new Exception(
                    "测试名称不能为空");
            }


            if (testCase.Steps == null ||
               testCase.Steps.Count == 0)
            {
                throw new Exception(
                    "测试步骤为空");
            }

        }

    }
}
