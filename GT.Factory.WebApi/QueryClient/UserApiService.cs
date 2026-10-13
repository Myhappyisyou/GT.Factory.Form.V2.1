using GT.Factory.Common.Interfaces;
using GT.Factory.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.WebApi.QueryClient
{
    public class UserApiService : IUserService // 走 HTTP
    {
        private readonly UserApiClient _client;

        //private readonly ClientTaskSender _clientTaskSender;

        public UserApiService(string serverUrl, string taskServerUrl = null)
        {
            _client = new UserApiClient(serverUrl);
            //_clientTaskSender = new ClientTaskSender(taskServerUrl);
        }

        public async Task<User> GetUserByLogin(string userId, string password)
        {
            var result = await _client.LoginAsync(userId, password);

            if (!result.Success)
                throw new Exception(result.Message);

            return result.User;
        }

        public async Task<User> GetUserByRoleId(string userId, string password)
        {
            var result = await _client.RoleIdLoginAsync(userId, password);

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
            //await _clientTaskSender.SendTaskAsync("InsertUserSwipeLog", user);
        }
    }
}
