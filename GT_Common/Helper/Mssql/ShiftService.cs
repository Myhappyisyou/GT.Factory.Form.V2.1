using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.Mssql
{
    public class ShiftInfo
    {
        public string ShiftName { get; set; }     // 日班/夜班
        public DateTime StartTime { get; set; }   // 班次起点
        public DateTime EndTime { get; set; }     // 班次结束
    }

    public static class ShiftService
    {
        // 这里可改成读取配置/数据库
        public static ShiftInfo GetCurrentShift(DateTime now)
        {
            // 假设日班 08:00-20:00，夜班 20:00-次日08:00
            DateTime today8 = now.Date.AddHours(8);
            DateTime today20 = now.Date.AddHours(20);

            if (now >= today8 && now < today20)
            {
                return new ShiftInfo
                {
                    ShiftName = "日班",
                    StartTime = today8,
                    EndTime = today20
                };
            }
            else
            {
                DateTime nightStart = now < today8 ? now.Date.AddDays(-1).AddHours(20) : today20;
                DateTime nightEnd = nightStart.AddHours(12); // 20:00-08:00
                return new ShiftInfo
                {
                    ShiftName = "夜班",
                    StartTime = nightStart,
                    EndTime = nightEnd
                };
            }
        }
    }
}
