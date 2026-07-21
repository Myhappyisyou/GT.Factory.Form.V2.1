using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GT_Common.DriverForm.ProductCode
{
    [XmlRoot("ProductCodeConfig")]
    public class ProductCodeConfig
    {
        public List<ProductCodeRule> Rules { get; set; } = new List<ProductCodeRule>();

        private static string FilePath => PathCenter.ConfigFile("ProductCodeConfig.xml");

        public static ProductCodeConfig Instance { get; private set; }

        static ProductCodeConfig()
        {
            Load();
        }

        public static void Load()
        {
            if (!File.Exists(FilePath))
            {
                Instance = new ProductCodeConfig();
                Save();
                return;
            }

            using (var fs = new FileStream(FilePath, FileMode.Open))
            {
                var ser = new XmlSerializer(typeof(ProductCodeConfig));
                Instance = (ProductCodeConfig)ser.Deserialize(fs);
            }
        }

        public static void Save()
        {
            using (var fs = new FileStream(FilePath, FileMode.Create))
            {
                var ser = new XmlSerializer(typeof(ProductCodeConfig));
                ser.Serialize(fs, Instance);
            }
        }

        // 🔥 核心查询
        public static ProductCodeRule GetRule(string model, CodeType type)
        {
            return Instance.Rules.FirstOrDefault(x =>
                x.Model == model &&
                x.CodeType == type &&
                x.Enable);
        }

        public static bool Validate(string code, ProductCodeRule rule, out string error)
        {
            error = null;

            if (rule == null)
            {
                error = "规则不存在";
                return false;
            }

            if (!rule.Enable)
            {
                error = "规则未启用";
                return false;
            }

            if (code.Length != rule.Length)
            {
                error = $"长度错误，应为 {rule.Length}";
                return false;
            }

            if (!string.IsNullOrEmpty(rule.CodeMark) &&
                !code.Contains(rule.CodeMark))
            {
                error = $"标识错误，应为 {rule.CodeMark}";
                return false;
            }

            return true;
        }
    }
}
