using AYNETTEK.HFModbus;
using GT_Common.Driver.Aynettek;
using GT_Common.Helper.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Aynettek
{
    public class AynettekMethode
    {

        public Reader reader { get; private set; }

        private const string DeviceId = "1";
        private byte _deviceId = 0;
        private byte _rfidType = 0;


        public AynettekMethode(Reader reader)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }
        //string addr, int port   Config.Instance.UIDIP, Config.Instance.UIDPort
        public bool InitDriver(string addr, int port)
        {
            // 添加 null 检查
            if (reader == null)
            {
                DisplayLog.Error("Reader 对象未初始化",null,true);
                return false;
            }
            try
            {
                if (!reader.Connect( addr,  port))
                {
                    DisplayLog.Info("读卡器连接失败");
                    return false;
                }

                _deviceId = byte.Parse(DeviceId);

                // 获取软件版本
                byte[] softVersion = null;
                if (reader.GetSoftVer(_deviceId, ref softVersion) != Execute_Status.SUCCESS)
                {
                    reader.DisConnect();
                    return false;
                }

                // 确定RFID类型
                DetermineRfidType(softVersion[1]);

                // 获取设备信息（仅对新版本）
                string softVer = BaseConversion.ByteToHexString(softVersion, 0, softVersion.Length, ".");
                byte tmpVersion = BaseConversion.HexStringToOneByte(softVer.Substring(softVer.Length - 2, 2), 0);

                if (tmpVersion >= 2)
                {
                    string sn = "", name = "";
                    if (reader.GetDeviceProductName(_deviceId, ref name) != Execute_Status.SUCCESS ||
                        reader.GetDeviceProductSn(_deviceId, ref sn) != Execute_Status.SUCCESS)
                    {
                        reader.DisConnect();
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                DisplayLog.Error("读卡器连接失败", ex);

                return false;
            }
        }

        private void DetermineRfidType(byte flag)
        {
            _rfidType = 0;
            if ((flag & 0x40) == 0x40 && (flag & 0x60) == 0x40)
            {
                _rfidType = 1;
            }
            else if ((flag & 0x40) == 0x40 && (flag & 0x60) == 0x60)
            {
                _rfidType = 2;
            }
        }

        /// <summary>
        /// 获取标签在线信号
        /// </summary>
        //public (string uid, bool readSuccess)  GetTagOnline()
        //{
        //    bool tagSingle = false;
        //    switch (reader.TagOnline(_deviceId, ref tagSingle))
        //    {
        //        case Execute_Status.SUCCESS:
        //            break;
        //        case Execute_Status.CONNECT_CLOSE:
        //            break;
        //        case Execute_Status.NO_RESPONSE:
        //            break;
        //        default:
        //            break;
        //    }

        //    if (tagSingle)
        //    {
        //        return ReadUid();
        //    }
        //    else
        //        return string.Empty;
        //}


        public (string uid, bool success) GetTagOnline()
        {
            bool tagSingle = false;
            var status = reader.TagOnline(_deviceId, ref tagSingle);

            bool readSuccess = status == Execute_Status.SUCCESS;

            if (readSuccess && tagSingle)
            {
                string uid = ReadUid();
                return (uid, tagSingle);
            }
            else
            {
                return (string.Empty, tagSingle);
            }
        }

        public string ReadUid()
        {
            byte[] uid = null;
            Execute_Status status = Execute_Status.FAILURE;
            switch (_rfidType)
            {
                case 0:
                    status = reader.ReadUID(_deviceId, ref uid);
                    break;
                case 1:
                    status = reader.ReadUID_ISO14443(_deviceId, ref uid);
                    break;
                default:
                    status = reader.ReadUID_ISO14443_7_Bytes(_deviceId, ref uid);
                    break;
            }

            return status == Execute_Status.SUCCESS && uid != null
                ? BaseConversion.ByteToHexString(uid, 0, uid.Length, "")
                : string.Empty;
        }
    }
}
