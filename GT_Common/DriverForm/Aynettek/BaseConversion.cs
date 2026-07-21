using System;
using System.Collections.Generic;
using System.Text;

namespace GT_Common.Driver.Aynettek
{
   public class BaseConversion
    {
        //对byte数组进行比较
        public static bool MyMemcmp(byte[] data1, int offset1, byte[] data2, int offset2, int len)
        {
            try
            {
                for (int i = 0; i < len; i++)
                {
                    if (data1[i + offset1] != data2[i + offset2])
                    {
                        return false;
                    }
                }
            }
            catch { return false; }

            return true;
        }

       public static byte[] BytesToU16Array(byte[] datas, int offset, int len)
       {
           List<byte> tmpDatas = new List<byte>();
           for (int i = offset; i < len; i++)
           {
               tmpDatas.Add(0x00);
               tmpDatas.Add(datas[i]);
           }
           return tmpDatas.ToArray();
       }
       //U16变量赋值给Byte[]数组
       public static void U16ToByteArray(UInt16 source, ref byte[] datas, UInt32 des_index)
       {
           datas[des_index] = (byte)(source >> 8);
           datas[des_index + 1] = (byte)(source & 0x00ff);
       }
        //将字节数组形式的mac地址转化为对应的字符串
        public static string MacToString(byte[] mac)
        {
            string MacString = "";

            for (int i = 0; i < 6; i++)
            {
                MacString += ByteToHexString(mac[i]);
                if (i < 5)
                {
                    MacString += ":";
                }
            }

            return MacString;
        }

        //将字符串形式的mac地址转化为对应的字节数组
        public static byte[] StringToMac(string str)
        {
            string temp = "";

            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] != ' ') && (str[i] != ':'))
                {
                    temp += str[i];
                }
            }

            return HexStringToByte(temp, 0, 6);//6字节长度
        }
        //将字节类型的数据转化为十六进制字符串
        public static string ByteToHexString(byte[] data, int pos, int length, string space)
        {
            string outString = "";

            for (int i = pos; i < pos + length; i++)
            {
                outString += ByteToHexString(data[i]);
                if (i != pos + length - 1)
                {
                    outString += space;
                }
            }

            return outString;
        }

        public static byte[] HexStringToByte(string str, int pos)
        {
            string tempStr = GetStringWithoutSpace(str, pos);

            return HexStringToByte(tempStr, 0, (tempStr.Length + 1) >> 1);
        }

        public static byte HexStringToOneByte(string str, int pos)
        {
            string tempStr = GetStringWithoutSpace(str, pos);
            if ((!ValidHexString(tempStr)) || ((tempStr.Length - pos + 1) >> 1 != 1))
            {
                return 0;
            }
            return (byte)(HexStringToHex(str, pos) * 16 + HexStringToHex(str, pos + 1));
        }

        //将字符串的pos位置开始，转化为数组，转化后数组的长度为cnt
        private static byte[] HexStringToByte(string str, int pos, int cnt)
        {
            if ((!ValidHexString(str)) || ((str.Length - pos + 1) >> 1 < cnt))
            {
                return null;
            }

            if (str.Length % 2 != 0)
            {
                str = "0" + str;
            }
            byte[] data = new byte[cnt];

            for (int i = 0; i < cnt; i++)
            {
                data[i] = (byte)(HexStringToHex(str, 2 * i + pos) * 16 + HexStringToHex(str, 2 * i + pos + 1));
            }

            return data;
        }
        private static string GetStringWithoutSpace(string str, int pos)
        {
            string temp = "";
            for (int i = pos; i < str.Length; i++)
            {
                if ((str[i] != ' ') && (str[i] != ':'))
                {
                    temp += str[i];
                }
            }
            return temp;
        }
        //判断字符串是否是十六进制字符串
        public static bool ValidHexString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!
                    (((str[i] >= '0') && (str[i] <= '9')) ||
                    ((str[i] >= 'a') && (str[i] <= 'f')) ||
                    ((str[i] >= 'A') && (str[i] <= 'F')))
                    )
                {
                    return false;
                }
            }

            return true;
        }
        //将单个十六进制字符(4 bits)转化为byte
        private static byte HexStringToHex(string str, int pos)
        {
            byte value = 0;

            if (str.Length <= pos)
            {
                return value;
            }
            if ((str[pos] >= '0') && (str[pos] <= '9'))
            {
                value = (byte)(str[pos] - '0');
            }
            else if ((str[pos] >= 'a') && (str[pos] <= 'f'))
            {
                value = (byte)(str[pos] - 'a' + 10);
            }
            else if ((str[pos] >= 'A') && (str[pos] <= 'F'))
            {
                value = (byte)(str[pos] - 'A' + 10);
            }

            return value;
        }

        //将字节类型的数据转化为十六进制字符串
        public static string ByteToHexString(byte data)
        {
            string outString = "";

            if (data < 16)
            {
                outString += "0";
            }
            outString += data.ToString("X");

            return outString;
        }
    }
}
