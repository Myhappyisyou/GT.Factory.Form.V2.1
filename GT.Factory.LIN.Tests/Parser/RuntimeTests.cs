using GT.Factory.LIN.Core;
using GT.Factory.LIN.Parser;

namespace GT.Factory.LIN.Tests.Parser
{

    [TestClass]
    public class RuntimeTests
    {
        /// <summary>
        /// 测试ELOP LDF文件Token解析
        ///
        /// 目的：
        ///
        /// 1. 文件可以读取
        /// 2. Token数量正常
        /// 3. Token内容正确
        ///
        /// </summary>
        [TestMethod]
        public void Test_ELOP_LDF_Tokenize()
        {

            string path =
           @"C:\Users\ding0\Desktop\BYD_300W_HEV_ELOP_V1.2(1).ldf";



            Assert.IsTrue(
                File.Exists(path),
                "LDF文件不存在");

            var reader =
 new LdfReader();


            var tokens =
             reader.Read(path);


            var parser =
             new LdfParser();


            var network =
             parser.Parse(tokens);



            var runtime =
             new LinRuntime(network);

            runtime.SetSignalValue(
 "WRITE_SPEED",
 3000);


            var builder =
             new LinFrameBuilder(network);


            var msg =
             builder.Build(
             "ELOP_Cmd",
             runtime);


            Console.WriteLine(
             BitConverter.ToString(msg.Data));


            Assert.AreEqual(
             4,
             network.Frames.Count);


        }


    }
}
