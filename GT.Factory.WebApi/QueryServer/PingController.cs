using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace GT.Factory.WebApi.QueryServer
{
    public class PingController : ApiController
    {
        [HttpGet]
        [Route("api/ping")]
        public string Ping()
        {
            return "OK";
        }
    }

}
