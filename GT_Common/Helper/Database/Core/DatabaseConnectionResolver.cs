using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Database.Core
{
    public static class DatabaseConnectionResolver
    {
        public static string Resolve(DatabaseNodeOptions option)
        {
            switch (option.DbType)
            {
                case DatabaseType.SQLite:

                    return ResolveSqlite(option.DbType, option.ConnectionString);

                case DatabaseType.Access:

                    return ResolveSqlite(option.DbType, option.ConnectionString);


                default:

                    return option.ConnectionString;
            }
        }



        private static string ResolveSqlite(DatabaseType type, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return connectionString;


            //已经是绝对路径
            if (IsPathFullyQualified(connectionString))
                return connectionString;


            var builder = new SQLiteConnectionStringBuilder(connectionString);


            var fileName = builder.DataSource;


            //统一放 History/SQLite
            var sqlitePath =
                PathCenter.HistoryFile(
                    Path.Combine(
                        type.ToString(),
                        fileName));


            Directory.CreateDirectory(
                Path.GetDirectoryName(sqlitePath));


            builder.DataSource = sqlitePath;


            return builder.ToString();
        }

        private static bool IsPathFullyQualified(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // 绝对路径判断，兼容 Windows 和 Unix
            return Path.IsPathRooted(path) && !string.IsNullOrEmpty(Path.GetPathRoot(path)?.Trim('\\', '/'));
        }
    }
}
