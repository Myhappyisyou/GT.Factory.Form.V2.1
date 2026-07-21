using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    // 登录请求参数
    public class LoginRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    public class LoginCardRequest
    {
        public string UID { get; set; }
    }

    public class ConsumablesRequest
    {
        public string Process_no { get; set; }
    }
}
