using GT.Factory.LIN.Model;
using GT.Factory.LIN.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;


namespace GT.Factory.LIN.Tests.Parser
{


    /// <summary>
    /// LDF Tokenizer测试
    ///
    /// 验证：
    ///
    /// LDF文本
    ///     ↓
    /// Token列表
    ///
    /// </summary>
    [TestClass]
    public class LdfTokenizerTests
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




            //
            // 读取文本
            //
            string text =
                File.ReadAllText(
                    path,
                    Encoding.UTF8);




            //
            // Tokenizer
            //
            LdfTokenizer tokenizer =
                new LdfTokenizer();




            var tokens =
                tokenizer.Tokenize(text);




            //
            // 基本验证
            //
            Assert.IsNotNull(tokens);



            Assert.IsTrue(
                tokens.Count > 0,
                "没有生成Token");




            //
            // 打印全部Token
            //
            foreach (var token in tokens)
            {

                Console.WriteLine(
                    $"{token.Line}:{token.Column}  " +
                    $"{token.Type,-12} " +
                    token.Text);

            }



            //
            // 最后必须EOF
            //
            Assert.AreEqual(
                LdfTokenType.EndOfFile,
                tokens[tokens.Count - 1].Type);




        }



    }


}
