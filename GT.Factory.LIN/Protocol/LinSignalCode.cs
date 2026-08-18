using System;
using GT.Factory.LIN.Model;


namespace GT.Factory.LIN.Protocol
{

    /// <summary>
    /// LIN Signal编解码器
    ///
    /// 负责:
    ///
    /// Signal值
    ///     |
    ///     |
    ///     ↓
    ///
    /// Frame Data
    ///
    ///
    /// 例如:
    ///
    /// WRITE_SPEED = 3000
    ///
    /// 转换:
    ///
    /// DATA[2]
    /// DATA[3]
    ///
    /// </summary>
    public static class LinSignalCode
    {

        /// <summary>
        /// 写入Signal到Frame数据
        ///
        /// </summary>
        public static void Encode(
            LinFrame frame,
            LinSignal signal,
            object value,
            byte[] data)
        {

            if (frame == null)
                throw new ArgumentNullException(
                    nameof(frame));


            if (signal == null)
                throw new ArgumentNullException(
                    nameof(signal));


            if (data == null)
                throw new ArgumentNullException(
                    nameof(data));

            //
            // 获取Signal在Frame中的定义
            //
            LinFrameSignal frameSignal =
                FindFrameSignal(
                    frame,
                    signal.Name);

            if (frameSignal == null)
            {
                throw new System.Exception(
                    $"Frame {frame.Name} 不包含Signal {signal.Name}");
            }

            //
            // 物理值转换 Raw
            //
            int raw =
                ConvertToRaw(
                    signal,
                    value);

            //
            // 写入bit
            //
            WriteBits(
                data,
                frameSignal.StartBit,
                signal.Length,
                raw);

        }

        /// <summary>
        /// 从Frame读取Signal
        ///
        /// </summary>
        public static object Decode(
            LinFrame frame,
            LinSignal signal,
            byte[] data)
        {

            LinFrameSignal frameSignal =
                FindFrameSignal(
                    frame,
                    signal.Name);

            if (frameSignal == null)
            {

                throw new System.Exception(
                    $"Frame {frame.Name} 不包含 {signal.Name}");

            }

            int raw =
                ReadBits(
                    data,
                    frameSignal.StartBit,
                    signal.Length);

            return
                ConvertRawToValue(
                    signal,
                    raw);

        }

        #region 位操作

        /// <summary>
        /// 写入指定bit
        ///
        /// LIN采用Little Endian
        ///
        /// bit0:
        ///
        /// byte0 bit0
        ///
        /// </summary>
        private static void WriteBits(
            byte[] data,
            int startBit,
            int length,
            int value)
        {

            for (int i = 0; i < length; i++)
            {

                int bitIndex =
                    startBit + i;

                int byteIndex =
                    bitIndex / 8;

                int offset =
                    bitIndex % 8;

                bool bit =
                    ((value >> i) & 1) == 1;

                if (bit)
                {

                    data[byteIndex]
                        |=
                        (byte)(1 << offset);

                }
                else
                {

                    data[byteIndex]
                        &=
                        (byte)~(1 << offset);

                }

            }

        }

        /// <summary>
        /// 读取bit
        ///
        /// </summary>
        private static int ReadBits(
            byte[] data,
            int startBit,
            int length)
        {

            int value = 0;

            for (int i = 0; i < length; i++)
            {

                int bitIndex =
                    startBit + i;

                int byteIndex =
                    bitIndex / 8;

                int offset =
                    bitIndex % 8;

                int bit =
                    (
                    data[byteIndex]
                    >>
                    offset
                    )
                    &
                    1;

                value |=
                    bit << i;

            }

            return value;

        }

        #endregion

        #region 物理值转换

        /// <summary>
        /// 物理值转换Raw
        ///
        /// Raw =
        ///
        /// (Physical-Offset)/Factor
        ///
        /// </summary>
        private static int ConvertToRaw(
            LinSignal signal,
            object value)
        {
            //
            // 先处理Logical
            //
            int logicalRaw;

            if (TryConvertLogicalToRaw(
                signal,
                value,
                out logicalRaw))
            {
                return logicalRaw;
            }

            //
            // Physical
            //
            if (signal.Encoding == null
                ||
               signal.Encoding.PhysicalValues.Count == 0)
            {
                return Convert.ToInt32(value);
            }

            var range =
                signal.Encoding.PhysicalValues[0];

            double physicalValue = Convert.ToDouble(value);

            return
                (int)
                (
                    (physicalValue - range.Offset)
                    /
                    range.Factor
                );
        }


        private static bool TryConvertLogicalToRaw(
    LinSignal signal,
    object value,
    out int raw)
        {
            raw = 0;


            if (signal.Encoding == null)
                return false;


            if (signal.Encoding.LogicalValues == null)
                return false;


            if (value == null)
                return false;


            //
            // 用户已经传Raw
            //
            if (int.TryParse(
                value.ToString(),
                out raw))
            {
                return true;
            }


            //
            // 用户传Logical文本
            //
            foreach (var item in
                signal.Encoding.LogicalValues)
            {

                if (string.Equals(
                    item.Value,
                    value.ToString(),
                    StringComparison.OrdinalIgnoreCase))
                {
                    raw = item.Key;
                    return true;
                }

            }


            return false;
        }

        /// <summary>
        /// Raw转物理值
        ///
        /// Physical =
        ///
        /// Raw*Factor+Offset
        ///
        /// </summary>
        private static object ConvertRawToValue(
            LinSignal signal,
            int raw)
        {

            if (signal.Encoding != null)
            {

                //
                // 纯Logical
                //
                if (signal.Encoding.PhysicalValues.Count == 0 &&
                   signal.Encoding.LogicalValues.ContainsKey(raw))
                {
                    return signal.Encoding.LogicalValues[raw];
                }


                //
                // Physical特殊值
                //
                if (signal.Encoding.LogicalValues.ContainsKey(raw))
                {
                    return signal.Encoding.LogicalValues[raw];
                }

            }


            //
            // Physical
            //
            if (signal.Encoding != null &&
               signal.Encoding.PhysicalValues.Count > 0)
            {

                var range =
                    signal.Encoding.PhysicalValues[0];


                return
                    raw * range.Factor
                    +
                    range.Offset;

            }


            return raw;

        }

        #endregion

        /// <summary>
        /// 查找Signal在Frame中的位置
        ///
        /// </summary>
        private static LinFrameSignal FindFrameSignal(
            LinFrame frame,
            string name)
        {


            foreach (var item
                in frame.Signals)
            {

                if (item.Signal.Name == name)
                    return item;

            }

            return null;

        }

    }

}

