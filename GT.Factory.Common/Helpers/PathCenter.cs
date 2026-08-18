using System;
using System.Diagnostics;
using System.IO;

namespace GT.Factory.Common.Helper
{
    public static class PathCenter
    {
        public static readonly string AppRoot =
            AppDomain.CurrentDomain.BaseDirectory;


        private static string _stationRoot;

        public static string StationRoot
        {
            get
            {
                if (_stationRoot == null)
                {
                    _stationRoot =
                        FindStationRoot(AppRoot);
                }

                return _stationRoot;
            }
        }

        public static string Config =>
            Path.Combine(StationRoot, "Config");

        public static string Data =>
            Path.Combine(StationRoot, "Data");

        public static string Log =>
            Path.Combine(Data, "Log");

        public static string History =>
            Path.Combine(Data, "History");

        private static string FindStationRoot(string start)
        {
            if (Process.GetCurrentProcess()
                       .ProcessName
                       .Equals("devenv",
                           StringComparison.OrdinalIgnoreCase))
            {
                return start;
            }

            var dir = new DirectoryInfo(start);

            while (dir != null)
            {
                if (Directory.Exists(
                    Path.Combine(dir.FullName, "Config")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            throw new Exception(
                "无法定位站点根目录（未找到 Config 文件夹）");
        }

        public static string ConfigFile(string name)
            => Path.Combine(Config, name);

        public static string DataFile(string name)
            => Path.Combine(Data, name);
        public static string LogFile(string name)
          => Path.Combine(Log, name);
        public static string HistoryFile(string name)
           => Path.Combine(History, name);


        public static string Model(
      string modelName)
        {
            return Path.Combine(
                Config,
                modelName);
        }


        /// <summary>
        /// LIN模型目录
        ///
        /// 例如:
        ///
        /// Config
        ///   └── BYD_300W_HEV
        ///       ├── LinProject.json
        ///       ├── xxx.ldf
        ///       └── LinConfig.json
        ///
        /// </summary>
        public static string ModelLin(
            string model)
        {
            return Path.Combine(
                Config,
                model,
                "LIN");
        }


        public static string ModelFile(
       string modelName,
       string file)
        {
            return Path.Combine(
                Model(modelName),
                file);
        }



        public static string ModelLinFile(
            string modelName,
            string file)
        {
            return Path.Combine(
                ModelLin(modelName),
                file);
        }

    }

    public enum HistoryKind
    {
        TWH,
        MyCsvData,
        水爆件生产数据,
        切割件生产数据,
    }

    public static class HistoryKindExtensions
    {
        public static string GetFolderName(this HistoryKind kind)
        {
            return kind.ToString(); // 简单用枚举名，也可以自定义
        }
    }

}
