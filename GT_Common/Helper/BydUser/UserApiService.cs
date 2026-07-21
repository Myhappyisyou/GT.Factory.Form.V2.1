using GT_Common.Helper.ClientTask;
using GT_Common.Helper.QueryClient;
using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.BydUser
{
    public class UserApiService : IUserService // 走 HTTP
    {
        private readonly UserApiClient _client;

        private readonly ClientTaskSender _clientTaskSender;

        public UserApiService(string serverUrl, string taskServerUrl)
        {
            _client = new UserApiClient(serverUrl);
            _clientTaskSender = new ClientTaskSender(taskServerUrl);
        }

        public async Task<User> GetUserByLogin(string userId, string password)
        {
            var result = await _client.LoginAsync(userId, password);

            if (!result.Success)
                throw new Exception(result.Message);

            return result.User;
        }

        public async Task<User> GetUserByCard(string uid)
        {
            return await _client.GetUserByCardAsync(uid);
        }

        public async Task InsertUserSwipeLog(User user)
        {
            await _clientTaskSender.SendTaskAsync("InsertUserSwipeLog", user);
        }
    }
}
