using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class TwhPowerDataParser
    {
        public class PowerData
        {
            public DateTime Timestamp { get; set; }
            public string DeviceId { get; set; }
            public string DataType { get; set; }
            public int Count { get; set; }
            public Dictionary<int, decimal> Measurements { get; set; } = new Dictionary<int, decimal>();

            public override string ToString()
            {
                return $"{Timestamp:MM-dd HH:mm:ss} {DeviceId}:{DataType},CNT:{Count},测量点:{Measurements.Count}个";
            }
        }


        //// 解析单个数据包
        //var powerData = PowerDataParser.Parse(data);

        //Console.WriteLine($"解析结果: {powerData}");
        //    Console.WriteLine($"统计信息: {PowerDataParser.GetStatistics(powerData)}");
        //    Console.WriteLine($"是否有异常值: {PowerDataParser.HasAbnormalValues(powerData)}");
            
        //    // 获取前10个测量点的值
        //    var first10 = PowerDataParser.GetMeasurementsInRange(powerData, 1, 10);

        /// <summary>
        /// 解析电力数据报文
        /// </summary>
        /// <param name="dataString">原始数据字符串</param>
        /// <returns>解析后的电力数据对象</returns>
        public static PowerData Parse(string dataString)
        {
            if (string.IsNullOrWhiteSpace(dataString))
                throw new ArgumentException("数据字符串不能为空");

            var powerData = new PowerData();

            try
            {
                // 1. 解析时间戳和设备信息
                ParseHeader(dataString, powerData);

                // 2. 解析测量数据
                ParseMeasurements(dataString, powerData);

                return powerData;
            }
            catch (Exception ex)
            {
                throw new FormatException($"数据解析失败: {ex.Message}", ex);
            }
        }

        private static void ParseHeader(string dataString, PowerData powerData)
        {
            // 匹配时间戳和设备信息：!09-25 19:21:10 0100M101:DINV
            var headerMatch = Regex.Match(dataString, @"^!(\d{2}-\d{2} \d{2}:\d{2}:\d{2}) (\w+):(\w+),CNT:\s*(\d+)");

            if (!headerMatch.Success)
                throw new FormatException("数据头格式不正确");

            // 解析时间戳（假设当前年份）
            var now = DateTime.Now;
            var timeStr = headerMatch.Groups[1].Value;
            powerData.Timestamp = DateTime.ParseExact($"{now.Year}-{timeStr}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            powerData.DeviceId = headerMatch.Groups[2].Value;  // 0100M101
            powerData.DataType = headerMatch.Groups[3].Value;  // DINV
            powerData.Count = int.Parse(headerMatch.Groups[4].Value);  // 38
        }

        private static void ParseMeasurements(string dataString, PowerData powerData)
        {
            // 匹配所有测量点：X1,0.00KA;X2,0.00KA;...
            var measurementMatches = Regex.Matches(dataString, @"X(\d+),([\d.]+)KA");

            foreach (Match match in measurementMatches)
            {
                if (match.Groups.Count == 3)
                {
                    int pointNumber = int.Parse(match.Groups[1].Value);
                    decimal value = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                    powerData.Measurements[pointNumber] = value;
                }
            }

            // 验证数量是否匹配
            //if (powerData.Measurements.Count != powerData.Count)
            //{
            //    Console.WriteLine($"警告: 解析到的测量点数量({powerData.Measurements.Count})与声明数量({powerData.Count})不匹配");
            //}
        }

        /// <summary>
        /// 批量解析数据
        /// </summary>
        public static List<PowerData> ParseMultiple(string dataString)
        {
            // 如果包含多个数据包，按换行符分割
            var lines = dataString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var results = new List<PowerData>();

            foreach (var line in lines)
            {
                if (line.StartsWith("!")) // 只处理有效的数据行
                {
                    try
                    {
                        results.Add(Parse(line));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"解析失败: {line.Substring(0, Math.Min(50, line.Length))}... 错误: {ex.Message}");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 获取指定范围的测量值
        /// </summary>
        public static Dictionary<int, decimal> GetMeasurementsInRange(PowerData data, int start, int end)
        {
            return data.Measurements
                .Where(kv => kv.Key >= start && kv.Key <= end)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// 检查是否有异常值（非零值）
        /// </summary>
        public static bool HasAbnormalValues(PowerData data, decimal threshold = 0.01m)
        {
            return data.Measurements.Any(kv => kv.Value > threshold);
        }

        /// <summary>
        /// 生成统计信息
        /// </summary>
        public static string GetStatistics(PowerData data)
        {
            if (data.Measurements.Count == 0)
                return "无测量数据";

            var values = data.Measurements.Values.ToList();
            return $"总数: {data.Measurements.Count}, 最小值: {values.Min():F2}KA, 最大值: {values.Max():F2}KA, 平均值: {values.Average():F2}KA";
        }
    }
}
