using System;
using System.Collections.Generic;
using System.Text;
using AYNETTEK.HFModbus;

namespace GT_Common.DriverForm.Aynettek
{
   public class ErrReason
    {
       private Dictionary<MODBUS_ERR_STATUS, string> errList = new Dictionary<MODBUS_ERR_STATUS, string>();

       public ErrReason()
       {
           errList.Add(MODBUS_ERR_STATUS.MB_SUCESS, "成功执行MODBUS指令请求");
           errList.Add(MODBUS_ERR_STATUS.MB_INVALID_FUNCODE_ERR,"功能码错误");
           errList.Add(MODBUS_ERR_STATUS.MB_INVALID_FRAME_ERR, "MODBUS请求帧格式错误");
           errList.Add(MODBUS_ERR_STATUS.MB_SYS_DATA_INVALIDE_ERR, "写入数据错误,数据大于0xFF");

           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_SINGLE_DATALEN_ERR, "写单个寄存器数据长度错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_SINGLE_REG_ADDR_ERR, "写单个寄存器地址错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_TCP_CFG_ERR, "写TCP参数(IP,GateWay)参数错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_TCP_REGCNT_ERR, "写TCP参数寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_UDP_MAC_ERR, "UDP修改IP地址，MAC地址认证错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_UDP_REGCNT_ERR, "UDP写TCP参数寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_UDP_TCP_CFG_ERR, "UDP写TCP参数(IP,GateWay)参数错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_MAC_RETCNT_ERR,"写MAC地址寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_UPGRADE_ERR,"升级功能寄存器错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_REG_ADDR_ERR,"写多个寄存器地址错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_REG_DATALEN_ERR,"写多个寄存器数量与数量长度不相等错误");
           errList.Add(MODBUS_ERR_STATUS.MB_WRITE_MULTI_DATALNE_DATAS_ERR,"写多个寄存器中的数据长度与实际数据长度不相等错误");

           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_DATALEN_ERR,"读多个寄存器指令数据长度错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_CHIP_ID_DATA_ERR,"获取芯片ID和MAC转MODBUS数据错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_CHIP_ID_REGCNT_ERR,"获取芯片ID和MAC寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_SOFT_VER_REGCNT_ERR,"读取软件版号寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_TCP_CFG_DATA_ERR,"读取网口参数转MODBUS数据错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_TCP_CFG_REGCNT_ERR,"读取网口参数寄存器错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_UDP_CFG_DATA_ERR, "UDP读取网口参数转MODBUS数据错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_UDP_CFG_REGCNT_ERR, "UDP读取网口参数转MODBUS数据错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_ERR_STATUS_REGCNT_ERR, "读取错误码寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_REG_ADDR_ERR, "读多个寄存器地址错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_INTERBRD_SN_REGCNT_ERR,"获取产品序列号寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_READ_MULTI_INTERBRD_NAME_REGCNT_ERR,"获取产品型号寄存器数量错误");

           errList.Add(MODBUS_ERR_STATUS.MB_RFID_REG_ADDR_ERR,"标签访问寄存器地址错误");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_REG_CNT_ERR,"标签访问寄存器数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_TAG_MEM_CNT_ERR,"标签字节数量错误");
           errList.Add(MODBUS_ERR_STATUS.MB_TAG_MEM_VALUE_ERR,"标签内存数据错误");
           errList.Add(MODBUS_ERR_STATUS.MB_NO_SAME_CMD_TIMOUT,"等待指令错误或超时");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_WAIT_TIMEOUT,"等待指令超时错误");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_STATUS_ERR,"标签访问状态机错误");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_EXEC_ERR, "指令执行失败");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_EXEC_FAILURE, "指令执行失败");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_NO_TAG_RESPONSE,"识别范围内无标签");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_RADIO_TRAN_ERR, "射频受到干扰，导致数据传输时发生格式错误");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_WRITE_BLOCK_LOCK, "写数据块被锁");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_WRITE_BLOCK_FAILURE, "写数据块失败");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_READ_BLOCK_FAILURE, "读取标签失败，读取一半标签离开");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_WRITE_TAG_REMOVE_OUT, "写标签失败，写数据过程中标签离开");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_WRITE_TAG_HALF_ERR, "写标签失败，写数据过程中射频数据传输错误");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_TAG_TYPE_ERR, "读取的标签暂不能兼容");
           errList.Add(MODBUS_ERR_STATUS.MB_RFID_RADIO_FREQUENCY_ERR, "射频模块异常");
       }

       public string get_errReasion(MODBUS_ERR_STATUS status)
       {
           string reasion = "";
           if (errList.TryGetValue(status, out reasion))
           {
               return reasion;
           }
           else
           {
               return "其他错误请参考文档说明";
           }
       }
    }
}
