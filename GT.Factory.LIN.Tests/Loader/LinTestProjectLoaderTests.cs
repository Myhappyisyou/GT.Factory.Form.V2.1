using GT.Factory.LIN.Test.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Tests.Test.Loader
{
    /// <summary>
    /// LIN测试工程加载器测试。
    ///
    /// 测试目标：
    ///
    /// Motor_Test.json
    ///     ↓
    /// LinTestProjectLoader
    ///     ↓
    /// LinTestProject
    ///
    /// 本测试不负责：
    ///
    /// .linproj解析
    /// LinProject
    /// LinRuntime
    /// LinController
    /// </summary>
    [TestClass]
    public class LinTestProjectLoaderTests
    {
        /// <summary>
        /// 测试LIN测试工程能够正确加载。
        /// </summary>
        [TestMethod]
        public void Test_Load_LinTestProject()
        {
            string path =
                @"D:\盖特龙项目\2025\新建文件夹\程序\C#demo V2.1 - 副本\GT.Factory.LIN\bin\Config\BYD_300W\LIN\Motor_Test.json";

            //
            // 确认测试工程文件存在。
            //
            Assert.IsTrue(
                File.Exists(path),
                "LIN测试工程文件不存在");

            //
            // 加载测试工程。
            //
            var test =
                LinTestProjectLoader.Load(
                    path);

            //
            // 确认工程对象加载成功。
            //
            Assert.IsNotNull(
                test,
                "LinTestProject加载失败");

            //
            // 验证工程名称。
            //
            Assert.AreEqual(
                "BYD_300W_Motor_Test",
                test.Name);

            //
            // 验证测试案例集合。
            //
            Assert.IsNotNull(
                test.Cases,
                "Cases为空");

            Assert.IsTrue(
                test.Cases.Count > 0,
                "测试工程没有测试案例");

            //
            // 验证第一个测试案例。
            //
            var testCase =
                test.Cases[0];

            Assert.IsNotNull(
                testCase,
                "测试案例为空");

            //
            // 验证测试案例名称。
            //
            Assert.IsFalse(
                string.IsNullOrWhiteSpace(
                    testCase.Name),
                "测试案例名称为空");

            //
            // 验证测试步骤。
            //
            Assert.IsNotNull(
                testCase.Steps,
                "测试步骤为空");

            Assert.IsTrue(
                testCase.Steps.Count > 0,
                "测试案例没有测试步骤");
        }
    }
}
