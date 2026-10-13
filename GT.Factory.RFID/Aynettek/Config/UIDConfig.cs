using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GT.Factory.RFID.Aynettek
{
    /// <summary>
    /// MES模块配置。
    /// </summary>
    public class UIDConfig
    {
        [Category("读卡器配置")]
        [DisplayName("IP")]
        public string UIDIP { get; set; } = "192.168.0.95";//192.168.2.29

        [Category("读卡器配置")]
        [DisplayName("端口号")]
        public int UIDPort { get; set; } = 1030;


        public UIDConfig()
        {
            UIDIP = "127.0.0.1";
            UIDPort = 1030;
        }
    }
}