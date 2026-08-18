using System;
using System.Collections.Generic;
using System.Text;


namespace GT.Factory.LIN.Parser
{


    /// <summary>
    /// LDF词法分析器
    ///
    /// 负责：
    ///
    /// LDF文本
    ///     ↓
    /// Token集合
    ///
    /// 类似编译器中的 Lexer。
    ///
    /// 不负责理解LIN含义。
    ///
    /// 例如：
    ///
    /// Master
    ///
    /// 它只知道：
    ///
    /// 这是一个Identifier。
    ///
    /// 至于是不是节点，
    /// 由LdfParser判断。
    ///
    /// </summary>
    public class LdfTokenizer
    {


        /// <summary>
        /// 当前解析文本
        /// </summary>
        private string _text;



        /// <summary>
        /// 当前字符位置
        /// </summary>
        private int _position;



        /// <summary>
        /// 当前行号
        /// </summary>
        private int _line;



        /// <summary>
        /// 当前列号
        /// </summary>
        private int _column;




        /// <summary>
        /// 解析入口
        ///
        /// 输入完整LDF文本。
        ///
        /// 返回Token列表。
        ///
        /// </summary>
        public List<LdfToken> Tokenize(string text)
        {

            _text = text;

            _position = 0;

            _line = 1;

            _column = 1;


            var tokens =
                new List<LdfToken>();


            while (!IsEnd())
            {

                SkipWhiteSpace();


                if (IsEnd())
                    break;



                //处理注释
                if (SkipComment())
                {
                    continue;
                }



                char c = Current;



                //字符串
                if (c == '"')
                {
                    tokens.Add(ReadString());

                    continue;
                }



                //
                // 数字
                //
                // 支持:
                // 123
                // -50
                // 19.2
                // 0x27
                //
                if (char.IsDigit(c)
                    ||
                    (c == '-'
                     &&
                     char.IsDigit(Peek(1))))
                {
                    tokens.Add(ReadNumber());

                    continue;
                }



                //标识符
                if (IsIdentifierStart(c))
                {
                    tokens.Add(ReadIdentifier());

                    continue;
                }



                //符号
                if (IsSymbol(c))
                {

                    tokens.Add(
                        new LdfToken
                        {
                            Type = LdfTokenType.Symbol,
                            Text = c.ToString(),
                            Line = _line,
                            Column = _column
                        });


                    MoveNext();

                    continue;
                }



                throw new System.Exception(
                    $"无法识别字符:{c},位置:{_line}:{_column}");

            }



            tokens.Add(
                new LdfToken
                {
                    Type = LdfTokenType.EndOfFile,
                    Text = "",
                    Line = _line,
                    Column = _column
                });


            return tokens;

        }





        #region 字符读取


        /// <summary>
        /// 当前字符
        /// </summary>
        private char Current
        {
            get
            {
                return _text[_position];
            }
        }





        /// <summary>
        /// 是否结束
        /// </summary>
        private bool IsEnd()
        {
            return _position >= _text.Length;
        }




        /// <summary>
        /// 移动一个字符
        /// </summary>
        private void MoveNext()
        {

            if (Current == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }


            _position++;

        }


        #endregion




        #region Token读取



        /// <summary>
        /// 读取字符串
        ///
        /// 示例：
        ///
        /// "2.1"
        ///
        /// </summary>
        private LdfToken ReadString()
        {

            int line = _line;
            int column = _column;


            StringBuilder sb =
                new StringBuilder();



            //跳过开始引号
            MoveNext();



            while (!IsEnd() && Current != '"')
            {

                sb.Append(Current);

                MoveNext();

            }



            if (!IsEnd())
            {
                MoveNext();
            }



            return new LdfToken
            {
                Type = LdfTokenType.String,
                Text = sb.ToString(),
                Line = line,
                Column = column
            };

        }






