using GT.Factory.LIN.Model;
using GT.Factory.LIN.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.Tests.Parser
{
    public class LdfParserTests
    {
        [TestMethod]
        public void Test_ELOP_LDF_Model()
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


            LinNetwork network =
                parser.Parse(tokens);



            foreach (var frame in network.Frames)
            {

                Console.WriteLine(
                    $"Frame:{frame.Name} ID:{frame.Id} DLC:{frame.Length}");


                foreach (var sig in frame.Signals)
                {

                    Console.WriteLine(
                        $"   {sig.Signal.Name} bit:{sig.StartBit}");

                }

            }



        }
    }
}
