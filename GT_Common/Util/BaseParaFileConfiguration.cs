using GT_Common.Helper.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Util
{
    
    public abstract class BaseParaFileConfiguration<T> where T : new()
    {
        private static readonly object _lock = new object();
        private static string _configPath;
        private static Lazy<T> _instance;
        private static bool _initialized;

        public static void Initialize(string path)
        {
            lock (_lock)
            {
                if (_initialized && _configPath == path) return;

                _configPath = path;
                _instance = new Lazy<T>(LoadConfiguration);
                _initialized = true;
            }
        }

        private static T LoadConfiguration()
        {
            try
            {
                if (string.IsNullOrEmpty(_configPath)) throw new ArgumentNullException(nameof(_configPath));
                if (!File.Exists(_configPath)) throw new FileNotFoundException($"配置文件未找到: {_configPath}");

                var jsonContent = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<T>(jsonContent);

                if (config == null)
                {
                    //DisplayLog.ShowLogError($"PLC配置文件反序列化返回null");

                    throw new InvalidOperationException("反序列化返回null");
                }
                return config;
            }
            catch (Exception ex)
            {
                DisplayLog.Error("加载配置失败", ex, true);
                return new T();
            }
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException(
                        $"配置未初始化，请先调用 {nameof(Initialize)} 方法并确保配置文件存在且有效");
                }

                if (!_initialized)
                {
                    throw new InvalidOperationException(
                        $"配置初始化未完成，请检查 {nameof(Initialize)} 方法是否成功执行");
                }

                return _instance.Value;
            }
        }
    }
}
