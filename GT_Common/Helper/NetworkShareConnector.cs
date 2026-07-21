using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class NetworkShareConnector
    {
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(ref NETRESOURCE netResource, string password, string username, int flags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential)]
        private struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        /// <summary>
        /// 清理旧连接（避免多重连接问题）
        /// </summary>
        public static void Disconnect(string networkPath)
        {
            try
            {
                WNetCancelConnection2(networkPath, 0, true);
            }
            catch { /* 忽略未连接的情况 */ }
        }

        /// <summary>
        /// 连接到共享目录
        /// </summary>
        public static void Connect(string networkPath, string username, string password)
        {
            Disconnect(networkPath); // 先清旧连接

            NETRESOURCE nr = new NETRESOURCE
            {
                dwType = 1, // RESOURCETYPE_DISK
                lpRemoteName = networkPath
            };

            int result = WNetAddConnection2(ref nr, password, username, 0);
            if (result != 0)
            {
                throw new Win32Exception(result);
            }
        }

        /// <summary>
        /// 检查路径是否有效
        /// </summary>
        public static bool PathExists(string path)
        {
            return System.IO.Directory.Exists(path) || System.IO.File.Exists(path);
        }
    }
}
