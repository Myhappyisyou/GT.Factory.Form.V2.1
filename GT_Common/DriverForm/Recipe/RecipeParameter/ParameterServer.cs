using AYNETTEK.HFModbus;
using GT_Common.ProcessConfig;
using GT_Common.Helper.LanModelSync;
using GT_Common.Helper.Logging;
using GT_Common.Helper.PlcComm;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.DriverForm.Recipe.RecipeParameter
{

    public static class ParameterServer
    {
        
        public static void WriteParameterToPlc(LimitConfig limitConfig, PatchReader reader, ProductModel model)
        {
            DisplayLog.Info("开始切换工单配方",true);

            reader.Plc.Write(Config.Instance.MES_Trigger, 1);

            //  配方号
            reader.Plc.Write(Config.Instance.MES_Recipe_Code, (short)model.BaseInfo.RecipeCode);

            DisplayLog.Info($"切换{model.BaseInfo.ProductName}工单配方号{(short)model.BaseInfo.RecipeCode}", true);

            foreach (var item in limitConfig.Groups)
            {
                foreach (var para in item.Parameters)
                {
                    if (para is LimitParameter ls)
                    {
                        WriteLimitValue(ls.UpperLimit, reader);
                        WriteLimitValue(ls.LowerLimit, reader);
                    }
                    if (para is IntParameter li)
                    {
                        WriteLimitValue(li.Value, reader);
                    }
                }
            }

            DisplayLog.Info($"{model.BaseInfo.ProductName}工单配方号{(short)model.BaseInfo.RecipeCode}切换完成", true);

            short plcFeedbackResult = reader.Plc.ReadInt16(Config.Instance.PLC_Feedback_Result).Content;

            DisplayLog.Info($"PLC反馈结果{plcFeedbackResult}", true);

            short plcChangeoverFinished = reader.Plc.ReadInt16(Config.Instance.PLC_Changeover_Finished).Content;

            DisplayLog.Info($"PLC反馈切换完成{plcChangeoverFinished}", true);

            short plcCurrentType = reader.Plc.ReadInt16(Config.Instance.PLC_Current_Type).Content;

            DisplayLog.Info($"PLC反馈当前工单号{plcCurrentType}", true);
        }

        private static void WriteLimitValue(LimitPlcInfo limitValue, PatchReader reader)
        {
            if (limitValue?.Value == null) return;

            switch (limitValue.ValueType?.ToLower())
            {
                case "float":
                    reader.Plc.Write(limitValue.Address,Convert.ToSingle(limitValue.Value));
                    break;
                case "int":
                    reader.Plc.Write(limitValue.Address, Convert.ToInt16(limitValue.Value));
                    break;
                default:
                    // 可以添加日志记录或异常处理
                    break;
            }
        }

    }
}
