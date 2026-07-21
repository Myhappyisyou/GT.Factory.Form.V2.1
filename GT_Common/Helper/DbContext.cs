using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class DbContext
    {
        public static AccessMdbHelper CurrentDb { get; private set; }
        public static string CurrentMonth { get; private set; }

        public static void Set(AccessMdbHelper db, string month)
        {
            CurrentDb = db;
            CurrentMonth = month;
        }
    }
}
