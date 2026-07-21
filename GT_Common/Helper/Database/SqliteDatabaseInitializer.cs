using System;
using System.Data.SQLite;
using System.IO;

namespace GT_Common.Helper.Database
{
    public static class SqliteDatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            var builder = new SQLiteConnectionStringBuilder(connectionString);

            var dbFile = builder.DataSource;

            //确保目录存在
            var dir = Path.GetDirectoryName(dbFile);

            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }


            //数据库不存在会自动创建
            SQLiteConnection.CreateFile(dbFile);


            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();


                CreateSyncQueue(conn);
            }
        }



        private static void CreateSyncQueue(SQLiteConnection conn)
        {
            string sql = @"

                        CREATE TABLE IF NOT EXISTS SyncQueue
                        (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        
                            DataType TEXT NOT NULL,
                        
                            DataJson TEXT,
                        
                            Status INTEGER DEFAULT 0,
                        
                            RetryCount INTEGER DEFAULT 0,
                        
                            ErrorMessage TEXT,
                        
                            CreateTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                        
                            UpdateTime DATETIME
                        
                        );
                        
                        ";


            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
