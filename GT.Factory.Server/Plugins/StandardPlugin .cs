using Newtonsoft.Json;
using Server.Help;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaskContracts;
using TaskContracts.Models;

namespace Plugins
{
    public class StandardPlugin : IDbTaskPlugin
    {
        public string TaskType => "Multi";

        public void Execute(DbTask task, object dbObj, Action<string, string> log)
        {
            var db = dbObj as AccessMdbHelper;
            if (db == null) throw new InvalidOperationException("db 参数必须是 AccessShortConnectionHelper");

            switch (task.TaskType)
            {
                case "InsertAlarm":
                    var alarm = JsonConvert.DeserializeObject<AlarmInfo>(task.PayloadJson);
                    UploadSql.InsertAlarmInfor(db, alarm); // 调用 Service
                    log?.Invoke("[Plugin]", $"插入报警 {alarm.ProcessName}--{alarm.AlarmStation}--{alarm.Description}");
                    break;
                case "UpdateConsumable":
                    var consumables = JsonConvert.DeserializeObject<Consumables>(task.PayloadJson);
                    UploadSql.UpdateConsumables(db, consumables); // 调用 Service
                    log?.Invoke("[Plugin]", $"自动更新易损件 {consumables.ProcessName}--{consumables.Location}--{consumables.Name}--{consumables.UsedCount}");
                    break;
                case "InsertOrUpdateConsumable":
                    var consumable = JsonConvert.DeserializeObject<Consumables>(task.PayloadJson);
                    UploadSql.InsertOrUpdateConsumablesInfor(db, consumable); // 调用 Service
                    log?.Invoke("[Plugin]", $"手动更新易损件 {consumable.ProcessName}--{consumable.Location}--{consumable.Name}--{consumable.UsedCount}");
                    break;
                case "DeleteConsumable":
                    var DeleteConsumable = JsonConvert.DeserializeObject<Consumables>(task.PayloadJson);
                    UploadSql.DeleteConsumableInfor(db, DeleteConsumable); // 调用 Service
                    log?.Invoke("[Plugin]", $"删除易损件 {DeleteConsumable.ProcessName}--{DeleteConsumable.Location}--{DeleteConsumable.Name}--{DeleteConsumable.UsedCount}");
                    break;
                case "InsertProcessProperty":
                    var processData = JsonConvert.DeserializeObject<ProcessData>(task.PayloadJson);
                    UploadSql.InsertProcessPropertyWithResult(db, processData); // 调用 Service
                    log?.Invoke("[Plugin]", $"插入测试数据 {processData.Process_no}--{processData.DataItem.Sn}--{processData.DataItem.Data.Length}");
                    break;
                case "InsertUserSwipeLog":  //  用户登录记录
                    var user = JsonConvert.DeserializeObject<User>(task.PayloadJson);
                    UploadSql.InsertOrUpdateUser(UserDbContext.Db, user); // 调用 Service
                    log?.Invoke("[Plugin]", $"更新用户登录记录 {user.JobNub}--{user.UserName}--{user.UID}");
                    break;

                    // 其他任务类型...
            }
        }
    }
}
