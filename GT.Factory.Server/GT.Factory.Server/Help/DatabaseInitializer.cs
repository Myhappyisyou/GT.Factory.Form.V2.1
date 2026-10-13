using GT.Factory.Logging.Logging;
using System.Collections.Generic;
using System.IO;
using TaskContracts.Models;

namespace GT.Factory.Server.Help
{
    public static class DatabaseInitializer
    {
        public static void InitMonthlyDatabase(AccessMdbHelper newDb, bool isNew)
        {
            if (!isNew) return;

            string prevPath = MonthlyAccessDbManager.GetPreviousMonthDbPath();

            if (!File.Exists(prevPath))
                return;

            List<Consumables> oldData;

            using (var oldDb = new AccessMdbHelper(prevPath))
            {
                oldData = UploadSql.SelectConsumables(oldDb);
            }

            if (oldData == null || oldData.Count == 0)
                return;

            foreach (var item in oldData)
            {
                UploadSql.InsertOrUpdateConsumablesInfor(newDb, item);
            }

            DisplayLog.Info("跨月 Consumables 迁移完成");
        }
    }

}
