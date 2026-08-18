namespace GT.Factory.LIN.Parser
{


    /// <summary>
    /// LDF Token
    ///
    /// LDF文件经过Tokenizer后，
    /// 会被拆分成一个个Token。
    ///
    /// 例如：
    ///
    /// Master: LIN_Master;
    ///
    /// 会变成：
    ///
    /// Identifier Master
    /// Colon :
    /// Identifier LIN_Master
    /// Semicolon ;
    ///
    /// </summary>
    public class LdfToken
    {


        /// <summary>
        /// Token类型
        /// </summary>
        public LdfTokenType Type
        {
            get;
            set;
        }



        /// <summary>
        /// Token文本
        ///
        /// 示例：
        ///
        /// Master
        /// LIN_Master
        /// 26
        ///
        /// </summary>
        public string Text
        {
            get;
            set;
        }



        /// <summary>
        /// Token所在行
        ///
        /// 方便错误定位。
        /// </summary>
        public int Line
        {
            get;
            set;
        }



        /// <summary>
        /// Token所在列
        /// </summary>
        public int Column
        {
            get;
            set;
        }



        public override string ToString()
        {
            return
            $"{Line}:{Column} {Type} [{Text}]";
        }

    }




    /// <summary>
    /// LDF Token类型
    ///
    /// </summary>
    public enum LdfTokenType
    {


        /// <summary>
        /// 标识符
        ///
        /// 例如：
        ///
        /// Master
        /// Slave_1
        /// ELOP_Rsp1
        ///
        /// </summary>
        Identifier,



        /// <summary>
        /// 数字
        ///
        /// 例如：
        ///
        /// 26
        /// 65535
        ///
        /// </summary>
        Number,



        /// <summary>
        /// 字符串
        ///
        /// 例如：
        ///
        /// "2.1"
        ///
        /// </summary>
        String,



        /// <summary>
        /// 符号
        ///
        /// 例如：
        ///
        /// {
        /// }
        /// :
        /// ;
        /// ,
        ///
        /// </summary>
        Symbol,



        /// <summary>
        /// 文件结束
        /// </summary>
        EndOfFile

    }


}
