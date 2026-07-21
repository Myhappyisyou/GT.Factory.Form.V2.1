using HslCommunication.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.PlcComm
{
    public class Int16Block
    {
        public byte[] AllValueRead { get; set; }

        public int ValueStartIndex { get; set; }

        public IByteTransform Transformer { get; private set; }


        public int CountOfByte = 1;

        public Int16Block(IByteTransform transformer)
        {
            Transformer = transformer;
        }


        public bool B(int index, int bitIndex)
        {
            short temp = Transformer.TransInt16(AllValueRead, (index - ValueStartIndex) * CountOfByte);
            return (temp & (1 << bitIndex)) != 0;
        }

        public short S(int index)
        {
            return Transformer.TransInt16(AllValueRead, (index - ValueStartIndex) * CountOfByte);
        }

        public ushort US(int index)
        {
            return Transformer.TransUInt16(AllValueRead, (index - ValueStartIndex) * CountOfByte);
        }

        public int I(int index)
        {
            return Transformer.TransInt32(AllValueRead, (index - ValueStartIndex) * CountOfByte);
        }

        public uint UI(int index)
        {
            return Transformer.TransUInt32(AllValueRead, (index - ValueStartIndex) * CountOfByte);
        }

        public float F(int index)
        {
            return Transformer.TransSingle(AllValueRead, (index - ValueStartIndex) * CountOfByte);
        }

        public double D(int index)
        {
            return Transformer.TransDouble(AllValueRead, (index - ValueStartIndex) * CountOfByte);
        }

        public string Str(int index, int length)
        {
            return Transformer.TransString(AllValueRead, (index - ValueStartIndex) * CountOfByte, length, Encoding.ASCII);
        }

        public string Str2(int index, int length)
        {
            int readLength = length;
            if (length % 2 != 0)
            {
                readLength += 1;
            }
            string str = Transformer.TransString(AllValueRead, (index - ValueStartIndex) * CountOfByte, readLength, Encoding.ASCII);
            return str.Substring(0, length);
        }

        public short[] ShortArray(int index, int length)
        {
            return Transformer.TransInt16(AllValueRead, (index - ValueStartIndex) * CountOfByte, length);
        }

        public bool[] BoolArray(int index, int length)
        {
            //return Transformer.TransBool(AllValueRead, (index - ValueStartIndex) * CountOfByte, length);
            List<bool> bl = new List<bool>(length);
            int readByteLength = length / 8 + length % 8 == 0 ? 0 : 1;
            for (int i = 0; i < readByteLength; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    bl.Add((AllValueRead[(index - ValueStartIndex + i) * CountOfByte] & (1 << j)) != 0);
                }
            }

            return bl.Take(length).ToArray();
        }
    }
}
