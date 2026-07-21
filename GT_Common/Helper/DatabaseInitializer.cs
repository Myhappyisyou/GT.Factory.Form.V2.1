using GT_Common;
using GT_Common.Helper.Logging;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
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
                oldData = UploadSql.Ac_SelectAllConsumables(oldDb);
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
