using GT_Common.Helper;
using GT_Common.Helper.Logging;
using GT_Common.Helper.PlcComm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Batch
{
    public static class BatchRuntimeManager
    {
        public static string CurrentModel { get; private set; }

        public static List<PartRuntime> Parts { get; private set; } = new List<PartRuntime>();

        public static PatchReader reader { get; set; }   // ⭐ PLC注入

        // ===== 初始化型号 =====
        public static void Init(string model, List<ClientBatchConfig> allConfigs)
        {
            CurrentModel = model;

            var configs = allConfigs.Where(c => c.Model == model).ToList();

            Parts = configs.Select(c => new PartRuntime
            {
                PartName = c.PartName,
                MaterialCode = c.MaterialCode, // 如果没有匹配项，返回 null
                BYDBatchNub = c.BYDBatchNub,
                TotalQty = 0,
                UsedQty = 0,
                Config = c
            }).ToList();

            RefreshFromPlc();
        }

        public static PartRuntime GetPart(string part)
        {
            return Parts.FirstOrDefault(p => p.PartName == part);
        }

        // ===== 写入PLC（总数量）=====
        public static void WriteToPlc(PartRuntime part)
        {
            reader?.Plc.Write(part.Config.PlcWriteTotolAddress, part.TotalQty);
        }

        public static void ScanWriteToPlc(PartRuntime part)
        {
            reader?.Plc.Write(part.Config.PlcWriteTotolAddress, part.TotalQty);
            reader?.Plc.Write(part.Config.PlcWriteBYDBatchNubAddress, part.BYDBatchNub);
        }

        // ===== 从PLC读取（已用数量）=====
        public static void RefreshFromPlc()
        {
            if (reader == null) return;

            foreach (var part in Parts)
            {
                string result = StrHelper.ReplaceIllegalChar(reader.Plc.ReadString(part.Config.PlcWriteBYDBatchNubAddress, 30).Content);
                result = result.Trim('?');

                if (result == null)
                {
                    part.BYDBatchNub = string.Empty;
                    DisplayLog.Warn("PLC读取BYD批次号为空");
                }
                else if (result.Length < 2)
                {
                    part.BYDBatchNub = result;
                    DisplayLog.Warn($"PLC批次号长度异常: {result}");
                }
                else
                {
                    part.BYDBatchNub = result;
                }
                part.TotalQty = reader.Plc.ReadInt16(part.Config.PlcWriteTotolAddress).Content;
                part.UsedQty = reader.Plc.ReadInt16(part.Config.PlcReadUsedQtyAddress).Content;
            }

            Shared.Instance.partRuntimes = Parts;

        }
    }
}
