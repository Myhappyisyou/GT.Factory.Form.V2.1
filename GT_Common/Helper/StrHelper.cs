using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class StrHelper
    {
        public static string ReplaceIllegalChar(string str)
        {
            if (str == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder(str.Length);
            foreach (var item in str)
            {
                if (item >= 33 && item <= 126)
                {
                    sb.Append(item);
                }
                else
                {
                    sb.Append('?');
                }
            }
            return sb.ToString();
        }


        public static string GetValidStr(string Str)
        {
            string validStr = "";
            if (Str.IndexOf("?") == -1)
            {
                validStr = Str.Substring(0, Str.Length);
            }
            else
            {
                validStr = Str.Substring(0, Str.IndexOf("?"));
            }           
             
            return validStr;
        }
                
    }
}
