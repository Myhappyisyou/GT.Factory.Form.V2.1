using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.QueryClient
{
    public class UserApiClient
    {
        private readonly QueryClient _client;

        public UserApiClient(string serverUrl)
        {
            _client = new QueryClient(serverUrl);
        }

        /// <summary>
        /// 获取所有用户
        /// GET /api/user/all
        /// </summary>
        public Task<List<User>> GetAllUsersAsync()
        {
            return _client.CallAsync<List<User>>("api/user/all");
        }

        /// <summary>
        /// 根据卡号获取用户信息
        /// POST /api/user/uid
        /// </summary>
        public Task<User> GetUserByCardAsync(string uid)
        {
            var req = new LoginCardRequest { UID = uid };
            return _client.CallAsync<User, LoginCardRequest>("api/user/uid", req);
        }

        /// <summary>
        /// 用户登录
        /// POST /api/user/login
        /// </summary>
        public Task<LoginResponse> LoginAsync(string userId, string password)
        {
            
                var req = new LoginRequest
                {
                    UserId = userId,
                    Password = password
                };
                return _client.CallAsync<LoginResponse, LoginRequest>("api/user/login", req);
            
        }


    }
}
