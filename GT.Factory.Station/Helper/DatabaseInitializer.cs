using GT.Factory.Common.Models;
using GT.Factory.Station;
using GT.Factory.Station.Helper.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Station.Helper
{
    public static class DatabaseInitializer
    {
        public static async Task InitMonthlyDatabase(AccessMdbHelper newDb, bool isNew)
        {
            if (!isNew) return;

            string prevPath = MonthlyAccessDbManager.GetPreviousMonthDbPath();

            if (!File.Exists(prevPath))
                return;

            List<Consumables> oldData;

            using (var oldDb = new AccessMdbHelper(prevPath))
            {
                oldData = await AppRuntime.Current.DataRuntime.Repositories.Access.GetAllConsumableAsync();
                //oldData = UploadSql.Ac_SelectAllConsumables(oldDb);
            }

            if (oldData == null || oldData.Count == 0)
                return;

            foreach (var item in oldData)
            {
                await AppRuntime.Current.DataRuntime.Repositories.Access.UpdateConsumableUsageAsync(item);

                //UploadSql.InsertOrUpdateConsumablesInfor(newDb, item);
            }

            DisplayLog.Info("跨月 Consumables 迁移完成");
        }
    }
}
