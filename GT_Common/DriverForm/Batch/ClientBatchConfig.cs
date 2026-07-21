using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Batch
{
    //配置模型
    public class ClientBatchConfig
    {
        public string Model { get; set; }          // ⭐ 型号
        public string PartName { get; set; }       // 零件名

        public string MaterialField { get; set; } = "M";      // 物料编码标识
        public string MaterialCode { get; set; }       // 物料编码
        //BYD批次标识
        public string PartField { get; set; } = "B";
        // BYD批次号
        public string BYDBatchNub { get; set; } = "";
        //供应商批次标识
        public string LotField { get; set; } = "Lot";
        //数量标识
        public string QuantityField { get; set; } = "Q";

        // ⭐ PLC地址（核心）
        public string PlcWriteTotolAddress { get; set; }   // 写总数量
        public string PlcWriteBYDBatchNubAddress { get; set; }   // 写BYD批次号

        public string PlcReadUsedQtyAddress { get; set; }    // 读已用数量
    }

}
