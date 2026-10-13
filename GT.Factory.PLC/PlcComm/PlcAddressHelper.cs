using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.PLC.PlcComm
{
    public static class PlcAddressHelper
    {
        /// <summary>
        /// int 地址 → string 写入地址
        /// 例如：sectionName="D", address=100 → "D100"
        /// </summary>
        public static string ToWriteAddress(string sectionName, int address)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
                throw new ArgumentException("地址标识不能为空", nameof(sectionName));

            return $"{sectionName}{address}";
        }

        /// <summary>
        /// string 地址 → int 读取地址
        /// 例如："D100" → 100，"100" → 100
        /// </summary>
        public static int ToReadAddress(string address)
        {
            if (!TryToReadAddress(address, out int result))
                throw new FormatException($"无法解析地址: {address}");

            return result;
        }

        /// <summary>
        /// 尝试将 PLC 地址字符串解析为 int 读取偏移量
        /// 支持格式：
        ///   纯数字:       "100"      → 100
        ///   前缀字母:     "D100"     → 100
        ///   西门子DB块:   "DB4.DW20" → 20（取最后一段数字）
        ///                "DB4.DBW20"→ 20
        ///                "DB4.DBD20"→ 20
        /// </summary>
        public static bool TryToReadAddress(string address, out int result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(address))
                return false;

            address = address.Trim();

            // ========== 纯数字：直接解析 ==========
            if (int.TryParse(address, out result))
                return true;

            // ========== 西门子 DB 块格式：DB4.DW20 / DB4.DBW20 / DB4.DBD20 ==========
            if (address.IndexOf('.') >= 0)
            {
                // 取 '.' 后面的部分，再提取数字
                string afterDot = address.Substring(address.IndexOf('.') + 1);

                string numberPart = new string(
                    afterDot.SkipWhile(c => !char.IsDigit(c)).ToArray()
                );

                return int.TryParse(numberPart, out result);
            }

            // ========== 普通前缀字母格式：D100 / M100 / DT100 ==========
            {
                string numberPart = new string(
                    address.SkipWhile(c => !char.IsDigit(c)).ToArray()
                );

                return int.TryParse(numberPart, out result);
            }
        }

        /// <summary>
        /// 判断是否含前缀字母
        /// </summary>
        public static bool IsStringAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;

            return !int.TryParse(address, out _);
        }
    }
}

