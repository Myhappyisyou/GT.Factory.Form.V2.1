using GT.Factory.LIN.Model;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GT.Factory.LIN.Parser
{


    /// <summary>
    /// LDF解析器
    ///
    /// 负责：
    ///
    /// Token集合
    ///       |
    ///       ↓
    ///
    /// LinNetwork对象
    ///
    ///
    /// 不负责：
    ///
    /// 1. 文件读取
    /// 2. Token生成
    ///
    /// 
    /// 调用流程：
    ///
    /// LdfReader
    ///
    ///      ↓
    ///
    /// LdfParser
    ///
    ///      ↓
    ///
    /// LinNetwork
    ///
    /// </summary>
    public class LdfParser
    {

        /// <summary>
        /// Encoding定义集合
        ///
        /// Key:
        ///     Encoding名称
        ///
        /// Value:
        ///     Encoding对象
        ///
        /// </summary>
        private Dictionary<string, LinSignalEncoding> _encodings
            =
            new Dictionary<string, LinSignalEncoding>();



        /// <summary>
        /// Signal表示关系缓存
        ///
        /// 保存:
        ///
        /// EncodingName
        ///
        /// 对应哪些Signal
        ///
        /// 因为LDF中:
        ///
        /// Signal_representation
        ///
        /// 可能出现在:
        ///
        /// Signal定义之前
        ///
        /// 所以先缓存。
        ///
        /// </summary>
        private Dictionary<string, List<string>> _signalRepresentations
            =
            new Dictionary<string, List<string>>();


        /// <summary>
        /// Token列表
        /// </summary>
        private List<LdfToken> _tokens;

        /// <summary>
        /// 当前Token位置
        /// </summary>
        private int _index;

        /// <summary>
        /// 当前Token
        /// </summary>
        private LdfToken Current
        {
            get
            {

                if (_index >= _tokens.Count)
                    return null;


                return _tokens[_index];

            }
        }

        /// <summary>
        /// 解析入口
        ///
        /// </summary>
        public LinNetwork Parse(
            List<LdfToken> tokens)
        {

            if (tokens == null)
                throw new ArgumentNullException(
                    nameof(tokens));

            _tokens = tokens;

            _index = 0;

            _encodings.Clear();

            _signalRepresentations.Clear();


            LinNetwork network =
                new LinNetwork();

            while (Current.Type
                  != LdfTokenType.EndOfFile)
            {

                //读取章节名称

                string section =
                    Current.Text;

                switch (section)
                {

                    case "LIN_protocol_version":

                        ParseProtocolVersion(network);
                        break;

                    case "LIN_language_version":

                        ParseLanguageVersion(network);
                        break;

                    case "LIN_speed":

                        ParseSpeed(network);
                        break;

                    case "Nodes":

                        ParseNodes(network);
                        break;

                    case "Signals":

                        ParseSignals(network);

                        break;

                    case "Frames":

                        ParseFrames(network);

                        break;

                    case "Signal_encoding_types":

                        ParseSignalEncodingTypes();

                        break;

                    case "Signal_representation":

                        ParseSignalRepresentation();

                        break;

                    case "Schedule_tables":

                        ParseScheduleTables(network);

                        break;

                    default:

                        //
                        // 未处理章节
                        //
                        // 例如：
                        //
                        // Diagnostic_signals
                        //
                        // 后续逐步增加
                        //

                        SkipUnknown();

                        break;

                }

            }

            BuildRelations(network);

            return network;

        }


        private void BuildRelations(
            LinNetwork network)
        {
            //
            // Signal绑定Encoding
            //
            BindSignalEncoding(network);

            //
            // Schedule绑定Frame
            //
            BindScheduleFrames(network);

            BindNodeFrames(network);

        }


        private void BindNodeFrames(
            LinNetwork network)
        {

            foreach (var frame
                in network.Frames)
            {

                frame.Publisher
                     .PublishFrames
                     .Add(frame);


                foreach (var sig
                    in frame.Signals)
                {

                    var signal =
                        network.Signals
                        .First(
                        x => x.Name == sig.Signal.Name);


                    signal.Subscriber
                          .SubscribeFrames
                          .Add(frame);

                }

            }

        }


        #region Token操作

        /// <summary>
        /// 获取当前Token并移动
        /// </summary>
        private LdfToken Next()
        {

            var token = Current;

            _index++;

            return token;

        }

        /// <summary>
        /// 判断当前是否匹配文本
        /// </summary>
        private bool Match(string text)
        {

            if (Current == null)
                return false;

            if (Current.Text == text)
            {
                _index++;

                return true;
            }

            return false;

        }

        /// <summary>
        /// 必须匹配
        ///
        /// 不匹配直接报错
        /// </summary>
        private void Expect(string text)
        {

            if (!Match(text))
            {

                throw new System.Exception(
                    $"LDF解析错误:期望 {text}, 当前 {Current}");

            }

        }

        /// <summary>
        /// 读取标识符
        /// </summary>
        private string ReadIdentifier()
        {

            if (Current.Type
                != LdfTokenType.Identifier)
            {

                throw new System.Exception(
                    $"需要Identifier,当前:{Current}");

            }

            return Next().Text;

        }

        #endregion

        #region Header解析

        private void ParseProtocolVersion(
            LinNetwork network)
        {

            ReadIdentifier();

            Expect("=");

            network.ProtocolVersion =
                Next().Text;

            Expect(";");

        }

        private void ParseLanguageVersion(
            LinNetwork network)
        {

            ReadIdentifier();

            Expect("=");

            network.LanguageVersion =
                Next().Text;

            Expect(";");

        }

        private void ParseSpeed(
            LinNetwork network)
        {

            ReadIdentifier();

            Expect("=");


            string value =
                Next().Text;


            //
            // 19.2
            //
            if (Match("."))
            {
                value += ".";

                value += Next().Text;
            }


            //
            // kbps
            //
            string unit =
                ReadIdentifier();

            if (unit == "kbps")
            {

                network.BaudRate =
                    (int)
                    (double.Parse(value) * 1000);

            }


            Expect(";");

        }

        #endregion

        /// <summary>
        /// 暂时跳过未知章节
        ///
        /// 防止解析中断。
        ///
        /// 后续逐步增加。
        ///
        /// </summary>
        private void SkipUnknown()
        {

            while (Current != null
                &&
                Current.Text != ";")
            {

                if (Current.Text == "{")
                {

                    SkipBlock();

                    return;

                }

                _index++;

            }

            if (Current != null)
                _index++;

        }

        /// <summary>
        /// 跳过{}
        /// </summary>
        private void SkipBlock()
        {

            int level = 0;

            do
            {

                if (Current.Text == "{")
                    level++;

                if (Current.Text == "}")
                    level--;

                _index++;


            }
            while (level > 0
                &&
                Current != null);

        }

        /// <summary>
        /// 解析Nodes章节
        ///
        /// LDF:
        ///
        /// Nodes {
        ///
        /// Master:
        ///     LIN_Master,1 ms,0 ms;
        ///
        /// Slaves:
        ///     Slave_1;
        ///
        /// }
        ///
        /// 
        /// 生成:
        ///
        /// LinNode
        ///
        /// </summary>
        private void ParseNodes(
            LinNetwork network)
        {

            //
            // 消耗:
            //
            // Nodes
            //
            ReadIdentifier();

            Expect("{");

            while (Current != null
                &&
                Current.Text != "}")
            {

                string type =
                    ReadIdentifier();

                Expect(":");

                switch (type)
                {

                    case "Master":

                        ParseMasterNode(network);

                        break;

                    case "Slaves":

                        ParseSlaveNodes(network);

                        break;

                    default:

                        throw new System.Exception(
                            $"未知节点类型:{type}");

                }

            }

            Expect("}");

        }

        /// <summary>
        /// 解析Master节点
        ///
        /// </summary>
        private void ParseMasterNode(
            LinNetwork network)
        {

            string name =
                ReadIdentifier();

            LinNode node =
                new LinNode
                {
                    Name = name,
                    Type = LinNodeType.Master
                };

            network.Nodes.Add(node);

            //
            // 跳过:
            //
            // ,1 ms,0 ms
            //
            //

            while (Current.Text != ";")
            {

                _index++;

            }

            Expect(";");

        }


        /// <summary>
        /// 解析Slave节点
        ///
        /// LDF允许:
        ///
        /// Slaves:
        ///
        /// Slave1,Slave2;
        ///
        /// 所以这里循环读取。
        ///
        /// </summary>
        private void ParseSlaveNodes(
            LinNetwork network)
        {

            while (Current.Text != ";")
            {

                string name =
                    ReadIdentifier();

                LinNode node =
                    new LinNode
                    {
                        Name = name,
                        Type = LinNodeType.Slave
                    };

                network.Nodes.Add(node);

                //
                // 多个Slave:
                //
                // Slave1,
                // Slave2
                //
                //

                if (Match(","))
                {
                    continue;
                }

            }

            Expect(";");

        }


        /// <summary>
        /// 根据名称查找节点
        ///
        /// Signal和Frame解析时使用
        ///
        /// </summary>
        private LinNode FindNode(
            LinNetwork network,
            string name)
        {


            foreach (var node in network.Nodes)
            {

                if (node.Name == name)
                    return node;

            }


            throw new System.Exception(
                $"找不到LIN节点:{name}");

        }

        /// <summary>
        /// 解析Signals章节
        ///
        /// LDF:
        ///
        /// Signals {
        ///
        /// SPEED:
        /// 16,
        /// 65535,
        /// Slave_1,
        /// LIN_Master;
        ///
        /// }
        ///
        /// </summary>
        private void ParseSignals(
            LinNetwork network)
        {

            ReadIdentifier();


            Expect("{");


            while (Current.Text != "}")
            {

                ParseSingleSignal(network);

            }


            Expect("}");

        }

        /// <summary>
        /// 解析一个Signal
        ///
        /// </summary>
        private void ParseSingleSignal(
            LinNetwork network)
        {

            //
            // Signal名称
            //
            string name =
                ReadIdentifier();


            Expect(":");


            //
            // Bit长度
            //
            ushort length =
                ushort.Parse(
                    Next().Text);


            Expect(",");


            //
            // 初始值
            //
            //int init =
            //    int.Parse(
            //        Next().Text);
            int init =
           ParseNumber(Next().Text);


            Expect(",");


            //
            // Publisher
            //
            string publisherName =
                ReadIdentifier();


            Expect(",");


            //
            // Subscriber
            //
            string subscriberName =
                ReadIdentifier();


            Expect(";");


            LinSignal signal =
                new LinSignal
                {

                    Name = name,

                    Length = length,

                    InitialValue = init,


                    Publisher =
                        FindNode(
                            network,
                            publisherName),


                    Subscriber =
                        FindNode(
                            network,
                            subscriberName)

                };


            network.Signals.Add(signal);

        }


        private int ParseNumber(string text)
        {

            if (text.StartsWith("0x"))
            {
                return Convert.ToInt32(
                    text.Substring(2),
                    16);
            }


            return int.Parse(text);

        }

        /// <summary>
        /// 解析Frames章节
        ///
        /// </summary>
        private void ParseFrames(
            LinNetwork network)
        {

            ReadIdentifier();


            Expect("{");


            while (Current.Text != "}")
            {

                ParseSingleFrame(network);

            }


            Expect("}");

        }

        /// <summary>
        /// 解析一个Frame
        ///
        /// 示例:
        ///
        /// ELOP_Rsp1:
        /// 26,
        /// Slave_1,
        /// 8
        /// {
        ///
        /// }
        ///
        /// </summary>
        private void ParseSingleFrame(
            LinNetwork network)
        {

            //
            // Frame名称
            //
            string name =
                ReadIdentifier();

            Expect(":");

            //
            // Frame ID
            //
            byte id =
                byte.Parse(
                    Next().Text);

            Expect(",");

            //
            // Publisher
            //
            string publisherName =
                ReadIdentifier();

            Expect(",");

            //
            // DLC
            //
            byte length =
                byte.Parse(
                    Next().Text);

            Expect("{");

            LinFrame frame =
                    new LinFrame
                    {
                    
                        Name = name,
                    
                        Id = id,
                    
                        Length = length,
                    
                    
                        Publisher =
                            FindNode(
                                network,
                                publisherName),
                    
                        Direction =
                            publisherName ==
                            network.Master.Name
                            ?
                            LinFrameDirection.MasterToSlave
                            :
                            LinFrameDirection.SlaveToMaster
                    
                    };

            //
            // 解析Frame内部Signal
            //
            while (Current.Text != "}")
            {

                ParseFrameSignal(
                    network,
                    frame);

            }

            Expect("}");

            network.Frames.Add(frame);

        }

        /// <summary>
        /// 解析Frame中的Signal
        ///
        /// </summary>
        private void ParseFrameSignal(
            LinNetwork network,
            LinFrame frame)
        {

            string signalName =
                ReadIdentifier();

            Expect(",");

            ushort startBit =
                ushort.Parse(
                    Next().Text);

            Expect(";");

            //
            // 检查Signal是否存在
            //
            bool exists = false;

            var signal =
                    network.Signals
                    .FirstOrDefault(
                        x => x.Name == signalName);

            if (signal == null)
            {
                throw new Exception(
                    $"Frame引用不存在Signal:{signalName}");
            }


            foreach (var s in network.Signals)
            {
                
                if (s.Name == signalName)
                {
                    exists = true;
                    break;
                }

            }


            if (!exists)
            {

                throw new System.Exception(
                    $"Frame引用不存在Signal:{signalName}");

            }


            frame.Signals.Add(

                new LinFrameSignal
                {
                    Signal = signal,
                    //SignalName = signalName,

                    StartBit = startBit

                }

            );


        }

        /// <summary>
        /// 解析Signal编码定义
        ///
        /// </summary>
        private void ParseSignalEncodingTypes()
        {

            ReadIdentifier();

            Expect("{");


            while (Current.Text != "}")
            {

                ParseSingleEncoding();

            }

            Expect("}");

        }

        private void ParseSingleEncoding()
        {

            string name =
                ReadIdentifier();


            LinSignalEncoding encoding =
                new LinSignalEncoding
                {
                    Name = name
                };


            Expect("{");


            while (Current.Text != "}")
            {


                string type =
                    ReadIdentifier();


                Expect(",");

                switch (type)
                {

                    case "logical_value":

                        ParseLogicalValue(
                            encoding);

                        break;


                    case "physical_value":

                        ParsePhysicalValue(
                            encoding);

                        break;


                    default:

                        throw new System.Exception(
                            $"未知Encoding类型:{type}");

                }

            }


            Expect("}");


            _encodings[name] = encoding;

        }

        private void ParseLogicalValue(
            LinSignalEncoding encoding)
        {

            int value =
                int.Parse(
                    Next().Text);

            Expect(",");

            string text =
                Next().Text;

            Expect(";");

            encoding.LogicalValues[value] = text;

        }

        private void ParsePhysicalValue(
            LinSignalEncoding encoding)
        {

            int min =
                ParseNumber(
                    Next().Text);

            Expect(",");

            int max =
                ParseNumber(
                    Next().Text);

            Expect(",");

            double factor =
                double.Parse(
                    Next().Text);

            Expect(",");

            double offset =
                double.Parse(
                    Next().Text);

            string unit = "";

            //
            // 判断是否还有unit
            //
            if (Current.Text == ",")
            {

                Next();

                unit =
                    ReadValue();

            }


            Expect(";");

            encoding.PhysicalValues.Add(
                new LinPhysicalRange
                {
                    RawMin = min,
                    RawMax = max,
                    Factor = factor,
                    Offset = offset,
                    Unit = unit
                });

        }


        private string ReadValue()
        {

            if (Current.Type ==
                LdfTokenType.String)
            {
                return Next()
                    .Text
                    .Trim('"');
            }

            return Next().Text;

        }

        /// <summary>
        /// 绑定Signal和Encoding
        ///
        /// 根据:
        ///
        /// Signal_representation
        ///
        /// 完成关联。
        ///
        /// </summary>
        private void BindSignalEncoding(
            LinNetwork network)
        {

            foreach (var item in _signalRepresentations)
            {

                string encodingName =
                    item.Key;

                //
                // 查找Encoding
                //
                if (!_encodings
                    .ContainsKey(encodingName))
                {
                    continue;
                }

                LinSignalEncoding encoding =
                    _encodings[encodingName];

                foreach (var signalName
                    in item.Value)
                {

                    foreach (var signal
                        in network.Signals)
                    {

                        if (signal.Name
                            == signalName)
                        {

                            signal.Encoding =
                                encoding;

                            break;

                        }

                    }

                }

            }

        }


        /// <summary>
        /// 解析Signal_representation
        ///
        /// 作用:
        ///
        /// 建立:
        ///
        /// Encoding
        ///      |
        ///      |
        ///      ↓
        ///
        /// Signal
        ///
        ///
        /// LDF:
        ///
        /// READ_STATUS_Encoding:
        ///     READ_STATUS;
        ///
        /// </summary>
        private void ParseSignalRepresentation()
        {

            //
            // 消耗:
            //
            // Signal_representation
            //
            ReadIdentifier();

            Expect("{");

            while (Current.Text != "}")
            {

                //
                // Encoding名称
                //
                string encodingName =
                    ReadIdentifier();

                Expect(":");

                //
                // Signal名称
                //
                string signalName =
                    ReadIdentifier();

                Expect(";");

                if (!_signalRepresentations
                    .ContainsKey(encodingName))
                {

                    _signalRepresentations
                        .Add(
                            encodingName,
                            new List<string>());

                }

                _signalRepresentations[encodingName]
                    .Add(signalName);

            }

            Expect("}");

        }

        /// <summary>
        /// 解析Schedule_tables
        ///
        /// LDF:
        ///
        /// Schedule_tables
        /// {
        ///
        /// TableName
        /// {
        ///
        /// Frame delay 10 ms;
        ///
        /// }
        ///
        /// }
        ///
        /// </summary>
        private void ParseScheduleTables(
            LinNetwork network)
        {

            //
            // 消耗:
            //
            // Schedule_tables
            //
            ReadIdentifier();

            Expect("{");

            while (Current.Text != "}")
            {

                ParseSingleSchedule(
                    network);

            }

            Expect("}");

        }

        /// <summary>
        /// 解析单个Schedule
        ///
        /// </summary>
        private void ParseSingleSchedule(
            LinNetwork network)
        {

            //
            // Schedule名称
            //
            string name =
                ReadIdentifier();

            LinSchedule schedule =
                new LinSchedule
                {
                    Name = name
                };


            Expect("{");


            int order = 0;

            while (Current.Text != "}")
            {

                LinScheduleItem item =
                    ParseScheduleItem(
                        network,
                        order);

                schedule.Items.Add(item);



                order++;

            }

            Expect("}");


            network.Schedules.Add(schedule);

        }

        /// <summary>
        /// 解析Schedule中的一项
        ///
        /// </summary>
        private LinScheduleItem ParseScheduleItem(
            LinNetwork network,
            int order)
        {

            //
            // Frame名称
            //
            string frameName =
                ReadIdentifier();

            //
            // delay关键字
            //
            string keyword =
                ReadIdentifier();


            if (keyword != "delay")
            {

                throw new System.Exception(
                    $"Schedule格式错误,期望delay,实际:{keyword}");

            }

            //
            // 时间
            //
            int delay =
                int.Parse(
                    Next().Text);

            //
            // ms
            //
            string unit =
                ReadIdentifier();

            if (unit != "ms")
            {

                throw new System.Exception(
                    $"Schedule时间单位错误:{unit}");

            }

            Expect(";");


            LinScheduleItem item =
                new LinScheduleItem
                {

                    FrameName = frameName,

                    DelayMs = delay,

                    Order = order

                };

            //
            // Frame可能还没有解析
            //
            // 所以这里不要强制绑定
            //
            // 后面统一Bind
            //


            return item;

        }


        /// <summary>
        /// 绑定Schedule中的Frame
        ///
        /// </summary>
        private void BindScheduleFrames(
            LinNetwork network)
        {


            foreach (var schedule
                in network.Schedules)
            {


                foreach (var item
                    in schedule.Items)
                {


                    foreach (var frame
                        in network.Frames)
                    {


                        if (frame.Name
                            == item.FrameName)
                        {

                            item.Frame =
                                frame;

                            break;

                        }

                    }


                }

            }


        }

    }

}