        /// <summary>
        /// 读取数字
        ///
        /// 支持:
        ///
        /// 123
        ///
        /// -50
        ///
        /// 19.2
        ///
        /// 0x27
        ///
        /// </summary>
        private LdfToken ReadNumber()
        {

            int line = _line;

            int column = _column;


            StringBuilder sb =
                new StringBuilder();



            //
            // 负号
            //
            if (Current == '-')
            {
                sb.Append(Current);

                MoveNext();
            }



            //
            // 十六进制
            //
            if (Current == '0'
               &&
               Peek(1) == 'x')
            {

                sb.Append(Current);

                MoveNext();


                sb.Append(Current);

                MoveNext();



                while (!IsEnd()
                    &&
                    IsHex(Current))
                {

                    sb.Append(Current);

                    MoveNext();

                }


            }
            else
            {

                //
                // 普通数字
                //
                while (!IsEnd()
                    &&
                    char.IsDigit(Current))
                {

                    sb.Append(Current);

                    MoveNext();

                }



                //
                // 小数
                //
                if (!IsEnd()
                    &&
                    Current == '.')
                {

                    sb.Append(Current);

                    MoveNext();



                    while (!IsEnd()
                        &&
                        char.IsDigit(Current))
                    {

                        sb.Append(Current);

                        MoveNext();

                    }

                }

            }



            return new LdfToken
            {
                Type = LdfTokenType.Number,

                Text = sb.ToString(),

                Line = line,

                Column = column
            };

        }



        /// <summary>
        /// 判断16进制字符
        /// </summary>
        private bool IsHex(char c)
        {

            return char.IsDigit(c)
                ||
                (c >= 'A' && c <= 'F')
                ||
                (c >= 'a' && c <= 'f');

        }






        /// <summary>
        /// 读取标识符
        ///
        /// 示例:
        ///
        /// Slave_1
        /// ELOP_Rsp1
        ///
        /// </summary>
        private LdfToken ReadIdentifier()
        {

            int line = _line;
            int column = _column;


            StringBuilder sb =
                new StringBuilder();



            while (!IsEnd()
                &&
                IsIdentifierPart(Current))
            {

                sb.Append(Current);

                MoveNext();

            }



            return new LdfToken
            {
                Type = LdfTokenType.Identifier,
                Text = sb.ToString(),
                Line = line,
                Column = column
            };

        }



        #endregion




        #region 判断


        private bool IsIdentifierStart(char c)
        {

            return char.IsLetter(c)
                   ||
                   c == '_';

        }



        private bool IsIdentifierPart(char c)
        {

            return char.IsLetterOrDigit(c)
                   ||
                   c == '_';

        }




        private bool IsSymbol(char c)
        {

            switch (c)
            {
                case '{':
                case '}':
                case ':':
                case ';':
                case ',':
                case '=':
                    return true;
                case '.':
                    return true;

                default:
                    return false;
            }

        }



        #endregion




        #region 空白和注释



        private void SkipWhiteSpace()
        {

            while (!IsEnd()
                &&
                char.IsWhiteSpace(Current))
            {
                MoveNext();
            }

        }





        /// <summary>
        /// 跳过注释
        ///
        /// 支持:
        ///
        /// // comment
        ///
        /// /* comment */
        ///
        /// </summary>
        private bool SkipComment()
        {

            if (Current == '/'
                &&
                Peek(1) == '/')
            {

                while (!IsEnd()
                    &&
                    Current != '\n')
                {
                    MoveNext();
                }

                return true;
            }



            if (Current == '/'
                &&
                Peek(1) == '*')
            {

                MoveNext();
                MoveNext();



                while (!IsEnd())
                {

                    if (Current == '*'
                       &&
                       Peek(1) == '/')
                    {
                        MoveNext();
                        MoveNext();

                        break;
                    }


                    MoveNext();

                }


                return true;
            }



            return false;

        }




        private char Peek(int offset)
        {

            int index = _position + offset;


            if (index >= _text.Length)
                return '\0';


            return _text[index];

        }



        #endregion


    }

}

