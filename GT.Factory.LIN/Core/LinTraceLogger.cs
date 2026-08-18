using GT.Factory.LIN.Model;
using GT.Factory.LIN.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GT.Factory.LIN.Core
{


    /// <summary>
    /// LIN Trace记录器
    ///
    /// 用于:
    ///
    /// 1. 调试显示
    ///
    /// 2. 日志保存
    ///
    /// 3. UI绑定
    ///
    /// </summary>
    public class LinTraceLogger
    {

        /// <summary>
        /// 最大保存数量
        ///
        /// 工业软件一般限制缓存
        ///
        /// </summary>
        private readonly int _maxCount;

        /// <summary>
        /// Trace缓存
        ///
        /// </summary>
        private readonly List<LinTraceItem> _items;


        public event Action<LinTraceItem> ItemAdded;

        /// <summary>
        /// 只读访问
        ///
        /// </summary>
        public IReadOnlyList<LinTraceItem> Items
        {
            get
            {
                return _items;
            }
        }

        public LinTraceLogger(
            int maxCount = 5000)
        {

            _maxCount = maxCount;


            _items =
                new List<LinTraceItem>();

        }

        /// <summary>
        /// 添加一条LIN记录
        ///
        /// </summary>
        public void Add(
            LinTraceItem item)
        {


            if (item == null)
                return;

            _items.Add(item);

            //
            // 超出限制删除最早数据
            //
            if (_items.Count > _maxCount)
            {

                _items.RemoveAt(0);

            }

            ItemAdded?.Invoke(item);

        }

        /// <summary>
        /// 清空日志
        ///
        /// </summary>
        public void Clear()
        {

            _items.Clear();

        }

        /// <summary>
        /// 创建Trace记录
        ///
        /// </summary>
        public void Log(
            string direction,
            LinFrame frame,
            LinMessage message)
        {


            Add(
                new LinTraceItem
                {

                    Time =
                        message.Timestamp,


                    Direction =
                        direction,


                    FrameName =
                        frame.Name,


                    Id =
                        message.Id,


                    Length =
                        message.Length,


                    Data =
                        message.Data.ToArray(),


                    ChecksumOK =
                        message.IsChecksumValid,

                    Signals =
                DecodeSignals(
                    frame,
                    message.Data)


                });


        }




        private List<LinSignalValueItem> DecodeSignals(
    LinFrame frame,
    byte[] data)
        {

            var list =
                new List<LinSignalValueItem>();


            foreach (var item in frame.Signals)
            {


                var value =
                    LinSignalCode.Decode(
                        frame,
                        item.Signal,
                        data);



                list.Add(
                    new LinSignalValueItem
                    {

                        Name =
                            item.Signal.Name,


                        Value =
                            value

                    });

            }


            return list;

        }
    }

}

