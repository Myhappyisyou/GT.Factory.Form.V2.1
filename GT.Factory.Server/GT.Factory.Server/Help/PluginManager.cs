using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaskContracts;

namespace GT.Factory.Server.Help
{
    public class PluginManager
    {
        private readonly Dictionary<string, IDbTaskPlugin> _plugins = new Dictionary<string, IDbTaskPlugin>();

        public void LoadPlugins(string folder)
        {
            if (!Directory.Exists(folder)) return;

            foreach (var file in Directory.GetFiles(folder, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(file);
                    var types = asm.GetTypes().Where(t => typeof(IDbTaskPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                    foreach (var type in types)
                    {
                        var plugin = (IDbTaskPlugin)Activator.CreateInstance(type);
                        if (!_plugins.ContainsKey(plugin.TaskType))
                            _plugins[plugin.TaskType] = plugin;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载插件 {file} 失败: {ex.Message}");
                }
            }
        }

        public bool TryGetPlugin(string taskType, out IDbTaskPlugin plugin)
        {
            if (_plugins.TryGetValue(taskType, out plugin)) return true;
            if (_plugins.TryGetValue("Multi", out plugin)) return true; // 通用插件
            plugin = null;
            return false;
        }
    }
}
