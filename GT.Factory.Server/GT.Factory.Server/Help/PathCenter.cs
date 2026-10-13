using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Help
{
    public static class PathCenter
    {
        public static readonly string AppRoot =
            AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string StationRoot =
            FindStationRoot(AppRoot);

        public static readonly string Config =
            Path.Combine(StationRoot, "Config");

        public static readonly string Model =
            Path.Combine(Config, "Model");

        public static readonly string Plugins =
            Path.Combine(Config, "Plugins");

        public static readonly string UserRole =
           Path.Combine(Config, "UserRole");

        public static readonly string Data =
            Path.Combine(StationRoot, "Data");

        public static readonly string Log =
            Path.Combine(Data, "Log");

        public static readonly string History =
            Path.Combine(Data, "History");

        private static string FindStationRoot(string start)
        {
            var dir = new DirectoryInfo(start);

            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "Config")))
                    return dir.FullName;

                dir = dir.Parent;
            }

            throw new Exception("无法定位站点根目录（未找到 Config 文件夹）");
        }

        public static string ConfigFile(string name)
            => Path.Combine(Config, name);
        public static string ModelFile(string name)
            => Path.Combine(Model, name);
        public static string PluginsFile(string name)
            => Path.Combine(Plugins, name);
        public static string UserRoleFile(string name)
           => Path.Combine(UserRole, name);
        public static string DataFile(string name)
            => Path.Combine(Data, name);
        public static string LogFile(string name)
            => Path.Combine(Log, name);
        public static string HistoryFile(string name)
            => Path.Combine(History, name);
    }

    public enum HistoryKind
    {
        TWH,
        MyCsvData
    }

    public static class HistoryKindExtensions
    {
        public static string GetFolderName(this HistoryKind kind)
        {
            return kind.ToString(); // 简单用枚举名，也可以自定义
        }
    }

}
