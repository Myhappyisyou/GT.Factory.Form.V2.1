using Serilog;
using GT.Factory.Server.Core.Consumable;
using GT.Factory.Server.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using TaskContracts.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Consumables = TaskContracts.Models.Consumables;

namespace GT.Factory.Server.WebApi
{
    public class ConsumablesController : ApiController
    {
        public static AccessMdbHelper Db { get; set; }  // 静态注入

        [HttpPost]
        [Route("api/consumables/processNo")]
        public List<Consumables> GetConsumablesByprocessNo([FromBody] ConsumablesRequest req)
        {
            return UploadSql.SelectConsumables(Db, req.Process_no);
        }


        [HttpPost]
        [Route("api/consumables/insertOrUpdate")]
        public ApiResult InsertOrUpdateConsumable([FromBody] Consumables req)
        {
            try
            {
                var result = UploadSql.InsertOrUpdateConsumablesInfor(Db, req);

                return result
                    ? ApiResult.Ok("保存成功")
                    : ApiResult.Fail("保存失败");
            }
            catch (Exception)
            {
                Log.Error("保存易损件失败");
                return ApiResult.Fail("保存失败，请联系管理员");
            }
        }


        [HttpPost]
        [Route("api/consumables/delete")]
        public ApiResult DeleteConsumable([FromBody] Consumables req)
        {
            try
            {
                var rows = UploadSql.DeleteConsumableInfor(Db, req);

                return rows > 0
                    ? ApiResult.Ok("删除成功")
                    : ApiResult.Fail("记录不存在");
            }
            catch (Exception)
            {
                Log.Error("删除易损件失败");
                return ApiResult.Fail("删除失败，请联系管理员");
            }
        }

    }
}
