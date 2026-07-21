using GT_Common.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.ProductCode
{
    public class ProductCodeSetting
    {
        public Dictionary<string, string> CodeTypeMap { get; set; }
    }

    public static class ProductCodeSettingService
    {
        private static ProductCodeSetting _config;

        public static ProductCodeSetting Instance
        {
            get
            {
                if (_config == null)
                    Load();
                return _config;
            }
        }

        public static void Load()
        {
            string path = "ProductCodeSetting.json";

            if (!File.Exists(PathCenter.ConfigFile(path)))
            {
                // 默认生成一份
                _config = new ProductCodeSetting
                {
                    CodeTypeMap = new Dictionary<string, string>
                {
                    { "Shell", "管壳" },
                    { "Housing", "箱体" },
                        { "Virtual","虚拟码"},
                        { "Diffuser","扩散器"}
                }
                };

                File.WriteAllText(PathCenter.ConfigFile(path), JsonConvert.SerializeObject(_config, Formatting.Indented));
            }
            else
            {
                _config = JsonConvert.DeserializeObject<ProductCodeSetting>(
                    File.ReadAllText(PathCenter.ConfigFile(path)));
            }
        }
    }
}
