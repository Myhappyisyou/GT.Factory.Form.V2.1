using GT.Factory.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GT.Factory.WebApi.QueryClient
{
    public class ConsumablesApiClient
    {
        private readonly QueryClient _client;

        public ConsumablesApiClient(string serverUrl)
        {
            _client = new QueryClient(serverUrl);
        }

        /// <summary>
        /// 获取工序易损件
        /// GET /api/user/all
        /// </summary>
        public Task<List<Consumables>> GetProcessNoConsumablesAsync(string process_no)
        {
            var req = new ConsumablesRequest { Process_no = process_no };
            return _client.CallAsync <List<Consumables>, ConsumablesRequest>("api/consumables/processNo", req);
        }

        /// <summary>
        /// 获取工序易损件
        /// GET /api/user/all
        /// </summary>
        public Task<ApiResult> InsertOrUpdateConsumableAsync(Consumables consumables)
        {
           return _client.CallAsync<ApiResult, Consumables>("api/consumables/insertOrUpdate", consumables);
        }

        /// <summary>
        /// 获取工序易损件
        /// GET /api/user/all
        /// </summary>
        public Task<ApiResult> DeleteConsumableAsync(Consumables consumables)
        {
           return _client.CallAsync<ApiResult, Consumables>("api/consumables/delete", consumables);
        }
    }
}
