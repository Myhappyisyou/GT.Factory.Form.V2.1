using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskContracts.Models;

namespace GT.Factory.Server.WebApi
{
    // 登录返回结果
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
    }

    public class ApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static ApiResult Ok(string msg = "成功")
            => new ApiResult { Success = true, Message = msg };

        public static ApiResult Fail(string msg)
            => new ApiResult { Success = false, Message = msg };
    }

}
