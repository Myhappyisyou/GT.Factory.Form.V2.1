using GT.Factory.Server.Help;
using System.Collections.Generic;
using System.Web.Http;
using TaskContracts.Models;

namespace GT.Factory.Server.WebApi
{
    public class UserController : ApiController
    {
        public static AccessMdbHelper Db { get; set; }  // 静态注入

        [HttpGet]
        [Route("api/user/all")]
        public List<User> GetAllUsers()
        {
            return UploadSql.SelectUsersInfors(Db);
        }

        [HttpPost]
        [Route("api/user/uid")]
        public User GetUserByCard([FromBody] LoginCardRequest req)
        {
            return UploadSql.SelectUsersInforByUID(Db, req.UID);
        }

        [HttpPost]
        [Route("api/user/login")]
        public LoginResponse Login([FromBody] LoginRequest req)
        {
            var user = UploadSql.SelectUsersInforByJobNub(Db, req.UserId, req.Password);

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
