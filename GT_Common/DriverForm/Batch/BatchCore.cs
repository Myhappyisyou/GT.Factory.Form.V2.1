using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Batch
{
    //解析 + 匹配核心
    public static class BatchCore
    {
        // ===== 解析 Key:Value =====
        public static Dictionary<string, string> Parse(string input)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var pairs = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in pairs)
            {
                var kv = p.Split(new[] { ':' }, 2, StringSplitOptions.None);
                if (kv.Length == 2)
                {
                    dict[kv[0].Trim()] = kv[1].Trim();
                }
            }

            return dict;
        }

        // ===== 是否属于当前机台 =====
        public static bool IsMatch(Dictionary<string, string> data, ClientBatchConfig config)
        {
            if (config == null || data == null) return false;

            if (!data.TryGetValue(config.MaterialField, out string value))
                return false;

            // 如果值包含 /，取 / 前面部分
            string actualValue = value;
            int slashIndex = value.IndexOf('/');
            if (slashIndex >= 0)
                actualValue = value.Substring(0, slashIndex);

            // 精确匹配 BYDBatchNub
            return string.Equals(actualValue, config.MaterialCode, StringComparison.OrdinalIgnoreCase);
        }

        // ===== Q解析 =====
        public static QuantityInfo ParseQuantity(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return new QuantityInfo();

            var parts = q.Split('/');

            return new QuantityInfo
            {
                Total = TryParse(parts, 0),
                BoxNUb = TryParse(parts, 1),
                BoxCount = TryParse(parts, 2),
            };
        }

        private static short TryParse(string[] arr, int index)
        {
            if (arr.Length > index && short.TryParse(arr[index], out short val))
                return val;
            return 0;
        }

        public static string Get(Dictionary<string, string> data, string key)
        {
            return data.TryGetValue(key, out var val) ? val : string.Empty;
        }
    }

    public class QuantityInfo
    {
        public short Total { get; set; }
        public short BoxNUb { get; set; }
        public short BoxCount { get; set; }
    }
}
