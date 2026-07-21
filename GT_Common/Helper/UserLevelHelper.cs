using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class UserLevelHelper
    {
        public static string ToLevelString(UserLevel level)
        {
            switch (level)
            {
                case UserLevel.OP:
                    return "OP";
                case UserLevel.ME:
                    return "ME";
                case UserLevel.QE:
                    return "QE";
                case UserLevel.PE:
                    return "PE";
                case UserLevel.ADM:
                    return "ADM";
                default:
                    return "none";
            }
        }

        public static UserLevel Parse(string str)
        {
            if (string.IsNullOrEmpty(str))
                return UserLevel.None;

            switch (str)
            {
                case "OP":
                    return UserLevel.OP;
                case "ME":
                    return UserLevel.ME;
                case "QE":
                    return UserLevel.QE;
                case "PE":
                    return UserLevel.PE;
                case "ADM":
                    return UserLevel.ADM;
                default:
                    return UserLevel.None;
            }
        }
    }

}
