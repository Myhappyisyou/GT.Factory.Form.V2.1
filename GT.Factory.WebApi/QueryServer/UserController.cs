using GT.Factory.Common.Models;
using GT.Factory.GTUser.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace GT.Factory.WebApi.QueryServer
{
    public class UserController : ApiController
    {

        private readonly ILocalUserService _localUserService;


        public UserController(ILocalUserService localUserService)
        {
            _localUserService = localUserService;
        }


        [HttpGet]
        [Route("api/user/all")]
        public async Task<List<User>> GetAllUsers()
        {
            return await _localUserService.GetAllAsync();
        }

        [HttpPost]
        [Route("api/user/uid")]
        public async Task<User> GetUserByCard([FromBody] LoginCardRequest req)
        {
            return await _localUserService.GetByUIDAsync(req.UID);
        }

        [HttpPost]
        [Route("api/user/login")]
        public async Task<LoginResponse> Login([FromBody] LoginRequest req)
        {
            var user = await _localUserService.GetByJobAsync(req.UserId, req.Password);

            if (user != null)
            {
                return new LoginResponse
                {
                    Success = true,
                    Message = "登录成功",
                    User = user
                };
            }
            else
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "账号或密码错误",
                    User = null
                };
            }
        }
    }
}
