using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace GT.Factory.LIN.Parser
{


    /// <summary>
    /// LDF文件读取器
    ///
    /// 负责：
    ///
    /// 1. 从文件读取LDF文本
    /// 2. 处理编码
    /// 3. 调用Tokenizer生成Token
    ///
    /// 不负责：
    ///
    /// 1. LIN语义解析
    /// 2. 创建LinNetwork
    ///
    /// 
    /// 流程：
    ///
    /// xxx.ldf
    ///
    ///     |
    ///     |
    ///     ↓
    ///
    /// LdfReader
    ///
    ///     |
    ///     |
    ///     ↓
    ///
    /// List<LdfToken>
    ///
    /// </summary>
    public class LdfReader
    {


        /// <summary>
        /// Tokenizer
        /// 
        /// 负责词法分析。
        /// </summary>
        private readonly LdfTokenizer _tokenizer;



        /// <summary>
        /// 构造函数
        /// </summary>
        public LdfReader()
        {

            _tokenizer =
                new LdfTokenizer();

        }





        /// <summary>
        /// 读取LDF文件
        ///
        /// 示例：
        ///
        /// var tokens =
        ///     reader.Read(
        ///       "ELOP.ldf");
        ///
        /// </summary>
        /// 
        /// <param name="filePath">
        /// LDF文件路径
        /// </param>
        /// 
        /// <returns>
        /// Token列表
        /// </returns>
        public List<LdfToken> Read(string filePath)
        {


            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(
                    nameof(filePath));
            }



            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "LDF文件不存在",
                    filePath);
            }




            string content =
                File.ReadAllText(
                    filePath,
                    DetectEncoding(filePath));



            return _tokenizer.Tokenize(content);

        }







        /// <summary>
        /// 读取LDF文本
        ///
        /// 用于：
        ///
        /// 1. 单元测试
        /// 2. 网络加载
        /// 3. 数据库存储
        ///
        /// </summary>
        /// 
        public List<LdfToken> ReadText(string text)
        {


            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(
                    nameof(text));
            }


            return _tokenizer.Tokenize(text);

        }







        /// <summary>
        /// 简单检测文件编码
        ///
        /// 常见LDF：
        ///
        /// UTF8
        /// ASCII
        ///
        /// 
        /// 如果带BOM：
        ///
        /// 自动识别。
        ///
        /// </summary>
        private Encoding DetectEncoding(string file)
        {

            using (var stream =
                new FileStream(
                    file,
                    FileMode.Open,
                    FileAccess.Read))
            {


                byte[] bom =
                    new byte[3];


                stream.Read(
                    bom,
                    0,
                    3);



                //UTF8 BOM
                if (bom[0] == 0xEF
                    &&
                   bom[1] == 0xBB
                    &&
                   bom[2] == 0xBF)
                {

                    return Encoding.UTF8;

                }


            }



            //默认UTF8
            return Encoding.UTF8;

        }



    }

}

