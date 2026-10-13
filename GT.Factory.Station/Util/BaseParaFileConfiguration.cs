using GT.Factory.Logging.Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GT.Factory.Station.Util
{

    /// <summary>
    /// 可选：配置类实现此接口后，Reload 时会自动做合法性校验，
    /// 校验失败则不应用新配置，保留原运行时配置不变。
    /// </summary>
    public interface IValidatableConfig
    {
        void Validate();
    }

    public abstract class BaseParaFileConfiguration<T> where T : new()
    {
        private static readonly object _lock = new object();
        private static string _configPath;
        private static Lazy<T> _instance;
        private static bool _initialized;

        /// <summary>
        /// 配置被重新加载后触发（同一个 Instance 引用，内容已刷新）
        /// </summary>
        public static event Action<T> Reloaded;

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
                    throw new InvalidOperationException("反序列化返回null");

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

        /// <summary>
        /// 热重载：不改变 Instance 引用，仅刷新其内容。
        /// 会先校验新配置（若实现了 IValidatableConfig），校验失败则抛异常、不应用。
        /// </summary>
        public static void Reload()
        {
            lock (_lock)
            {
                if (!_initialized)
                    throw new InvalidOperationException("配置尚未初始化，无法 Reload");

                if (string.IsNullOrEmpty(_configPath) || !File.Exists(_configPath))
                    throw new FileNotFoundException($"配置文件未找到: {_configPath}");

                var jsonContent = File.ReadAllText(_configPath);
                var fresh = JsonConvert.DeserializeObject<T>(jsonContent);

                if (fresh == null)
                    throw new InvalidOperationException("重新加载配置失败：反序列化返回 null");

                // 先校验新配置，不合法就不应用，运行时配置保持不变
                if (fresh is IValidatableConfig validatable)
                    validatable.Validate();

                // 原地覆盖：Instance 引用不变，内部所有属性（含子对象、集合）被替换为新内容
                var json = JsonConvert.SerializeObject(fresh);
                JsonConvert.PopulateObject(json, _instance.Value, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace // 集合/子对象整体替换，避免合并残留
                });
            }

            Reloaded?.Invoke(_instance.Value);
        }
    }
}
