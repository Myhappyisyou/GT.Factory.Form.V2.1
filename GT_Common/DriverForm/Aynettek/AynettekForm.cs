using AYNETTEK.HFModbus;
using GT_Common.Driver.Aynettek;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Aynettek
{
    public partial class AynettekForm : Form
    {
        string DEMO_DLL = "V1.02";
        byte RFID_TYPE = 0;

        byte deviceID = 0;
        Reader reader = new Reader();
        MODBUS_ERR_STATUS err_status = MODBUS_ERR_STATUS.MB_SUCESS;
        ErrReason errReason = new ErrReason();
        CheckBox[] gpiBox = null;
        public AynettekForm(string UIDIP, int UIDPort)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            byte[] datas = new byte[] { 01, 03, 04, 0xE5, 0xB0, 0xCB, 0x78, 0x9B, 0xCA };
            tB_tagMemdatas.Text = BaseConversion.ByteToHexString(datas, 0, datas.Length, " ");
            tB_ip.Text = UIDIP;
            tB_ipPort.Text = UIDPort.ToString();

        }


        /// <summary>
        /// 读取错误码寄存器，分析错误原因
        /// </summary>
        private void ReadErrCode()
        {
            if (reader.GetCodeErr(deviceID, ref err_status) == Execute_Status.SUCCESS)
            {
                //分析出错误原因
                byte[] datas = new byte[2] { (byte)(((ushort)err_status >> 8) & 0xFF), (byte)(((ushort)err_status >> 0) & 0xFF) };
                string tmp = BaseConversion.ByteToHexString(datas, 0, datas.Length, "");
                SetStatus("错误码:0x" + tmp + " 错误原因：" + errReason.get_errReasion(err_status), Color.Red);
            }
        }

        private void SetStatus(string status, Color color)
        {
            this.BeginInvoke((EventHandler)delegate
            {
                if (richTB_status.TextLength > 5000)
                {
                    richTB_status.Text = "";
                }
                string temp = string.Format("{0}->{1}\r\n", System.DateTime.Now.ToString("MM-dd hh:mm:ss"), status);
                richTB_status.SelectionColor = color;
                richTB_status.AppendText(temp);
                richTB_status.SelectionStart = richTB_status.Text.Length;
                richTB_status.ScrollToCaret();
            });
        }

        private void CtrlEnable(bool state)
        {
            tB_ip.Enabled = state;
            tB_ipPort.Enabled = state;
            tB_ID.Enabled = state;
            tSMItem_close.Enabled = state;

            tabControl1.Enabled = !state;
            gB_productInfo.Enabled = !state;

            if (state)
            {
                label_demoVer.Text = "--";
                label_dllVer.Text = "--";
                label_readername.Text = "--";
                label_readerSn.Text = "--";
                label_readersoft.Text = "--";
            }
            else
            {
                richTB_status.Text = "";
                tB_tagMemdatas.Text = "";
                tB_cacheTime.Text = "";
                tB_tagMemAddr.Text = "";
                tB_tagMemCnt.Text = "";
              
                tB_UID.Text = "";
                pB_tagSingle.Image = global::GT_Common.Properties.Resources.red;
            }
        }

        private void rfid_ctrl(byte flag)
        {
            RFID_TYPE = 0;
            bool temp = true;
            if ((flag & 0x40) == 0x40 && (flag & 0x60) == 0x40)
            {
                temp = false;
                RFID_TYPE = 1;
            }
            else if ((flag & 0x40) == 0x40 && (flag & 0x60) == 0x60)
            {
                temp = false;
                RFID_TYPE = 2;
            }

            radioBt_readMem.Enabled = temp;
            radioBt_writeMem.Enabled = temp;
            tB_tagMemAddr.Enabled = temp;
            tB_tagMemCnt.Enabled = temp;
            tB_tagMemdatas.Enabled = temp;
        }

        private void bt_connectIP_Click(object sender, EventArgs e)
        {
            if (bt_connectIP.Text == "连接")
            {
                if (reader.Connect(tB_ip.Text, 1030))
                {
                    deviceID = byte.Parse(tB_ID.Text);
                    //DEMO 版本号
                    label_demoVer.Text = DEMO_DLL;
                    label_dllVer.Text = reader.CmdGetDLLVerions();

                    //软件版本号
                    byte[] softVersion = null;
                    if (reader.GetSoftVer(deviceID, ref softVersion) != Execute_Status.SUCCESS)
                    {
                        reader.DisConnect();
                        SetStatus("获取读写器软件版本号失败!", Color.Red);
                        return;
                    }
                    // 软件版本号区分协议
                    rfid_ctrl(softVersion[1]);

                    //RFID_TYPE
                    string softVer = BaseConversion.ByteToHexString(softVersion, 0, softVersion.Length, ".");
                    byte tmpVersion = BaseConversion.HexStringToOneByte(softVer.Substring(softVer.Length - 2, 2), 0);
                    if (tmpVersion >= 2)
                    {
                        string sn = "", name = "";
                        //读写器型号
                        if (reader.GetDeviceProductName(deviceID, ref name) != Execute_Status.SUCCESS)
                        {
                            reader.DisConnect();
                            SetStatus("获取产品名字失败!", Color.Red);
                            return;
                        }
                        label_readername.Text = name;
                        SetStatus("产品名字: " + name, Color.Green);
                        //读写器序列号
                        if (reader.GetDeviceProductSn(deviceID, ref sn) != Execute_Status.SUCCESS)
                        {
                            reader.DisConnect();
                            SetStatus("获取产品序列号失败!", Color.Red);
                            return;
                        }
                        label_readerSn.Text = sn;
                        SetStatus("产品序列号: " + sn, Color.Green);
                    }
                    label_readersoft.Text = softVer;
                    CtrlEnable(false);
                  
                    bt_connectIP.Text = "断开";
                    SetStatus("连接成功", Color.Green);
                }
                else
                {
                    SetStatus("连接失败!", Color.Red);
                    return;
                }
            }
            else
            {
                reader.DisConnect();
                bt_connectIP.Text = "连接";
                CtrlEnable(true);
              
                SetStatus("断开成功", Color.Green);
            }
        }

      

     
        private void tSMItem_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #region 执行标签相关功能
        /// <summary>
        /// 读取UID
        /// </summary>
        private void ReadUid()
        {
            byte[] uid = null;
            Execute_Status status = Execute_Status.FAILURE;
            if (RFID_TYPE == 0)
                status = reader.ReadUID(deviceID, ref uid);
            else if (RFID_TYPE == 1)
                status = reader.ReadUID_ISO14443(deviceID, ref uid);
            else
                status = reader.ReadUID_ISO14443_7_Bytes(deviceID, ref uid);
            switch (status)
            {
                case Execute_Status.SUCCESS:
                    if (uid != null)
                    {
                        tB_UID.Text = BaseConversion.ByteToHexString(uid, 0, uid.Length, "");
                        SetStatus("标签UID: " + tB_UID.Text, Color.Green);
                    }
                    else
                    {
                        SetStatus("读取UID失败!", Color.Red);
                    }
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("读取UID失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        /// <summary>
        /// 读取标签内存
        /// </summary>
        private void ReadMem()
        {
            if (tB_tagMemAddr.Text == "" || tB_tagMemCnt.Text == "")
            {
                SetStatus("地址或寄存器数量不能为空!", Color.Red);
                return;
            }
            if ((int.Parse(tB_tagMemAddr.Text) > (int)ModbusReg.MODBUS_TAG_END_REG_ADDRESS) ||
               ((int.Parse(tB_tagMemCnt.Text) > 120)))
            {
                SetStatus("地址或寄存器数量错误!", Color.Red);
                return;
            }

            byte[] datas = null;
            Execute_Status status = reader.ReadMemByWord(deviceID, ushort.Parse(tB_tagMemAddr.Text), byte.Parse(tB_tagMemCnt.Text), ref datas);
            switch (status)
            {
                case Execute_Status.SUCCESS:
                    if (datas != null)
                    {
                        tB_tagMemdatas.Text = BaseConversion.ByteToHexString(datas, 0, datas.Length, " ");
                        SetStatus("读取内存成功", Color.Green);
                    }
                    else
                    {
                        SetStatus("读取内存失败!", Color.Red);
                    }
                    break;
                case Execute_Status.PARA_ERR:
                    SetStatus("地址或字节数量参数错误!", Color.Red);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("读取内存失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        /// <summary>
        /// 写标签内存
        /// </summary>
        private void WriteMem()
        {
            if (tB_tagMemAddr.Text == "" || tB_tagMemCnt.Text == "")
            {
                SetStatus("地址或寄存器数量不能为空!", Color.Red);
                return;
            }
            if ((int.Parse(tB_tagMemAddr.Text) > (int)ModbusReg.MODBUS_TAG_END_REG_ADDRESS) ||
                 ((int.Parse(tB_tagMemCnt.Text) > 120)))
            {
                SetStatus("地址或寄存器数量错误!", Color.Red);
                return;
            }
            if (tB_tagMemdatas.Text == "")
            {
                SetStatus("数据不能为空!", Color.Red);
                return;
            }
            byte[] wirte = BaseConversion.HexStringToByte(tB_tagMemdatas.Text, 0);
            switch (reader.WriteMemByWord(deviceID, ushort.Parse(tB_tagMemAddr.Text), wirte))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("写标签内存成功", Color.Green);
                    break;
                case Execute_Status.PARA_ERR:
                    SetStatus("地址或字节数量参数错误!", Color.Red);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("写标签内存失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        /// <summary>
        /// 获取标签在线信号
        /// </summary>
        private void GetTagOnline()
        {
            bool tagSingle = false;
            switch (reader.TagOnline(deviceID, ref tagSingle))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("获取标签在线信号成功", Color.Green);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    return;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    return;
                default:
                    SetStatus("获取标签在线信号失败!", Color.Red);
                    ReadErrCode();
                    return;
            }
            pB_tagSingle.Image = tagSingle ? global::GT_Common.Properties.Resources.green : global::GT_Common.Properties.Resources.red;

            string tagResult = "识别范围内无标签";
            if (tagSingle)
            {
                tagResult = "识别范围内存在标签";
                ReadUid();
                SetStatus("标签UID: " + tB_UID.Text, Color.Green);
                SetStatus(tagResult, Color.Green);
            }
            else
                SetStatus(tagResult, Color.Red);
        }
        /// <summary>
        /// 获取标签缓存时间
        /// </summary>
        private void GetTagCacheTime()
        {
            ushort tagCacheTime = 0;
            switch (reader.GetTagCacheTime(deviceID, ref tagCacheTime))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("读取标签缓存时间成功", Color.Green);
                    tB_cacheTime.Text = tagCacheTime.ToString();
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("读取标签缓存时间失败!", Color.Red);
                    ReadErrCode();
                    break;
            }

        }
        /// <summary>
        /// 设置标签缓存时间
        /// </summary>
        private void SetTagCacheTime()
        {
            if (tB_cacheTime.Text == "")
            {
                SetStatus("标签缓存时间不能为空!", Color.Red);
                return;
            }

            switch (reader.SetTagCacheTime(deviceID, ushort.Parse(tB_cacheTime.Text)))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("设置标签缓存时间成功", Color.Green);
                    break;
                case Execute_Status.PARA_ERR:
                    SetStatus("缓存时间范围为0x001-0xFFF", Color.Red);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("设置标签缓存时间失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        private void bt_exec_Click(object sender, EventArgs e)
        {
            //区分不同控件
            if (radioBt_UID.Checked) //UID
            {
                ReadUid();
            }
            else if (radioBt_readMem.Checked)//读内存
            {
                ReadMem();
            }
            else if (radioBt_writeMem.Checked)//写内存
            {
                WriteMem();
            }
            else if (radioBt_tagsingle.Checked)//标签信号
            {
                GetTagOnline();
            }
            else if (radioBt_getcachetime.Checked)//获取标签缓存
            {
                GetTagCacheTime();
            }
            else if (radioBt_setCacheTime.Checked)
            {
                SetTagCacheTime();
            }
        }
        #endregion


        #region 复位-出厂设置-保存参数
        /// <summary>
        /// 保存参数
        /// </summary>
        private void SaveCfg()
        {
            switch (reader.SaveCfg(deviceID))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("保存参数成功", Color.Green);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("保存参数失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        /// <summary>
        /// 出厂设置
        /// </summary>
        private void DefaultCfg()
        {
            switch (reader.DefaultCfg(deviceID))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("恢复出厂设置成功，读写器重启生效", Color.Green);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("恢复出厂设置失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        /// <summary>
        /// 软件重启读写器
        /// </summary>
        private void ResetMcu()
        {
            if (reader.SoftResetMCU(deviceID) != Execute_Status.SUCCESS)
            {
                SetStatus("重启失败!", Color.Red);
                return;
            }
            SetStatus("重启成功", Color.Green);
           
            if (bt_connectIP.Text == "断开")
            {
                bt_connectIP_Click(this, null);
            }
        }
       

        #endregion

        #region 产品相关信息
       

        private void GetDeviceSn()
        {
            string sn = "";
            switch (reader.GetDeviceProductSn(deviceID, ref sn))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("产品序列号: " + sn, Color.Green);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("获取产品序列号失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        private void GetDeviceName()
        {
            string name = "";
            switch (reader.GetDeviceProductName(deviceID, ref name))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("产品名字: " + name, Color.Green);
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("获取产品名字失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }
        private void bt_getProduct_Click(object sender, EventArgs e)
        {
            GetDeviceSn();
            GetDeviceName();
        }
        #endregion

        #region 升级功能
        
        private void CloseConnect()
        {
            this.Invoke((EventHandler)delegate
            {
               
                if (bt_connectIP.Text == "断开")
                {
                    bt_connectIP_Click(this, null);
                }
            });
        }

        #endregion


       

        #region GPIO

        private void bt_getGPI_Click(object sender, EventArgs e)
        {
            List<GpiPin> gpio = new List<GpiPin>();
            switch (reader.GetGPI(deviceID, ref gpio))
            {
                case Execute_Status.SUCCESS:
                    SetStatus("获取GPI成功", Color.Green);
                    foreach (GpiPin gpi in gpio)
                    {
                        gpiBox[gpi.IO_Id - 1].Checked = (gpi.IO_Level) ? true : false;
                    }
                    break;
                case Execute_Status.CONNECT_CLOSE:
                    SetStatus("通信连接已断开!", Color.Red);
                    break;
                case Execute_Status.NO_RESPONSE:
                    SetStatus("无响应数据", Color.Red);
                    break;
                default:
                    SetStatus("获取GPI失败!", Color.Red);
                    ReadErrCode();
                    break;
            }
        }

      
       

        #endregion

    }
}
