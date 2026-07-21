using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public static class ConsumablesMapper
    {
        public static GT_Common.Model.Consumables ToLocalConsumable(
            this TaskContracts.Models.Consumables consumables)
        {
            if (consumables == null)
                return null;

            return new GT_Common.Model.Consumables
            {
                ID = consumables.ID,
                ProcessName = consumables.ProcessName,
                StationName = consumables.StationName,
                Location = consumables.Location,
                Name = consumables.Name,
                TheoreticalCount = consumables.TheoreticalCount,
                UsedCount = consumables.UsedCount,
                RemainderCount = consumables.RemainderCount,
            };
        }

        public static TaskContracts.Models.Consumables ToContractConsumable(
            this GT_Common.Model.Consumables  consumables)
        {
            if (consumables == null)
                return null;

            return new TaskContracts.Models.Consumables
            {
                ID = consumables.ID,
                ProcessName = consumables.ProcessName,
                StationName = consumables.StationName,
                Location = consumables.Location,
                Name = consumables.Name,
                TheoreticalCount = consumables.TheoreticalCount,
                UsedCount = consumables.UsedCount,
                RemainderCount = consumables.RemainderCount,

            };
        }
    }
}
