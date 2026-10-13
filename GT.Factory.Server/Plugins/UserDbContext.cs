using Server.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins
{
    public static class UserDbContext
    {
        public static readonly AccessMdbHelper Db =
            new AccessMdbHelper(
                @"D:\BYD_Users\Users_Data.mdb");
    }
}
